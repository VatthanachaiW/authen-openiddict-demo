using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BookStore.Identities.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace BookStore.Identities.Controllers
{
  [Route("api/connect")]
  public class AuthorizationController : ControllerBase
  {
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthorizationController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
      _signInManager = signInManager;
      _userManager = userManager;
    }

    [HttpPost("token")]
    [Produces("application/json")]
    public async Task<IActionResult> SignIn([FromBody] OpenIddictRequest request)
    {
      // var request = HttpContext.GetOpenIddictServerRequest();
      if (request.IsPasswordGrantType())
      {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null)
        {
          var properties = new AuthenticationProperties(new Dictionary<string, string>
          {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The username/password couple is invalid."
          });

          return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
          var properties = new AuthenticationProperties(new Dictionary<string, string>
          {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
              "The username/password couple is invalid."
          });

          return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var principal = await _signInManager.CreateUserPrincipalAsync(user);
        principal.SetScopes(new[]
        {
          OpenIddictConstants.Scopes.OpenId,
          OpenIddictConstants.Scopes.Email,
          OpenIddictConstants.Scopes.Profile,
          OpenIddictConstants.Scopes.Roles
        }.Intersect(request.GetScopes()));

        foreach (var claim in principal.Claims)
        {
          claim.SetDestinations(GetDestinations(claim, principal));
        }

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
      }

      throw new NotImplementedException("The specified grant type is not implemented.");
    }

    private IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
    {
      switch (claim.Type)
      {
        case OpenIddictConstants.Claims.Name:
          yield return OpenIddictConstants.Destinations.AccessToken;

          if (principal.HasScope(OpenIddictConstants.Permissions.Scopes.Profile))
            yield return OpenIddictConstants.Destinations.IdentityToken;

          yield break;

        case OpenIddictConstants.Claims.Email:
          yield return OpenIddictConstants.Destinations.AccessToken;

          if (principal.HasScope(OpenIddictConstants.Permissions.Scopes.Email))
            yield return OpenIddictConstants.Destinations.IdentityToken;

          yield break;

        case OpenIddictConstants.Claims.Role:
          yield return OpenIddictConstants.Destinations.AccessToken;

          if (principal.HasScope(OpenIddictConstants.Permissions.Scopes.Roles))
            yield return OpenIddictConstants.Destinations.IdentityToken;

          yield break;

        // Never include the security stamp in the access and identity tokens, as it's a secret value.
        case "AspNet.Identity.SecurityStamp": yield break;

        default:
          yield return OpenIddictConstants.Destinations.AccessToken;
          yield break;
      }
    }
  }
}