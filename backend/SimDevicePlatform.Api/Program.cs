using Microsoft.EntityFrameworkCore;
using SimDevicePlatform.Core.Interfaces;
using SimDevicePlatform.Infrastructure.Data;
using SimDevicePlatform.Processing.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<SimDeviceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add HTTP context accessor for audit service
builder.Services.AddHttpContextAccessor();

// Add services
builder.Services.AddScoped<IDataProcessingService, DataProcessingService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SimDeviceDbContext>();
    context.Database.EnsureCreated();
}

app.Run();