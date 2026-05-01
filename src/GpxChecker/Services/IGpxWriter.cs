using GpxTogetherTimeChecker.Models;

namespace GpxTogetherTimeChecker.Services;

internal interface IGpxWriter
{
    void Write(double distanceThresholdMeters, int minDurationSeconds, Interval interval, int index);
}
