using Grokomatic.Models;
using Newtonsoft.Json;
using OpenAI.Chat;
using OpenAI;
using System.Text;
using System.ClientModel;
using System.Diagnostics.Metrics;

string innovationsPath = "Data/Innovations.json";
string completionsPath = "Data/Completions.json";

try
{
    GenerateText(innovationsPath, completionsPath);

    // Generate picture
    // Post text and picture on X
    // Post text and picture on Facebook
    // Append selected innovation to list of completions and write to Completions file
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}

static void GenerateText(string innovationsPath, string completionsPath)
{
    List<TechInnovation> innovationsPool = ReadJsonFile(innovationsPath);
    List<TechInnovation> previouslyPosted = ReadJsonFile(completionsPath);
    List<TechInnovation> remainingInnovations = innovationsPool.Except(previouslyPosted).ToList();

    if (remainingInnovations.Count > 0)
    {
        int randomNumber = PickRandomNumber(remainingInnovations.Count - 1);
        TechInnovation selectedInnovation = remainingInnovations[randomNumber - 1];

        StringBuilder systemStringBuilder = new StringBuilder();
        systemStringBuilder.Append("You are a helpful assistant that creates social media post based on the user's request.");
        systemStringBuilder.Append("The post should be interesting, fun, accurate information");
        systemStringBuilder.Append("This is for Tiger Triangle Technology, a creator of tutorials, news and commentary related to subjects like AI, .NET, and the maker movement.");
        systemStringBuilder.Append("It should contain appropriate hashtags including #TigerTriangleTechnology");
        systemStringBuilder.Append("Include fun facts. Feel free to strategically place an emoji where appropriate.");
        systemStringBuilder.Append("It should be ready to post as is and be in the format of a social media post with no additional instructions.");

        string userPrompt = $"Please generate text for a single post on social media about {selectedInnovation.Category}.";

        OpenAIClientOptions options = new OpenAIClientOptions();
        options.Endpoint = new Uri("https://api.x.ai/v1");
        ChatClient client = new(model: "grok-beta", new ApiKeyCredential(Environment.GetEnvironmentVariable("GROK_API_KEY")), options);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemStringBuilder.ToString()),
            new UserChatMessage(userPrompt)
        };

        //The sampling temperature, between 0 and 1. Higher values like 0.8 will make the output more random,
        //while lower values like 0.2 will make it more focused and deterministic.
        ChatCompletionOptions completionOptions = new()
        {
            Temperature = 0.8f
        };

        // Generate text
        ChatCompletion completion = client.CompleteChat(messages, completionOptions);

        // Remove markdown formatting from the text
        string textForPost = completion.Content[0].Text.Replace("**", "").Replace("__", "");

        // Add selected innovation to the previouslyPosted list
        previouslyPosted.Add(selectedInnovation);

        // Write serialized list of previouslyPosted to completions file in JSON format
        File.WriteAllText(completionsPath, JsonConvert.SerializeObject(previouslyPosted, Formatting.Indented));

        Console.WriteLine("[ASSISTANT]");
        Console.WriteLine(textForPost);
    }
}

/// <summary>
/// Reads a JSON file from the specified file path and deserializes it into a list of TechInnovation objects.
/// </summary>
/// <param name="filePath">The path to the JSON file to read.</param>
/// <returns>A list of TechInnovation objects deserialized from the JSON file.</returns>
/// <exception cref="FileNotFoundException">Thrown when the specified file is not found.</exception>
static List<TechInnovation> ReadJsonFile(string filePath)
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
static int PickRandomNumber(int max)
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