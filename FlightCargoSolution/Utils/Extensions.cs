namespace FlightCargoSolution.Utils
{
    public static class Extensions
    {
        //get distance using the Haversine formula
        public static double GetDistance(double[] city1, double[] city2)
        {
            var lat1 = city1[0];
            var lon1 = city1[1];
            var lat2 = city2[0];
            var lon2 = city2[1];

            var R = 3959.87433; // In miles
            var dLat = ToRadian(lat2 - lat1);
            var dLon = ToRadian(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(ToRadian(lat1)) * Math.Cos(ToRadian(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));

            var distance = R * c;

            return distance;
        }
        //cal radian
        public static double ToRadian(double deg)
        {
            return deg * (Math.PI / 180);
        }
    }
}
