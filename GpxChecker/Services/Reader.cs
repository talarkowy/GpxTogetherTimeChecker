using GpxChecker.Models;
using System.Globalization;
using System.Xml.Linq;

namespace GpxChecker.Services;

internal class Reader
{
    private const string GPX_NAMESPACE = "http://www.topografix.com/GPX/1/1";

    public Track Execute(string path)
    {
        var ns = XNamespace.Get(GPX_NAMESPACE);
        var doc = XDocument.Load(path);
        var track = new Track();

        var trkpts = doc.Descendants(ns + "trkpt");

        foreach (var pt in trkpts)
        {
            if (pt is not null)
            {
                var lat = double.Parse(pt!.Attribute("lat")?.Value!, CultureInfo.InvariantCulture);
                var lon = double.Parse(pt!.Attribute("lon")?.Value!, CultureInfo.InvariantCulture);
                double? ele = null;
                var eleEl = pt.Element(ns + "ele");

                if (eleEl is not null)
                {
                    ele = double.Parse(eleEl.Value, CultureInfo.InvariantCulture);
                }

                var timeEl = pt.Element(ns + "time");
                if (timeEl is null)
                {
                    continue;
                }

                var time = DateTime.Parse(timeEl.Value, null, DateTimeStyles.RoundtripKind);
                track.Add(new TrackPoint(lat, lon, time, ele));
            }
        }

        track.Sort();

        return track;
    }
}