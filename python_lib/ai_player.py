import os
import subprocess
import time

import ollama


class AiPlayer:
    def __init__(self, person, gen_audio=False):
        self.model = "gemma4:e2b-it-q4_K_M"
        self.history = []
        self.system_prompt = None
        self.piper_model_path = person.voice_path
        self.output_dir = "assets/output"
        self.person = person
        self.gen_audio = gen_audio
        os.makedirs(self.output_dir, exist_ok=True)

    def set_person(self):
        self.history.append(
            {'role': 'system', 'content': self.person.system_prompt}
        )

    def set_object(self, content, image_path):
        self.history.append(
            {
                "role": "system",
                "content": content,
                "images": [image_path]
            }
        )
        self.send()

    def _save_as_wav(self, text):
        import sys

        text_clean = text.replace("*", "").replace("#", "").replace("`", "")
        text_clean = text_clean.replace("\n", " ").strip()

        timestamp = int(time.time())
        wav_path = os.path.join(self.output_dir, f"antwort_{timestamp}.wav")

        try:
            piper_exe = os.path.join(os.path.dirname(sys.executable), "piper")
            if not os.path.exists(piper_exe):
                piper_exe = "piper"

            piper_cmd = [
                piper_exe,
                "--model", self.piper_model_path,
                "--output_file", wav_path,
                "--cuda",
                "--length_scale", "1.0",
                "--sentence_silence", "0.3",
                "--speaker", "2"
            ]

            print("\n[System] Generiere Audio (Synchron mit CUDA)...")
            result = subprocess.run(
                piper_cmd,
                input=text_clean,
                text=True,
                capture_output=True
            )

            if result.returncode != 0:
                print(f"\n[Piper-Absturz]: {result.stderr}")
                return None

            if not os.path.exists(wav_path) or os.path.getsize(wav_path) == 0:
                print("\n[Fehler] Piper lief durch, hat aber eine leere/keine Datei erzeugt.")
                return None

            print(f"[System] Audio erfolgreich gespeichert als: {wav_path}")

            return wav_path
        except FileNotFoundError as e:
            print(f"\n[Pfad-Fehler]: Piper wurde nicht gefunden. ({e})")
        except Exception as e:
            print(f"\n[System-Fehler beim Audio]: {e}")

    def add_message(self, text="", image_path: str | None = None, audio_path: str | None = None,
                    is_system_prompt: bool = False):
        msg = {'role': 'user' if not is_system_prompt else 'system', 'content': text}

        if image_path:
            msg['images'] = image_path

        if audio_path:
            msg['audio'] = audio_path

        self.history.append(msg)

    def send(self):
        print(f"\n[{self.model}]: ", end="", flush=True)
        stream = ollama.chat(model=self.model, messages=self.history, stream=True)

        full_response = ""
        for chunk in stream:
            word = chunk['message']['content']
            print(word, end="", flush=True)
            full_response += word
        print("\n")

        self.history.append({'role': 'assistant', 'content': full_response})

        path = self._save_as_wav(full_response) if self.gen_audio else None

        return full_response, path
