namespace GpxTogetherTimeChecker.Models;

internal record TrackPoint(double Lat, double Lon, DateTime Time, double? Ele = null);
