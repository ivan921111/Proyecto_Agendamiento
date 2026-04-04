using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF;
using QuestPDF.Infrastructure;
using Scheduling.Api.Application.Services;
using Scheduling.Api.Infrastructure.Data;

// Configuración de QuestPDF (sin licencia para desarrollo)
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// 🔥 IMPORTANTE: escuchar en Docker
builder.WebHost.UseUrls("http://0.0.0.0:80");

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlServerOptionsAction: sqlOptions =>
    {
        // Habilitar reintentos de conexión. La aplicación esperará y reintentará si la BD no está lista.
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
    }));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // Especifica los orígenes del frontend
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar servicios PDF/Email
builder.Services.AddSingleton<IPdfService, PdfService>();

var useMockEmail = bool.TryParse(builder.Configuration["Email:UseMock"], out var isMock) && isMock;
if (useMockEmail)
{
    builder.Services.AddScoped<IEmailService, MockEmailService>();
}
else
{
    builder.Services.AddScoped<IEmailService, EmailService>();
}

// Configuración de JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ⚠️ Puedes comentar esto para evitar problemas en Docker
// app.UseHttpsRedirection();

app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoint de prueba adicional
app.MapGet("/", () => "API funcionando 🚀");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        )
    ).ToArray();

    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}