using GpxTogetherTimeChecker.Extensions;
using GpxTogetherTimeChecker.Models;
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
            Console.WriteLine("Give file paths as arguments");
        }

        var filePaths = args.Length > 0
            ? args.ToList()
            : [@"f:\Download\file1.gpx", @"f:\Download\file2.gpx"];

        var serviceProvider = DependencyInjection
            .ConfigureServices()
            ?? throw new InvalidOperationException("Cannot configure services");

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build() 
            ?? throw new InvalidOperationException("Cannot build configuration");

        var scope = serviceProvider
            .CreateScope()
            ?? throw new InvalidOperationException("Cannot create scope");

        var reader = scope.ServiceProvider
            .GetRequiredService<Reader>()
            ?? throw new InvalidOperationException("Cannot get reader");

        var comparer = scope.ServiceProvider
            .GetRequiredService<Comparer>()
            ?? throw new InvalidOperationException("Cannot get comparer");

        var writer = scope.ServiceProvider
            .GetRequiredService<Writer>()
            ?? throw new InvalidOperationException("Cannot get writer");

        var options = configuration
            .GetSection(nameof(Options))
            .Get<Options.Options>()
            ?? throw new InvalidOperationException("Cannot read options from configuration");

        List<List<TrackPoint>> tracks = [.. filePaths
                .Take(2)
                .Select(reader.Execute)
                .Select(readedTrack =>
                    readedTrack.ResampleByTime(options.ResolutionInSeconds))];

        var track1 = tracks[0];
        var track2 = tracks[1];

        ComputeTogetherTime(comparer, options, track1, track2);
        ComputeTogetherIntervals(comparer, writer, options, track1, track2);
    }

    private static void ComputeTogetherTime(
        Comparer comparer,
        Options.Options options,
        List<TrackPoint> resA,
        List<TrackPoint> resB)
    {
        foreach (var distance in options.Distances)
        {
            var (TogetherTime, TotalTime, Percent) = comparer.
                ComputeTogetherTime(resA, resB, distance);

            Console.WriteLine($"Distance < {distance}m: " +
                $"Together: {TogetherTime} out of {TotalTime} " +
                $"= {Percent:F1}% of ride");
        }
    }

    private static void ComputeTogetherIntervals(
        Comparer comparer, 
        Writer writer,
        Options.Options options,
        List<TrackPoint> resA,
        List<TrackPoint> resB)
    {
        foreach (var keyValuePair in options.IntervalsVsDistances)
        {
            var distanceThresholdMeters = keyValuePair.Key;

            foreach (var value in keyValuePair.Value)
            {
                var index = 1;
                var minDurationSeconds = value;

                var intervals = comparer
                    .ComputeTogetherInterval(resA, resB, distanceThresholdMeters, minDurationSeconds);

                Console.WriteLine($"Distance < {distanceThresholdMeters}m, Duration > {minDurationSeconds}s");

                foreach (var interval in intervals)
                {
                    Console.WriteLine($"{interval.Start:o} -> {interval.End:o}, " +
                        $"dur={interval.DurationSeconds}s, " +
                        $"dist={interval.DistanceMeters:F1}m, " +
                        $"pts={interval.Points.Count}");

                    writer.Execute(distanceThresholdMeters, minDurationSeconds, interval, index);
                    index++;
                }

                Console.WriteLine();
            }
        }
    }
}