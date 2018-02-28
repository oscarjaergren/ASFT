using System.Linq;
using Authentication;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Database;
using DataTypes;

namespace DataAccess.Repositories.Implementations
{
	public class LocationRepository : ILocationRepository
	{
		private IssueManagerContainer _context = new IssueManagerContainer();
		private IAuthentication _authentication;

		public LocationRepository( IAuthentication authentication )
		{
			_authentication = authentication;
		}

		public LocationModel Get( int id )
		{
			var location = _context.Locations.FirstOrDefault( x => x.Id == id );
			return ModelFromEntity( location );
		}

		public LocationModel Get( string name )
		{
			var location = _context.Locations.FirstOrDefault( x => string.Compare( x.Name, name ) == 0 );
			return ModelFromEntity( location );
		}

		public IQueryable<LocationModel> GetAll()
		{
			return ToModel( _context.Locations );
		}

		public IQueryable<LocationModel> GetAllFromUser( string userName )
		{
			var locations = _authentication.GetUsersLocations( userName, _context.Locations );
			return ToModel( locations );
		}

		private IQueryable<LocationModel> ToModel( IQueryable<Location> locations )
		{
			return from location in locations
					select new LocationModel
					{
						Id = location.Id,
						Name = location.Name,
						FullName = location.FullName,
						Longitude = location.Longitude,
						Latitude = location.Latitude,
						TimeZone = location.TimeZone
					};
		}

		public void Create( LocationModel model )
		{
			if ( _context.Locations.Any( x => string.Compare( x.Name, model.Name ) == 0 ) )
				throw new NotUniqueException();

			var location = new Location
			{
				Name = model.Name,
				FullName = model.FullName,
				Longitude = model.Longitude,
				Latitude = model.Latitude,
				TimeZone = model.TimeZone
			};

			_context.Locations.Add( location );
			_context.SaveChanges();

			model.Id = location.Id;
		}

		public void Update( LocationModel model )
		{
			var location = _context.Locations.FirstOrDefault( x => x.Id == model.Id );
			if ( location == null )
				throw new LocationNotFoundException();

			if ( string.CompareOrdinal( location.Name, model.Name ) != 0 && _context.Locations.Any( x => string.Compare( x.Name, model.Name ) == 0 ) )
				throw new NotUniqueException();
			
			location.Name = model.Name;
			location.FullName = model.FullName;
			location.Longitude = model.Longitude;
			location.Latitude = model.Latitude;
			location.TimeZone = model.TimeZone;

			_context.SaveChanges();
		}

		public void Delete(int id)
		{
			var location = _context.Locations.FirstOrDefault( x => x.Id == id );
			if ( location == null )
				throw new LocationNotFoundException();
			
			_context.Locations.Remove( location );
			_context.SaveChanges();
		}

		private LocationModel ModelFromEntity( Location location )
		{
			if ( location == null )
				return null;

			return new LocationModel
			{
				Id = location.Id,
				Name = location.Name,
				FullName = location.FullName,
				Longitude = location.Longitude,
				Latitude = location.Latitude,
				TimeZone = location.TimeZone
			};
		}
	}
}
