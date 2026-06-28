import json
import os
import tempfile
import threading

from flask import Flask, request, jsonify

from ai_player import AiPlayer
from person import Person


class HttpHoloLensAppManager:
    def __init__(self, host="0.0.0.0", port=5000):
        self.host = host
        self.port = port

        self.app = Flask(__name__)
        self.ai_player = AiPlayer(person=Person.STANDARD)

        self.ai_lock = threading.Lock()

        if not os.path.exists("audio_records"):
            os.makedirs("audio_records")

        self._register_routes()

    def _register_routes(self):
        @self.app.route("/health", methods=["GET"])
        def health():
            return jsonify({
                "status": "ok",
                "message": "HoloLens HTTP AI server running"
            })

        @self.app.route("/speech", methods=["POST"])
        def speech():
            try:
                audio_path = self._save_incoming_audio()
                game_state_text = self._read_game_state()

                print("[HTTP] Audio received:", audio_path)

                if game_state_text:
                    print("[HTTP] Game state:", game_state_text)

                with self.ai_lock:
                    if game_state_text:
                        self.ai_player.add_message(
                            is_system_prompt=True,
                            text=(
                                "Aktueller Spielzustand des Mixed-Reality-Suchspiels:\n"
                                + game_state_text
                                + "\n\nAntworte kurz, hilfreich und auf Deutsch."
                            )
                        )

                    self.ai_player.add_message(audio_path=audio_path)

                    answer, audio = self.ai_player.send()

                print("[AI ANSWER]", answer)

                return jsonify({
                    "answer": answer
                })

            except Exception as error:
                print("[HTTP ERROR]", error)

                return jsonify({
                    "error": str(error)
                }), 500

    def _save_incoming_audio(self):
        audio_bytes = None

        if "audio" in request.files:
            audio_bytes = request.files["audio"].read()
        else:
            audio_bytes = request.get_data()

        if not audio_bytes:
            raise ValueError("No audio data received.")

        temp_file = tempfile.NamedTemporaryFile(
            delete=False,
            suffix=".wav",
            dir="audio_records"
        )

        temp_file.write(audio_bytes)
        temp_file.close()

        return temp_file.name

    def _read_game_state(self):
        game_state = request.headers.get("X-Game-State", "")

        if not game_state and "game_state" in request.form:
            game_state = request.form["game_state"]

        if not game_state:
            return ""

        try:
            parsed = json.loads(game_state)
            return json.dumps(parsed, ensure_ascii=False, indent=2)
        except Exception:
            return game_state

    def start(self):
        print(f"[HTTP] Starting server on {self.host}:{self.port}")
        self.app.run(host=self.host, port=self.port, threaded=True)


if __name__ == "__main__":
    manager = HttpHoloLensAppManager(host="0.0.0.0", port=5000)
    manager.start()