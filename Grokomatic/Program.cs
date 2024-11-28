using Grokomatic.Models;
using Newtonsoft.Json;
using System.Text;
using Grokomatic.Services;
using Grokomatic;

string openAiApiKey = Utilities.GetEnvironmentVariable("OPENAI_API_KEY");
string grokApiKey = Utilities.GetEnvironmentVariable("GROK_API_KEY");

try
{
    string postText = GeneratePost();

    string imagePrompt = GenerateImagePrompt(postText);

    string fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
    string pngPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\AI Art\Grokomatic\{fileName}.png";
    string jpgPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\AI Art\Grokomatic\{fileName}.jpg";
    await GenerateImage(imagePrompt, pngPath);

    Utilities.ConvertPngToJpg(pngPath, jpgPath, 80);

    // Post text and picture on X
    // Post text and picture on Facebook
    // Append selected innovation to list of completions and write to Completions file
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
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
    systemStringBuilder.Append("This is for Tiger Triangle Technology, a creator of tutorials, news and commentary related to subjects like AI, .NET, and the maker movement.");
    systemStringBuilder.Append("It should contain appropriate hashtags including #TigerTriangleTechnology");
    systemStringBuilder.Append("Include fun facts. Feel free to strategically place an emoji where appropriate.");
    systemStringBuilder.Append("It should be ready to post as is and be in the format of a social media post with no additional instructions.");

    string userPrompt = $"Please generate text for a single post on social media about {selectedInnovation.Category}.";

    var textGenerator = new GrokTextService();
    string textForPost = textGenerator.GenerateText(systemStringBuilder.ToString(), userPrompt, grokApiKey);

    // Add selected innovation to the previouslyPosted list
    previouslyPosted.Add(selectedInnovation);

    // Write serialized list of previouslyPosted to completions file in JSON format
    File.WriteAllText(completionsPath, JsonConvert.SerializeObject(previouslyPosted, Formatting.Indented));

    Console.WriteLine("[ASSISTANT]");
    Console.WriteLine(textForPost);
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
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while generating the image: {ex.Message}");
    }
}

