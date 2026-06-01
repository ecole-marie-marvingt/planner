using Planner.Api.Endpoints;
using Planner.Api.Repositories;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── Aspire service defaults (observabilité, health checks, service discovery) ──
builder.AddServiceDefaults();

// ── PostgreSQL via Aspire (NpgsqlDataSource injecté automatiquement) ──────────
builder.AddNpgsqlDataSource("plannerdb", configureDataSourceBuilder: builder =>
{
    builder.MapComposite<DateOnly>("date");
});

// ── Sérialisation JSON : camelCase pour correspondre aux types TypeScript ─────
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy =
        System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter(
            System.Text.Json.JsonNamingPolicy.CamelCase));
    options.SerializerOptions.DefaultIgnoreCondition =
        System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

// ── Repositories ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<ISlotRepository, SlotRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// ── OpenAPI / Swagger ─────────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ── CORS (pour le frontend React en dev) ──────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
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

// ── Endpoints métier ──────────────────────────────────────────────────────────
app.MapSlotsEndpoints();

app.Run();