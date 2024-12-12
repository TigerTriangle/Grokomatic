using System.Net.Http.Json;
using Grokomatic.Models;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tweetinvi.Core.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Grokomatic.Services
{
    public class FacebookService
    {
        public async Task<FacebookResponse> PostOnFacebook(string text, string imagePath, string pageAccessToken, string pageId)
        {
            using var httpClient = new HttpClient();
            
            var formData = new MultipartFormDataContent
            {
                { new StringContent(text), "message" },
                { new ByteArrayContent(File.ReadAllBytes(imagePath)), "source", "photo.jpg" }
            };

            var response = await httpClient.PostAsync($"https://graph.facebook.com/{pageId}/photos?access_token={pageAccessToken}", formData);
            var responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseString);            
            Console.WriteLine(jsonResponse);
            var facebookResponse = JsonConvert.DeserializeObject<FacebookResponse>(responseString);
            return facebookResponse;
        }

        public async Task<FacebookPicture> GetFacebookPicture(string id, string pageAccessToken)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://graph.facebook.com/{id}/picture?redirect=false&access_token={pageAccessToken}");
            var responseString = await response.Content.ReadAsStringAsync();
            var newString = responseString.Replace("https:", "");
            
            var facebookPictureData = JsonConvert.DeserializeObject<FacebookPictureData>(responseString);

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
            // convert the response to a FacebookResponse object

            var responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseString);
            Console.WriteLine(jsonResponse);
        }
    }
}
