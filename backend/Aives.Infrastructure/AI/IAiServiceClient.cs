namespace Aives.Infrastructure.AI;

public interface IAiServiceClient
{
    void GenerateQuestion();
    void GenerateFollowUp();
    void SpeechToText();
    void TextToSpeech();
    void EvaluateAnswer();
}
