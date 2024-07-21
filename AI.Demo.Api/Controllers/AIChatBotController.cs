using AI.Demo.Application;
using AI.Demo.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AI.Demo.Api.Controllers
{
    [ApiController]
    [Route("api/v1/ai-chat-bot")]
    [AllowAnonymous]
    [Produces("application/json")]
    public class AIChatBotController(IAIChatBotService _chatService, ILogger<AIResponse> _logger) : ControllerBase
    {
        [HttpPost()]
        /// <summary>
        /// Answer the question using AI and RAG.
        /// </summary>
        public async Task<AIResponse> AskAI([FromBody] UserRequest userRequest)
        {
            return await _chatService.GetAnswer(userRequest);
        }
    }
}
