using System.Threading.Tasks;
using BookStore.Identities.Contexts;
using BookStore.Identities.Dtos.Accounts;
using BookStore.Identities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Identities.Controllers
{
  [Route("api/account")]
  [Authorize]
  public class AccountController : ControllerBase
  {
    private readonly UserManager<ApplicationUser> _userManager;


    public AccountController(UserManager<ApplicationUser> userManager)
    {
      _userManager = userManager;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
      if (ModelState.IsValid)
      {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user != null) return StatusCode(StatusCodes.Status409Conflict);

        user = new ApplicationUser
        {
          UserName = request.Username,
          Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (result.Succeeded) return Ok();
        AddErrors(result);
      }

      return BadRequest(ModelState);
    }

    private void AddErrors(IdentityResult result)
    {
      foreach (var error in result.Errors)
      {
        ModelState.AddModelError(string.Empty, error.Description);
      }
    }
  }
}