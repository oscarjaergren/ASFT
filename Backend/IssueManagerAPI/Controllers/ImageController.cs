using DataAccess.Models;
using DataAccess.Repositories.Interfaces;
using DataTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ViewModels;
using ViewModels.Issue;

namespace IssueManagerAPI.Controllers
{
  [RoutePrefix("api/PImage")]
  public class PublicImageController : ApiController
  {
    private readonly IIssueRepository _issueRepository;
    private readonly IIssueImageRepository _imageRepository;
    private readonly ILocationRepository _locationRepository;
    public PublicImageController(IIssueRepository issueRepository, IIssueImageRepository imageRepository, ILocationRepository locationRepository)
    {
      _issueRepository = issueRepository;
      _imageRepository = imageRepository;
      _locationRepository = locationRepository;
    }

    [HttpGet]
    [Route("v1/GetImage")]
    public HttpResponseMessage GetImage(string imageFilename)
    {
      try 
      {
        var image = _imageRepository.GetByFilename(imageFilename);

        int locationId = _issueRepository.GetLocationIdForIssue(image.IssueId);
        if (locationId == null)
          return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Issue for image does not exists, or access denied");

        var location = _locationRepository.Get(locationId);
        if (location == null)
          return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Location not found");

        String imageStorageLocation = @"C:\IssueManager\ImagesStorage\";
        imageStorageLocation += location.Name.ToUpper();
        imageStorageLocation += @"\";
        imageStorageLocation += image.IssueId.ToString();
        imageStorageLocation += @"\";
        imageStorageLocation += image.Image.FileName;

        if (File.Exists(imageStorageLocation) == false)
        {
          return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Image file not found");
        }

        var result = Request.CreateResponse(HttpStatusCode.OK);

        FileInfo f = new FileInfo(imageStorageLocation);

        var stream = new FileStream(imageStorageLocation, FileMode.Open);

        result.Content = new StreamContent(stream);
        result.Content.Headers.ContentLength = f.Length;

        // check if it really is a jpg..else do image/png
        result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        return result;

      }
      catch(Exception ex)
      {
        return Request.CreateResponse(HttpStatusCode.NotFound);
      }
    }
  }

	[Authorize]
	[RoutePrefix( "api/Image" )]
	public class ImageController : ApiController
	{
    private readonly IIssueRepository _issueRepository;
    private readonly IIssueImageRepository _imageRepository;
    private readonly ILocationRepository _locationRepository;
    public ImageController(IIssueRepository issueRepository, IIssueImageRepository imageRepository, ILocationRepository locationRepository)
    {
      _issueRepository = issueRepository;
      _imageRepository = imageRepository;
      _locationRepository = locationRepository;
    }

    [HttpPost]
    [Route("Upload")]
    public async Task<HttpResponseMessage> Upload(int issueId)
    {
      if (!Request.Content.IsMimeMultipartContent())
      {
        return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "The request content must be in multipart format.");
      }

      // Fetch issue from db
      var issue = _issueRepository.Get(issueId, User.Identity.Name);
      if (issue == null)
        throw new IssueNotFoundException();

      var location = _locationRepository.Get(issue.LocationId);
      if (issue == null)
        throw new LocationNotFoundException();

			// TODO: Validate user access to this issue
      // ?

      // Include airport name in path (upper case)
      // C:\IssueManager\ImagesStorage\LJU\2\2-guid.jpg"
      String imageStorageLocation = @"C:\IssueManager\ImagesStorage\";
      imageStorageLocation += location.Name.ToUpper();
      imageStorageLocation += @"\";
      imageStorageLocation += issueId.ToString();

      // Makesure path exists
      if (System.IO.Directory.Exists(imageStorageLocation) == false)
      {
        System.IO.Directory.CreateDirectory(imageStorageLocation);
      }

			//string fileSaveLocation = HttpContext.Current.Server.MapPath("~/App_Data");
      var provider = new CustomMultipartFormDataStreamProvider(_imageRepository, imageStorageLocation, issueId);

			HttpResponseMessage response;
			try
			{
				await Request.Content.ReadAsMultipartAsync(provider);


        var vm = _imageRepository.Get(provider.ImageID);

        ApiResponse result = new ApiResponse
        {
          Result = ApiResult.Success,
          JsonContent = JsonConvert.SerializeObject(vm, Formatting.None)
        };

        return Request.CreateResponse(HttpStatusCode.OK, result);
			}
			catch ( IOException ex)
			{
        switch (provider.Error)
        {
          case CustomMultipartFormDataStreamProvider.ErrorType.InvalidImageType:
            response = Request.CreateResponse( HttpStatusCode.UnsupportedMediaType, "Invalid image type specified. Must be jpg or png." ); 
            break;
          default:
            response = Request.CreateResponse( HttpStatusCode.InternalServerError ); 
            break;
				}
			}
			catch ( Exception e )
			{
				response = Request.CreateResponse( HttpStatusCode.InternalServerError );
			}

			Rollback( provider );

			return response;
		}


    [HttpGet]
    [Route("v1/GetImage")]
    public HttpResponseMessage GetImage(int imageId)
    {
      var image = _imageRepository.Get(imageId);
      if(image == null)
        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalid image id");

      var issue = _issueRepository.Get(image.IssueId, User.Identity.Name);
      if(issue == null)
        return Request.CreateErrorResponse(HttpStatusCode.NotFound,"Issue for image does not exists, or access denied");

      var location = _locationRepository.Get(issue.LocationId);
      if(location == null)
        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Location not found");
      
      String filepath = @"C:\IssueManager\ImagesStorage\";
      filepath += location.Name.ToUpper();
      filepath += @"\";
      filepath += image.IssueId.ToString();
      filepath += @"\";
      filepath += image.Image.FileName;

      if(File.Exists(filepath) == false)
      {
        return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Image file not found");
      }

      var result = Request.CreateResponse(HttpStatusCode.OK);

      FileInfo f = new FileInfo(filepath);

      var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
      
      result.Content = new StreamContent(stream);
      result.Content.Headers.ContentLength = f.Length;
      
      // check if it really is a jpg..else do image/png
      result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpg");


      return result;
     }

    [HttpGet]
    [Route("v1/GetImages")]
    public HttpResponseMessage GetImages(int issueId)
    {
      ApiResult result;
      try
      {
        var images = _imageRepository.GetImagesForIssue(issueId).ToList();
        var imageVMs = images.Select(ModelToVM).ToList();

        return Request.CreateResponse(HttpStatusCode.OK,
                        new ApiResponse
                        {
                          Result = ApiResult.Success,
                          JsonContent = JsonConvert.SerializeObject(imageVMs, Formatting.None)
                        });
      }
      catch (LocationNotFoundException)
      {
        result = ApiResult.LocationNotFound;
      }
      catch (UserNotFoundException)
      {
        result = ApiResult.UserNotFound;
      }
      catch (UnauthorizedAccessException)
      {
        return new HttpResponseMessage(HttpStatusCode.Unauthorized);
      }
      catch (Exception ex)
      {
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return Request.CreateResponse(HttpStatusCode.OK, new ApiResponse { Result = result });
    }


    [HttpGet]
    [Route("v1/GetImageInfo")]
    public HttpResponseMessage GetImageInfo(int imageId)
    {
      ApiResult result;
      try
      {
        var image = _imageRepository.Get(imageId);
        if (image == null)
          return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalid image id");

        var issue = _issueRepository.Get(image.IssueId, User.Identity.Name);
        if (issue == null)
          return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Issue for image does not exists, or access denied");

        var imageVMs = ModelToVM(image);

        return Request.CreateResponse(HttpStatusCode.OK,
                        new ApiResponse
                        {
                          Result = ApiResult.Success,
                          JsonContent = JsonConvert.SerializeObject(image, Formatting.None)
                        });
      }
      catch (LocationNotFoundException)
      {
        result = ApiResult.LocationNotFound;
      }
      catch (UserNotFoundException)
      {
        result = ApiResult.UserNotFound;
      }
      catch (UnauthorizedAccessException)
      {
        return new HttpResponseMessage(HttpStatusCode.Unauthorized);
      }
      catch (Exception ex)
      {
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return Request.CreateResponse(HttpStatusCode.OK, new ApiResponse { Result = result });
    }


    private void Rollback(CustomMultipartFormDataStreamProvider provider)
    {
      foreach (var file in provider.FileData)
      {
        try
        {
          File.Delete(file.LocalFileName);
        }
        catch (Exception) { }
      }
    }

    private ImageVM ModelToVM(IssueImageModel model)
    {
      return new ImageVM
      {
        Id = model.Id,
        IssueId = model.IssueId,
        Created = model.Created,
        Image = new ImageVM.ImageInfo(model.Image.FileName)
      };
    }

	}



	// We implement MultipartFormDataStreamProvider to override the filename of File which
	// will be stored on server, or else the default name will be of the format like Body-
	// Part_{GUID}. In the following implementation we simply get the FileName from 
	// ContentDisposition Header of the Request Body.
	public class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
	{
		public enum ErrorType
		{
			None,
			InvalidImageType
		}

    private readonly IIssueImageRepository _imageRepository;
    private readonly int _issueID;
    public int ImageID { get; private set; }
		public ErrorType Error { get; private set; }

		public CustomMultipartFormDataStreamProvider(IIssueImageRepository imageRepository, string path, int issueID) 
      : base(path)
		{
      ImageID = 0;
      _imageRepository = imageRepository;
      _issueID = issueID;

      // TODO: Start transaction
      // HOW ?
		}

		public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
		{
			return base.GetStream(parent, headers);
		}

		public override Task ExecutePostProcessingAsync(CancellationToken cancellationToken)
		{
			// TODO: Commit transaction
			return base.ExecutePostProcessingAsync(cancellationToken);
		}

		public override string GetLocalFileName(HttpContentHeaders headers)
		{
			if ( string.CompareOrdinal( "png", headers.ContentDisposition.FileName ) != 0 &&
				 string.CompareOrdinal( "jpg", headers.ContentDisposition.FileName ) != 0 )
			{
				Error = ErrorType.InvalidImageType;
				throw new Exception();
			}

			// TODO: Write to db and use entity id as filename.
      //  
      String filename = Guid.NewGuid().ToString() + "." + headers.ContentDisposition.FileName;

      // Since we get string from header.. makesure all dangerous characters are removed
      filename = filename.Replace("\\", "_");
      filename = filename.Replace("/", "_");
      filename = filename.Replace("(", "_");
      filename = filename.Replace(")", "_");
      filename = filename.Replace(":", "_");

      var model = new IssueImageModel
      {
       IssueId = _issueID,
       Created = DateTime.Now,
       Image = new IssueImageModel.ImageInfo(filename)
      };

      _imageRepository.Create(model);

      ImageID = model.Id;
      return filename;
		}
	}

}
