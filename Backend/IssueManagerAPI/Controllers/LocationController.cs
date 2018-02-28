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
using ViewModels.Location;

namespace IssueManagerAPI.Controllers
{
	[Authorize]
	[RoutePrefix( "api/Location" )]
	public class LocationController : ApiController
	{
		private readonly ILocationRepository _locationRepository;

		public LocationController( ILocationRepository locationRepository )
		{
			_locationRepository = locationRepository;
		}

		[HttpGet]
		[Route( "v1/GetLocations" )]
		public HttpResponseMessage GetLocations()
		{
			try
			{
				var locations = _locationRepository.GetAll().ToList();

				var locationVMs = locations.Select( x => new LocationVM
				{
					Id = x.Id,
					Name = x.Name,
					FullName = x.FullName,
					Latitude = x.Latitude,
					Longitude = x.Longitude,
					TimeZone = x.TimeZone
				} ).ToList();

				return Request.CreateResponse( HttpStatusCode.OK,
												new ApiResponse
												{
													Result = ApiResult.Success,
													JsonContent = JsonConvert.SerializeObject( locationVMs, Formatting.None )
												} );
			}
			catch ( Exception )
			{
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}
		}

		[HttpGet]
		[Route( "v1/GetUsersLocations" )]
		public HttpResponseMessage GetUsersLocations()
		{
			ApiResult result;
			try
			{
				var locations = _locationRepository.GetAllFromUser( User.Identity.Name ).ToList();

				var locationVMs = locations.Select( x => new LocationVM
				{
					Id = x.Id,
					Name = x.Name,
					FullName = x.FullName,
					Latitude = x.Latitude,
					Longitude = x.Longitude,
					TimeZone = x.TimeZone
				} ).ToList();

				return Request.CreateResponse( HttpStatusCode.OK,
												new ApiResponse
												{
													Result = ApiResult.Success,
													JsonContent = JsonConvert.SerializeObject( locationVMs, Formatting.None )
												} );
			}
			catch ( UserNotFoundException )
			{
				result = ApiResult.UserNotFound;
			}
			catch ( Exception )
			{
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}

			return Request.CreateResponse( HttpStatusCode.OK, new ApiResponse { Result = result } );
		}

		[HttpGet]
		[Route( "v1/GetLocation" )]
		public HttpResponseMessage GetLocation( int locationId )
		{
			ApiResult result;
			try
			{
				var location = _locationRepository.Get( locationId );
				if ( location == null )
					throw new LocationNotFoundException();
				var vm = ModelToVM( location );

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
			catch ( Exception )
			{
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}

			return Request.CreateResponse( HttpStatusCode.OK, new ApiResponse { Result = result } );
		}

		private LocationVM ModelToVM( LocationModel model )
		{
			return new LocationVM
			{
				Id = model.Id,
				Name = model.Name,
				FullName = model.FullName,
				Longitude = model.Longitude,
				Latitude = model.Latitude,
				TimeZone = model.TimeZone
			};
		}
		
		[HttpPost]
		[Route( "v1/CreateLocation" )]
		[Authorize( Roles = "ASFTAdmin" )]
		public HttpResponseMessage CreateLocation( NewLocationVM vm )
		{
			ApiResult result;
			try
			{
				var model = new LocationModel
				{
					Name = vm.Name,
					FullName = vm.FullName,
					Longitude = vm.Longitude,
					Latitude = vm.Latitude,
					TimeZone = vm.TimeZone
				};

				_locationRepository.Create(model);
				return Request.CreateResponse(	HttpStatusCode.OK,
												new ApiResponse
												{
													Result = ApiResult.Success,
													JsonContent = JsonConvert.SerializeObject(new {Id = model.Id}, Formatting.None)
												});
			}
			catch (NotUniqueException)
			{
				result = ApiResult.NotUnique;
			}
			catch (Exception)
			{
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}

			return Request.CreateResponse(	HttpStatusCode.OK, new ApiResponse { Result = result } );
		}

		[HttpPut]
		[Route( "v1/UpdateLocation" )]
		[Authorize( Roles = "ASFTAdmin" )]
		public HttpResponseMessage UpdateLocation( LocationVM vm )
		{
			ApiResult result;
			try
			{
				var model = new LocationModel
				{
					Id = vm.Id,
					Name = vm.Name,
					FullName = vm.FullName,
					Longitude = vm.Longitude,
					Latitude = vm.Latitude,
					TimeZone = vm.TimeZone
				};

				_locationRepository.Update( model );
				result = ApiResult.Success;
			}
			catch ( LocationNotFoundException )
			{
				result = ApiResult.LocationNotFound;
			}
			catch ( NotUniqueException )
			{
				result = ApiResult.NotUnique;
			}
			catch ( Exception )
			{
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}

			return Request.CreateResponse( HttpStatusCode.OK, new ApiResponse { Result = result } );
		}
		
		[HttpDelete]
		[Route( "v1/DeleteLocation" )]
		[Authorize( Roles = "ASFTAdmin" )]
		public HttpResponseMessage DeleteLocation( int locationId )
		{
			ApiResult result;
			try
			{
				_locationRepository.Delete( locationId );
				result = ApiResult.Success;
			}
			catch ( LocationNotFoundException )
			{
				result = ApiResult.LocationNotFound;
			}
			catch ( Exception )
			{
				return new HttpResponseMessage( HttpStatusCode.InternalServerError );
			}

			return Request.CreateResponse( HttpStatusCode.OK, new ApiResponse { Result = result } );
		}
	}
}