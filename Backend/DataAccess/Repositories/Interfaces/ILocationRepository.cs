using System.Linq;
using DataAccess.Models;

namespace DataAccess.Repositories.Interfaces
{
	public interface ILocationRepository
	{
		LocationModel Get( int id );
		LocationModel Get( string name );
		IQueryable<LocationModel> GetAll();
		IQueryable<LocationModel> GetAllFromUser( string userName );
		void Create( LocationModel model );
		void Update( LocationModel model );
		void Delete(int id);
	}
}