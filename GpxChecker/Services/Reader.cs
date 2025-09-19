using GpxChecker.Models;
using System.Globalization;
using System.Xml.Linq;

namespace GpxChecker.Services;

internal class Reader
{
    private const string GPX_NAMESPACE = "http://www.topografix.com/GPX/1/1";
    private const string TRKPT = "trkpt";
    private const string LAT = "lat";
    private const string LON = "lon";
    private const string ELE = "ele";
    private const string TIME = "time";

    public Track Execute(string path)
    {
        var ns = XNamespace.Get(GPX_NAMESPACE);
        var doc = XDocument.Load(path);
        var track = new Track();

        var elements = doc.Descendants(ns + TRKPT);

        foreach (var element in elements.Where(_ => _ is not null))
        {
            var lat = double.Parse(element!.Attribute(LAT)?.Value!, CultureInfo.InvariantCulture);
            var lon = double.Parse(element!.Attribute(LON)?.Value!, CultureInfo.InvariantCulture);

            double? ele = null;
            var eleElement = element.Element(ns + ELE);

            if (eleElement is not null)
            {
                ele = double.Parse(eleElement.Value, CultureInfo.InvariantCulture);
            }

            var eleTime = element.Element(ns + TIME);
            if (eleTime is null)
            {
                continue;
            }

            var time = DateTime.Parse(eleTime.Value, null, DateTimeStyles.RoundtripKind);
            track.Add(new TrackPoint(lat, lon, time, ele));            
        }

        track.Sort();

        return track;
    }
}