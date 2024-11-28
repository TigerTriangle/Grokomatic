using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace Grokomatic.Services
{
    public class GrokTextService
    {
        public string GenerateText(string systemPrompt, string userPrompt, string apiKey)
        {
            OpenAIClientOptions options = new OpenAIClientOptions();
            options.Endpoint = new Uri("https://api.x.ai/v1");
            ChatClient client = new(model: "grok-beta", new ApiKeyCredential(apiKey), options);            

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(systemPrompt),
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
                return textForPost;
        }       
    }
}
