using GpxChecker.Models;
using System.Globalization;
using System.Xml.Linq;

namespace GpxChecker.Services;

internal class Writer
{
    public void Execute(double distanceThresholdMeters, int minDurationSeconds, Interval interval, int index)
    {
        var filePath = FilePath(distanceThresholdMeters, minDurationSeconds, index);
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

    private const string GPX = "gpx";
    private const string VERSION = "version";
    private const string VERSION_NR = "1.1";
    private const string CREATOR = "creator";
    private const string TOGETHER_DETECTOR = "TogetherDetector";
    private const string TRK = "trk";
    private const string NAME = "name";
    private const string TRKSEG = "trkseg";
    private const string TRKPT = "trkpt";
    private const string LAT = "lat";
    private const string LON = "lon";
    private const string TIME = "time";
    private const string ELE = "ele";
    private const string O = "o";

    private void WriteGpx(Interval interval, string path)
    {
        XNamespace ns = "http://www.topografix.com/GPX/1/1";

        var doc = new XDocument(
            new XElement(ns + GPX,
                new XAttribute(VERSION, VERSION_NR),
                new XAttribute(CREATOR, TOGETHER_DETECTOR),
                new XElement(ns + TRK,
                    new XElement(ns + NAME, Content(interval)),
                    new XElement(ns + TRKSEG,
                        interval.Points.Select(p =>
                            new XElement(ns + TRKPT,
                                new XAttribute(LAT, p.Lat.ToString(CultureInfo.InvariantCulture)),
                                new XAttribute(LON, p.Lon.ToString(CultureInfo.InvariantCulture)),
                                new XElement(ns + TIME, p.Time.ToString(O)),
                                p.Ele.HasValue ? new XElement(ns + ELE, p.Ele.Value.ToString(CultureInfo.InvariantCulture)) : null
                            )
                        )
                    )
                )
            )
        );

        doc.Save(path);
    }

    private string Content(Interval interval) =>
        $"Together {interval.Start:yyyyMMdd_HHmmss}";

    private string FilePath(double distanceThresholdMeters, int minDurationSeconds, int index) =>
        $"D:\\download\\gpx\\{minDurationSeconds}\\{distanceThresholdMeters}\\{index:D2}.gpx";
}