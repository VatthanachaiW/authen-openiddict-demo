using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace BookStore.Identities.Dtos.Registers.Requests
{
  [JsonObject]
  public class RegisterRequest
  {
    [JsonProperty] public string Username { get; set; }

    [DataType(DataType.Password)]
    [JsonProperty]
    public string Password { get; set; }
  }
}