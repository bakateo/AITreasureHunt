import time

from python_lib.ai_player import AiPlayer
from python_lib.audio_speech_detector import AudioSpeechDetector
from python_lib.audio_stream_receiver import AudioStreamReceiver
from python_lib.object_stream_receiver import ObjectStreamReceiver
from python_lib.person import Person
from python_lib.response_sender import ResponseSender


class HoloLensAppManager:
    def __init__(self, hololens_ip, object_port=50000, response_port=50001):
        self.hololens_ip = hololens_ip
        self.is_running = False

        self.object_receiver = ObjectStreamReceiver(listen_port=object_port)
        self.audio_receiver = AudioStreamReceiver(hololens_ip=self.hololens_ip)
        self.speech_detector = AudioSpeechDetector()
        self.response_sender = ResponseSender(hololens_ip=self.hololens_ip, target_port=response_port)
        self.ai_player = AiPlayer(person=Person.STANDARD)

    def start(self):
        self.is_running = True

        self.object_receiver.start()
        self.audio_receiver.open_stream()

        print("[Manager] Alle Sub-Systeme initialisiert. Starte Hauptschleife...")
        self._main_loop()

    def _main_loop(self):
        try:
            while self.is_running:
                pcm_data = self.audio_receiver.get_next_pcm_chunk()
                if pcm_data:
                    path = self.speech_detector.process_audio(pcm_data)
                    current_game_state = self.object_receiver.current_data

                    self.ai_player.add_message(is_system_prompt=True, text=current_game_state)
                    self.ai_player.add_message(audio_path=path)

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
        self.audio_receiver.close_stream()
        self.response_sender.close()
        print("[Manager] Alle Systeme sauber beendet.")
