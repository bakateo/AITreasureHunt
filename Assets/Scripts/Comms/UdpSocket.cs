/*
Created by Youssef Elashry to allow two-way communication between Python3 and Unity to send and receive strings

Feel free to use this in your individual or commercial projects BUT make sure to reference me as: Two-way communication between Python 3 and Unity (C#) - Y. T. Elashry
It would be appreciated if you send me how you have used this in your projects (e.g. Machine Learning) at youssef.elashry@gmail.com

Use at your own risk
Use under the Apache License 2.0

Modified by: 
Youssef Elashry 12/2020 (replaced obsolete functions and improved further - works with Python as well)
Based on older work by Sandra Fang 2016 - Unity3D to MATLAB UDP communication - [url]http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC[/url]
*/

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
    [SerializeField] int rxPort = 8000; // port to receive data from Python on
    [SerializeField] int txPort = 8001; // port to send data to Python on

    [Serializable]
    public class SpeechMessage
    {
        public string type;
        public string text;
    }

    public TextToSpeechManager textToSpeech;

    // Create necessary UdpClient objects
    UdpClient client;
    IPEndPoint remoteEndPoint;
    Thread receiveThread; // Receiving Thread

    public void SendData(string message) // Use to send data to Python
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
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
        // PROCESS INPUT RECEIVED STRING HERE

        if (!isTxStarted) // First data arrived so tx started
        {
            isTxStarted = true;
        }

        try
        {
            SpeechMessage message =
                JsonUtility.FromJson<SpeechMessage>(input);



            if (message == null)
            {
                Debug.LogWarning(
                    "JSON parsing failed");
                return;
            }



            Debug.Log("Received type: "
                + message.type
            );


            Debug.Log(
                "Received text: "
                + message.text
            );

            if (message.type == "tts")
            {

                if (textToSpeech != null)
                {
                    textToSpeech.Speak(
                        message.text
                    );
                }

                else
                {
                    Debug.LogWarning(
                        "No TTS Manager found"
                    );
                }

            }
        }
        catch (Exception e)
        {
            Debug.LogError(
             "JSON Error: " + e.Message
         );
        }


            if (textToSpeech != null)
        {
            textToSpeech.Speak(input);
        }
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