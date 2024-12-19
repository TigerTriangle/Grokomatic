using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace Grokomatic.Services
{
    public class GrokTextService
    {
        /// <summary>
        /// Uses Grok to generate text based on the provided system and user prompts using the OpenAI API.
        /// </summary>
        /// <param name="systemPrompt">The system prompt to guide the AI's behavior.</param>
        /// <param name="userPrompt">The user prompt to provide context or questions for the AI.</param>
        /// <param name="apiKey">The API key for authenticating with the Grok API.</param>
        /// <returns>A string containing the generated text with markdown formatting removed.</returns>
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
