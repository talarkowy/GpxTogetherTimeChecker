using GpxTogetherTimeChecker.Extensions;
using GpxTogetherTimeChecker.Models;
using GpxTogetherTimeChecker.Options;
using Microsoft.Extensions.Options;

namespace GpxTogetherTimeChecker.Services;

internal class GpxAnalyzer
{
    private readonly IGpxReader _reader;
    private readonly ITrackComparer _comparer;
    private readonly IGpxWriter _writer;
    private readonly Options.Options _options;

    public GpxAnalyzer(
        IGpxReader reader,
        ITrackComparer comparer,
        IGpxWriter writer,
        IOptions<Options.Options> options)
    {
        _reader = reader;
        _comparer = comparer;
        _writer = writer;
        _options = options.Value;
    }

    public void Analyze(IReadOnlyList<string> filePaths)
    {
        var tracks = filePaths
            .Take(2)
            .Select(_reader.Read)
            .Select(track => track.ResampleByTime(_options.ResolutionInSeconds))
            .ToList();

        var track1 = tracks[0];
        var track2 = tracks[1];

        PrintTogetherTime(track1, track2);
        PrintTogetherIntervals(track1, track2);
    }

    private void PrintTogetherTime(List<TrackPoint> track1, List<TrackPoint> track2)
    {
        foreach (var distance in _options.Distances)
        {
            var (togetherTime, totalTime, percent) = _comparer.ComputeTogetherTime(track1, track2, distance);

            Console.WriteLine($"Distance < {distance}m: " +
                $"Together: {togetherTime} out of {totalTime} " +
                $"= {percent:F1}% of ride");
        }
    }

    private void PrintTogetherIntervals(List<TrackPoint> track1, List<TrackPoint> track2)
    {
        foreach (var (distanceThresholdMeters, durations) in _options.IntervalsVsDistances)
        {
            foreach (var minDurationSeconds in durations)
            {
                var index = 1;
                var intervals = _comparer.ComputeTogetherInterval(
                    track1, track2, distanceThresholdMeters, minDurationSeconds);

                Console.WriteLine($"Distance < {distanceThresholdMeters}m, Duration > {minDurationSeconds}s");

                foreach (var interval in intervals)
                {
                    Console.WriteLine($"{interval.Start:o} -> {interval.End:o}, " +
                        $"dur={interval.DurationSeconds}s, " +
                        $"dist={interval.DistanceMeters:F1}m, " +
                        $"pts={interval.Points.Count}");

                    _writer.Write(distanceThresholdMeters, minDurationSeconds, interval, index);
                    index++;
                }

                Console.WriteLine();
            }
        }
    }
}
