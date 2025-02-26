﻿using Grokomatic.Models;
using Newtonsoft.Json;
using System.Text;
using Serilog;
using Grokomatic.Configs;

namespace Grokomatic.Services
{
    public class AiGeneratorService
    {
        private readonly TextService _grokTextService;
        private readonly ImageService _imageService;

        public AiGeneratorService(TextService textService, ImageService imageService)
        {
            _grokTextService = textService;
            _imageService = imageService;
        }

        /// <summary>
        /// Generates a social media post text based on a random innovation from the innovations pool.
        /// </summary>
        /// <param name="aiConfig">Configuration for the OpenAI compatible API.</param>
        /// <returns>The generated social media post text.</returns>
        /// <exception cref="Exception">Thrown when the ApiKey is null.</exception>
        public string GeneratePostText(OpenAiConfig aiConfig)
        {
            string innovationsPath = "Data/Innovations.json";
            string completionsPath = "Data/Completions.json";

            List<TechInnovation> innovationsPool = Utilities.ReadJsonFile(innovationsPath);
            List<TechInnovation> previouslyPosted = Utilities.ReadJsonFile(completionsPath);
            List<TechInnovation> remainingInnovations = innovationsPool.Except(previouslyPosted).ToList();

            if (remainingInnovations.Count == 0)
            {
                Log.Logger.Information("All innovations have been posted. No new posts available.");
                return string.Empty;
            }

            int randomNumber = Utilities.PickRandomNumber(remainingInnovations.Count);
            TechInnovation selectedInnovation = remainingInnovations[randomNumber - 1];

            StringBuilder systemStringBuilder = new StringBuilder();
            systemStringBuilder.Append("You are a helpful assistant that creates social media post based on the user's request.");
            systemStringBuilder.Append("You are sarcastic and funny, including humor and wit that makes people smile.");
            systemStringBuilder.Append("The post should be interesting, fun, accurate information");
            systemStringBuilder.Append("It should contain appropriate hashtags including #TigerTriangleTechnologies");
            systemStringBuilder.Append("Include fun facts. Feel free to strategically place an emoji where appropriate.");
            systemStringBuilder.Append("It should be ready to post as is and be in the format of a social media post with no additional instructions.");

            Log.Logger.Information("[SYSTEM] {0}", systemStringBuilder.ToString());

            string userPrompt = $"Please generate text for a single post on social media about {selectedInnovation.Name} ({selectedInnovation.Description}) within the {selectedInnovation.Category} category.";

            Log.Logger.Information("[USER] {0}", userPrompt);

            string finePrint = "\n\nThis post was generated by AI as an experiment. It may contain errors or inaccuracies. Please feel free to fact check and/or leave a comment on how you think it did. Thanks!";

            string textForPost = $"{_grokTextService.GenerateText(systemStringBuilder.ToString(), userPrompt, aiConfig)}{finePrint}";

            // Add selected innovation to the previouslyPosted list
            previouslyPosted.Add(selectedInnovation);

            // Write serialized list of previouslyPosted to completions file in JSON format
            File.WriteAllText(completionsPath, JsonConvert.SerializeObject(previouslyPosted, Formatting.Indented));

            Log.Logger.Information("[ASSISTANT] {0}", textForPost);

            return textForPost;
        }

        /// <summary>
        /// Generates an image prompt for an AI image generator based on the provided raw text.
        /// </summary>
        /// <param name="rawText">The raw text to base the image prompt on.</param>
        /// <param name="aiConfig">Configuration for the OpenAI compatible API.</param>
        /// <returns>The generated image prompt.</returns>
        /// <exception cref="Exception">Thrown when the ApiKey is null.</exception>
        public string GenerateImagePrompt(string rawText, OpenAiConfig aiConfig)
        {

            StringBuilder systemStringBuilder = new StringBuilder();
            systemStringBuilder.Append("You are a helpful assistant that generates a single image prompt for an AI image generator.");
            systemStringBuilder.Append("It will based on text given by the user.");
            systemStringBuilder.Append("It will be passed directly to a model to generate the image.");

            Log.Logger.Information("[SYSTEM] {0}", systemStringBuilder.ToString());

            string imagePrompt = _grokTextService.GenerateText(systemStringBuilder.ToString(), rawText, aiConfig);
            
            Log.Logger.Information("[ASSISTANT] {0}", imagePrompt);

            return imagePrompt;
        }

        
        /// <summary>
        /// Generates an image based on the provided image prompt and saves it to the specified file path.
        /// </summary>
        /// <param name="imagePrompt">The prompt text to generate the image.</param>
        /// <param name="aiConfig">Configuration for the OpenAI compatible API.</param>
        /// <param name="pngFilePath">The file path where the generated image will be saved.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the image prompt or file path is null or empty.</exception>
        /// <exception cref="Exception">Thrown when an error occurs during image generation.</exception>
        public async Task GenerateImage(string imagePrompt, OpenAiConfig aiConfig, string pngFilePath)
        {
            if (string.IsNullOrEmpty(imagePrompt))
            {
                throw new ArgumentNullException(nameof(imagePrompt), "Image prompt is required.");
            }

            if (string.IsNullOrEmpty(pngFilePath))
            {
                throw new ArgumentNullException(nameof(pngFilePath), "Image file path is required.");
            }

            try
            {
                await _imageService.GenerateImage(imagePrompt, aiConfig, pngFilePath);

                Log.Logger.Information("Image generated successfully and saved to {0}", pngFilePath);
            }
            catch (Exception ex)
            {
                Log.Logger.Error("An error occurred while generating the image: {0}", ex.Message);
                Utilities.AppStatus = 1;
            }
        }
    }
}
