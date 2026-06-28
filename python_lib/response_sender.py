import json
import socket
import time


class ResponseSender:
    def __init__(self, hololens_ip, target_port=50001):
        self.hololens_ip = hololens_ip
        self.port = target_port
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    def send_response(self, command_name, answer):
        """Sendet einen strukturierten Befehl als JSON an Unity."""
        payload = {
            "command": command_name,
            "timestamp": time.time(),
            "answer": answer
        }
        json_str = json.dumps(payload)
        try:
            self.sock.sendto(json_str.encode('utf-8'), (self.hololens_ip, self.port))
        except Exception as e:
            print(f"[ResponseSender] Fehler beim Senden: {e}")

    def close(self):
        self.sock.close()
