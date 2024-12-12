using Grokomatic.Models;
using Newtonsoft.Json;
using System.Text;
using Grokomatic.Services;
using Grokomatic;
using Tweetinvi;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Logger;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;

internal class Program
{

    private static IInstaApi InstaApi;
    private static string openAiApiKey;
    private static string grokApiKey;
    private static string basePath;
    private static string txtFile;
    private static string logFile;
    private static string pngFile;
    private static string jpgFile;
    private static string xConsumerKey;
    private static string xConsumerSecret;
    private static string xAccessToken;
    private static string xAccessTokenSecret;
    private static string userAccessToken;
    private static string pageAccessToken;
    private static string pageId;
    private static string instagramUserId;
    private static string instagramUsername;
    private static string instagramPassword;

    private static async Task Main(string[] args)
    {
        try
        {
            await Initialize();            

            string postText = GeneratePost();
            File.WriteAllText(txtFile, postText);

            string imagePrompt = GenerateImagePrompt(postText);

            await GenerateImage(imagePrompt, pngFile);

            Utilities.ConvertPngToJpg(pngFile, jpgFile, 80);

            // Post text and picture on X
            await PostOnX(postText, jpgFile);

            // Post text and picture on Facebook
            var fbPost = new FacebookService();
            await fbPost.PostOnFacebook(postText, jpgFile, pageAccessToken, pageId);

            // Post text and picture on Instagram
            await PostOnInstagram(postText, jpgFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        Console.WriteLine("Thanks for using Grokomatic!");
        Console.ReadLine();

        async Task Initialize()
        {
            openAiApiKey = Utilities.GetEnvironmentVariable("OPENAI_API_KEY");
            grokApiKey = Utilities.GetEnvironmentVariable("GROK_API_KEY");
            xConsumerKey = Utilities.GetEnvironmentVariable("X_API_KEY");
            xConsumerSecret = Utilities.GetEnvironmentVariable("X_API_SECRET");
            xAccessToken = Utilities.GetEnvironmentVariable("X_ACCESS_TOKEN");
            xAccessTokenSecret = Utilities.GetEnvironmentVariable("X_ACCESS_TOKEN_SECRET");
            userAccessToken = Utilities.GetEnvironmentVariable("FACEBOOK_USER_ACCESS_TOKEN");
            pageAccessToken = Utilities.GetEnvironmentVariable("FACEBOOK_PAGE_ACCESS_TOKEN");
            pageId = Utilities.GetEnvironmentVariable("FACEBOOK_PAGE_ID");
            instagramUserId = Utilities.GetEnvironmentVariable("INSTAGRAM_USER_ID");
            instagramUsername = Utilities.GetEnvironmentVariable("INSTAGRAM_USERNAME");
            instagramPassword = Utilities.GetEnvironmentVariable("INSTAGRAM_PASSWORD");

            basePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\Grokomatic";
            string fileName = DateTime.Now.ToString("yyyyMMddHHmmss");

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            logFile = Path.Combine(basePath, $"{fileName}.log");
            txtFile = Path.Combine(basePath, $"{fileName}.txt");
            pngFile = Path.Combine(basePath, $"{fileName}.png");
            jpgFile = Path.Combine(basePath, $"{fileName}.jpg");            
        }

        string GeneratePost()
        {
            string innovationsPath = "Data/Innovations.json";
            string completionsPath = "Data/Completions.json";

            List<TechInnovation> innovationsPool = Utilities.ReadJsonFile(innovationsPath);
            List<TechInnovation> previouslyPosted = Utilities.ReadJsonFile(completionsPath);
            List<TechInnovation> remainingInnovations = innovationsPool.Except(previouslyPosted).ToList();

            if (remainingInnovations.Count == 0)
            {
                Console.WriteLine("All innovations have been posted. No new posts available.");
                return string.Empty;
            }

            int randomNumber = Utilities.PickRandomNumber(remainingInnovations.Count - 1);
            TechInnovation selectedInnovation = remainingInnovations[randomNumber - 1];

            StringBuilder systemStringBuilder = new StringBuilder();
            systemStringBuilder.Append("You are a helpful assistant that creates social media post based on the user's request.");
            systemStringBuilder.Append("The post should be interesting, fun, accurate information");
            systemStringBuilder.Append("It should contain appropriate hashtags including #TigerTriangleTechnologies");
            systemStringBuilder.Append("Include fun facts. Feel free to strategically place an emoji where appropriate.");
            systemStringBuilder.Append("It should be ready to post as is and be in the format of a social media post with no additional instructions.");

            string userPrompt = $"Please generate text for a single post on social media about {selectedInnovation.Name} ({selectedInnovation.Description}) within the {selectedInnovation.Category} category.";

            string finePrint = "\n\nThis post was generated by AI as an experiment. It may contain errors or inaccuracies. Please feel free to fact check and/or leave a comment on how you think it did. Thanks!";

            var textGenerator = new GrokTextService();
            string textForPost = $"{textGenerator.GenerateText(systemStringBuilder.ToString(), userPrompt, grokApiKey)}{finePrint}";

            // Add selected innovation to the previouslyPosted list
            previouslyPosted.Add(selectedInnovation);

            // Write serialized list of previouslyPosted to completions file in JSON format
            File.WriteAllText(completionsPath, JsonConvert.SerializeObject(previouslyPosted, Formatting.Indented));

            Console.WriteLine("[ASSISTANT]");
            Console.WriteLine(textForPost);
            File.AppendAllText(logFile, textForPost);
            return textForPost;
        }

        string GenerateImagePrompt(string rawText)
        {
            StringBuilder systemStringBuilder = new StringBuilder();
            systemStringBuilder.Append("You are a helpful assistant that generates a single image prompt for an AI image generator.");
            systemStringBuilder.Append("It will based on text given by the user.");
            systemStringBuilder.Append("It will be passed directly to a model to generate the image.");

            var textGenerator = new GrokTextService();
            string imagePrompt = textGenerator.GenerateText(systemStringBuilder.ToString(), rawText, grokApiKey);

            Console.WriteLine("[ASSISTANT]");
            Console.WriteLine(imagePrompt);
            File.AppendAllText(logFile, imagePrompt);
            return imagePrompt;
        }

        async Task GenerateImage(string imagePrompt, string filePath)
        {
            if (string.IsNullOrEmpty(imagePrompt))
            {
                throw new ArgumentNullException(nameof(imagePrompt));
            }

            var imageGenerator = new OpenAiImageService();
            try
            {
                await imageGenerator.GenerateImage(imagePrompt, filePath, openAiApiKey);
                Console.WriteLine($"Image generated successfully and saved to {filePath}");
                File.AppendAllText(logFile, $"Image generated successfully and saved to {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while generating the image: {ex.Message}");
                File.AppendAllText(logFile, $"An error occurred while generating the image: {ex.Message}");
            }
        }

        async Task PostOnX(string text, string imagePath)
        {
            // Post text and picture on X
            var client = new TwitterClient(
                        xConsumerKey,
                        xConsumerSecret,
                        xAccessToken,
                        xAccessTokenSecret
                    );

            byte[] mediaData = File.ReadAllBytes(imagePath);

            dynamic? uploadedMedia = null;
            if (imagePath.EndsWith("mp4"))
            {
                uploadedMedia = await client.Upload.UploadTweetVideoAsync(mediaData.ToArray());
            }
            else
            {
                uploadedMedia = await client.Upload.UploadTweetImageAsync(mediaData.ToArray());
            }

            // Check if the image upload was successful
            if (uploadedMedia != null)
            {
                var xPostRequest = new XPostRequest
                {
                    Text = text,
                    media = new mediaIDS()
                    {
                        media_ids = [uploadedMedia.Id.ToString()],
                    },

                };

                var poster = new XService(client);
                // Send tweet request to Twitter API
                var result = await poster.PostX(xPostRequest);

                // Check if the tweet was successfully posted
                if (result.Response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"You published the x post. {result.Content}");
                    File.AppendAllText(logFile, $"You published the x post. {result.Content}");
                }
                else
                {
                    Console.WriteLine($"Error when posting x post: {result.Content}");
                    File.AppendAllText(logFile, $"Error when posting x post: {result.Content}");
                }
            }
            else
            {
                Console.WriteLine("Error when uploading image.");
                File.AppendAllText(logFile, "Error when uploading image.");
            }
        }

        async Task PostOnInstagram(string text, string imagePath)
        {
            // create user session data and provide login details
            var userSession = new UserSessionData
            {
                UserName = instagramUsername,
                Password = instagramPassword
            };

            var delay = RequestDelay.FromSeconds(2, 2);
            // create new InstaApi instance using Builder
            InstaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.All)) // use logger for requests and debug messages
                .SetRequestDelay(delay)
                .Build();

            string stateFile = Path.Combine(basePath, "state.bin");
            try
            {
                if (File.Exists(stateFile))
                {
                    Console.WriteLine("Loading state from file");
                    using (var fs = File.OpenRead(stateFile))
                    {
                        InstaApi.LoadStateDataFromString(new StreamReader(fs).ReadToEnd());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                File.AppendAllText(logFile, $"Error when writing to state file: {e.Message}");
            }

            if (!InstaApi.IsUserAuthenticated)
            {
                // login
                Console.WriteLine($"Logging in as {userSession.UserName}");
                //delay.Disable();
                var logInResult = await InstaApi.LoginAsync();
                //delay.Enable();

                if (logInResult.Succeeded)
                {
                    Console.WriteLine("App connected.");
                    // Save session 
                    SaveSession(stateFile);
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
                        var twoFactorLogin = await InstaApi.TwoFactorLoginAsync(verificationCode);
                        if (twoFactorLogin.Succeeded)
                        {
                            // connected
                            // save session
                            SaveSession(stateFile);
                            Console.WriteLine("App connected.");
                        }
                        else
                        {
                            Console.WriteLine($"Error: {twoFactorLogin.Info.Message}");
                            File.AppendAllText(logFile, $"Instagram 2FA Error: {twoFactorLogin.Info.Message}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"App connected.");
            }            

            var igPost = new InstagramService(InstaApi);
            await igPost.PostOnInstagram(text, imagePath);
        }

        void SaveSession(string stateFile)
        {
            var state = InstaApi.GetStateDataAsString();
            File.WriteAllText(stateFile, state);
        }
    }
}
