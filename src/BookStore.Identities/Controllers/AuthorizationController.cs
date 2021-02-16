using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Security.Claims;
using System.Threading.Tasks;
using BookStore.Identities.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation;

namespace BookStore.Identities.Controllers
{
  [ApiController]
  [Route("~/connect")]
  public class AuthorizationController : ControllerBase
  {
    private IOpenIddictScopeManager _scopeManager;

    //private ApplicationDbContext _context;
    private SignInManager<Profile> _signInManager;
    private UserManager<Profile> _userManager;

    public AuthorizationController(IOpenIddictScopeManager scopeManager, SignInManager<Profile> signInManager, UserManager<Profile> userManager)
    {
      _scopeManager = scopeManager;
      _signInManager = signInManager;
      _userManager = userManager;
    }

    [HttpPost("authorize")]
    public async Task<IActionResult> Authorize()
    {
      var request = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The connection request was retrieved.");
      if (!User.Identity.IsAuthenticated)
      {
        if (request.HasPrompt(OpenIddictConstants.Prompts.None))
        {
          var properties = new AuthenticationProperties(new Dictionary<string, string>
          {
            [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.LoginRequired,
            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "This user is not sign-in."
          });

          return Forbid(properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return Challenge();
      }

      var user = await _userManager.GetUserAsync(User) ?? throw new InvalidOperationException("The user cannot be retrieved.");

      var principal = await _signInManager.CreateUserPrincipalAsync(user);
      var scopes = request.GetScopes();

      principal.SetScopes(request.GetScopes());
      principal.SetResources(await _scopeManager.ListResourcesAsync(scopes).ToListAsync());

      foreach (var claim in principal.Claims)
      {
        claim.SetDestinations(GetDestinations(claim, principal));
      }

      return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    public async Task<IActionResult> Signout()
    {
      await _signInManager.SignOutAsync();
      return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal)
    {
      switch (claim.Type)
      {
        case OpenIddictConstants.Claims.Name:
          yield return OpenIddictConstants.Destinations.AccessToken;
          if (principal.HasScope(OpenIddictConstants.Scopes.Profile))
            yield return OpenIddictConstants.Destinations.IdentityToken;
          yield break;

        case OpenIddictConstants.Claims.Email:
          yield return OpenIddictConstants.Destinations.AccessToken;
          if (principal.HasScope(OpenIddictConstants.Scopes.Email))
            yield return OpenIddictConstants.Destinations.IdentityToken;
          yield break;

        case OpenIddictConstants.Claims.Role:
          yield return OpenIddictConstants.Destinations.AccessToken;
          if (principal.HasScope(OpenIddictConstants.Scopes.Roles))
            yield return OpenIddictConstants.Destinations.IdentityToken;
          yield break;

        case "AspNet.Identity.SecurityStamp": yield break;

        default:
          yield return OpenIddictConstants.Destinations.AccessToken;
          yield break;
      }
    }
  }

  public static class AsyncEnumerableExtensions
  {
    public static Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }

      return ExecuteAsync();

      async Task<List<T>> ExecuteAsync()
      {
        var list = new List<T>();

        await foreach (var element in source)
        {
          list.Add(element);
        }

        return list;
      }
    }
  }
}