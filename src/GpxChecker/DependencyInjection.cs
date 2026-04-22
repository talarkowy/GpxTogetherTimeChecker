using GpxTogetherTimeChecker.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GpxTogetherTimeChecker;

internal static class DependencyInjection
{
    public static ServiceProvider ConfigureServices()
    {
        var serviceProvider = new ServiceCollection()
            .AddScoped<Reader>()
            .AddScoped<Comparer>()
            .AddScoped<Writer>()
            .BuildServiceProvider();

        return serviceProvider;
    }
}
