using GpxChecker.Models;
using System.Globalization;
using System.Xml.Linq;

namespace GpxChecker.Services;

internal class Writer
{
    public void Execute(double distanceThresholdMeters, int minDurationSeconds, Interval interval, int idx)
    {
        var filePath = $@"f:\tmp\{minDurationSeconds}\{distanceThresholdMeters}\{idx:D2}.gpx";
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
        XNamespace ns = "http://www.topografix.com/GPX/1/1";

        var doc = new XDocument(
            new XElement(ns + "gpx",
                new XAttribute("version", "1.1"),
                new XAttribute("creator", "TogetherDetector"),
                new XElement(ns + "trk",
                    new XElement(ns + "name", $"Together {interval.Start:yyyyMMdd_HHmmss}"),
                    new XElement(ns + "trkseg",
                        interval.Points.Select(p =>
                            new XElement(ns + "trkpt",
                                new XAttribute("lat", p.Lat.ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("lon", p.Lon.ToString(CultureInfo.InvariantCulture)),
                                new XElement(ns + "time", p.Time.ToString("o")),
                                p.Ele.HasValue ? new XElement(ns + "ele", p.Ele.Value.ToString(CultureInfo.InvariantCulture)) : null
                            )
                        )
                    )
                )
            )
        );

        doc.Save(path);
    }
}
