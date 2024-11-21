using Grokomatic.Models;
using Newtonsoft.Json;

string innovationsPath = "Data/Innovations.json";
string completionsPath = "Data/Completions.json";

try
{
    List<TechInnovation> innovations = ReadJsonFile(innovationsPath);
    List<TechInnovation> completions = ReadJsonFile(completionsPath);
    List<TechInnovation> remainingInnovations = innovations.Except(completions).ToList();

    // Print all innovations
    foreach (var innovation in remainingInnovations)
    {
        Console.WriteLine(innovation.ToString());
    }

    if (remainingInnovations.Count > 0)
    {
        int randomNumber = PickRandomNumber(remainingInnovations.Count - 1);
        TechInnovation selectedInnovation = remainingInnovations[randomNumber - 1];
        
        // Generate text
        // Generate picture
        // Post text and picture on X
        // Post text and picture on Facebook
        // Append selected innovation to list of completions and write to Completions file

        Console.WriteLine("Selected Tech Innovation");
        Console.WriteLine(selectedInnovation.ToString());
    }    
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}

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