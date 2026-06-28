import whisper

path = "D:\Uni\Master\HAII\python_lib\audio_records\speech_20260628-194548.wav"
model = whisper.load_model("tiny")
result = model.transcribe(path, language="de")

print(result["text"])