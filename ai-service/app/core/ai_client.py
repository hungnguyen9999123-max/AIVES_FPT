"""
AIVES AI Service - AI Client
Abstract base + concrete implementations for each AI provider.
Swap providers by changing AI_PROVIDER env var.
"""

from __future__ import annotations

import abc
import json
from typing import Optional

import httpx
from loguru import logger

from app.core.config import get_settings

settings = get_settings()


# ─────────────────────────────────────────────
#  Abstract Base Client
# ─────────────────────────────────────────────


class BaseAIClient(abc.ABC):
    """Abstract AI client. All providers must implement `complete()`."""

    @abc.abstractmethod
    async def complete(self, system_prompt: str, user_message: str) -> str:
        """
        Send a prompt to the LLM and return the text response.

        Args:
            system_prompt: The system/context instructions for the AI.
            user_message: The user's input message.

        Returns:
            The AI's text response.
        """
        ...


# ─────────────────────────────────────────────
#  Mock Client (for development/testing)
# ─────────────────────────────────────────────


class MockAIClient(BaseAIClient):
    """
    Mock AI client that returns realistic placeholder responses.
    Use in development without needing real API keys.
    """

    async def complete(self, system_prompt: str, user_message: str) -> str:
        logger.debug("[MockAIClient] Returning mock response.")

        # Detect intent from system_prompt keywords to return relevant mocks
        sp_lower = system_prompt.lower()

        if "câu hỏi" in sp_lower or "question" in sp_lower or "hỏi" in sp_lower:
            return json.dumps(
                {
                    "question": "Bạn có thể giải thích khái niệm đã học trong môn này và cho ví dụ thực tế không?",
                    "question_type": "conceptual",
                    "difficulty": "medium",
                    "expected_keywords": ["khái niệm", "ví dụ", "ứng dụng"],
                },
                ensure_ascii=False,
            )

        if "follow" in sp_lower or "đào sâu" in sp_lower or "phụ" in sp_lower:
            return json.dumps(
                {
                    "follow_up_question": "Bạn có thể giải thích chi tiết hơn về ví dụ vừa đề cập không?",
                    "reason": "Student cần làm rõ thêm về ví dụ thực tế.",
                },
                ensure_ascii=False,
            )

        if "chấm điểm" in sp_lower or "scoring" in sp_lower or "đánh giá" in sp_lower or "evaluat" in sp_lower:
            return json.dumps(
                {
                    "score": 7.5,
                    "max_score": 10.0,
                    "feedback": "Câu trả lời đã nêu đúng khái niệm cơ bản, tuy nhiên cần bổ sung thêm ví dụ thực tế.",
                    "strengths": ["Nắm đúng khái niệm", "Trình bày rõ ràng"],
                    "weaknesses": ["Thiếu ví dụ minh hoạ", "Chưa liên hệ thực tế"],
                    "missing_content": ["Ví dụ ứng dụng thực tế", "Liên hệ với learning outcomes"],
                },
                ensure_ascii=False,
            )

        # Default fallback
        return json.dumps(
            {"response": "Mock AI response. Hãy cấu hình AI_PROVIDER thực tế."},
            ensure_ascii=False,
        )


# ─────────────────────────────────────────────
#  Gemini Client
# ─────────────────────────────────────────────


class GeminiAIClient(BaseAIClient):
    """
    Google Gemini API client.
    Set AI_PROVIDER=gemini and GEMINI_API_KEY in .env to use.
    """

    BASE_URL = "https://generativelanguage.googleapis.com/v1beta/models"

    def __init__(self) -> None:
        self.api_key = settings.gemini_api_key
        self.model = settings.gemini_model
        if not self.api_key:
            raise ValueError("GEMINI_API_KEY is not set in environment variables.")

    async def complete(self, system_prompt: str, user_message: str) -> str:
        url = f"{self.BASE_URL}/{self.model}:generateContent?key={self.api_key}"
        payload = {
            "system_instruction": {"parts": [{"text": system_prompt}]},
            "contents": [{"role": "user", "parts": [{"text": user_message}]}],
            "generationConfig": {
                "temperature": 0.7,
                "maxOutputTokens": 2048,
                "responseMimeType": "application/json",
            },
        }

        async with httpx.AsyncClient(timeout=30.0) as client:
            response = await client.post(url, json=payload)
            response.raise_for_status()
            data = response.json()

        try:
            return data["candidates"][0]["content"]["parts"][0]["text"]
        except (KeyError, IndexError) as exc:
            logger.error(f"[GeminiAIClient] Unexpected response structure: {data}")
            raise ValueError("Failed to parse Gemini response.") from exc


# ─────────────────────────────────────────────
#  OpenAI Client
# ─────────────────────────────────────────────


class OpenAIClient(BaseAIClient):
    """
    OpenAI Chat Completions API client.
    Set AI_PROVIDER=openai and OPENAI_API_KEY in .env to use.
    """

    BASE_URL = "https://api.openai.com/v1/chat/completions"

    def __init__(self) -> None:
        self.api_key = settings.openai_api_key
        self.model = settings.openai_model
        if not self.api_key:
            raise ValueError("OPENAI_API_KEY is not set in environment variables.")

    async def complete(self, system_prompt: str, user_message: str) -> str:
        headers = {
            "Authorization": f"Bearer {self.api_key}",
            "Content-Type": "application/json",
        }
        payload = {
            "model": self.model,
            "messages": [
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": user_message},
            ],
            "temperature": 0.7,
            "max_tokens": 2048,
            "response_format": {"type": "json_object"},
        }

        async with httpx.AsyncClient(timeout=30.0) as client:
            response = await client.post(self.BASE_URL, headers=headers, json=payload)
            response.raise_for_status()
            data = response.json()

        try:
            return data["choices"][0]["message"]["content"]
        except (KeyError, IndexError) as exc:
            logger.error(f"[OpenAIClient] Unexpected response structure: {data}")
            raise ValueError("Failed to parse OpenAI response.") from exc


# ─────────────────────────────────────────────
#  Client Factory
# ─────────────────────────────────────────────


def get_ai_client() -> BaseAIClient:
    """
    Factory function. Returns the AI client based on AI_PROVIDER env var.
    Supported values: 'mock', 'gemini', 'openai'
    """
    provider = settings.ai_provider.lower()
    logger.info(f"[AIClient] Initialising provider: {provider}")

    if provider == "gemini":
        return GeminiAIClient()
    elif provider == "openai":
        return OpenAIClient()
    else:
        if provider != "mock":
            logger.warning(
                f"[AIClient] Unknown provider '{provider}'. Falling back to Mock."
            )
        return MockAIClient()
