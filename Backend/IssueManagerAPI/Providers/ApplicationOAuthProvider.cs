using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Security;
using Authentication;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace IssueManagerAPI.Providers
{
	public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
	{
		private readonly string _publicClientId;

		private readonly IAuthentication _authentication;
		
		public ApplicationOAuthProvider( string publicClientId, IAuthentication authentication )
		{
			if (publicClientId == null)
			{
				throw new ArgumentNullException("publicClientId");
			}

			_publicClientId = publicClientId;
			_authentication = authentication;
		}

		public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
		{
			if ( !await Task.Run( () => _authentication.Login( context.UserName, context.Password ) ) )
			{
				context.SetError( "invalid_grant", "The user name or password is incorrect." );
				return;
			}

			var ticket = CreateAuthTicket( context.UserName );
			context.Validated(ticket);

			var cookiesIdentity = new ClaimsIdentity( ticket.Identity.Claims, "Cookies", ClaimTypes.Name, ClaimTypes.Role );
			context.Request.Context.Authentication.SignIn(cookiesIdentity);
		}

		public AuthenticationTicket CreateAuthTicket( string userName )
		{
			var oAuthIdentity = CreateOAuthIdentity( userName );

			var properties = CreateProperties( userName );
			var ticket = new AuthenticationTicket( oAuthIdentity, properties );
			return ticket;
		}

		public ClaimsIdentity CreateOAuthIdentity( string userName )
		{
			var claims = new List<Claim>();
			claims.Add( new Claim( ClaimTypes.Name, userName ) );

			foreach (var role in Roles.GetRolesForUser(userName))
			{
				claims.Add( new Claim( ClaimTypes.Role, role ) );
			}

			return new ClaimsIdentity( claims, "Bearer", ClaimTypes.Name, ClaimTypes.Role );
		}

		public override Task TokenEndpoint(OAuthTokenEndpointContext context)
		{
			foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
			{
				context.AdditionalResponseParameters.Add(property.Key, property.Value);
			}

			return Task.FromResult<object>(null);
		}

		public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
		{
			// Resource owner password credentials does not provide a client ID.
			if (context.ClientId == null)
			{
				context.Validated();
			}

			return Task.FromResult<object>(null);
		}

		public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
		{
			if (context.ClientId == _publicClientId)
			{
				Uri expectedRootUri = new Uri(context.Request.Uri, "/");

				if (expectedRootUri.AbsoluteUri == context.RedirectUri)
				{
					context.Validated();
				}
			}

			return Task.FromResult<object>(null);
		}

		public static AuthenticationProperties CreateProperties(string userName)
		{
			IDictionary<string, string> data = new Dictionary<string, string>
			{
				{ "userName", userName }
			};
			return new AuthenticationProperties(data);
		}
	}
}