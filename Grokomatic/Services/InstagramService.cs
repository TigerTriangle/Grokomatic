using Grokomatic.Configs;
using Grokomatic.Models;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Logger;
using Serilog;

namespace Grokomatic.Services
{
    public class InstagramService
    {
        public async Task PostOnInstagram(SocialPost socialPost, AppConfiguration appConfig)
        {
            if (appConfig.BasePath == null) throw new Exception("BasePath is null.");

            // create user session data and provide login details
            var userSession = new UserSessionData
            {
                UserName = appConfig.InstagramUsername,
                Password = appConfig.InstagramPassword
            };

            var delay = RequestDelay.FromSeconds(2, 2);
            // create new InstaApi instance using Builder
            IInstaApi instaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.All)) // use logger for requests and debug messages
                .SetRequestDelay(delay)
                .Build();

            string stateFile = Path.Combine(appConfig.BasePath, "state.bin");
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
            }
        }

        void SaveSession(IInstaApi instaApi, string stateFile)
        {
            var state = instaApi.GetStateDataAsString();
            File.WriteAllText(stateFile, state);
        }
    }
}
