using GpxTogetherTimeChecker.Models;

namespace GpxTogetherTimeChecker.Services;

internal interface IGpxReader
{
    List<TrackPoint> Read(string path);
}
