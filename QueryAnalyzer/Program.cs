using QueryAnalyzer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var databaseType = builder.Configuration["Database:Type"];
var connectionString = builder.Configuration["Database:ConnectionString"];

builder.Services.AddScoped<DatabaseService>(_ => new DatabaseService(databaseType, connectionString));

// for enabling CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
