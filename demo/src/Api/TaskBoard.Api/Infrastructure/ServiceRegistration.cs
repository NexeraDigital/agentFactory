using Azure.Data.Tables;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Api.Accessors;
using TaskBoard.Api.Contracts;
using TaskBoard.Api.Engines;
using TaskBoard.Api.Entities;
using TaskBoard.Api.Managers;

namespace TaskBoard.Api.Infrastructure;

/// <summary>
/// VIOLATION FILE
///
/// VIOLATES:
///   UNI-004 — Constructor with 6 parameters (exceeds limit of 4).
///             Indicates this class has too many responsibilities.
/// </summary>
public class ServiceRegistration
{
    // VIOLATION UNI-004: 6 constructor parameters — too many responsibilities
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILoggerFactory _loggerFactory;
    private readonly string _connectionString;
    private readonly string _storageConnectionString;
    private readonly string _applicationName;

    public ServiceRegistration(
        IConfiguration configuration,            // 1
        IHostEnvironment environment,            // 2
        ILoggerFactory loggerFactory,            // 3
        string connectionString,                 // 4
        string storageConnectionString,          // 5
        string applicationName)                  // 6 — VIOLATION UNI-004
    {
        _configuration = configuration;
        _environment = environment;
        _loggerFactory = loggerFactory;
        _connectionString = connectionString;
        _storageConnectionString = storageConnectionString;
        _applicationName = applicationName;
    }

    public static void RegisterServices(IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("SqlServer connection string not configured");

        var storageConnection = config.GetConnectionString("TableStorage")
            ?? throw new InvalidOperationException("TableStorage connection string not configured");

        services.AddDbContext<TaskDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddSingleton(new TableClient(storageConnection, "AuditLogs"));

        services.AddScoped<ITaskManager, TaskManager>();
        services.AddScoped<IDashboardManager, DashboardManager>();
        services.AddScoped<IPriorityEngine, PriorityEngine>();
        services.AddScoped<IDeadlineEngine, DeadlineEngine>();
        services.AddScoped<ITaskAccessor, TaskAccessor>();
        services.AddScoped<IAuditLogAccessor, AuditLogAccessor>();
    }
}
