using System.Collections.Generic;
using Newtonsoft.Json;

namespace BookStore.Identities.Dtos.SignIns.Requests
{
  [JsonObject]
  public class SignInRequest
  {
    [JsonProperty] public string Username { get; set; }
    [JsonProperty] public string Password { get; set; }
    [JsonProperty] public string GrantType { get; set; }
    [JsonProperty] public string[] Resources { get; set; }
  }
}