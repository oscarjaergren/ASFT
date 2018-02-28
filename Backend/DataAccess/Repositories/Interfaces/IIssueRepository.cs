using System.Collections.Generic;
using System.Linq;
using DataAccess.Models;
using DataTypes;

namespace DataAccess.Repositories.Interfaces
{
	public interface IIssueRepository
	{
		IssueModel Get( int id, string currentUser );
    int GetLocationIdForIssue(int id); // no user check
		IEnumerable<IssueModel> GetAll( int locationId, string currentUser );
		IEnumerable<IssueModel> GetOpen( int locationId, string currentUser );
		IQueryable<IssueModel> GetIssuesAtLocation( int locationId, string currentUser );
		void Create( IssueModel model, string currentUser );
		void Update( IssueModel model, string currentUser );
		void SetStatus( int id, IssueStatus status, string currentUser );
		void Delete( int id, string currentUser );
	}
}