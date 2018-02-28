using System.Linq;
using Database;

namespace Authentication
{
	public interface IAuthentication
	{
		bool Login( string userName, string password );
		IQueryable<Location> GetUsersLocations( string currentUser, IQueryable<Location> locations );
		void CheckUserAccess( string currentUser, Location location );
	}
}
