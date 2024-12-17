using Grokomatic.Configs;
using Grokomatic.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Grokomatic.Services
{
    public class FacebookService
    {
        public async Task PostOnFacebook(SocialPost socialPost, AppConfiguration appConfig)
        {
            using var httpClient = new HttpClient();
            
            var formData = new MultipartFormDataContent
            {
                { new StringContent(socialPost.PostText), "message" },
                { new ByteArrayContent(File.ReadAllBytes(socialPost.PostImage)), "source", "photo.jpg" }
            };

            var response = await httpClient.PostAsync($"https://graph.facebook.com/{appConfig.FbPageId}/photos?access_token={appConfig.FbPageAccessToken}", formData);
            var responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseString);
            var facebookResponse = JsonConvert.DeserializeObject<FacebookResponse>(responseString);
            Log.Information("You published the Facebook post. ID:{0}, Post ID: {1}", facebookResponse?.Id, facebookResponse?.PostId);
        }

        public async Task<FacebookPicture> GetFacebookPicture(string id, string pageAccessToken)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://graph.facebook.com/{id}/picture?redirect=false&access_token={pageAccessToken}");
            var responseString = await response.Content.ReadAsStringAsync();
            var newString = responseString.Replace("https:", "");
            
            var facebookPictureData = JsonConvert.DeserializeObject<FacebookPictureData>(responseString);

            if (facebookPictureData == null)
            {
                throw new Exception("Facebook did not return picture data.");
            }

            return facebookPictureData.Data;
        }

        public async Task UploadPhoto(string imagePath, string pageAccessToken, string pageId)
        {
            using var httpClient = new HttpClient();

            var formData = new MultipartFormDataContent
            {
                { new StringContent("false"), "published" },
                { new ByteArrayContent(File.ReadAllBytes(imagePath)), "source", "photo.jpg" }
            };
            var response = await httpClient.PostAsync($"https://graph.facebook.com/{pageId}/photos?access_token={pageAccessToken}", formData);

            var responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseString);
        }
    }
}
