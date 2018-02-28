using System;
using System.Linq;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Database;
using DataTypes;
using Authentication;
using System.Collections.Generic;

namespace DataAccess.Repositories.Implementations
{
	public class IssueImageRepository : IIssueImageRepository
	{
		private IssueManagerContainer _context = new IssueManagerContainer();
    private IAuthentication _authentication;

    public IssueImageRepository(IAuthentication authentication)
		{
			_authentication = authentication;
		}

		private IssueImage GetEntity( int id, bool throwIfNotFound = true )
		{
			var issueImage = _context.IssueImages.FirstOrDefault( x => x.Id == id );
			if ( issueImage == null && throwIfNotFound )
				throw new IssueNotFoundException();

			return issueImage;
		}

		public IssueImageModel Get( int id )
		{
			var issueImage = GetEntity( id, false );
			return ModelFromEntity( issueImage );
		}

    public List<IssueImageModel> GetImagesForIssue(int issueId)
		{    

      try
      {
        var items = from issue in _context.Issues
               where issue.Id == issueId
               from image in issue.Images
               select image;
               //select ModelFromEntity(image);

        var list = items.ToList();
        var issueVMs = list.Select(ModelFromEntity).ToList();
        return issueVMs;
      }
      catch(Exception ex)
      {
        throw ex;
      }
		}

    public IssueImageModel GetByFilename(String filename)
    {

      try
      {
        var issueImage = _context.IssueImages.FirstOrDefault(x => x.FileName == filename);
        if (issueImage == null)
          throw new IssueNotFoundException();

        return ModelFromEntity(issueImage);
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

		public void Create( IssueImageModel model )
		{
      var issue = _context.Issues.FirstOrDefault(x => x.Id == model.IssueId);
      if (issue == null)
        throw new IssueNotFoundException();

      // TODO: Validate user rights


      var issueImage = new IssueImage
      {
        IssueId = model.IssueId,
        FileName = model.Image.FileName,
        Created = DateTime.UtcNow
      };

      issue.Images.Add(issueImage);
      _context.SaveChanges();

      model.Id = issueImage.Id;
		}

		//public void Update( IssueImageModel model )
		//{
		//	var issueImage = GetEntity( model.Id );

		//	// TODO: Validate user rights

		//	issueImage.FileName = model.Image.FileName;

		//	_context.SaveChanges();
		//}

		public void Delete( int id )
		{
			var issueImage = GetEntity( id );

			// TODO: Validate user rights

			_context.IssueImages.Remove( issueImage );
			_context.SaveChanges();
		}

		public static DataAccess.Models.IssueImageModel ModelFromEntity( Database.IssueImage issueImage )
		{
			return new IssueImageModel
			{
				Id = issueImage.Id,
				IssueId = issueImage.IssueId,
				Image = new IssueImageModel.ImageInfo( issueImage.FileName ),
				Created = issueImage.Created
			};
		}
	}
}
