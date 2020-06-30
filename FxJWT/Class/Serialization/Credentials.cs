using Newtonsoft.Json;

namespace FxJWT.Class.Serialization
{
    class Credentials
    {
        [JsonProperty("user")]
        public string Username { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
