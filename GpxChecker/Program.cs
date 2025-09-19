using GpxChecker.Helpers;
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

        var trackA = reader
             .Execute(@"f:\Download\007-xxx.gpx");

        var trackB = reader
            .Execute(@"f:\Download\029-xxx.gpx");

        var resA = TrackExtensions
            .ResampleByTime(trackA, RESOLUTION_SECONDS);

        var resB = TrackExtensions
            .ResampleByTime(trackB, RESOLUTION_SECONDS);

        Console.WriteLine($"Track A: {trackA.Points.Count} pts, resampled: {resA.Count} pts");
        Console.WriteLine($"Track B: {trackB.Points.Count} pts, resampled: {resB.Count} pts");
        Console.WriteLine();

        ComputeTogetherTime(scope, resA, resB);
        ComputerTogetherIntervals(scope, resA, resB);
        Console.WriteLine();
    }

    private static void ComputeTogetherTime(IServiceScope scope, List<TrackPoint> resA, List<TrackPoint> resB)
    {
        var comparer = scope.ServiceProvider
            .GetRequiredService<Comparer>();

        foreach (var distance in DISTANCES)
        {
            var (TogetherTime, TotalTime, Percent) = comparer.ComputeTogetherTime(resA, resB, distance);
            Console.WriteLine($"Distance {distance}m: Together: {TogetherTime} out of {TotalTime} = {Percent:F1}% of ride");
        }
    }

    private static void ComputerTogetherIntervals(IServiceScope scope,
        List<TrackPoint> resA,
        List<TrackPoint> resB)
    {
        var comparer = scope.ServiceProvider
            .GetRequiredService<Comparer>();

        var writer = scope.ServiceProvider
            .GetRequiredService<Writer>();

        foreach (var keyValuePair in INTERVALS_VS_DISTANCES)
        {
            var distanceThresholdMeters = keyValuePair.Key;
            foreach (var value in keyValuePair.Value)
            {
                var index = 1;
                var minDurationSeconds = value;
                var intervals = comparer.ComputerTogetherInterval(resA, resB, distanceThresholdMeters, minDurationSeconds);

                Console.WriteLine($"Distance < {distanceThresholdMeters}m, Duration > {minDurationSeconds}s");

                foreach (var interval in intervals)
                {
                    Console.WriteLine($"{interval.Start:o} -> {interval.End:o}, " +
                        $"dur={interval.DurationSeconds}s, " +
                        $"dist={interval.DistanceMeters:F1}m," +
                        $" pts={interval.Points.Count}");

                    writer.Execute(distanceThresholdMeters, minDurationSeconds, interval, index);
                    index++;
                }

                Console.WriteLine();
            }
        }
    }
}