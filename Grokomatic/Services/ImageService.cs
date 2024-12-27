using Grokomatic.Configs;
using OpenAI.Images;
using Tweetinvi.Core.Extensions;

namespace Grokomatic.Services
{
    public class ImageService
    {
        /// <summary>
        /// Generates an image based on the provided prompt using an OpenAI compatible API.
        /// </summary>
        /// <param name="imagePrompt">The prompt to generate the image from.</param>
        /// <param name="aiConfig">The configuration for the OpenAI compatible API.</param>
        /// <param name="pngFilePath">The file path where the generated image will be saved.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="aiConfig.ApiKey"/>, <paramref name="imagePrompt"/>, or <paramref name="pngFilePath"/> is null or empty.
        /// </exception>
        public async Task GenerateImage(string imagePrompt, IAiConfig aiConfig, string pngFilePath)
        {
            if (string.IsNullOrEmpty(aiConfig.ApiKey)) throw new ArgumentNullException(nameof(aiConfig.ApiKey));
            if (string.IsNullOrEmpty(aiConfig.ImageModel)) throw new ArgumentNullException(nameof(aiConfig.ImageModel));
            if (string.IsNullOrEmpty(imagePrompt)) throw new ArgumentNullException(nameof(imagePrompt), "Image prompt is required.");
            if (string.IsNullOrEmpty(pngFilePath)) throw new ArgumentNullException(nameof(pngFilePath), "Image file path is required.");

            ImageClient client = new(aiConfig.ImageModel, aiConfig.ApiKey);

            ImageGenerationOptions options = new()
            {
                Quality = GeneratedImageQuality.High,
                Size = GeneratedImageSize.W1024xH1024,
                Style = GeneratedImageStyle.Vivid,
                ResponseFormat = GeneratedImageFormat.Bytes
            };

            GeneratedImage image = await client.GenerateImageAsync(imagePrompt, options);
            BinaryData bytes = image.ImageBytes;

            await File.WriteAllBytesAsync(pngFilePath, bytes.ToArray());
        }
    }
}
