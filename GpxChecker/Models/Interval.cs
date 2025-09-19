namespace GpxChecker.Models;

internal record Interval(DateTime Start, DateTime End, int DurationSeconds, double DistanceMeters, List<TrackPoint> Points);
