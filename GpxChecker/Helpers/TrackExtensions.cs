using GpxChecker.Models;

namespace GpxChecker.Helpers
{
    internal static class TrackExtensions
    {
        public static List<TrackPoint> ResampleByTime(this Track track, int resolutionSeconds = 1)
        {
            if (track.Points.Count == 0)
            {
                return [];
            }

            var t0 = track.Points.First().Time;
            var t1 = track.Points.Last().Time;
            int count = (int)((t1 - t0).TotalSeconds / resolutionSeconds) + 1;

            var results = new TrackPoint?[count];

            Parallel.For(0, count, new() { MaxDegreeOfParallelism = 16 }, i =>
            {
                results[i] = InterpolatedPositionAt(track.Points, t0.AddSeconds(i * resolutionSeconds));
            });

            return [.. results
                .Where(_ => _ is not null)
                .Select(_ => _!)];
        }

        private static TrackPoint? InterpolatedPositionAt(List<TrackPoint> points, DateTime target)
        {
            if (target <= points.First().Time)
            {
                return new TrackPoint(points.First().Lat, points.First().Lon, target, points.First().Ele);
            }

            if (target >= points.Last().Time)
            {
                return new TrackPoint(points.Last().Lat, points.Last().Lon, target, points.Last().Ele);
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
}
