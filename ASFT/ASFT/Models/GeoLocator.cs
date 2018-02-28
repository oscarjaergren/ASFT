namespace ASFT.Models
{
    public class GeoLocation
    {
        public GeoLocation(double la, double lo)
        {
            Latitude = la;
            Longitude = lo;
        }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}