using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// ── PostgreSQL ────────────────────────────────────────────────────────────────
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("planner-postgres-data")
    .WithPgAdmin()
    .WithInitFiles("../../db/init");  // scripts SQL d'init

var plannerDb = postgres.AddDatabase("plannerdb");

// ── API ───────────────────────────────────────────────────────────────────────
var api = builder.AddProject<Projects.Planner_Api>("planner-api")
    .WithReference(plannerDb)
    .WaitFor(plannerDb);

builder.AddViteApp("planner-front", "../../frontend")
    .WithYarn()
    .WithHttpEndpoint(port: 3000)
    .PublishAsStaticWebsite(apiPath: "/api", apiTarget: api)
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
