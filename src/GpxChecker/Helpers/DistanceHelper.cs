namespace GpxTogetherTimeChecker.Helpers
{
    internal static class DistanceHelper
    {
        private const double EARTH_RADIUS_METERS = 6371000.0;

        public static double DistanceBetweenTwoPointsInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            var toRad = Math.PI / 180.0;

            var dLat = (lat2 - lat1) * toRad;
            var dLon = (lon2 - lon1) * toRad;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * toRad) * Math.Cos(lat2 * toRad) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EARTH_RADIUS_METERS * c;
        }
    }
}