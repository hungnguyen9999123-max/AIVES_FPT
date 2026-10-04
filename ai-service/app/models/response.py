"""
AIVES AI Service - Response Models
Pydantic schemas for all outgoing API responses to the Backend.
"""

from typing import List, Optional
from pydantic import BaseModel, Field


# ─────────────────────────────────────────────
#  Shared / Common
# ─────────────────────────────────────────────


class BaseResponse(BaseModel):
    """Base wrapper for all AI service responses."""

    success: bool = Field(default=True)
    message: Optional[str] = Field(default=None)


class ErrorResponse(BaseModel):
    """Standard error response."""

    success: bool = False
    error: str
    detail: Optional[str] = None


# ─────────────────────────────────────────────
#  Interview - Generate Question Response
# ─────────────────────────────────────────────


class GenerateQuestionResponse(BaseResponse):
    """Response from AI question generation."""

    question: str = Field(..., description="The AI-generated interview question")
    question_type: str = Field(
        default="conceptual",
        description="Type: 'conceptual' | 'application' | 'analysis'",
    )
    difficulty: str = Field(
        default="medium", description="Difficulty: 'easy' | 'medium' | 'hard'"
    )
    expected_keywords: List[str] = Field(
        default_factory=list,
        description="Key concepts expected in a good answer",
    )
    topic: Optional[str] = Field(
        default=None, description="Topic/chapter this question covers"
    )


# ─────────────────────────────────────────────
#  Interview - Follow-up Question Response
# ─────────────────────────────────────────────


class GenerateFollowUpResponse(BaseResponse):
    """Response containing a follow-up/probing question."""

    follow_up_question: str = Field(
        ..., description="The AI-generated follow-up question"
    )
    reason: Optional[str] = Field(
        default=None,
        description="Why this follow-up was generated (for Teacher review)",
    )
    should_follow_up: bool = Field(
        default=True,
        description="Whether a follow-up is needed (False = answer was sufficient)",
    )


# ─────────────────────────────────────────────
#  Speech - Speech-to-Text Response
# ─────────────────────────────────────────────


class SpeechToTextResponse(BaseResponse):
    """Response from STT transcription."""

    transcript: str = Field(..., description="Transcribed text from student's audio")
    confidence: Optional[float] = Field(
        default=None,
        ge=0.0,
        le=1.0,
        description="Confidence score (0.0 to 1.0)",
    )
    language_detected: Optional[str] = Field(
        default=None, description="Detected language BCP-47 code"
    )


# ─────────────────────────────────────────────
#  Speech - Text-to-Speech Response
# ─────────────────────────────────────────────


class TextToSpeechResponse(BaseResponse):
    """Response from TTS conversion."""

    audio_base64: str = Field(
        ..., description="Base64-encoded audio data (WAV/MP3)"
    )
    audio_format: str = Field(default="mp3", description="Audio format: 'mp3' | 'wav'")
    duration_seconds: Optional[float] = Field(
        default=None, description="Duration of audio in seconds"
    )


# ─────────────────────────────────────────────
#  Evaluation - Single Question Score Response
# ─────────────────────────────────────────────


class EvaluateAnswerResponse(BaseResponse):
    """Response from evaluating a single Q&A pair."""

    question_id: str = Field(..., description="UUID of the evaluated Question")
    score: float = Field(..., ge=0, description="Score awarded for this answer")
    max_score: float = Field(..., ge=0, description="Maximum possible score")
    feedback: str = Field(..., description="AI-generated textual feedback")
    strengths: List[str] = Field(
        default_factory=list, description="What the student did well"
    )
    weaknesses: List[str] = Field(
        default_factory=list, description="Areas needing improvement"
    )
    missing_content: List[str] = Field(
        default_factory=list,
        description="Key content missing from the student's answer",
    )


# ─────────────────────────────────────────────
#  Evaluation - Full Session Score Response
# ─────────────────────────────────────────────


class QuestionScoreDetail(BaseModel):
    """Per-question score detail within a session evaluation."""

    question_id: str
    question_text: str
    student_answer: str
    score: float
    max_score: float
    feedback: str
    strengths: List[str] = Field(default_factory=list)
    weaknesses: List[str] = Field(default_factory=list)
    missing_content: List[str] = Field(default_factory=list)


class EvaluateSessionResponse(BaseResponse):
    """Response from evaluating a complete exam session."""

    exam_session_id: str = Field(..., description="UUID of the ExamSession")
    total_score: float = Field(..., ge=0, description="Total score for the session")
    max_total_score: float = Field(
        ..., ge=0, description="Maximum possible total score"
    )
    percentage: float = Field(
        ..., ge=0.0, le=100.0, description="Score as percentage"
    )
    overall_feedback: str = Field(
        ..., description="Overall AI feedback for the session"
    )
    question_scores: List[QuestionScoreDetail] = Field(
        ..., description="Per-question breakdown"
    )
    status: str = Field(
        default="PendingReview",
        description="Result status: always 'PendingReview' after AI evaluation",
    )


# ─────────────────────────────────────────────
#  Health Check
# ─────────────────────────────────────────────


class HealthResponse(BaseModel):
    """Health check response."""

    status: str = "ok"
    service: str = "aives-ai"
    version: str = "1.0.0"
    ai_provider: Optional[str] = None
    stt_provider: Optional[str] = None
    tts_provider: Optional[str] = None
