namespace BookStore.Identities.Dtos.Authorizations
{
  public class SignInRequest
  {
    public string Username { get; set; }
    public string Password { get; set; }
    public string Scopes { get; set; }
  }
}