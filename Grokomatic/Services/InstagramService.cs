using Grokomatic.Configs;
using Grokomatic.Models;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Logger;
using Serilog;
using Tweetinvi.Core.Extensions;

namespace Grokomatic.Services
{
    public class InstagramService
    {
        /// <summary>
        /// Posts a social media post on Instagram.
        /// </summary>
        /// <param name="socialPost">The social post containing the text and image to be posted.</param>
        /// <param name="instagramConfig">The configuration containing Instagram credentials and other settings.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="Exception">Thrown when the basePath is null or empty.</exception>
        public async Task PostOnInstagram(SocialPost socialPost, InstagramConfig instagramConfig, string basePath)
        {
            if (string.IsNullOrEmpty(basePath))
            {
                throw new ArgumentNullException(nameof(basePath), "BasePath is required.");
            }

            if (socialPost.PostImage.IsNullOrEmpty())
            {
                Log.Logger.Error("Image is required for Instagram.");
                Utilities.AppStatus = 1;
                return;
            }

            // create user session data and provide login details
            var userSession = new UserSessionData
            {
                UserName = instagramConfig.Username,
                Password = instagramConfig.Password
            };

            var delay = RequestDelay.FromSeconds(2, 2);
            // create new InstaApi instance using Builder
            IInstaApi instaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.Exceptions))
                .SetRequestDelay(delay)
                .Build();

            string stateFile = Path.Combine(basePath, "state.bin");
            try
            {
                if (File.Exists(stateFile))
                {
                    Log.Logger.Information("Instagram is loading state from file");
                    using (var fs = File.OpenRead(stateFile))
                    {
                        instaApi.LoadStateDataFromString(new StreamReader(fs).ReadToEnd());
                    }
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Error when reading from state file");
                throw;
            }

            if (!instaApi.IsUserAuthenticated)
            {
                Log.Logger.Information("Logging in as {0}", userSession.UserName);
                var logInResult = await instaApi.LoginAsync();

                if (logInResult.Succeeded)
                {
                    Log.Logger.Information("Instagram App connected.");
                    SaveSession(instaApi, stateFile);
                }
                else
                {
                    // two factor is required
                    if (logInResult.Value == InstaLoginResult.TwoFactorRequired)
                    {
                        // get input from keyboard
                        Console.WriteLine("Please type your verification code and press enter:");
                        var verificationCode = Console.ReadLine();

                        // send two factor code
                        var twoFactorLogin = await instaApi.TwoFactorLoginAsync(verificationCode);
                        if (twoFactorLogin.Succeeded)
                        {
                            SaveSession(instaApi, stateFile);
                            Log.Logger.Information("Instagram App connected.");
                        }
                        else
                        {
                            Log.Logger.Error("Error: {0}", twoFactorLogin.Info.Message);
                            Utilities.AppStatus = 1;
                        }
                    }
                }
            }
            else
            {
                Log.Logger.Information("Instagram App connected.");
            }

            var mediaImage = new InstaImageUpload
            {
                // leave zero if dimensions are unknown
                Height = 0,
                Width = 0,
                Uri = socialPost.PostImage
            };

            IResult<InstaMedia> result = await instaApi.MediaProcessor.UploadPhotoAsync(mediaImage, socialPost.PostText);

            if (result.Succeeded)
            {
                Log.Logger.Information("Media created: {0}", result.Info.Message);
            }
            else
            {
                Log.Logger.Error("Unable to upload photo: {0}", result.Info.Message);
                Utilities.AppStatus = 1;
            }
        }

        void SaveSession(IInstaApi instaApi, string stateFile)
        {
            var state = instaApi.GetStateDataAsString();
            File.WriteAllText(stateFile, state);
        }
    }
}
