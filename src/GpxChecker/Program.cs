using GpxTogetherTimeChecker.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GpxTogetherTimeChecker;

internal class Program
{
    internal static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Give two GPX file paths as arguments.");
            return;
        }

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var serviceProvider = DependencyInjection.ConfigureServices(configuration);

        using var scope = serviceProvider.CreateScope();

        var analyzer = scope.ServiceProvider.GetRequiredService<GpxAnalyzer>();
        analyzer.Analyze(args);
    }
}