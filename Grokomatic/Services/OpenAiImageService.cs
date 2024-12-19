using OpenAI.Images;

namespace Grokomatic.Services
{
    public class OpenAiImageService
    {
        /// <summary>
        /// Generates an image based on the provided prompt and saves it to the specified file path.
        /// </summary>
        /// <param name="prompt">The text prompt to generate the image from.</param>
        /// <param name="filePath">The file path where the generated image will be saved.</param>
        /// <param name="apiKey">The API key used to authenticate with the OpenAI service.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task GenerateImage(string prompt, string filePath, string apiKey)
        {
            ImageClient client = new("dall-e-3", apiKey);

            ImageGenerationOptions options = new()
            {
                Quality = GeneratedImageQuality.High,
                Size = GeneratedImageSize.W1024xH1024,
                Style = GeneratedImageStyle.Vivid,
                ResponseFormat = GeneratedImageFormat.Bytes
            };

            GeneratedImage image = await client.GenerateImageAsync(prompt, options);
            BinaryData bytes = image.ImageBytes;

            await File.WriteAllBytesAsync(filePath, bytes.ToArray());
        }
    }
}
