namespace BookStore.Identities.Models
{
  public class Profile
  {
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
  }
}