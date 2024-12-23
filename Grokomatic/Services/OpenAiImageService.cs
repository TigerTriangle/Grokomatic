using Grokomatic.Configs;
using OpenAI.Images;
using Tweetinvi.Core.Extensions;

namespace Grokomatic.Services
{
    public class OpenAiImageService
    {
        /// <summary>
        /// Generates an image based on the provided prompt and saves it to the specified file path.
        /// </summary>
        /// <param name="prompt">The text prompt to generate the image from.</param>
        /// <param name="appConfig">The application configuration containing the OpenAI API key and PNG file path.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task GenerateImage(string prompt, AppConfiguration appConfig)
        {
            if (string.IsNullOrEmpty(appConfig.PngFile)) throw new Exception("PngFile path must be specified.");

            ImageClient client = new("dall-e-3", appConfig.OpenAiApiKey);

            ImageGenerationOptions options = new()
            {
                Quality = GeneratedImageQuality.High,
                Size = GeneratedImageSize.W1024xH1024,
                Style = GeneratedImageStyle.Vivid,
                ResponseFormat = GeneratedImageFormat.Bytes
            };

            GeneratedImage image = await client.GenerateImageAsync(prompt, options);
            BinaryData bytes = image.ImageBytes;

            await File.WriteAllBytesAsync(appConfig.PngFile, bytes.ToArray());
        }
    }
}
