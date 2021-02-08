using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Linq;
using BookStore.Identites.Helpers;
using static OpenIddict.Abstractions.OpenIddictConstants;


namespace BookStore.Identites.Controllers
{
  [ApiController]
  public class AuthorizationController : ControllerBase
  {
    private IOpenIddictApplicationManager _applicationManager;
    private IOpenIddictScopeManager _scopeManager;


    public AuthorizationController(IOpenIddictApplicationManager applicationManager, IOpenIddictScopeManager scopeManager)
    {
      _applicationManager = applicationManager;
      _scopeManager = scopeManager;
    }

    /*
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
    */
    [HttpGet("~/connect/authorize")]
    public async Task<IActionResult> Authorize()
    {
      var request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
      if (request.Username != "admin" && request.Password != "P@ssw0rd")
      {
        var properties = new AuthenticationProperties(new Dictionary<string, string>
        {
          [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
          [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
            "The user is not logged in."
        });
        return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
      }

      var scopes = request.GetScopes();
      var principal = new ClaimsPrincipal();
      principal.SetScopes(request.GetScopes());

      principal.SetResources(await _scopeManager.ListResourcesAsync(scopes).ToListAsync());

      foreach (var claim in principal.Claims)
      {
        claim.SetDestinations(GetDestinations(claim, principal));
      }

      return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("~/connect/logout")]
    public async Task<IActionResult> Logout()
    {
      return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
    {
      // Note: by default, claims are NOT automatically included in the access and identity tokens.
      // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
      // whether they should be included in access tokens, in identity tokens or in both.

      switch (claim.Type)
      {
        case Claims.Name:
          yield return Destinations.AccessToken;

          if (principal.HasScope(Scopes.Profile))
            yield return Destinations.IdentityToken;

          yield break;

        case Claims.Email:
          yield return Destinations.AccessToken;

          if (principal.HasScope(Scopes.Email))
            yield return Destinations.IdentityToken;

          yield break;

        case Claims.Role:
          yield return Destinations.AccessToken;

          if (principal.HasScope(Scopes.Roles))
            yield return Destinations.IdentityToken;

          yield break;

        // Never include the security stamp in the access and identity tokens, as it's a secret value.
        case "AspNet.Identity.SecurityStamp": yield break;

        default:
          yield return Destinations.AccessToken;
          yield break;
      }
    }
  }
}