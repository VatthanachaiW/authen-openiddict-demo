using System.ComponentModel.DataAnnotations;

namespace BookStore.Identities.Dtos.Accounts
{
  public class RegisterDto
  {
    [Required]
    [Display(Name = "Username")]
    public string Username { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    [Required]
    [DataType(DataType.EmailAddress)]
    [Display(Name = "Email")]
    public string Email { get; set; }
  }
}