using Grokomatic.Models;
using Newtonsoft.Json;
using OpenAI.Chat;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Grokomatic
{
    static class Utilities
    {
        /// <summary>
        /// Retrieves the value of the specified environment variable.
        /// </summary>
        /// <param name="variableName">The name of the environment variable to retrieve.</param>
        /// <returns>The value of the specified environment variable.</returns>
        /// <exception cref="Exception">Thrown when the specified environment variable is not found or is empty.</exception>
        public static string GetEnvironmentVariable(string variableName)
        {
            string? variable = Environment.GetEnvironmentVariable(variableName);
            if (string.IsNullOrEmpty(variable))
            {
                throw new Exception($"{variableName} environment variable not found.");
            }
            return variable;
        }

        /// <summary>
        /// Reads a JSON file from the specified file path and deserializes it into a list of TechInnovation objects.
        /// </summary>
        /// <param name="filePath">The path to the JSON file to read.</param>
        /// <returns>A list of TechInnovation objects deserialized from the JSON file.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the specified file is not found.</exception>
        public static List<TechInnovation> ReadJsonFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file {filePath} was not found.");
            }

            string jsonContent = File.ReadAllText(filePath);

            // Deserialize the JSON into a list of TechInnovation objects
            List<TechInnovation> innovations = JsonConvert.DeserializeObject<List<TechInnovation>>(jsonContent) ?? [];

            return innovations;
        }

        /// <summary>
        /// Picks a random number between 1 and the specified maximum value, inclusive.
        /// </summary>
        /// <param name="max">The maximum value for the random number generation.</param>
        /// <returns>A random integer where 1 <= return value <= max</returns>
        /// <exception cref="ArgumentException">Thrown when max is less than 1.</exception>
        public static int PickRandomNumber(int max)
        {
            if (max < 1)
            {
                throw new ArgumentException("The maximum value must be at least 1.");
            }

            // Create a new Random object. Note: For better randomness in production, consider 
            // using a single Random instance or provide a seed to avoid predictable sequences.
            Random random = new Random();

            // Next returns a number between 0 and max-1, so we add 1 to shift our range to 1-max
            return random.Next(1, max + 1);
        }

        /// <summary>
        /// Converts a PNG file to a JPG file with specified quality.
        /// </summary>
        /// <param name="inputFilePath">The path to the input PNG file.</param>
        /// <param name="outputFilePath">The path to save the output JPG file.</param>
        /// <param name="quality">The quality of the output JPG file (0-100).</param>
        public static void ConvertPngToJpg(string inputFilePath, string outputFilePath, int quality)
        {
            if (!File.Exists(inputFilePath))
            {
                throw new FileNotFoundException($"The file {inputFilePath} was not found.");
            }

            using (Image image = Image.Load(inputFilePath))
            {
                var encoder = new JpegEncoder
                {
                    Quality = quality
                };

                image.Save(outputFilePath, encoder);
            }
        }


        /// <summary>
        /// Removes Markdown formatting from the given text.
        /// </summary>
        /// <param name="text">The text to be processed.</param>
        /// <returns>A string with Markdown formatting removed.</returns>
        public static string RemoveMarkdownFormatting(string text)
        {
            return text.Replace("**", "").Replace("__", "");
        }
    }
}
