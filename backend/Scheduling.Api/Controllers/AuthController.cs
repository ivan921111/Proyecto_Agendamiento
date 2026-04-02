using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scheduling.Api.Application.Dtos;
using Scheduling.Api.Domain;
using Scheduling.Api.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Scheduling.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public class ActualizarPerfilDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime? Birthdate { get; set; }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequest)
    {
        var user = new User
        {
            Username = registerRequest.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
            Email = registerRequest.Email,
            Birthdate = registerRequest.Birthdate
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

        if (user == null)
        {
            return Unauthorized("Credenciales Invalidas o Usuario no registrado");
        }

        bool validPassword;
        try
        {
            validPassword = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return Unauthorized("Credenciales Invalidas o Usuario no registrado");
        }

        if (!validPassword)
        {
            return Unauthorized("Credenciales Invalidas o Usuario no registrado");
        }

        var token = GenerateJwtToken(user);

        return Ok(new { Token = token });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> ObtenerMiPerfil()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim))
            return Unauthorized("Token inválido o expirado");

        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized("Token inválido");

        var usuario = await _context.Users.FindAsync(userId);
        if (usuario == null)
            return NotFound("Usuario no encontrado");

        return Ok(new
        {
            usuario.Id,
            usuario.Username,
            usuario.Email,
            usuario.Birthdate
        });
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> ActualizarMiPerfil([FromBody] ActualizarPerfilDto datos)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim))
            return Unauthorized("Token inválido o expirado");

        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized("Token inválido");

        var usuario = await _context.Users.FindAsync(userId);
        if (usuario == null)
            return NotFound("Usuario no encontrado");

        if (!string.IsNullOrWhiteSpace(datos.Email))
            usuario.Email = datos.Email;

        if (datos.Birthdate.HasValue)
            usuario.Birthdate = datos.Birthdate;

        if (!string.IsNullOrWhiteSpace(datos.Password))
            usuario.Password = BCrypt.Net.BCrypt.HashPassword(datos.Password);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            usuario.Id,
            usuario.Username,
            usuario.Email,
            usuario.Birthdate
        });
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
