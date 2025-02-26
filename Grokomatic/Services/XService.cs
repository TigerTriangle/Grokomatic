﻿using System.Text;
using Tweetinvi.Models;
using Tweetinvi;
using Grokomatic.Models;
using Serilog;
using Grokomatic.Configs;
using Tweetinvi.Core.Extensions;

namespace Grokomatic.Services
{
    public class XService
    {
        /// <summary>
        /// Posts text and picture on X (formerly Twitter).
        /// </summary>
        /// <param name="socialPost">The social post containing text and image to be posted.</param>
        /// <param name="xConfig">The configuration containing API keys and tokens for X.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task PostOnX(SocialPost socialPost, XConfig xConfig)
        {
            // Post text and picture on X
            var client = new TwitterClient(
                        xConfig.ConsumerKey,
                        xConfig.ConsumerSecret,
                        xConfig.AccessToken,
                        xConfig.AccessTokenSecret);

            var xPostRequest = new XPostRequest
            {
                Text = socialPost.PostText
            };

            if (!socialPost.PostImage.IsNullOrEmpty())
            {
                byte[] mediaData = File.ReadAllBytes(socialPost.PostImage);

                dynamic? uploadedMedia = null;
                if (socialPost.PostImage.EndsWith("mp4"))
                {
                    uploadedMedia = await client.Upload.UploadTweetVideoAsync(mediaData);
                }
                else
                {
                    uploadedMedia = await client.Upload.UploadTweetImageAsync(mediaData);
                }

                // Check if the image upload was successful
                if (uploadedMedia == null)
                {
                    Log.Logger.Error("Error when uploading image.");
                    throw new Exception("Error when uploading image.");
                }

                xPostRequest.media = new mediaIDS()
                {
                    media_ids = [uploadedMedia.Id.ToString()],
                };
            }

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
                Utilities.AppStatus = 1;
            }
        }
    }
}
