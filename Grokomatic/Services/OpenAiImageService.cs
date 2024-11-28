using OpenAI.Images;

namespace Grokomatic.Services
{
    public class OpenAiImageService
    {
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
