using GpxTogetherTimeChecker.Constants;
using GpxTogetherTimeChecker.Models;
using GpxTogetherTimeChecker.Options;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Xml.Linq;

namespace GpxTogetherTimeChecker.Services;

internal class GpxWriter : IGpxWriter
{
    private readonly string _outputDirectory;

    public GpxWriter(IOptions<Options.Options> options)
    {
        _outputDirectory = options.Value.OutputDirectory;
    }

    public void Write(double distanceThresholdMeters, int minDurationSeconds, Interval interval, int index)
    {
        var filePath = BuildFilePath(distanceThresholdMeters, minDurationSeconds, index);
        var directory = Path.GetDirectoryName(filePath);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        WriteGpx(interval, filePath);
    }

    private void WriteGpx(Interval interval, string path)
    {
        XNamespace ns = GpxConstants.Namespace;

        var doc = new XDocument(
            new XElement(ns + GpxConstants.Gpx,
                new XAttribute(GpxConstants.Version, GpxConstants.VersionNumber),
                new XAttribute(GpxConstants.Creator, GpxConstants.TogetherDetector),
                new XElement(ns + GpxConstants.Trk,
                    new XElement(ns + GpxConstants.Name, TrackName(interval)),
                    new XElement(ns + GpxConstants.Trkseg,
                        interval.Points.Select(p =>
                            new XElement(ns + GpxConstants.Trkpt,
                                new XAttribute(GpxConstants.Lat, p.Lat.ToString(CultureInfo.InvariantCulture)),
                                new XAttribute(GpxConstants.Lon, p.Lon.ToString(CultureInfo.InvariantCulture)),
                                new XElement(ns + GpxConstants.Time, p.Time.ToString("o")),
                                p.Ele.HasValue
                                    ? new XElement(ns + GpxConstants.Ele, p.Ele.Value.ToString(CultureInfo.InvariantCulture))
                                    : null
                            )
                        )
                    )
                )
            )
        );

        doc.Save(path);
    }

    private static string TrackName(Interval interval) =>
        $"Together {interval.Start:yyyyMMdd_HHmmss}";

    private string BuildFilePath(double distanceThresholdMeters, int minDurationSeconds, int index) =>
        Path.Combine(_outputDirectory, $"{minDurationSeconds}", $"{distanceThresholdMeters}", $"{index:D2}.gpx");
}
