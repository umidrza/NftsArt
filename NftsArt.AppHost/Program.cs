using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NftsArt.Database.Data;

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.NftsArt_ApiService>("apiservice");

builder.AddProject<Projects.NftsArt_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Build().Run();
