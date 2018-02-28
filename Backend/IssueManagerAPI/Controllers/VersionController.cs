using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IssueManagerAPI.Controllers
{
	[RoutePrefix( "api/Version" )]
	public class VersionController : ApiController
	{
		[HttpGet]
		[Route( "GetLatestVersion" )]
		public string GetLatestVersion()
		{
			return Versions.GetLatest().Name;
			//return Request.CreateResponse( HttpStatusCode.OK, Versions.GetLatest() );
		}

		[HttpGet]
		[Route( "IsVersionSupported" )]
		public bool IsVersionSupported( int versionNr )
		{
			return Versions.IsVersionSupported( versionNr );
			//return Request.CreateResponse( HttpStatusCode.OK, Versions.IsVersionSupported( versionName ).ToString() );
		}
	}
}
