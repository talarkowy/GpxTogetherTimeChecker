using GpxTogetherTimeChecker.Helpers;
using GpxTogetherTimeChecker.Models;

namespace GpxTogetherTimeChecker.Services;

internal class Comparer
{
    public (TimeSpan TogetherTime, TimeSpan TotalTime, double Percent) ComputeTogetherTime(
        List<TrackPoint> resA,
        List<TrackPoint> resB,
        double distanceThresholdMeters)
    {
        if (resA.Count == 0 || resB.Count == 0)
        {
            return (TimeSpan.Zero, TimeSpan.Zero, 0);
        }

        var start = Start(resA, resB);
        var end = End(resA, resB);

        if (start > end)
        {
            return (TimeSpan.Zero, TimeSpan.Zero, 0);
        }

        var dictA = Dict(resA);
        var dictB = Dict(resB);

        var togetherSeconds = 0;

        for (var t = start; t <= end; t = t.AddSeconds(1))
        {
            if (!dictA.TryGetValue(t, out var a) || !dictB.TryGetValue(t, out var b))
            {
                continue;
            }

            var distance = DistanceHelper
                .DistanceBetweenTwoPointsInMeters(a.Lat, a.Lon, b.Lat, b.Lon);

            if (distance < distanceThresholdMeters)
            {
                togetherSeconds++;
            }
        }

        var totalSeconds = (end - start).TotalSeconds + 1;
        var percents = togetherSeconds * 100.0 / totalSeconds;

        return (TimeSpan.FromSeconds(togetherSeconds),
            TimeSpan.FromSeconds(totalSeconds),
            percents);
    }

    public List<Interval> ComputeTogetherInterval(
        List<TrackPoint> resA,
        List<TrackPoint> resB,
        double distanceThresholdMeters,
        int minDurationSeconds)
    {

        if (resA.Count == 0 || resB.Count == 0)
        {
            return [];
        }

        var start = Start(resA, resB);
        var end = End(resA, resB);

        if (start > end)
        {
            return [];
        }

        var dictA = Dict(resA);
        var dictB = Dict(resB);

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
                var intervalEnd = t.AddSeconds(-1);
                var duration = (int)(intervalEnd - intervalStart).TotalSeconds + 1;

                if (duration >= minDurationSeconds)
                {
                    intervals.Add(new Interval(
                        intervalStart,
                        intervalEnd,
                        duration,
                        distanceSum,
                        [.. points]
                    ));
                }

                inInterval = false;
            }
        }

        if (inInterval)
        {
            var intervalEnd = end;
            var duration = (int)(intervalEnd - intervalStart).TotalSeconds + 1;

            if (duration >= minDurationSeconds)
            {
                intervals.Add(new Interval(
                    intervalStart,
                    intervalEnd,
                    duration,
                    distanceSum,
                    [.. points]
                ));
            }
        }

        return intervals;
    }

    private static DateTime Start(
        List<TrackPoint> resA,
        List<TrackPoint> resB) =>
            new[] { resA.First().Time, resB.First().Time }.Max();

    private static DateTime End(
        List<TrackPoint> resA,
        List<TrackPoint> resB) =>
            new[] { resA.Last().Time, resB.Last().Time }.Min();

    private static Dictionary<DateTime, TrackPoint> Dict(
        List<TrackPoint> points) =>
            points.ToDictionary(p => p.Time, p => p);
}