using Microsoft.EntityFrameworkCore;
using Quartz;
using TaskProcessingAPI.Application.Services;
using TaskProcessingAPI.Application.Services.Interfaces;
using TaskProcessingAPI.Configurations;
using TaskProcessingAPI.Infrastructure.Persistence;
using TaskProcessingAPI.Infrastructure.Schedulers.Quartz;
using TaskProcessingAPI.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Set up configurations data
builder.Services.Configure<QuartzConfig>(builder.Configuration.GetSection("QuartzConfigs"));
builder.Services.Configure<DevToolsConfig>(builder.Configuration.GetSection("DevToolsConfigs"));

// Set up dependencies
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IJobProcessing, JobProcessing>();
builder.Services.AddTransient<TaskProcessingJob>();
builder.Services.AddScoped<ITaskAssignmentService, TaskAssignmentService>();

// Set up quartz
builder.Services.AddQuartz();
builder.Services.AddQuartzHostedService(opt =>
    opt.WaitForJobsToComplete = true
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseExceptionHandler("/error");

app.MapControllers();

app.Run();
