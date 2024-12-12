using Newtonsoft.Json;

namespace Grokomatic.Models
{
    public class FacebookPicture
    {
        [JsonProperty("is_silhouette")]
        public bool IsSilhouette { get; set; }

        [JsonProperty("url")]
        public string? Url { get; set; }
    }
}
