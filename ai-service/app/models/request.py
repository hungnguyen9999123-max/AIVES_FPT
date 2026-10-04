"""
AIVES AI Service - Request Models
Pydantic schemas for all incoming API requests from the Backend.
"""

from typing import List, Optional
from pydantic import BaseModel, Field


# ─────────────────────────────────────────────
#  Shared / Common
# ─────────────────────────────────────────────


class DocumentContext(BaseModel):
    """Represents a .md document linked to a Course."""

    document_id: str = Field(..., description="UUID of the MarkdownDocument")
    title: str = Field(..., description="Document title")
    content: str = Field(..., description="Full markdown content of the document")
    document_type: str = Field(
        ..., description="'course_content' | 'learning_outcomes'"
    )


# ─────────────────────────────────────────────
#  Interview - Generate Question
# ─────────────────────────────────────────────


class GenerateQuestionRequest(BaseModel):
    """
    Request to generate the next interview question for a Student.
    Sent by Backend at the start of a session or after each answered question.
    """

    exam_session_id: str = Field(..., description="UUID of the ExamSession")
    exam_id: str = Field(..., description="UUID of the Exam")
    course_name: str = Field(..., description="Name of the course being examined")
    documents: List[DocumentContext] = Field(
        ..., description="List of .md documents (content + learning outcomes)"
    )
    question_index: int = Field(
        default=1,
        ge=1,
        description="Which question number this is (1-based)",
    )
    total_questions: int = Field(
        default=5, ge=1, description="Total number of questions in this exam"
    )
    already_asked_topics: List[str] = Field(
        default_factory=list,
        description="Topics already covered to avoid repetition",
    )


# ─────────────────────────────────────────────
#  Interview - Generate Follow-up Question
# ─────────────────────────────────────────────


class GenerateFollowUpRequest(BaseModel):
    """
    Request to generate a follow-up / probing question based on
    the student's previous answer.
    """

    exam_session_id: str = Field(..., description="UUID of the ExamSession")
    course_name: str = Field(..., description="Name of the course")
    documents: List[DocumentContext] = Field(
        ..., description="List of .md documents for context"
    )
    original_question: str = Field(..., description="The main question that was asked")
    student_answer: str = Field(
        ..., description="Student's answer (from STT transcript)"
    )
    question_index: int = Field(default=1, ge=1, description="Current question number")


# ─────────────────────────────────────────────
#  Speech - Speech-to-Text
# ─────────────────────────────────────────────


class SpeechToTextRequest(BaseModel):
    """
    Request to transcribe audio to text.
    The audio file is sent as multipart/form-data (see API router).
    This model is used for JSON metadata accompanying the audio.
    """

    exam_session_id: str = Field(..., description="UUID of the ExamSession")
    language: str = Field(default="vi-VN", description="BCP-47 language code")


# ─────────────────────────────────────────────
#  Speech - Text-to-Speech
# ─────────────────────────────────────────────


class TextToSpeechRequest(BaseModel):
    """Request to convert AI question text into audio."""

    text: str = Field(..., description="Text to convert to speech")
    language: str = Field(default="vi-VN", description="BCP-47 language code")
    voice: Optional[str] = Field(
        default=None, description="Voice name (provider-specific)"
    )


# ─────────────────────────────────────────────
#  Evaluation - Score Answer
# ─────────────────────────────────────────────


class QuestionAnswerPair(BaseModel):
    """A single Q&A pair from the interview session."""

    question_id: str = Field(..., description="UUID of the Question")
    question_text: str = Field(..., description="The question that was asked")
    student_answer: str = Field(..., description="Student's transcribed answer")
    is_follow_up: bool = Field(default=False, description="Whether this was a follow-up")


class EvaluateAnswerRequest(BaseModel):
    """
    Request to score a single question-answer pair.
    Called after each question is answered.
    """

    exam_session_id: str = Field(..., description="UUID of the ExamSession")
    course_name: str = Field(..., description="Name of the course")
    documents: List[DocumentContext] = Field(
        ..., description="Course content + learning outcomes for scoring context"
    )
    question_answer: QuestionAnswerPair = Field(
        ..., description="The Q&A pair to evaluate"
    )
    max_score: float = Field(
        default=10.0, ge=0, description="Maximum score per question"
    )


class EvaluateSessionRequest(BaseModel):
    """
    Request to evaluate an entire exam session at once.
    Used at the end of the session to generate the full Result.
    """

    exam_session_id: str = Field(..., description="UUID of the ExamSession")
    student_id: str = Field(..., description="UUID of the Student")
    course_name: str = Field(..., description="Name of the course")
    documents: List[DocumentContext] = Field(
        ..., description="Course content + learning outcomes"
    )
    question_answers: List[QuestionAnswerPair] = Field(
        ..., description="All Q&A pairs from the session"
    )
    max_score_per_question: float = Field(
        default=10.0, ge=0, description="Max score per question"
    )
