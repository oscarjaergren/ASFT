using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using DataTypes;
using Newtonsoft.Json;
using ViewModels;
using ViewModels.Issue;

namespace IssueManagerAPI.Controllers
{
	[Authorize]
	[RoutePrefix( "api/Issue" )]
	public class IssueController : ApiController
	{
		private readonly IIssueRepository _issueRepository;
    private readonly IIssueImageRepository _imageRepository;
    private readonly ILocationRepository _locationRepository;

		public IssueController( IIssueRepository issueRepository ,  IIssueImageRepository imageRepository, ILocationRepository locationRepository)
		{
			_issueRepository = issueRepository;
      _imageRepository = imageRepository;
      _locationRepository = locationRepository;
		}

		[HttpGet]
		[Route( "v1/GetAllIssuesAtLocation" )]
		public HttpResponseMessage GetAllIssuesAtLocation( int locationId )
		{
			ApiResult result;
			try
			{
				var issues = _issueRepository.GetAll( locationId, User.Identity.Name ).ToList();
				var issueVMs = issues.Select( ModelToVM ).ToList();

				return Request.CreateResponse( HttpStatusCode.OK,
												new ApiResponse
												{
													Result = ApiResult.Success,
													JsonContent = JsonConvert.SerializeObject( issueVMs, Formatting.None )
												} );
			}
			catch ( LocationNotFoundException )
			{
				result = ApiResult.LocationNotFound;
			}
			catch ( UserNotFoundException )
			{
				result = ApiResult.UserNotFound;
			}
			catch ( UnauthorizedAccessException )
			{
				return new HttpResponseMessage( HttpStatusCode.Unauthorized );
			}
			catch ( Exception )
			{
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}

			return Request.CreateResponse( HttpStatusCode.OK, new ApiResponse { Result = result } );
		}

		[HttpGet]
		[Route( "v1/GetOpenIssuesAtLocation" )]
		public HttpResponseMessage GetOpenIssuesAtLocation( int locationId )
		{
			ApiResult result;
			try
			{
				var issues = _issueRepository.GetOpen( locationId, User.Identity.Name ).ToList();
				var issueVMs = issues.Select( ModelToVM ).ToList();

				return Request.CreateResponse( HttpStatusCode.OK,
												new ApiResponse
												{
													Result = ApiResult.Success,
													JsonContent = JsonConvert.SerializeObject( issueVMs, Formatting.None )
												} );
			}
			catch ( LocationNotFoundException )
			{
				result = ApiResult.LocationNotFound;
			}
			catch ( UserNotFoundException )
			{
				result = ApiResult.UserNotFound;
			}
			catch ( UnauthorizedAccessException )
			{
				return new HttpResponseMessage( HttpStatusCode.Unauthorized );
			}
			catch ( Exception )
			{
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}

			return Request.CreateResponse( HttpStatusCode.OK, new ApiResponse { Result = result } );
		}

		[HttpGet]
		[Route( "v1/GetIssue" )]
		public HttpResponseMessage GetIssue( int issueId )
		{
			ApiResult result;
			try
			{
				var issue = _issueRepository.Get( issueId, User.Identity.Name );
				if ( issue == null )
					throw new IssueNotFoundException();
				var vm = ModelToVM( issue );

				return Request.CreateResponse( HttpStatusCode.OK,
												new ApiResponse
												{
													Result = ApiResult.Success,
													JsonContent = JsonConvert.SerializeObject( vm, Formatting.None )
												} );
			}
			catch ( LocationNotFoundException )
			{
				result = ApiResult.LocationNotFound;
			}
			catch ( UserNotFoundException )
			{
				result = ApiResult.UserNotFound;
			}
			catch ( UnauthorizedAccessException )
			{
				return new HttpResponseMessage( HttpStatusCode.Unauthorized );
			}
			catch ( IssueNotFoundException )
			{
				result = ApiResult.IssueNotFound;
			}
			catch ( Exception )
			{
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}

			return Request.CreateResponse( HttpStatusCode.OK, new ApiResponse { Result = result } );
		}

		[HttpPost]
		[Route( "v1/CreateIssue" )]
		public HttpResponseMessage CreateIssue( NewIssueVM vm )
		{
			ApiResult result;
			try
			{
				var model = new IssueModel
				{
					LocationId = vm.LocationId,
					Title = vm.Title,
					Description = vm.Description,
					Longitude = vm.Longitude,
					Latitude = vm.Latitude,
					Status = vm.Status,
					Severity = vm.Severity
				};

				_issueRepository.Create( model, User.Identity.Name );
				return Request.CreateResponse( HttpStatusCode.OK,
												new ApiResponse
												{
													Result = ApiResult.Success,
													JsonContent = JsonConvert.SerializeObject( new { Id = model.Id }, Formatting.None )
												} );
			}
			catch ( LocationNotFoundException )
			{
				result = ApiResult.LocationNotFound;
			}
			catch ( UserNotFoundException )
			{
				result = ApiResult.UserNotFound;
			}
			catch ( UnauthorizedAccessException )
			{
				return new HttpResponseMessage( HttpStatusCode.Unauthorized );
			}
			catch ( InvalidIssueSeverityException )
			{
				result = ApiResult.InvalidIssueSeverity;
			}
			catch ( InvalidIssueStatusException )
			{
				result = ApiResult.InvalidIssueStatus;
			}
			catch ( Exception )
			{
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}

			return Request.CreateResponse( HttpStatusCode.OK, new ApiResponse { Result = result } );
		}

		[HttpPut]
		[Route( "v1/UpdateIssue" )]
		public HttpResponseMessage UpdateIssue( IssueVM vm )
		{
			ApiResult result;
			try
			{
				var model = VmToModel( vm );

				_issueRepository.Update( model, User.Identity.Name );
				result = ApiResult.Success;
			}
			catch ( LocationNotFoundException )
			{
				result = ApiResult.LocationNotFound;
			}
			catch ( UserNotFoundException )
			{
				result = ApiResult.UserNotFound;
			}
			catch ( UnauthorizedAccessException )
			{
				return new HttpResponseMessage( HttpStatusCode.Unauthorized );
			}
			catch ( IssueNotFoundException )
			{
				result = ApiResult.IssueNotFound;
			}
			catch ( InvalidIssueSeverityException )
			{
				result = ApiResult.InvalidIssueSeverity;
			}
			catch ( InvalidIssueStatusException )
			{
				result = ApiResult.InvalidIssueStatus;
			}
			catch ( Exception )
			{
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}

			return Request.CreateResponse( HttpStatusCode.OK, new ApiResponse { Result = result } );
		}

		[HttpPut]
		[Route("v1/UpdateIssueStatus")]
		public HttpResponseMessage UpdateIssueStatus( IssueStatusVM vm )
		{
			ApiResult result;
			try
			{
				_issueRepository.SetStatus( vm.Id, vm.Status, User.Identity.Name );
				result = ApiResult.Success;
			}
			catch ( LocationNotFoundException )
			{
				result = ApiResult.LocationNotFound;
			}
			catch ( UserNotFoundException )
			{
				result = ApiResult.UserNotFound;
			}
			catch ( UnauthorizedAccessException )
			{
				return new HttpResponseMessage( HttpStatusCode.Unauthorized );
			}
			catch ( IssueNotFoundException )
			{
				result = ApiResult.IssueNotFound;
			}
			catch ( InvalidIssueStatusException )
			{
				result = ApiResult.InvalidIssueStatus;
			}
			catch ( Exception )
			{
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}

			return Request.CreateResponse( HttpStatusCode.OK, new ApiResponse { Result = result } );
		}

		[HttpDelete]
		[Route( "v1/DeleteIssue" )]
		public HttpResponseMessage DeleteIssue( int issueId )
		{
			ApiResult result;
			try
			{
        String locationName = "";
        var images = _imageRepository.GetImagesForIssue(issueId);
        if(images.Count > 0)
        {
          int locationId = _issueRepository.GetLocationIdForIssue(issueId);
          if (locationId == null)
            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Issue for image does not exists, or access denied");

          var location = _locationRepository.Get(locationId);
          if (location == null)
            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Location not found");

          locationName = location.Name;
        }

				_issueRepository.Delete( issueId, User.Identity.Name );

        // delete images from disk
        foreach(var image in images)
        {
          DeleteImageFile(locationName, image);
        }

				result = ApiResult.Success;
			}
			catch ( LocationNotFoundException )
			{
				result = ApiResult.LocationNotFound;
			}
			catch ( UserNotFoundException )
			{
				result = ApiResult.UserNotFound;
			}
			catch ( UnauthorizedAccessException )
			{
				return new HttpResponseMessage( HttpStatusCode.Unauthorized );
			}
			catch ( IssueNotFoundException )
			{
				result = ApiResult.IssueNotFound;
			}
			catch ( Exception )
			{
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}

			return Request.CreateResponse( HttpStatusCode.OK, new ApiResponse { Result = result } );
		}

    protected void DeleteImageFile(String location, IssueImageModel image )
    {
      try
      {
        String imageStorageLocation = @"C:\IssueManager\ImagesStorage\";
        imageStorageLocation += location.ToUpper();
        imageStorageLocation += @"\";
        imageStorageLocation += image.IssueId.ToString();
        imageStorageLocation += @"\";
        imageStorageLocation += image.Image.FileName;

        System.IO.File.Delete(imageStorageLocation);
      }
      catch(Exception ex)
      {

      }

    }
		private IssueVM ModelToVM( IssueModel model )
		{
			return new IssueVM
			{
				LocationId = model.LocationId,
				Id = model.Id,
				Title = model.Title,
				Description = model.Description,
				Longitude = model.Longitude,
				Latitude = model.Latitude,
				Severity = model.Severity,
				Status = model.Status,
				Created = model.Created,
				Edited = model.Edited,
        CreatedBy = model.CreatedBy,
			};
		}

		private IssueModel VmToModel( IssueVM vm )
		{
			return new IssueModel
			{
				LocationId = vm.LocationId,
				Id = vm.Id,
				Title = vm.Title,
				Description = vm.Description,
				Longitude = vm.Longitude,
				Latitude = vm.Latitude,
				Severity = vm.Severity,
				Status = vm.Status,
				Created = vm.Created,
				Edited = vm.Edited,
        CreatedBy = vm.CreatedBy
			};
		}
	}
}
