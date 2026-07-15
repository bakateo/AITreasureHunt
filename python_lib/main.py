from holo_lens_app_manager import HoloLensAppManager

if __name__ == "__main__":
    server = HoloLensAppManager(hololens_ip="192.168.178.145")

    try:
        server.start()

    except KeyboardInterrupt:
        print("\n[Test] Abbruch durch Benutzer (STRG+C).")
    finally:
        server.stop
