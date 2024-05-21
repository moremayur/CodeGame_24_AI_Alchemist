using Google.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<DbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLServerConnection"));
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "AI_Alchemist_WebApi", Version = "v2" });
});

//builder.Services
//    .AddHealthChecks()
//    .AddSqlServer(builder.Configuration.GetConnectionString("SQLServerConnection"));

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

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "AI_Alchemist_WebApi");
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    // endpoints.MapHealthChecks("/health");
});

app.MapControllers();

app.Run();
