using Google.Api;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<DbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServerConnection"));
});

builder.Services
    .AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("SQLServerConnection"));

builder.Services.Configure<IISServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        p => p.AllowAnyOrigin().
            AllowAnyHeader().
            AllowAnyMethod()
            );
});

var app = builder.Build();

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHealthChecks("/health");
});

app.MapControllers();

app.Run();
