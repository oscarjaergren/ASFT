
using Database;

namespace DataAccess.Models
{
	public class LocationModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string FullName { get; set; }
		public double Longitude { get; set; }
		public double Latitude { get; set; }
		public string TimeZone { get; set; }
	}
}
