using GpxTogetherTimeChecker.Models;

namespace GpxTogetherTimeChecker.Services;

internal interface ITrackComparer
{
    (TimeSpan TogetherTime, TimeSpan TotalTime, double Percent) ComputeTogetherTime(
        List<TrackPoint> resA,
        List<TrackPoint> resB,
        double distanceThresholdMeters);

    List<Interval> ComputeTogetherInterval(
        List<TrackPoint> resA,
        List<TrackPoint> resB,
        double distanceThresholdMeters,
        int minDurationSeconds);
}
