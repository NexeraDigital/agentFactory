using TaskBoard.Api.Entities;
using TaskBoard.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

ServiceRegistration.RegisterServices(builder.Services, builder.Configuration);

var app = builder.Build();

// VIOLATION SQL-002: Database.Migrate() at startup — race condition in multi-instance
TaskDbContext.InitializeDatabase(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<AuthMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();
