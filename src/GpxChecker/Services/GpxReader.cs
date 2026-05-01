using GpxTogetherTimeChecker.Constants;
using GpxTogetherTimeChecker.Models;
using System.Globalization;
using System.Xml.Linq;

namespace GpxTogetherTimeChecker.Services;

internal class GpxReader : IGpxReader
{
    public List<TrackPoint> Read(string path)
    {
        var ns = XNamespace.Get(GpxConstants.Namespace);
        var doc = XDocument.Load(path);
        var points = new List<TrackPoint>();

        foreach (var element in doc.Descendants(ns + GpxConstants.Trkpt))
        {
            var lat = double.Parse(element.Attribute(GpxConstants.Lat)!.Value, CultureInfo.InvariantCulture);
            var lon = double.Parse(element.Attribute(GpxConstants.Lon)!.Value, CultureInfo.InvariantCulture);

            double? ele = null;
            var eleElement = element.Element(ns + GpxConstants.Ele);
            if (eleElement is not null)
            {
                ele = double.Parse(eleElement.Value, CultureInfo.InvariantCulture);
            }

            var timeElement = element.Element(ns + GpxConstants.Time);
            if (timeElement is null)
            {
                continue;
            }

            var time = DateTime.Parse(timeElement.Value, null, DateTimeStyles.RoundtripKind);
            points.Add(new TrackPoint(lat, lon, time, ele));
        }

        points.Sort((a, b) => a.Time.CompareTo(b.Time));

        return points;
    }
}
