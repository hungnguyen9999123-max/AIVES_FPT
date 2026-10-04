using Microsoft.EntityFrameworkCore;
using Npgsql;
using BCrypt.Net;
using Aives.Domain.Enums;
using Aives.Infrastructure.Persistence;
using Aives.Application.Interfaces.Repositories;
using Aives.Application.Interfaces.Services;
using Aives.Infrastructure.Repositories;
using Aives.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext with Neon PostgreSQL and Postgres enum mappings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

// Map C# enum values → PostgreSQL enum labels (uppercase, matching DB schema)
dataSourceBuilder.MapEnum<UserRole>(     "user_role",        new NpgsqlUpperCaseTranslator());
dataSourceBuilder.MapEnum<ExamStatus>(   "exam_status",      new NpgsqlUpperCaseTranslator());
dataSourceBuilder.MapEnum<SessionStatus>("session_status",   new NpgsqlUpperCaseTranslator());
dataSourceBuilder.MapEnum<SubmissionStatus>("submission_status", new NpgsqlUpperCaseTranslator());
dataSourceBuilder.MapEnum<DocType>(      "doc_type",         new NpgsqlUpperCaseTranslator());
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AivesDbContext>(options =>
    options.UseNpgsql(dataSource));

// ------------------------------------------------------------------ //
//  Repositories (Infrastructure)                                       //
// ------------------------------------------------------------------ //
builder.Services.AddScoped<IUserRepository,             UserRepository>();
builder.Services.AddScoped<ICourseRepository,           CourseRepository>();
builder.Services.AddScoped<IMarkdownDocumentRepository, MarkdownDocumentRepository>();

// ------------------------------------------------------------------ //
//  Services (Application)                                              //
// ------------------------------------------------------------------ //
builder.Services.AddScoped<IUserService,             UserService>();
builder.Services.AddScoped<ICourseService,           CourseService>();
builder.Services.AddScoped<IMarkdownDocumentService, MarkdownDocumentService>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

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

// ------------------------------------------------------------------ //
//  Seed endpoint — Development only                                    //
//  POST /api/seed                                                       //
//  Body (optional JSON): { "password": "your-custom-password" }        //
// ------------------------------------------------------------------ //
if (app.Environment.IsDevelopment())
{
    app.MapPost("/api/seed", async (AivesDbContext db, SeedRequest? req) =>
    {
        // Default password nếu không truyền vào
        var plainPassword = req?.Password ?? "Teacher@123";

        // Hash password với BCrypt (work factor 12 — đủ mạnh cho dev)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 12);

        await DatabaseSeeder.SeedAsync(db, passwordHash);

        return Results.Ok(new
        {
            Message  = "Seed completed successfully.",
            Username = "teacher_test",
            Password = plainPassword,   // trả về để tiện test — chỉ dùng ở dev
            Courses  = new[] { "CS101 — Nhập môn Lập trình", "SE201 — Kỹ nghệ Phần mềm" }
        });
    });
}

app.Run();

// Request DTO cho seed endpoint
record SeedRequest(string? Password);
