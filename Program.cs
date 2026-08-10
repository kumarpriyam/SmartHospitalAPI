using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// ===============================
// SERVICES
// ===============================

builder.Services.AddOpenApi();

builder.Services.AddControllers();


// ===============================
// CORS
// ===============================

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// ===============================
// POSTGRESQL CONNECTION
// ===============================

builder.Services.AddScoped(sp =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("HospitalDatabase");

    return new NpgsqlConnection(connectionString);
});


var app = builder.Build();


// ===============================
// MIDDLEWARE
// ===============================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


// ===============================
// CORS
// ===============================

app.UseCors("FrontendPolicy");


// ===============================
// CONTROLLERS
// ===============================

app.MapControllers();


// ===============================
// DATABASE TEST
// ===============================

app.MapGet("/api/database-test", async (NpgsqlConnection connection) =>
{
    try
    {
        await connection.OpenAsync();

        return Results.Ok(new
        {
            success = true,
            message = "PostgreSQL database connected successfully!"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            title: "Database connection failed"
        );
    }
});


// ===============================
// WEATHER FORECAST
// ===============================

app.MapGet("/weatherforecast", () =>
{
    var summaries = new[]
    {
        "Freezing",
        "Bracing",
        "Chilly",
        "Cool",
        "Mild",
        "Warm",
        "Balmy",
        "Hot",
        "Sweltering",
        "Scorching"
    };

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(
                DateTime.Now.AddDays(index)
            ),

            Random.Shared.Next(-20, 55),

            summaries[
                Random.Shared.Next(
                    summaries.Length
                )
            ]
        ))
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast");


app.Run();


// ===============================
// WEATHER MODEL
// ===============================

record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string? Summary)
{
    public int TemperatureF =>
        32 + (int)(TemperatureC / 0.5556);
}

