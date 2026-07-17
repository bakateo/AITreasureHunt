using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq.Expressions;

public class UdpSocket : MonoBehaviour
{
    [HideInInspector] public bool isTxStarted = false;

    [SerializeField] string IP = "192.168.178.107"; // local host
    [SerializeField] int rxPort = 5000; // port to receive data from Python on
    [SerializeField] int txPort = 5001; // port to send data to Python on

    [Serializable]
    public class SpeechMessage
    {
        public string type;
        public string text;
    }

    [Serializable]
    public class LocationData
    {
        public float distance;
        public float angle;
        public float horizontalAngle;
        public string directionText;
        public bool visible;
        public string state;
    }

    [Serializable]
    public class LocationMessage
    {
        public string type;
        public LocationData location;
    }
    public TextToSpeechManager textToSpeech;
    public event System.Action<Vector3> OnLocationReceived;

    // Create necessary UdpClient objects
    UdpClient client;
    IPEndPoint remoteEndPoint;
    Thread receiveThread; // Receiving Thread

    public void SendTTS(string text)
    {
        SpeechMessage msg = new SpeechMessage
        {
            type = "tts",
            text = text
        };

        SendJson(JsonUtility.ToJson(msg));
    }

    public void SendLocation(LocationData location)
    {
        LocationMessage msg = new LocationMessage
        {
            type = "location",
            location = location
        };

        SendJson(JsonUtility.ToJson(msg));
    }

    private void SendJson(string json)
    {
        byte[] data = Encoding.UTF8.GetBytes(json);
        client.Send(data, data.Length, remoteEndPoint);

        Debug.Log("Sent: " + json);
    }

    void Awake()
    {
        // Create remote endpoint (to Matlab) 
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), txPort);

        // Create local client
        client = new UdpClient(rxPort);

        // local endpoint define (where messages are received)
        // Create a new thread for reception of incoming messages
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

        if (textToSpeech == null)
        {
            textToSpeech = FindObjectOfType<TextToSpeechManager>();
        }

        // Initialize (seen in comments window)
        print("UDP Comms Initialised");
    }

    // Receive data, update packets received
    private void ReceiveData()
    {
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                string json = Encoding.UTF8.GetString(data);
                Debug.Log("UDP JSON:" + json);

                ProcessInput(json);
            }
            catch (Exception err)
            {
                Debug.Log(err.ToString());
            }
        }
    }

    private void ProcessInput(string input)
    {
        try
        {
            MessageType header = JsonUtility.FromJson<MessageType>(input);

            switch (header.type)
            {
                case "tts":
                    {
                        SpeechMessage msg =
                            JsonUtility.FromJson<SpeechMessage>(input);

                        textToSpeech?.Speak(msg.text);
                        break;
                    }

                case "location":
                    {
                        LocationMessage msg = JsonUtility.FromJson<LocationMessage>(input);

                        float yaw = msg.location.angle * Mathf.Deg2Rad;
                        float pitch = msg.location.horizontalAngle * Mathf.Deg2Rad;

                        Vector3 direction = new Vector3(
                            Mathf.Sin(yaw) * Mathf.Cos(pitch),
                            Mathf.Sin(pitch),
                            Mathf.Cos(yaw) * Mathf.Cos(pitch)
                        );

                        Vector3 worldPosition =
                            Camera.main.transform.position +
                            direction * msg.location.distance;

                        OnLocationReceived?.Invoke(worldPosition);

                        break;
                    }

                default:
                    Debug.LogWarning("Unknown message type");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    [Serializable]
    public class MessageType
    {
        public string type;
    }

    //Prevent crashes - close clients and threads properly!
    void OnDisable()
    {
        if (receiveThread != null)
            receiveThread.Abort();

        if (client != null)
        {
            client.Close();
        }
    }

}