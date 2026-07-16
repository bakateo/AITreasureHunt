import os
import queue
import threading
import time

import av
import cv2
import requests
from requests.auth import HTTPBasicAuth


class HoloLenseDevicePortal:
    def __init__(self, ip, username, password):
        self.url = f"https://{ip}/api/holographic/stream/live_high.mp4"
        self.auth = HTTPBasicAuth(username, password)

        self.audio_queue = queue.Queue(maxsize=100)
        self.latest_video_frame = None
        self.is_running = False
        self.thread = None

    def start(self):
        self.is_running = True
        self.thread = threading.Thread(target=self._stream_loop, daemon=True)
        self.thread.start()
        print("[StreamReceiver] Thread gestartet. Verbinde zur HoloLens...")

    def _stream_loop(self):
        params = {"holo": "true", "pv": "true", "mic": "true", "loopback": "true"}

        try:
            response = requests.get(self.url, auth=self.auth, verify=False, params=params, stream=True)
            response.raise_for_status()

            container = av.open(response.raw, format='mp4')
            video_stream = container.streams.video[0] if container.streams.video else None
            audio_stream = container.streams.audio[0] if container.streams.audio else None

            resampler = av.AudioResampler(format='s16', layout='mono', rate=16000) # do not touch

            for packet in container.demux((video_stream, audio_stream)):
                if not self.is_running:
                    break

                for frame in packet.decode():
                    if packet.stream.type == 'video':
                        self.latest_video_frame = frame.to_ndarray(format='bgr24')

                    elif packet.stream.type == 'audio':
                        frame.pts = None
                        resampled_frames = resampler.resample(frame)
                        for resampled_frame in resampled_frames:
                            pcm_bytes = resampled_frame.to_ndarray().tobytes()
                            if not self.audio_queue.full():
                                self.audio_queue.put(pcm_bytes)

        except Exception as e:
            print(f"[StreamReceiver] Fehler im Stream: {e}")
        finally:
            self.is_running = False

    def get_next_pcm_chunk(self):
        """Holt den nächsten Audio-Chunk aus der Queue."""
        try:
            return self.audio_queue.get_nowait()
        except queue.Empty:
            return None

    def save_next_image(self, save_dir="images"):
        """Speichert das aktuellste Video-Frame als JPG und gibt den Pfad zurück."""
        if self.latest_video_frame is not None:
            if not os.path.exists(save_dir):
                os.makedirs(save_dir)

            timestamp = time.strftime("%Y%m%d-%H%M%S")
            path = os.path.join(save_dir, f"frame_{timestamp}.jpg")
            cv2.imwrite(path, self.latest_video_frame)
            return path
        return None

    def stop(self):
        self.is_running = False
        if self.thread:
            self.thread.join(timeout=2)