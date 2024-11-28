using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class LimewireImageService
{
    private readonly HttpClient client = new HttpClient();

    public async Task<string> GenerateImageAsync(string prompt, string apiKey)
    {
        var requestUrl = "https://api.limewire.com/v1/generate-image";
        var requestBody = new
        {
            prompt = prompt,
            samples = 1,
            quality = "HIGH",
            guidance_scale = 40,
            aspect_ratio = "1:1"
        };

        var jsonContent = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("X-Api-Version", "v1");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", apiKey);

        var response = await client.PostAsync(requestUrl, content);

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            // Assuming the response contains a URL to the generated image
            var imageUrl = JsonConvert.DeserializeObject<dynamic>(responseContent).image_url;
            return imageUrl;
        }
        else
        {
            throw new Exception($"Failed to generate image: {response.ReasonPhrase}");
        }
    }
}
