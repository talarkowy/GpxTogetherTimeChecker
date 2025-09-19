namespace GpxChecker.Models;

internal class Track
{
    public List<TrackPoint> Points = [];
    public void Add(TrackPoint p) => Points.Add(p);
    public void Sort() => Points = [.. Points.OrderBy(p => p.Time)];
}