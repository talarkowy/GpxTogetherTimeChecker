namespace GpxTogetherTimeChecker.Options;

internal class Options
{
    public int ResolutionInSeconds { get; set; }
    public List<int> Intervals { get; set; } = [];
    public List<double> Distances { get; set; } = [];
    public Dictionary<double, List<int>> IntervalsVsDistances 
        => Distances
        .ToDictionary(_ => _, _ => Intervals);
}
