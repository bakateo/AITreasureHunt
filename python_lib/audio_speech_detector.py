import collections
import os
import time
import wave

import webrtcvad


class AudioSpeechDetector:
    def __init__(self, sample_rate=16000, aggressiveness=3, save_dir="audio_records"):
        self.sample_rate = sample_rate
        self.vad = webrtcvad.Vad(aggressiveness)
        self.save_dir = save_dir

        if not os.path.exists(self.save_dir):
            os.makedirs(self.save_dir)

        self.frame_duration_ms = 30
        self.frame_size = int(self.sample_rate * (self.frame_duration_ms / 1000.0) * 2)

        self.ring_buffer = collections.deque(maxlen=10)  # 300ms Padding
        self.triggered = False
        self.voiced_frames = []
        self.audio_buffer = bytearray()

    def process_audio(self, pcm_data: bytes):
        """Verarbeitet PCM-Bytes, erkennt Sprache und speichert bei Stille automatisch."""
        self.audio_buffer.extend(pcm_data)

        while len(self.audio_buffer) >= self.frame_size:
            frame = bytes(self.audio_buffer[:self.frame_size])
            self.audio_buffer = self.audio_buffer[self.frame_size:]

            is_speech = self.vad.is_speech(frame, self.sample_rate)

            if not self.triggered:
                self.ring_buffer.append((frame, is_speech))
                num_voiced = len([f for f, speech in self.ring_buffer if speech])

                if num_voiced > 0.9 * self.ring_buffer.maxlen:
                    self.triggered = True
                    print("\n[VAD] Sprache erkannt! Aufzeichnung läuft...")
                    for f, s in self.ring_buffer:
                        self.voiced_frames.append(f)
                    self.ring_buffer.clear()

                return None
            else:
                self.voiced_frames.append(frame)
                self.ring_buffer.append((frame, is_speech))
                num_unvoiced = len([f for f, speech in self.ring_buffer if not speech])

                if num_unvoiced > 0.9 * self.ring_buffer.maxlen:
                    self.triggered = False
                    print("[VAD] Stille erkannt. Verarbeite Datei...")
                    timestamp = self._save_to_wav()
                    self.ring_buffer.clear()
                    self.voiced_frames = []

                    return timestamp
        return None

    def _save_to_wav(self):
        timestamp = time.strftime("%Y%m%d-%H%M%S")
        filename = os.path.join(self.save_dir, f"speech_{timestamp}.wav")
        with wave.open(filename, 'wb') as wf:
            wf.setnchannels(1)
            wf.setsampwidth(2)
            wf.setframerate(self.sample_rate)
            wf.writeframes(b''.join(self.voiced_frames))
        print(f"[VAD] Audio gespeichert: {filename}")

        return timestamp
