using Grokomatic.Models;
using Grokomatic.Services;
using Microsoft.Extensions.Configuration;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Grokomatic.Configs;
using Grokomatic;

string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
const int MAX_RETRIES = 3;

#region Configuration

// Setup configuration, services, and logging
IConfigurationRoot configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{environment}.json", true)
        .AddEnvironmentVariables()
        .AddUserSecrets<Program>()
        .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

var services = new ServiceCollection();
ConfigureServices(services, configuration);

var serviceProvider = services.BuildServiceProvider();

AppConfig appConfig = new();
OpenAiConfig aiImageConfig = new();
OpenAiConfig aiTextConfig = new();
XConfig xConfig = new();
FacebookConfig facebookConfig = new();
InstagramConfig instagramConfig = new();
Initialize();
#endregion

Console.WriteLine("Welcome to Grokomatic\r");
Console.WriteLine("Generate posts about tech innovations.\r");
Console.WriteLine("-----------------------------------\n");
Console.WriteLine("To run this in batch mode pass batch as an argument.");
Console.WriteLine("Example: Grokomatic.exe batch");

#region BatchOrInteractiveMode
try
{
    if (args.Length > 0)
    {
        foreach (var arg in args)
        {
            Log.Logger.Information($"Argument={arg} was passed in.");
            if (arg.Equals("batch", StringComparison.OrdinalIgnoreCase))
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var scopedServiceProvider = scope.ServiceProvider;
                    await PostOnAllPlatforms(scopedServiceProvider);
                    break;
                }
            }
            else
            {
                Log.Logger.Error($"Invalid argument={arg} was passed in.");
                Utilities.AppStatus = 1;
            }
        }
    }
    else
    {
        await ChooseModel();
        await ShowMenu();
    }
}
catch (Exception ex)
{
    Log.Logger.Error(ex, "An unhandled exception occurred.");
    Utilities.AppStatus = 1;
}
finally
{
    Log.CloseAndFlush();
}
#endregion

Environment.Exit(Utilities.AppStatus);

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddScoped<AiGeneratorService>();
    services.AddScoped<FacebookService>();
    services.AddScoped<TextService>();
    services.AddScoped<InstagramService>();
    services.AddScoped<ImageService>();
    services.AddScoped<XService>();
    services.AddSingleton(configuration);
}

void Initialize()
{
    appConfig.BasePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\Grokomatic";

    string fileName = DateTime.Now.ToString("yyyyMMddHHmmss");

    if (!Directory.Exists(appConfig.BasePath))
    {
        Directory.CreateDirectory(appConfig.BasePath);
    }

    appConfig.TxtFile = Path.Combine(appConfig.BasePath, $"{fileName}.txt");
    appConfig.PngFile = Path.Combine(appConfig.BasePath, $"{fileName}.png");
    appConfig.JpgFile = Path.Combine(appConfig.BasePath, $"{fileName}.jpg");

    configuration.GetSection("DalleConfig").Bind(aiImageConfig);
    configuration.GetSection("XConfig").Bind(xConfig);
    configuration.GetSection("FacebookConfig").Bind(facebookConfig);
    configuration.GetSection("InstagramConfig").Bind(instagramConfig);
}

async Task ChooseModel()
{
    // We will pull in the configuration for the AI model from appsettings.json and user secrets
    // based on the user's choice of model
    Console.Clear();
    Console.WriteLine("Which model do you want to use to generate text?");
    Console.WriteLine("----------------");
    Console.WriteLine("1. Grok");
    Console.WriteLine("2. OpenAI");
    Console.WriteLine("3. Ollama");
    Console.WriteLine("");
    Console.WriteLine("Please choose an option from the menu:");

    int i = 0;

    using (var scope = serviceProvider.CreateScope())
    {
        var scopedServiceProvider = scope.ServiceProvider;

        while (i <= MAX_RETRIES)
        {
            string? userChoice = Console.ReadLine();
            if (userChoice == null)
            {
                Console.WriteLine("Invalid choice. Please try again.");
                continue;
            }

            switch (userChoice)
            {
                case "1":
                    configuration.GetSection("GrokConfig").Bind(aiTextConfig);
                    i = MAX_RETRIES + 1;
                    break;
                case "2":
                    configuration.GetSection("OpenAiConfig").Bind(aiTextConfig);
                    i = MAX_RETRIES + 1;
                    break;
                case "3":
                    configuration.GetSection("OllamaConfig").Bind(aiTextConfig);
                    i = MAX_RETRIES + 1;
                    break;                
                default:
                    i++;
                    if (i > MAX_RETRIES)
                    {
                        Console.WriteLine("Max retries. Program exiting in 3 seconds.");
                        Console.WriteLine("\n");
                        await Task.Delay(3000);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice. Please try again.");
                        Console.WriteLine("\n");
                    }
                    break;
            }
        }
    }
}

async Task ShowMenu()
{
    Console.Clear();
    Console.WriteLine("1. Post Tech Innovation (all platforms)");
    Console.WriteLine("2. Generate Post Text");
    Console.WriteLine("3. Generate Post Text with Image");
    Console.WriteLine("4. Post to X");
    Console.WriteLine("5. Post to Facebook");
    Console.WriteLine("6. Post to Instagram");
    Console.WriteLine("");
    Console.WriteLine("Please choose an option from the menu:");

    int i = 0;

    using (var scope = serviceProvider.CreateScope())
    {
        var scopedServiceProvider = scope.ServiceProvider;

        while (i <= MAX_RETRIES)
        {
            string? userChoice = Console.ReadLine();
            if (userChoice == null)
            {
                Console.WriteLine("Invalid choice. Please try again.");
                continue;
            }

            switch (userChoice)
            {
                case "1":
                    await PostOnAllPlatforms(scopedServiceProvider);
                    i = MAX_RETRIES + 1;
                    break;
                case "2":
                    await GeneratePost(scopedServiceProvider, false);
                    i = MAX_RETRIES + 1;
                    break;
                case "3":
                    await GeneratePost(scopedServiceProvider, true);
                    i = MAX_RETRIES + 1;
                    break;
                case "4":
                    var socialPost = await GeneratePost(scopedServiceProvider, true);
                    await scopedServiceProvider.GetRequiredService<XService>().PostOnX(socialPost, xConfig);
                    i = MAX_RETRIES + 1;
                    break;
                case "5":
                    var socialPost2 = await GeneratePost(scopedServiceProvider, true);
                    await scopedServiceProvider.GetRequiredService<FacebookService>().PostOnFacebook(socialPost2, facebookConfig);
                    i = MAX_RETRIES + 1;
                    break;
                case "6":
                    var socialPost3 = await GeneratePost(scopedServiceProvider, true);
                    await scopedServiceProvider.GetRequiredService<InstagramService>().PostOnInstagram(socialPost3, instagramConfig, appConfig.BasePath!);
                    i = MAX_RETRIES + 1;
                    break;
                default:
                    i++;
                    if (i > MAX_RETRIES)
                    {
                        Console.WriteLine("Max retries. Program exiting in 3 seconds.");
                        Console.WriteLine("\n");
                        await Task.Delay(3000);
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Invalid choice. Please try again.");
                        Console.WriteLine("\n");
                    }
                    break;
            }
        }
    }
}

async Task PostOnAllPlatforms(IServiceProvider scopedServiceProvider)
{
    var socialPost = await GeneratePost(scopedServiceProvider, true);
    if (string.IsNullOrEmpty(socialPost.PostText))
    {
        return;
    }

    await scopedServiceProvider.GetRequiredService<XService>().PostOnX(socialPost, xConfig);
    await scopedServiceProvider.GetRequiredService<FacebookService>().PostOnFacebook(socialPost, facebookConfig);
    await scopedServiceProvider.GetRequiredService<InstagramService>().PostOnInstagram(socialPost, instagramConfig, appConfig.BasePath!);
}

async Task<SocialPost> GeneratePost(IServiceProvider scopedServiceProvider, bool includeImage)
{
    string postText = scopedServiceProvider.GetRequiredService<AiGeneratorService>().GeneratePostText(aiTextConfig);

    SocialPost socialPost = new SocialPost { PostText = postText };

    // Exit console app if postText is null or empty
    if (string.IsNullOrEmpty(postText))
    {
        Log.Logger.Error("Generated post text is null or empty. Exiting application.");
        Utilities.AppStatus = 1;
    }

    if (!string.IsNullOrEmpty(appConfig.TxtFile))
    {
        File.WriteAllText(appConfig.TxtFile, postText);
    }
    else
    {
        Log.Logger.Error("TxtFile path is null.");
        Utilities.AppStatus = 1;
    }

    // Let's go ahead and generate an image prompt in case we want to generate the image manually
    string imagePrompt = scopedServiceProvider.GetRequiredService<AiGeneratorService>().GenerateImagePrompt(postText, aiTextConfig);

    if (includeImage)
    {
        if (string.IsNullOrEmpty(appConfig.PngFile))
        {
            Log.Logger.Error("PngFile path is required. Check configs.");
            Utilities.AppStatus = 1;
            return socialPost;
        }

        if (string.IsNullOrEmpty(appConfig.JpgFile))
        {
            Log.Logger.Error("JpgFile path is required. Check configs.");
            Utilities.AppStatus = 1;
            return socialPost;
        }

        await scopedServiceProvider.GetRequiredService<AiGeneratorService>().GenerateImage(imagePrompt, aiImageConfig, appConfig.PngFile);

        if (File.Exists(appConfig.PngFile))
        {
            Utilities.ConvertPngToJpg(appConfig.PngFile, appConfig.JpgFile, 80);
            socialPost.PostImage = appConfig.JpgFile;
        }
        else
        {
            Log.Logger.Error($"PngFile does not exist at path: {appConfig.PngFile}");
            Utilities.AppStatus = 1;
        }
    }

    return socialPost;
}
