namespace AI.Demo.Domain.Models;

public class UserRequest
{
    public DateTime Date { get; set; }

    public string UserName { get; set; }

    public string? Message { get; set; }
}