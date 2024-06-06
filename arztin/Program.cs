using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using arztin.DataDomain;
using Microsoft.EntityFrameworkCore;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddDbContext<ArztinDbContext>(options => options.UseSqlServer("Server=tcp:arztin-server.database.windows.net,1433;Initial Catalog=arztin-db;Persist Security Info=False;User ID=arztin-admin;Password=WwL&2)yQf6;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"));
    })
    .Build();

host.Run();

