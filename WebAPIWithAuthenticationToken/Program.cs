using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using WebAPIWithAuthenticationToken;
using WebAPIWithAuthenticationToken.Models;
using WebAPIWithAuthenticationToken.Repositories;
using WebAPIWithAuthenticationToken.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var key = Encoding.ASCII.GetBytes(Settings.ApplicationID);

builder.Services
    .AddAuthentication(x =>
     {
         x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
         x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
     })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization(x =>
{
    x.AddPolicy("Admin", p => p.RequireRole("manager"));
    x.AddPolicy("Employee", p => p.RequireRole("employee"));
});



builder.Services.AddCors();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// ativando cors
//app.UseCors(options =>
//{
//    options.AllowAnyOrigin();
//    options.AllowAnyHeader();
//    options.AllowAnyMethod();
//});


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapPost("/login", (User model) =>
{
    var user = UserRepository.Get(model.UserName, model.Password);
    if (user == null)
        return Results.Unauthorized();

    var token = TokenService.GenereteToken(user);   
    user.Password = string.Empty;
    return Results.Ok(new { user.UserName, user.Role, token });
}).AllowAnonymous();

app.MapGet("/anonymous", () => { return Results.Ok(new { message = "Método anônimo permitido" }); }).AllowAnonymous();


app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return Results.Ok(forecast);
})
.WithName("GetWeatherForecast").RequireAuthorization();


app.MapGet("/authenticated", (ClaimsPrincipal user) =>
{
    return Results.Ok(new { message = $"Autenticado como {user.Identity.Name} ({user.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Role)).Value})" });
}).RequireAuthorization("Admin");

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}