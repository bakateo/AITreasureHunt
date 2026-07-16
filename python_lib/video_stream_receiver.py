import os
import time
import numpy as np
import cv2
from hl2ssserver.viewer import hl2ss, hl2ss_lnm

class VideoStreamReceiver:
    def __init__(self, hololens_ip, width=1920, height=1080, fps=30, profile=hl2ss.VideoProfile.H265_MAIN):
        self.ip = hololens_ip
        self.width = width
        self.height = height
        self.fps = fps
        self.profile = profile
        self.client = None

    def open_stream(self):
        print(f"[VideoReceiver] Verbinde mit hl2ss_lnm Video auf {self.ip}...")
        self.client = hl2ss_lnm.rx_pv(
            self.ip,
            hl2ss.StreamPort.PERSONAL_VIDEO,
            width=self.width,
            height=self.height,
            framerate=self.fps,
            profile=self.profile
        )
        self.client.open()
        print("[VideoReceiver] hl2ss_lnm Video-Stream geöffnet.")

    def get_next_frame(self) -> np.ndarray:
        data = self.client.get_next_packet()
        if data is None or data.payload is None:
            return None

        payload = data.payload
        if hasattr(payload, 'image'):
            frame = payload.image
        else:
            frame = payload

        return frame

    def save_next_image(self, output_folder="captured_frames", file_prefix="frame") -> str:
        """
        Holt das nächste Frame, speichert es und gibt den Pfad zurück.
        """
        frame = self.get_next_frame()
        if frame is None:
            print("[VideoReceiver] Kein Frame zum Speichern empfangen.")
            return None

        if not os.path.exists(output_folder):
            os.makedirs(output_folder)

        timestamp = int(time.time() * 1000)
        filename = f"{file_prefix}_{timestamp}.jpg"
        file_path = os.path.join(output_folder, filename)

        if len(frame.shape) == 3 and frame.shape[2] == 4:
            frame_to_save = cv2.cvtColor(frame, cv2.COLOR_BGRA2BGR)
        else:
            frame_to_save = frame

        success = cv2.imwrite(file_path, frame_to_save)
        if success:
            print(f"[VideoReceiver] Bild erfolgreich gespeichert: {file_path}")
            return file_path
        else:
            print(f"[VideoReceiver] Fehler beim Speichern unter: {file_path}")
            return None

    def close_stream(self):
        if self.client:
            self.client.close()
            print("[VideoReceiver] Video-Stream geschlossen.")