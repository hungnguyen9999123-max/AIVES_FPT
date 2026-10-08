using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Aives.Domain.Enums;
using Aives.Infrastructure.Persistence;
using Aives.Application.Auth.Interfaces;
using Aives.Application.Auth.Services;
using Aives.Infrastructure.Auth;
using Aives.Application.Exams.Interfaces;
using Aives.Application.Exams.Services;
using Aives.Infrastructure.Repositories;
using Aives.Infrastructure.AI;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────
// 1. PostgreSQL / EF Core
// ──────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
var nullTranslator = new VerbatimNameTranslator();
dataSourceBuilder.MapEnum<UserRole>("user_role", nameTranslator: nullTranslator);
dataSourceBuilder.MapEnum<ExamStatus>("exam_status", nameTranslator: nullTranslator);
dataSourceBuilder.MapEnum<SessionStatus>("session_status", nameTranslator: nullTranslator);
dataSourceBuilder.MapEnum<SubmissionStatus>("submission_status", nameTranslator: nullTranslator);
dataSourceBuilder.MapEnum<DocType>("doc_type", nameTranslator: nullTranslator);
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AivesDbContext>(options =>
    options.UseNpgsql(dataSource));

// ──────────────────────────────────────────────
// 2. JWT Authentication
// ──────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key configuration is required.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AIVES";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AIVES.Client";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// ──────────────────────────────────────────────
// 3. Dependency Injection – Auth
// ──────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ──────────────────────────────────────────────
// 3b. Dependency Injection – Exam & ExamSession
// ──────────────────────────────────────────────
builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<IExamSessionRepository, ExamSessionRepository>();
builder.Services.AddScoped<IQuestionBankRepository, QuestionBankRepository>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IExamSessionService, ExamSessionService>();

// ──────────────────────────────────────────────
// 3c. Dependency Injection – Documents, Results, AI
// ──────────────────────────────────────────────
builder.Services.AddScoped<IMarkdownDocumentRepository, MarkdownDocumentRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IResultRepository, ResultRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<IAIQuestionService, AIQuestionService>();

// HttpClient cho AI Service (FastAPI)
builder.Services.AddHttpClient<IAIQuestionService, AIQuestionService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["AI:BaseUrl"] ?? "http://localhost:8000");
    client.Timeout = TimeSpan.FromMinutes(5);
});

// ──────────────────────────────────────────────
// 4. Controllers + Swagger with JWT
// ──────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "AIVES API", Version = "v1" });

    // Enable "Authorize" button in Swagger UI
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter: Bearer {your JWT token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

// ──────────────────────────────────────────────
// 5. Middleware pipeline
// ──────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint (preserved from original)
app.MapGet("/api/health/db", async (AivesDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    var userCount = await db.Users.CountAsync();
    var courseCount = await db.Courses.CountAsync();
    return Results.Ok(new
    {
        Status = "Healthy",
        DatabaseConnected = canConnect,
        UserCount = userCount,
        CourseCount = courseCount,
        ServerTime = DateTime.UtcNow
    });
});

app.Run();

/// <summary>
/// Name translator that keeps C# enum member names verbatim (no snake_case conversion).
/// Required because the PostgreSQL user_role enum stores uppercase values: ADMIN, TEACHER, STUDENT.
/// </summary>
class VerbatimNameTranslator : Npgsql.INpgsqlNameTranslator
{
    public string TranslateMemberName(string clrName) => clrName;
    public string TranslateTypeName(string clrName) => clrName;
}
