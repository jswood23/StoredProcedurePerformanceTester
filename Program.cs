using System.Reflection;
using Microsoft.Extensions.Configuration;
using StoredProcedurePerformanceTester;

var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
var projectRoot = Directory.GetParent(assemblyLocation).Parent.Parent.FullName;

IConfigurationRoot config = new ConfigurationBuilder()
    .SetBasePath(projectRoot)
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

TestUsageSummaryPerformance test = new(config);

using (StreamWriter writer = new StreamWriter("UsageSummaryCreditsMetrics.csv"))
    await test.TestAll(writer);
