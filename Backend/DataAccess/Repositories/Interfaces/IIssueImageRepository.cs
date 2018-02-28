using System.Linq;
using DataAccess.Models;
using System.Collections.Generic;

namespace DataAccess.Repositories.Interfaces
{
	public interface IIssueImageRepository
	{
		IssueImageModel Get( int id );
    List<IssueImageModel> GetImagesForIssue(int issueId);
    IssueImageModel GetByFilename(string filename);
		void Create( IssueImageModel model );
		//void Update( IssueImageModel model );
		void Delete( int id );
	}
}