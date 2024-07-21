using AI.Demo.Domain.Models;

namespace AI.Demo.Application
{
    public interface IAIChatBotService
    {
       Task<AIResponse> GetAnswer(UserRequest userRequest);
    }
}