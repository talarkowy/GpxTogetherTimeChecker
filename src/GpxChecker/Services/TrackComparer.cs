using GpxTogetherTimeChecker.Helpers;
using GpxTogetherTimeChecker.Models;

namespace GpxTogetherTimeChecker.Services;

internal class TrackComparer : ITrackComparer
{
    public (TimeSpan TogetherTime, TimeSpan TotalTime, double Percent) ComputeTogetherTime(
        List<TrackPoint> resA,
        List<TrackPoint> resB,
        double distanceThresholdMeters)
    {
        var comparison = PrepareComparison(resA, resB);
        if (comparison is null)
        {
            return (TimeSpan.Zero, TimeSpan.Zero, 0);
        }

        var (start, end, dictA, dictB) = comparison.Value;
        var togetherSeconds = 0;

        for (var t = start; t <= end; t = t.AddSeconds(1))
        {
            if (!dictA.TryGetValue(t, out var a) || !dictB.TryGetValue(t, out var b))
            {
                continue;
            }

            var distance = DistanceHelper.DistanceBetweenTwoPointsInMeters(a.Lat, a.Lon, b.Lat, b.Lon);
            if (distance < distanceThresholdMeters)
            {
                togetherSeconds++;
            }
        }

        var totalSeconds = (end - start).TotalSeconds + 1;
        var percent = togetherSeconds * 100.0 / totalSeconds;

        return (TimeSpan.FromSeconds(togetherSeconds), TimeSpan.FromSeconds(totalSeconds), percent);
    }

    public List<Interval> ComputeTogetherInterval(
        List<TrackPoint> resA,
        List<TrackPoint> resB,
        double distanceThresholdMeters,
        int minDurationSeconds)
    {
        var comparison = PrepareComparison(resA, resB);
        if (comparison is null)
        {
            return [];
        }

        var (start, end, dictA, dictB) = comparison.Value;

        var inInterval = false;
        var intervalStart = DateTime.MinValue;
        double distanceSum = 0;
        TrackPoint? prevMid = null;
        var points = new List<TrackPoint>();
        var intervals = new List<Interval>();

        for (var t = start; t <= end; t = t.AddSeconds(1))
        {
            if (!dictA.TryGetValue(t, out var a) || !dictB.TryGetValue(t, out var b))
            {
                continue;
            }

            var distance = DistanceHelper.DistanceBetweenTwoPointsInMeters(a.Lat, a.Lon, b.Lat, b.Lon);

            if (distance < distanceThresholdMeters)
            {
                if (!inInterval)
                {
                    inInterval = true;
                    intervalStart = t;
                    distanceSum = 0;
                    prevMid = null;
                    points = [];
                }

                var midLat = (a.Lat + b.Lat) / 2.0;
                var midLon = (a.Lon + b.Lon) / 2.0;

                if (prevMid is not null)
                {
                    distanceSum += DistanceHelper.DistanceBetweenTwoPointsInMeters(prevMid.Lat, prevMid.Lon, midLat, midLon);
                }

                prevMid = new TrackPoint(midLat, midLon, t);
                points.Add(prevMid);
            }
            else if (inInterval)
            {
                if (TryCreateInterval(intervalStart, t.AddSeconds(-1), distanceSum, points, minDurationSeconds) is { } interval)
                {
                    intervals.Add(interval);
                }

                inInterval = false;
            }
        }

        if (inInterval)
        {
            if (TryCreateInterval(intervalStart, end, distanceSum, points, minDurationSeconds) is { } interval)
            {
                intervals.Add(interval);
            }
        }

        return intervals;
    }

    private static (DateTime Start, DateTime End, Dictionary<DateTime, TrackPoint> DictA, Dictionary<DateTime, TrackPoint> DictB)?
        PrepareComparison(List<TrackPoint> resA, List<TrackPoint> resB)
    {
        if (resA.Count == 0 || resB.Count == 0)
        {
            return null;
        }

        var start = new[] { resA[0].Time, resB[0].Time }.Max();
        var end = new[] { resA[^1].Time, resB[^1].Time }.Min();

        if (start > end)
        {
            return null;
        }

        return (start, end, resA.ToDictionary(p => p.Time), resB.ToDictionary(p => p.Time));
    }

    private static Interval? TryCreateInterval(
        DateTime intervalStart,
        DateTime intervalEnd,
        double distanceSum,
        List<TrackPoint> points,
        int minDurationSeconds)
    {
        var duration = (int)(intervalEnd - intervalStart).TotalSeconds + 1;

        if (duration < minDurationSeconds)
        {
            return null;
        }

        return new Interval(intervalStart, intervalEnd, duration, distanceSum, [.. points]);
    }
}
