namespace GpxTogetherTimeChecker.Options;

internal class Options
{
    public int ResolutionInSeconds { get; set; }
    public List<int> Intervals { get; set; } = [];
    public List<double> Distances { get; set; } = [];
    public string OutputDirectory { get; set; } = string.Empty;

    private Dictionary<double, List<int>>? _intervalsVsDistances;

    public Dictionary<double, List<int>> IntervalsVsDistances =>
        _intervalsVsDistances ??= Distances.ToDictionary(d => d, _ => new List<int>(Intervals));
}
