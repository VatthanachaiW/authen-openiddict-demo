using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace BookStore.Identites.Controllers
{
  [ApiController]
  public class AuthorizationController : ControllerBase
  {
    private IOpenIddictApplicationManager _applicationManager;

    public AuthorizationController(IOpenIddictApplicationManager applicationManager)
    {
      _applicationManager = applicationManager;
    }

    [HttpPost("~/connect/token"), Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
      var request = HttpContext.GetOpenIddictServerRequest();
      if (!request.IsClientCredentialsGrantType())
      {
        throw new NotImplementedException("The specified grant is not implemented.");
      }


      var application =
        await _applicationManager.FindByClientIdAsync(request.ClientId) ??
        throw new InvalidOperationException("The application cannot be found.");

      var identity = new ClaimsIdentity(
        TokenValidationParameters.DefaultAuthenticationType,
        OpenIddictConstants.Claims.Name, OpenIddictConstants.Claims.Role);

      ClaimsPrincipal claimsPrincipal = null;

      if (request.IsClientCredentialsGrantType())
      {
        //var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        identity.AddClaim(OpenIddictConstants.Claims.Subject,
          await _applicationManager.GetClientIdAsync(application),
          OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);

        identity.AddClaim(OpenIddictConstants.Claims.Name,
          await _applicationManager.GetDisplayNameAsync(application),
          OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken);

        //identity.AddClaim("some-claim", "some-value", OpenIddictConstants.Destinations.AccessToken);

        claimsPrincipal = new ClaimsPrincipal(identity);

        claimsPrincipal.SetScopes(request.GetScopes());
      }

      return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
  }
}