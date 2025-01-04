using Grokomatic.Configs;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using Tweetinvi.Core.Extensions;

namespace Grokomatic.Services
{
    public class TextService
    {
        /// <summary>
        /// Uses an OpenAI compatible API to generate text based on the provided system and user prompts.
        /// </summary>
        /// <param name="systemPrompt">The system prompt to guide the AI's behavior.</param>
        /// <param name="userPrompt">The user prompt to provide context or questions for the AI.</param>
        /// <param name="aiConfig">The configuration for the OpenAI compatible API.</param>
        /// <returns>A string containing the generated text with markdown formatting removed.</returns>
        public string GenerateText(string systemPrompt, string userPrompt, OpenAiConfig aiConfig)
        {
            if (string.IsNullOrEmpty(aiConfig.ApiKey)) throw new Exception("API Key is required for AI Text generation.");

            OpenAIClientOptions options = new OpenAIClientOptions();
            if (!string.IsNullOrEmpty(aiConfig.Endpoint)) options.Endpoint = new Uri(aiConfig.Endpoint);
            
            ChatClient client = new(aiConfig.Model, new ApiKeyCredential(aiConfig.ApiKey), options);

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

            string textForPost = Utilities.RemoveMarkdownFormatting(completion.Content[0].Text);
            return textForPost;
        }
    }
}
