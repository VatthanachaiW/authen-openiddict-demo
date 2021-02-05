namespace BookStore.API
{
  public class IdentitySetting
  {
    public string EndpointUrl { get; set; }
    public string Audiences { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
  }
}