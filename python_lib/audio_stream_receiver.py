import numpy as np
import scipy.signal

from hl2ssserver.viewer import hl2ss, hl2ss_lnm  # clone hl2ss in ordner hl2ss_ext


class AudioStreamReceiver:
    def __init__(self, hololens_ip, profile=hl2ss.AudioProfile.AAC_24000):
        self.ip = hololens_ip
        self.profile = profile
        self.client = None

        self.source_rate = 48000 if (self.profile == hl2ss.AudioProfile.RAW) else 24000
        self.target_rate = 16000

    def open_stream(self):
        print(f"[AudioReceiver] Verbinde mit hl2ss_lnm Audio auf {self.ip}...")

        self.client = hl2ss_lnm.rx_microphone(
            self.ip, hl2ss.StreamPort.MICROPHONE, profile=self.profile
        )
        self.client.open()
        print("[AudioReceiver] hl2ss_lnm Stream geöffnet.")

    def get_next_pcm_chunk(self) -> bytes:
        """
        Holt das nächste Paket via hl2ss_lnm, konvertiert float32 zu int16
        und resampelt es auf 16kHz für die webrtcvad-Erkennung.
        """
        data = self.client.get_next_packet()

        if data is None or data.payload is None:
            return b""

        payload = data.payload

        if self.profile != hl2ss.AudioProfile.RAW:
            if len(payload.shape) > 1:
                mono_data = payload[0]
            else:
                mono_data = payload

            mono_data = np.clip(mono_data * 32767.0, -32768.0, 32767.0).astype(np.int16)
        else:
            if len(payload.shape) > 1:
                mono_data = payload[:, 0]
            else:
                mono_data = payload

        if self.source_rate != self.target_rate:
            resampled = scipy.signal.resample_poly(
                mono_data, self.target_rate, self.source_rate
            )
            mono_data = resampled.astype(np.int16)

        return mono_data.tobytes()

    def close_stream(self):
        if self.client:
            self.client.close()
            print("[AudioReceiver] Stream geschlossen.")
