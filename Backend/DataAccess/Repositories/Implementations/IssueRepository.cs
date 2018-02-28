using System;
using System.Collections.Generic;
using System.Linq;
using Authentication;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using Database;
using DataTypes;

namespace DataAccess.Repositories.Implementations
{
	public class IssueRepository : IIssueRepository
	{
		private IssueManagerContainer _context = new IssueManagerContainer();
		private IAuthentication _authentication;

		public IssueRepository( IAuthentication authentication )
		{
			_authentication = authentication;
		}

		private Issue GetEntity( int id, bool throwIfNotFound = true )
		{
			var issue = _context.Issues.FirstOrDefault( x => x.Id == id );
			if ( issue == null && throwIfNotFound )
				throw new IssueNotFoundException();

			return issue;
		}
    private IEnumerable<IssueImage> GetImageEntity(int issueId)
    {
      return _context.IssueImages.Where(x => x.IssueId == issueId);
    }
		public IssueModel Get( int id, string currentUser )
		{
			var issue = GetEntity( id, false );
			
			_authentication.CheckUserAccess( currentUser, issue.Location );

			return ModelFromEntity( issue );
		}

    public int GetLocationIdForIssue(int id)
    {
      var issue = GetEntity(id, false);
      return issue.LocationId;
    }

		public IEnumerable<IssueModel> GetAll( int locationId, string currentUser )
		{
			var location = CheckUserAccess( currentUser, locationId );

			return	location.Issues
					.Select( issue => new IssueModel
					{
						LocationId = issue.LocationId,
						Id = issue.Id,
						Title = issue.Title,
						Description = issue.Description,
						Longitude = issue.Longitude,
						Latitude = issue.Latitude,
						Severity = issue.SeverityEnum,
						Status = issue.StatusEnum,
						Created = issue.Created,
						Edited = issue.Edited,
            CreatedBy = issue.CreatedBy
					} );
		}

		public IEnumerable<IssueModel> GetOpen( int locationId, string currentUser )
		{
			var location = CheckUserAccess( currentUser, locationId );

			return	location.Issues
					.Where( x => x.Status != (short)IssueStatus.Done )
					.Select( issue => new IssueModel
					{
						LocationId = issue.LocationId,
						Id = issue.Id,
						Title = issue.Title,
						Description = issue.Description,
						Longitude = issue.Longitude,
						Latitude = issue.Latitude,
						Severity = issue.SeverityEnum,
						Status = issue.StatusEnum,
						Created = issue.Created,
						Edited = issue.Edited,
            CreatedBy = issue.CreatedBy
            
					} );
		}

		public IQueryable<IssueModel> GetIssuesAtLocation( int locationId, string currentUser )
		{
			CheckUserAccess( currentUser, locationId );

			return	from l in _context.Locations
					where l.Id == locationId
					from i in l.Issues
					select ModelFromEntity( i );
		}

		public void Create( IssueModel model, string currentUser )
		{
			var location = CheckUserAccess( currentUser, model.LocationId );

			ValidateSeverityEnum( model.Severity );
			ValidateStatusEnum( model.Status );

			var issue = new Issue
			{
				Title = model.Title,
				Description = model.Description,
				Longitude = model.Longitude,
				Latitude = model.Latitude,
				SeverityEnum = model.Severity,
				StatusEnum = model.Status,
				Created = DateTime.UtcNow,
				Edited = DateTime.UtcNow,
        CreatedBy = currentUser
			};

			location.Issues.Add( issue );
			_context.SaveChanges();

			model.Id = issue.Id;
		}

		public void Update( IssueModel model, string currentUser )
		{
			var issue = GetEntity( model.Id );

			CheckUserAccess( currentUser, issue.LocationId );

			issue.Title = model.Title;
			issue.Description = model.Description;
			issue.Longitude = model.Longitude;
			issue.Latitude = model.Latitude;

			ValidateSeverityEnum( model.Severity );
			issue.SeverityEnum = model.Severity;

			ValidateStatusEnum( model.Status );
			issue.StatusEnum = model.Status;

			issue.Edited = DateTime.UtcNow;

			_context.SaveChanges();
		}

		public void SetStatus( int id, IssueStatus status, string currentUser )
		{
			var issue = GetEntity( id );

			CheckUserAccess( currentUser, issue.LocationId );

			ValidateStatusEnum(status);

			issue.StatusEnum = status;
			issue.Edited = DateTime.UtcNow;

			_context.SaveChanges();
		}

		public void Delete( int id, string currentUser )
		{
      
      var issue = GetEntity(id);

      CheckUserAccess(currentUser, issue.LocationId);

      List<IssueImage> images = issue.Images.ToList();
      foreach (var i in images)
      {
        issue.Images.Remove(i);
        _context.IssueImages.Remove(i);
        _context.SaveChanges();
      }
      
      
      _context.Issues.Remove(issue);
      _context.SaveChanges();
		}

		private IssueModel ModelFromEntity( Issue issue )
		{
			if ( issue == null )
				return null;

			return new IssueModel
			{
				LocationId = issue.LocationId,
				Id = issue.Id,
				Title = issue.Title,
				Description = issue.Description,
				Longitude = issue.Longitude,
				Latitude = issue.Latitude,
				Status = issue.StatusEnum,
				Severity = issue.SeverityEnum,
				Created = issue.Created,
				Edited = issue.Edited,
        CreatedBy = issue.CreatedBy
			};
		}

		private Location CheckUserAccess( string currentUser, int locationId )
		{
			var location = _context.Locations.FirstOrDefault( x => x.Id == locationId );
			if ( location == null )
				throw new LocationNotFoundException();

			_authentication.CheckUserAccess( currentUser, location );

			return location;
		}

		private static void ValidateSeverityEnum( IssueSeverity severity )
		{
			if ( !Enum.IsDefined( typeof( IssueSeverity ), severity ) )
				throw new InvalidIssueSeverityException();
		}

		private static void ValidateStatusEnum( IssueStatus status )
		{
			if ( !Enum.IsDefined( typeof( IssueStatus ), status ) )
				throw new InvalidIssueStatusException();
		}
	}
}
