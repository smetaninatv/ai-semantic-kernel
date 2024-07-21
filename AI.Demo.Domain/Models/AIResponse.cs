namespace AI.Demo.Domain.Models;

public class AIResponse
{
    public DateTime? Date { get; set; }

    public UserRequest? UserRequest { get; set; }

    public string? Message { get; set; }
}