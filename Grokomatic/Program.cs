using Grokomatic.Models;
using Grokomatic.Services;
using Grokomatic;
using Microsoft.Extensions.Configuration;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Grokomatic.Configs;

string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
const int MAX_RETRIES = 3;

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

Console.WriteLine("Welcome to Grokomatic\r");
Console.WriteLine("Generate posts about tech innovations.\r");
Console.WriteLine("-----------------------------------\n");
Console.WriteLine("To run this in batch mode pass batch as an argument.");
Console.WriteLine("Example: Grokomatic.exe batch");

var services = new ServiceCollection();
ConfigureServices(services, configuration);

var serviceProvider = services.BuildServiceProvider();

int exitCode = 0;

AppConfiguration appConfig = new AppConfiguration();
configuration.GetSection("AppConfiguration").Bind(appConfig);

Initialize(appConfig);

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
                exitCode = 1;
            }
        }
    }
    else
    {
        await ShowMenu();
    }
}
catch (Exception ex)
{
    Log.Logger.Error(ex, "An unhandled exception occurred.");
    exitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}

Environment.Exit(exitCode);

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddScoped<AiGeneratorService>();
    services.AddScoped<FacebookService>();
    services.AddScoped<GrokTextService>();
    services.AddScoped<InstagramService>();
    services.AddScoped<OpenAiImageService>();
    services.AddScoped<XService>();
    services.AddSingleton(configuration);
}

async Task ShowMenu()
{
    Console.Clear();
    Console.WriteLine("1. Post Tech Innovation (all platforms)");
    Console.WriteLine("2. Generate Post Only");
    Console.WriteLine("3. Post to X");
    Console.WriteLine("4. Post to Facebook");
    Console.WriteLine("5. Post to Instagram");
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
                    await GeneratePost(scopedServiceProvider);
                    break;
                case "3":
                    var socialPost = await GeneratePost(scopedServiceProvider);
                    await scopedServiceProvider.GetRequiredService<XService>().PostOnX(socialPost, appConfig);
                    i = MAX_RETRIES + 1;
                    break;
                case "4":
                    var socialPost2 = await GeneratePost(scopedServiceProvider);
                    await scopedServiceProvider.GetRequiredService<FacebookService>().PostOnFacebook(socialPost2, appConfig);
                    i = MAX_RETRIES + 1;
                    break;
                case "5":
                    var socialPost3 = await GeneratePost(scopedServiceProvider);
                    await scopedServiceProvider.GetRequiredService<InstagramService>().PostOnInstagram(socialPost3, appConfig);
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

void Initialize(AppConfiguration appConfig)
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
}

async Task PostOnAllPlatforms(IServiceProvider scopedServiceProvider)
{
    var socialPost = await GeneratePost(scopedServiceProvider);
    await scopedServiceProvider.GetRequiredService<XService>().PostOnX(socialPost, appConfig);
    await scopedServiceProvider.GetRequiredService<FacebookService>().PostOnFacebook(socialPost, appConfig);
    await scopedServiceProvider.GetRequiredService<InstagramService>().PostOnInstagram(socialPost, appConfig);
}

async Task<SocialPost> GeneratePost(IServiceProvider scopedServiceProvider)
{
    string postText = scopedServiceProvider.GetRequiredService<AiGeneratorService>().GeneratePostText(appConfig);
    Log.Logger.Information(postText);

    if (appConfig.TxtFile != null)
    {
        File.WriteAllText(appConfig.TxtFile, postText);
    }
    else
    {
        Log.Logger.Error("TxtFile path is null.");
    }

    string imagePrompt = scopedServiceProvider.GetRequiredService<AiGeneratorService>().GenerateImagePrompt(postText, appConfig);

    await scopedServiceProvider.GetRequiredService<AiGeneratorService>().GenerateImage(imagePrompt, appConfig);

    if (appConfig.PngFile == null)
    {
        throw new Exception("PngFile path is null.");
    }

    if (appConfig.JpgFile == null)
    {
        throw new Exception("JpgFile path is null.");
    }

    Utilities.ConvertPngToJpg(appConfig.PngFile, appConfig.JpgFile, 80);
    return new SocialPost { PostText = postText, PostImage = appConfig.JpgFile };
}
