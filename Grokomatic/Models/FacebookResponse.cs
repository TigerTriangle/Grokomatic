using Newtonsoft.Json;

namespace Grokomatic.Models
{
    public class FacebookResponse
    {
        [JsonProperty("id")]
        public required string Id { get; set; }

        [JsonProperty("post_id")]
        public string? PostId { get; set; }
    }
}
