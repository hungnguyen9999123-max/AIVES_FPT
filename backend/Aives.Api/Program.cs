using Microsoft.EntityFrameworkCore;
using Npgsql;
using Aives.Domain.Enums;
using Aives.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext with Neon PostgreSQL and Postgres enum mappings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<UserRole>("user_role");
dataSourceBuilder.MapEnum<ExamStatus>("exam_status");
dataSourceBuilder.MapEnum<SessionStatus>("session_status");
dataSourceBuilder.MapEnum<SubmissionStatus>("submission_status");
dataSourceBuilder.MapEnum<DocType>("doc_type");
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AivesDbContext>(options =>
    options.UseNpgsql(dataSource));

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

app.Run();
