using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using WebAPIWithAuthenticationToken.Models;
using WebAPIWithAuthenticationToken.Repositories;
using WebAPIWithAuthenticationToken.Services;

namespace WebAPIWithAuthenticationToken.Configuration
{
    public static class AppConfiguration
    {
        public static void AddAuthenticationAuthorization(WebApplicationBuilder builder)
        {
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
        }


        public static void MapRoutes(WebApplication app)
        {
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

            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };
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
        }


    }
}
