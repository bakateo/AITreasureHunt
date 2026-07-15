import json
import threading

import UdpComms as U


class ObjectStreamReceiver:
    def __init__(self, listen_port=50000):
        self.port = listen_port
        self.current_data = {
            "distance": 0.0, "angle": 0.0, "horizontalAngle": 0.0,
            "directionText": "", "visible": False, "state": "Init"
        }
        self._is_running = False
        self._thread = None
        self.socket = U.UdpComms(udpIP="127.0.0.1", portTX=8000, portRX=8001, enableRX=True, suppressWarnings=True)

    def start(self):
        self._is_running = True
        self._thread = threading.Thread(target=self._run, daemon=True)
        self._thread.start()

    def _run(self):

        while self._is_running:
            data = self.socket.ReadReceivedData()

            if data is not None:
                self.current_data = json.loads(data)

    def stop(self):
        self._is_running = False
        if self._thread:
            self._thread.join()

    def send_data(self, text):
        reply = {"answer": text}
        self.socket.SendData(json.dumps(reply))
