from fastapi import FastAPI

app = FastAPI(title="AIVES AI Service")

@app.get("/health")
def health_check():
    return {"status": "ok", "service": "aives-ai"}

@app.post("/ai/interview/question")
def generate_question():
    return {"success": True, "message": "AI endpoint placeholder"}

@app.post("/ai/interview/follow-up")
def generate_followup():
    return {"success": True, "message": "AI endpoint placeholder"}

@app.post("/ai/speech-to-text")
def speech_to_text():
    return {"success": True, "message": "AI endpoint placeholder"}

@app.post("/ai/text-to-speech")
def text_to_speech():
    return {"success": True, "message": "AI endpoint placeholder"}

@app.post("/ai/evaluate")
def evaluate_answer():
    return {"success": True, "message": "AI endpoint placeholder"}
