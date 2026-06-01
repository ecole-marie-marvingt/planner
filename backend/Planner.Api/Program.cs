using Planner.Api.Repositories;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Aspire service defaults (observabilité, health checks, service discovery) ──
builder.AddServiceDefaults();

// ── PostgreSQL via Aspire (NpgsqlDataSource injecté automatiquement) ──────────
builder.AddNpgsqlDataSource("plannerdb");
Dapper.SqlMapper.AddTypeMap(typeof(DateOnly), System.Data.DbType.Date, true);
Dapper.SqlMapper.AddTypeMap(typeof(TimeOnly), System.Data.DbType.Time, true);

// ── Sérialisation JSON : camelCase pour correspondre aux types TypeScript ─────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter(
                System.Text.Json.JsonNamingPolicy.CamelCase));
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<ISlotRepository, SlotRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// ── OpenAPI / Swagger ─────────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
                "https://ecole-marie-marvingt.github.io",
                builder.Configuration["Frontend:Url"] ?? "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ── Pipeline ──────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors();
app.UseHttpsRedirection();

// ── Aspire : health checks & service discovery ────────────────────────────────
app.MapDefaultEndpoints();

// ── Controllers ──────────────────────────────────────────────────────────────
app.MapControllers();

app.Run();