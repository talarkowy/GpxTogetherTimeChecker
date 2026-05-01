using GpxTogetherTimeChecker.Options;
using GpxTogetherTimeChecker.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GpxTogetherTimeChecker;

internal static class DependencyInjection
{
    public static ServiceProvider ConfigureServices(IConfiguration configuration)
    {
        return new ServiceCollection()
            .Configure<Options.Options>(opts => configuration.GetSection(nameof(Options.Options)).Bind(opts))
            .AddScoped<IGpxReader, GpxReader>()
            .AddScoped<ITrackComparer, TrackComparer>()
            .AddScoped<IGpxWriter, GpxWriter>()
            .AddScoped<GpxAnalyzer>()
            .BuildServiceProvider();
    }
}
