using GpxTogetherTimeChecker.Models;

namespace GpxTogetherTimeChecker.Extensions;

internal static class TrackExtensions
{
    public static List<TrackPoint> ResampleByTime(this List<TrackPoint> points, int resolutionSeconds = 1)
    {
        if (points.Count == 0)
        {
            return [];
        }

        var t0 = points[0].Time;
        var t1 = points[^1].Time;
        int count = (int)((t1 - t0).TotalSeconds / resolutionSeconds) + 1;

        var results = new TrackPoint?[count];

        Parallel.For(0, count, new() { MaxDegreeOfParallelism = 32 }, i =>
        {
            results[i] = InterpolatedPositionAt(points, t0.AddSeconds(i * resolutionSeconds));
        });

        return [.. results
            .Where(p => p is not null)
            .Select(p => p!)];
    }

    private static TrackPoint? InterpolatedPositionAt(List<TrackPoint> points, DateTime target)
    {
        if (target <= points[0].Time)
        {
            return new TrackPoint(points[0].Lat, points[0].Lon, target, points[0].Ele);
        }

        if (target >= points[^1].Time)
        {
            return new TrackPoint(points[^1].Lat, points[^1].Lon, target, points[^1].Ele);
        }

        var i = points.FindLastIndex(p => p.Time <= target);

        if (i < 0 || i >= points.Count - 1)
        {
            return null;
        }

        var a = points[i];
        var b = points[i + 1];
        var total = (b.Time - a.Time).TotalSeconds;

        if (total <= 0)
        {
            return new TrackPoint(a.Lat, a.Lon, target, a.Ele);
        }

        var frac = (target - a.Time).TotalSeconds / total;
        var lat = a.Lat + (b.Lat - a.Lat) * frac;
        var lon = a.Lon + (b.Lon - a.Lon) * frac;

        double? ele = null;
        if (a.Ele.HasValue && b.Ele.HasValue)
        {
            ele = a.Ele + (b.Ele - a.Ele) * frac;
        }

        return new TrackPoint(lat, lon, target, ele);
    }
}
