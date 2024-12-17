using System.Text;
using Tweetinvi.Models;
using Tweetinvi;
using Grokomatic.Models;
using Serilog;
using Grokomatic.Configs;

namespace Grokomatic.Services
{
    public class XService
    {
        public async Task PostOnX(SocialPost socialPost, AppConfiguration appConfig)
        {
            // Post text and picture on X
            var client = new TwitterClient(
                        appConfig.XConsumerKey,
                        appConfig.XConsumerSecret,
                        appConfig.XAccessToken,
                        appConfig.XAccessTokenSecret);

            byte[] mediaData = File.ReadAllBytes(socialPost.PostImage);

            dynamic? uploadedMedia = null;
            if (socialPost.PostImage.EndsWith("mp4"))
            {
                uploadedMedia = await client.Upload.UploadTweetVideoAsync(mediaData.ToArray());
            }
            else
            {
                uploadedMedia = await client.Upload.UploadTweetImageAsync(mediaData.ToArray());
            }

            // Check if the image upload was successful
            if (uploadedMedia == null)
            {
                Log.Logger.Error("Error when uploading image.");
            }

            var xPostRequest = new XPostRequest
            {
                Text = socialPost.PostText,
                media = new mediaIDS()
                {
                    media_ids = [uploadedMedia.Id.ToString()],
                },

            };

            var jsonBody = client.Json.Serialize(xPostRequest);
            var result = await client.Execute.AdvanceRequestAsync((ITwitterRequest request) =>
            {
                request.Query.Url = "https://api.twitter.com/2/tweets";
                request.Query.HttpMethod = Tweetinvi.Models.HttpMethod.POST;
                request.Query.HttpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            });

            if (result.Response.IsSuccessStatusCode)
            {
                Log.Logger.Information("You published the x post. {0}", result.Content);
            }
            else
            {
                Log.Logger.Error("Error when posting x post: {0}", result.Content);
            }
        }
    }
}
