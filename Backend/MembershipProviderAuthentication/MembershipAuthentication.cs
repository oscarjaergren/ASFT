using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using Authentication;
using Database;
using DataTypes;

namespace MembershipProviderAuthentication
{
	public class MembershipAuthentication : IAuthentication
	{
		public bool Login( string userName, string password )
		{
			return Membership.ValidateUser( userName, password );
		}

		public IQueryable<Location> GetUsersLocations( string currentUser, IQueryable<Location> locations )
		{
			var airportName = GetAirportNameFromUser( currentUser );

			return locations.Where( location => string.Compare( airportName, location.Name, true ) == 0 );
		}

		public void CheckUserAccess( string currentUser, Location location )
		{
			var airportName = GetAirportNameFromUser( currentUser );
      if (airportName == null || string.Compare(airportName, location.Name, true) != 0)
				throw new UnauthorizedAccessException();
		}

		private string GetAirportNameFromUser( string currentUser )
		{
			var user = Membership.GetUser( currentUser );
			if ( user == null )
				throw new UserNotFoundException();

			var airportName = Path.GetFileNameWithoutExtension( user.Comment );
			return airportName;
		}
	}
}
