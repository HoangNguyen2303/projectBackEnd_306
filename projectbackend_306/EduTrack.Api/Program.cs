using EduTrack.Application;
using EduTrack.Infrastructure;
using EduTrack.Infrastructure.Identity;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Controllers + Swagger ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT from POST /api/auth/login here (no 'Bearer ' prefix needed)."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// --- Data + Identity + JWT (see EduTrack.Infrastructure/DependencyInjection.cs) ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendDev");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// --- Dev-only sample accounts (Admin/Teacher/Student) so the team can test without registering manually ---
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
