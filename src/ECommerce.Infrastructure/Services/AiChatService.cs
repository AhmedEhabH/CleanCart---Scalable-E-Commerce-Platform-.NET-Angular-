using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ECommerce.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class AiChatService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AiChatService> _logger;
    private readonly string _systemPrompt;

    private const string FallbackMessage = "I'm currently experiencing technical difficulties. Please try again in a moment, or browse our products directly.";

    public AiChatService(HttpClient httpClient, IConfiguration configuration, ILogger<AiChatService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _systemPrompt = _configuration["Ai:SystemPrompt"] ?? GetDefaultSystemPrompt();
    }

    public async Task<string> GetChatResponseAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>
        {
            new("system", _systemPrompt),
            new("user", userMessage)
        };

        return await SendRequestAsync(messages, cancellationToken);
    }

    public async Task<string> GetChatResponseWithHistoryAsync(List<ChatMessage> messages, CancellationToken cancellationToken = default)
    {
        var fullMessages = new List<ChatMessage> { new("system", _systemPrompt) };
        fullMessages.AddRange(messages);

        return await SendRequestAsync(fullMessages, cancellationToken);
    }

    private async Task<string> SendRequestAsync(List<ChatMessage> messages, CancellationToken cancellationToken)
    {
        try
        {
            var provider = _configuration["Ai:Provider"]?.ToLowerInvariant() ?? "generic";
            var apiKey = _configuration["Ai:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("AI API key not configured. Returning fallback message.");
                return FallbackMessage;
            }

            var endpoint = _configuration["Ai:Endpoint"];
            if (string.IsNullOrEmpty(endpoint))
            {
                endpoint = provider switch
                {
                    "gemini" => "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent",
                    "openai" => "https://api.openai.com/v1/chat/completions",
                    _ => throw new InvalidOperationException($"Unknown AI provider: {provider}")
                };
            }

            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            var response = provider switch
            {
                "gemini" => await SendGeminiRequestAsync(endpoint, apiKey, messages, cancellationToken),
                "openai" => await SendOpenAiRequestAsync(endpoint, apiKey, messages, cancellationToken),
                _ => await SendGenericRequestAsync(endpoint, apiKey, messages, cancellationToken)
            };

            return response ?? FallbackMessage;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogWarning(ex, "AI request timed out");
            return "The request took too long to process. Please try again with a shorter message.";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "AI service HTTP error");
            return FallbackMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in AI service");
            return FallbackMessage;
        }
    }

    private async Task<string?> SendGeminiRequestAsync(string endpoint, string apiKey, List<ChatMessage> messages, CancellationToken cancellationToken)
    {
        var url = $"{endpoint}?key={apiKey}";

        var contents = messages
            .Where(m => m.Role != "system")
            .Select(m => new
            {
                role = m.Role == "assistant" ? "model" : "user",
                parts = new[] { new { text = m.Content } }
            })
            .ToList();

        var systemInstruction = messages.FirstOrDefault(m => m.Role == "system");
        var requestBody = new
        {
            contents,
            systemInstruction = systemInstruction != null ? new { parts = new[] { new { text = systemInstruction.Content } } } : null
        };

        var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        if (json.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
        {
            var candidate = candidates[0];
            if (candidate.TryGetProperty("content", out var content) &&
                content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
            {
                return parts[0].GetProperty("text").GetString();
            }
        }

        return null;
    }

    private async Task<string?> SendOpenAiRequestAsync(string endpoint, string apiKey, List<ChatMessage> messages, CancellationToken cancellationToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var requestBody = new
        {
            model = _configuration["Ai:Model"] ?? "gpt-3.5-turbo",
            messages = messages.Select(m => new { role = m.Role, content = m.Content }),
            max_tokens = 500,
            temperature = 0.7
        };

        var response = await _httpClient.PostAsJsonAsync(endpoint, requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        if (json.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
        {
            return choices[0].GetProperty("message").GetProperty("content").GetString();
        }

        return null;
    }

    private async Task<string?> SendGenericRequestAsync(string endpoint, string apiKey, List<ChatMessage> messages, CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            messages = messages.Select(m => new { role = m.Role, content = m.Content }),
            max_tokens = 500,
            temperature = 0.7
        };

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var response = await _httpClient.PostAsJsonAsync(endpoint, requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return content;
    }

    private static string GetDefaultSystemPrompt() =>
        """
        You are an expert shopping assistant for E-Shop, a premium e-commerce platform. 
        Help users find products based on their needs, preferences, and budget. 
        Be friendly, concise, and helpful. 
        You can assist with:
        - Product recommendations by category (electronics, clothing, accessories, etc.)
        - Price comparisons and budget-friendly options
        - Product features and specifications
        - General shopping advice
        
        Keep responses brief (2-4 sentences) and actionable. If asked about specific products, 
        suggest browsing the catalog for the most up-to-date information.
        """;
}
