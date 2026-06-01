using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// ── PostgreSQL ────────────────────────────────────────────────────────────────

var databaseName = "plannerdb";
var plannerDb = builder.AddPostgres("postgres")
    .WithDataVolume("planner-postgres-data")
    .WithPgAdmin()
    .WithInitFiles("../../db/init")
    .WithEnvironment("POSTGRES_DB", databaseName)
    .AddDatabase(databaseName);

// ── API ───────────────────────────────────────────────────────────────────────
var api = builder.AddProject<Projects.Planner_Api>("planner-api")
    .WithReference(plannerDb)
    .WaitFor(plannerDb);

builder.AddViteApp("planner-front", "../../frontend")
    .WithArgs("--mode", "development")
    .WithEnvironment("VITE_API_BASE_URL", api.GetEndpoint("https"))
    .WithYarn()
    .PublishAsStaticWebsite(apiPath: "/api", apiTarget: api)
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
