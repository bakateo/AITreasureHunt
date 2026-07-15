import collections
import os
import time
import wave

import webrtcvad


class AudioSpeechDetector:
    def __init__(
        self,
        sample_rate=16000,
        aggressiveness=3,
        save_dir="audio_records",
        pause_threshold_ms=1200,
    ):
        self.sample_rate = sample_rate
        self.vad = webrtcvad.Vad(aggressiveness)
        self.save_dir = save_dir

        if not os.path.exists(self.save_dir):
            os.makedirs(self.save_dir)

        self.frame_duration_ms = 30
        self.frame_size = int(self.sample_rate * (self.frame_duration_ms / 1000.0) * 2)

        # Ringbuffer nur noch für den START der Aufnahme (300ms)
        self.ring_buffer = collections.deque(maxlen=10)
        self.triggered = False
        self.voiced_frames = []
        self.audio_buffer = bytearray()

        # NEU: Toleranz für Pausen am ENDE der Aufnahme
        # pause_threshold_ms = 1200ms (1,2 Sekunden) ist ein guter Standardwert für natürliche Sprache
        self.pause_threshold_frames = int(pause_threshold_ms / self.frame_duration_ms)
        self.silent_frame_count = 0

    def process_audio(self, pcm_data: bytes):
        """Verarbeitet PCM-Bytes, erkennt Sprache und speichert bei längerer Stille automatisch."""
        self.audio_buffer.extend(pcm_data)

        while len(self.audio_buffer) >= self.frame_size:
            frame = bytes(self.audio_buffer[: self.frame_size])
            self.audio_buffer = self.audio_buffer[self.frame_size :]

            is_speech = self.vad.is_speech(frame, self.sample_rate)

            if not self.triggered:
                self.ring_buffer.append((frame, is_speech))
                num_voiced = len([f for f, speech in self.ring_buffer if speech])

                # Trigger: Wenn 90% der letzten 300ms Sprache waren
                if num_voiced > 0.9 * self.ring_buffer.maxlen:
                    self.triggered = True
                    print("\n[VAD] Sprache erkannt! Aufzeichnung läuft...")
                    for f, s in self.ring_buffer:
                        self.voiced_frames.append(f)
                    self.ring_buffer.clear()
                    self.silent_frame_count = 0  # Stille-Zähler zurücksetzen

            else:
                self.voiced_frames.append(frame)

                # NEU: Zähler für aufeinanderfolgende Stille-Frames
                if not is_speech:
                    self.silent_frame_count += 1
                else:
                    self.silent_frame_count = (
                        0  # Sobald wieder gesprochen wird, Zähler auf 0 setzen
                    )

                # Trigger-Ende: Wenn ununterbrochene Stille das Limit (z.B. 1.2 Sekunden) überschreitet
                if self.silent_frame_count >= self.pause_threshold_frames:
                    self.triggered = False
                    print("[VAD] Stille erkannt. Verarbeite Datei...")
                    timestamp = self._save_to_wav()

                    self.voiced_frames = []
                    self.silent_frame_count = 0
                    self.ring_buffer.clear()

                    return timestamp
        return None

    def _save_to_wav(self):
        timestamp = time.strftime("%Y%m%d-%H%M%S")
        filename = os.path.join(self.save_dir, f"speech_{timestamp}.wav")
        with wave.open(filename, "wb") as wf:
            wf.setnchannels(1)
            wf.setsampwidth(2)
            wf.setframerate(self.sample_rate)
            wf.writeframes(b"".join(self.voiced_frames))
        print(f"[VAD] Audio gespeichert: {filename}")

        return filename
