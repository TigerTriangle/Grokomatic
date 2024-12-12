using Newtonsoft.Json;

namespace Grokomatic.Models
{
    public class FacebookPictureData
    {
        [JsonProperty("data")]
        public FacebookPicture Data { get; set; }
    }
}
