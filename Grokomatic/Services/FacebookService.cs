using System.Net.Http.Json;
using Grokomatic.Models;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using Tweetinvi.Core.Models;

namespace Grokomatic.Services
{
    public class FacebookService
    {
        public async Task PostOnFacebook(string text, string imagePath, string pageAccessToken, string pageId)
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
        }
    }
}
