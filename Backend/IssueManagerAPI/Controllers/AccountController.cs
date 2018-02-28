using System.Net;
using System.Net.Http;
using System.Web.Http;
using IssueManagerAPI.Providers;

namespace IssueManagerAPI.Controllers
{
	[Authorize]
	[RoutePrefix("api/Account")]
	public class AccountController : ApiController
	{
		[HttpGet]
		[Route( "v1/GetUserName" )]
		public HttpResponseMessage GetUserName()
		{
			HttpResponseMessage response = Request.CreateResponse( HttpStatusCode.OK, User.Identity.Name );
			
			return response;
		}

		[HttpGet]
		[Route("v1/CreateTokenForUser")]
		[Authorize( Roles = "TrustedApplication" )]
		public string CreateTokenForUser(string userName)
		{
			var ticket = ((ApplicationOAuthProvider)Startup.OAuthOptions.Provider).CreateAuthTicket( userName );

			var token = Startup.OAuthOptions.AccessTokenFormat.Protect( ticket );
			return token;
		}
	}
}
