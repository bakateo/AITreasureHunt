import time
import random

from ai_player import AiPlayer
from audio_speech_detector import AudioSpeechDetector
from holo_lense_device_portal import HoloLenseDevicePortal
from object_stream_receiver import ObjectStreamReceiver
from person import Person
from response_sender import ResponseSender


class HoloLensAppManager:
    def __init__(self, hololens_ip, username, password, object_port=5000, response_port=5001):
        self.hololens_ip = hololens_ip
        self.is_running = False

        self.object_receiver = ObjectStreamReceiver(listen_port=object_port)

        self.stream_receiver = HoloLenseDevicePortal(ip=hololens_ip, username=username, password=password)

        self.speech_detector = AudioSpeechDetector()
        self.response_sender = ResponseSender(hololens_ip=self.hololens_ip, target_port=response_port)
        self.ai_player = AiPlayer(person=Person.STANDARD)

    def start(self):
        self.is_running = True

        self.object_receiver.start()

        self.stream_receiver.start()

        print("[Manager] Warte auf erste Video/Audio-Frames...")
        time.sleep(2)

        self.get_object()

        print("[Manager] Alle Sub-Systeme initialisiert. Starte Hauptschleife...")
        self._main_loop()

    def get_object(self):
        current_game_state = self.object_receiver.current_data

        for i in range(4):
            time.sleep(random.randint(1, 3))
            current_game_video = self.stream_receiver.save_next_image()
            if current_game_video:
                self.ai_player.add_message(current_game_state, image_path=current_game_video)

        self.ai_player.add_message('wähle aus den letzten vier bildern ein Objekt und gib mir die Koordinaten zurück')
        answer, audio = self.ai_player.send()

        self.object_receiver.send_data(answer, message_type='object')

    def _main_loop(self):
        try:
            while self.is_running:
                pcm_data = self.stream_receiver.get_next_pcm_chunk()

                if pcm_data:
                    path = self.speech_detector.process_audio(pcm_data)

                    if path is not None:
                        current_game_state = self.object_receiver.current_data

                        self.ai_player.add_message(is_system_prompt=True, text=current_game_state)
                        self.ai_player.add_message(audio_path=path)

                        print(f"Verarbeite Audio-Datei: {path}")

                        answer, audio = self.ai_player.send()

                        self.object_receiver.send_data(answer)

                time.sleep(0.001)

        except KeyboardInterrupt:
            print("\n[Manager] Abbruch durch Benutzer...")
        finally:
            self.stop()

    def stop(self):
        print("[Manager] Beende alle Systeme...")
        self.is_running = False
        self.object_receiver.stop()
        self.stream_receiver.stop()
        self.response_sender.close()
        print("[Manager] Alle Systeme sauber beendet.")