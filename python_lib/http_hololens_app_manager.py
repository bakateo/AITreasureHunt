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

        # Wichtig:
        # AiPlayer NICHT direkt beim Serverstart initialisieren.
        # Sonst kann der Server beim Start hängen, wenn Ollama/Whisper/etc. lädt.
        self.ai_player = None
        self.ai_lock = threading.Lock()

        self.audio_dir = "audio_records"
        os.makedirs(self.audio_dir, exist_ok=True)

        self._register_routes()

    def _get_ai_player(self):
        if self.ai_player is None:
            print("[HTTP] Initialisiere AiPlayer...")
            self.ai_player = AiPlayer(person=Person.STANDARD)
            print("[HTTP] AiPlayer initialisiert.")

        return self.ai_player

    def _register_routes(self):
        @self.app.route("/health", methods=["GET"])
        def health():
            return jsonify({
                "status": "ok",
                "message": "HoloLens HTTP AI server running"
            })

        @self.app.route("/speech", methods=["POST"])
        def speech():
            print("[HTTP] /speech request received")
            print("[HTTP] Content-Length:", request.content_length)

            try:
                audio_path = self._save_incoming_audio()
                game_state_text = self._read_game_state()

                print("[HTTP] Audio saved:", audio_path)

                if game_state_text:
                    print("[HTTP] Game state received:")
                    print(game_state_text)

                with self.ai_lock:
                    ai_player = self._get_ai_player()

                    if game_state_text:
                        ai_player.add_message(
                            is_system_prompt=True,
                            text=(
                                "Aktueller Spielzustand des Mixed-Reality-Suchspiels:\n"
                                + game_state_text
                                + "\n\n"
                                "Du bist ein freundlicher KI-Mitspieler. "
                                "Gib kurze, hilfreiche Hinweise auf Deutsch. "
                                "Verrate nicht direkt die exakte Position des Suchobjekts."
                            )
                        )

                    print("[HTTP] Sending audio to AiPlayer...")
                    ai_player.add_message(audio_path=audio_path)

                    print("[HTTP] Waiting for AiPlayer response...")
                    answer, audio = ai_player.send()

                if answer is None:
                    answer = ""

                answer = str(answer).strip()

                print("[HTTP] AI answer:")
                print(answer)

                return jsonify({
                    "answer": answer
                })

            except Exception as error:
                print("[HTTP ERROR]", repr(error))

                return jsonify({
                    "error": str(error)
                }), 500

    def _save_incoming_audio(self):
        audio_bytes = None

        # Variante 1: Unity sendet multipart/form-data mit Feld "audio"
        if "audio" in request.files:
            audio_bytes = request.files["audio"].read()

        # Variante 2: Unity sendet raw audio/wav im Body
        else:
            audio_bytes = request.get_data()

        if not audio_bytes:
            raise ValueError("No audio data received.")

        temp_file = tempfile.NamedTemporaryFile(
            delete=False,
            suffix=".wav",
            dir=self.audio_dir
        )

        temp_file.write(audio_bytes)
        temp_file.close()

        return temp_file.name

    def _read_game_state(self):
        # Variante 1: Unity sendet GameState als Header
        game_state = request.headers.get("X-Game-State", "")

        # Variante 2: Unity sendet GameState als Form-Field
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
        print(f"[HTTP] Health check: http://{self.host}:{self.port}/health")
        print(f"[HTTP] Speech endpoint: http://{self.host}:{self.port}/speech")

        self.app.run(
            host=self.host,
            port=self.port,
            threaded=True,
            debug=False,
            use_reloader=False
        )


if __name__ == "__main__":
    manager = HttpHoloLensAppManager(
        host="0.0.0.0",
        port=5000
    )

    manager.start()