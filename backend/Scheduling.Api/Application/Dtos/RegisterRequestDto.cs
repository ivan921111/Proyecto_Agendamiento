namespace Scheduling.Api.Application.Dtos;

public class RegisterRequestDto
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public DateTime? Birthdate { get; set; }
}
