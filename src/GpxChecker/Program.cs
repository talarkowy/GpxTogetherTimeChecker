using GpxChecker.Extensions;
using GpxChecker.Models;
using GpxChecker.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GpxChecker;

internal class Program
{
    private const int RESOLUTION_SECONDS = 1;

    private static readonly List<int> INTERVALS = [60, 180, 300, 600, 1200, 3600];
    private static readonly List<double> DISTANCES = [5.0, 10.0, 50.0, 100.0, 200.0, 500.0];

    private static readonly Dictionary<double, List<int>> INTERVALS_VS_DISTANCES = CreateDict();
    private static Dictionary<double, List<int>> CreateDict() => DISTANCES
        .ToDictionary(distance => distance, distance => INTERVALS);

    internal static void Main()
    {
        var serviceProvider = DependencyInjection
            .ConfigureServices();

        var scope = serviceProvider.CreateScope();

        var reader = scope.ServiceProvider
            .GetRequiredService<Reader>();
        
        var comparer = scope.ServiceProvider
            .GetRequiredService<Comparer>();

        var writer = scope.ServiceProvider
            .GetRequiredService<Writer>();

        var filesPath = new List<string> { @"f:\Download\007.gpx", @"f:\Download\029.gpx" };

        List<List<TrackPoint>> tracks = [.. filesPath
                .Take(2)
                .Select(reader.Execute)
                .Select(readedTrack =>
                    TrackExtensions.ResampleByTime(readedTrack, RESOLUTION_SECONDS))];

        var track1 = tracks[0];
        var track2 = tracks[1];

        ComputeTogetherTime(comparer, track1, track2);
        ComputeTogetherIntervals(comparer, writer, track1, track2);

        //Console.WriteLine($"Track A: {trackA.Points.Count} pts, resampled: {track1.Count} pts");
        //Console.WriteLine($"Track B: {trackB.Points.Count} pts, resampled: {track2.Count} pts");
        Console.WriteLine();
    }

    private static void ComputeTogetherTime(
        Comparer comparer,
        List<TrackPoint> resA,
        List<TrackPoint> resB)
    {
        foreach (var distance in DISTANCES)
        {
            var (TogetherTime, TotalTime, Percent) = comparer.
                ComputeTogetherTime(resA, resB, distance);

            Console.WriteLine($"Distance {distance}m: " +
                $"Together: {TogetherTime} out of {TotalTime} " +
                $"= {Percent:F1}% of ride");
        }
    }

    private static void ComputeTogetherIntervals(
        Comparer comparer, 
        Writer writer,
        List<TrackPoint> resA,
        List<TrackPoint> resB)
    {
        foreach (var keyValuePair in INTERVALS_VS_DISTANCES)
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