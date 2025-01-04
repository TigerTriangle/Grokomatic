using Grokomatic.Configs;
using Grokomatic.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Grokomatic.Services
{
    public class FacebookService
    {
        /// <summary>
        /// Posts a social media post on Facebook.
        /// </summary>
        /// <param name="socialPost">The social post containing the text and image to be posted.</param>
        /// <param name="appConfig">The configuration containing Facebook page details and access token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task PostOnFacebook(SocialPost socialPost, FacebookConfig facebookConfig)
        {
            using var httpClient = new HttpClient();

            var formData = new MultipartFormDataContent
            {
                { new StringContent(socialPost.PostText), "message" }
            };

            if (!string.IsNullOrEmpty(socialPost.PostImage))
            {
                formData.Add(new ByteArrayContent(File.ReadAllBytes(socialPost.PostImage)), "source", "photo.jpg");
            }

            var response = await httpClient.PostAsync($"https://graph.facebook.com/{facebookConfig.PageId}/photos?access_token={facebookConfig.PageAccessToken}", formData);
            var responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseString);
            var facebookResponse = JsonConvert.DeserializeObject<FacebookResponse>(responseString);
            Log.Information("You published the Facebook post. ID:{0}, Post ID: {1}", facebookResponse?.Id, facebookResponse?.PostId);
        }
    }
}
