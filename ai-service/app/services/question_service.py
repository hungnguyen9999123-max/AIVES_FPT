"""
AIVES AI Service - Question Service
Handles AI question generation logic for viva interview.
"""

import json
import os
from pathlib import Path
from typing import List

from loguru import logger

from app.core.ai_client import BaseAIClient, get_ai_client
from app.models.request import GenerateQuestionRequest, DocumentContext
from app.models.response import GenerateQuestionResponse

# Load prompt template once at module level
_PROMPT_PATH = Path(__file__).parent.parent / "prompts" / "question_prompt.txt"
_QUESTION_PROMPT_TEMPLATE: str = _PROMPT_PATH.read_text(encoding="utf-8")


def _build_document_context(documents: List[DocumentContext]) -> str:
    """Format the list of .md documents into a single context string for the prompt."""
    parts = []
    for doc in documents:
        parts.append(
            f"### {doc.document_type.replace('_', ' ').title()} - {doc.title}\n\n{doc.content}"
        )
    return "\n\n---\n\n".join(parts)


class QuestionService:
    """
    Service responsible for generating interview questions using AI.
    Uses the pluggable AI client (Mock / Gemini / OpenAI).
    """

    def __init__(self, ai_client: BaseAIClient | None = None) -> None:
        self.ai_client = ai_client or get_ai_client()

    async def generate_question(
        self, request: GenerateQuestionRequest
    ) -> GenerateQuestionResponse:
        """
        Generate the next interview question based on course content.

        Args:
            request: GenerateQuestionRequest with exam context and documents.

        Returns:
            GenerateQuestionResponse with the generated question and metadata.
        """
        logger.info(
            f"[QuestionService] Generating question {request.question_index}/{request.total_questions} "
            f"for session {request.exam_session_id}"
        )

        document_context = _build_document_context(request.documents)
        already_asked = (
            ", ".join(request.already_asked_topics)
            if request.already_asked_topics
            else "Chưa có"
        )

        system_prompt = _QUESTION_PROMPT_TEMPLATE.format(
            course_name=request.course_name,
            document_context=document_context,
            question_index=request.question_index,
            total_questions=request.total_questions,
            already_asked_topics=already_asked,
        )

        user_message = (
            f"Hãy tạo câu hỏi số {request.question_index} "
            f"cho buổi vấn đáp môn {request.course_name}."
        )

        raw_response = await self.ai_client.complete(system_prompt, user_message)

        try:
            data = json.loads(raw_response)
            return GenerateQuestionResponse(
                success=True,
                question=data.get("question", ""),
                question_type=data.get("question_type", "conceptual"),
                difficulty=data.get("difficulty", "medium"),
                expected_keywords=data.get("expected_keywords", []),
                topic=data.get("topic"),
            )
        except (json.JSONDecodeError, KeyError) as exc:
            logger.error(
                f"[QuestionService] Failed to parse AI response: {raw_response!r}"
            )
            raise ValueError(f"AI returned invalid response format: {exc}") from exc
