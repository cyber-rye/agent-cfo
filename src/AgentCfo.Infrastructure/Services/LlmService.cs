using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace AgentCfo.Infrastructure.Services;

/// <summary>
/// Abstraction for LLM completion. Used by AgentService to generate
/// natural-language reasoning instead of deterministic templates.
/// </summary>
public interface ILlmService
{
    Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct = default);
    Task<string?> TryCompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct = default);
}

/// <summary>
/// Multi-provider LLM service with automatic fallback:
/// 1. OpenRouter → Nemotron 3 Ultra (free)
/// 2. OpenRouter → Nemotron 3 Super 120B (free)
/// 3. Xiaomi MiMo v2.5 Pro (direct API)
/// </summary>
public class OpenRouterLlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenRouterLlmService> _logger;
    private readonly string _apiKey;
    private readonly string _primaryModel;
    private readonly string _fallbackModel;
    private readonly string _xiaomiApiKey;
    private readonly string _xiaomiBaseUrl;
    private readonly string _xiaomiModel;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public OpenRouterLlmService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OpenRouterLlmService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiKey = configuration["OpenRouter:ApiKey"]
                  ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
                  ?? "";
        _primaryModel = configuration["OpenRouter:Model"]
                        ?? "nvidia/nemotron-3-super-120b-a12b";
        _fallbackModel = null; // removed — skip straight to Xiaomi
        _xiaomiApiKey = configuration["Xiaomi:ApiKey"]
                        ?? Environment.GetEnvironmentVariable("XIAOMI_API_KEY")
                        ?? "";
        _xiaomiBaseUrl = configuration["Xiaomi:BaseUrl"]
                         ?? Environment.GetEnvironmentVariable("XIAOMI_BASE_URL")
                         ?? "https://token-plan-sgp.xiaomimimo.com/v1";
        _xiaomiModel = configuration["Xiaomi:Model"]
                       ?? "mimo-v2.5";

        _httpClient.BaseAddress = new Uri("https://openrouter.ai/api/v1/");
        if (!string.IsNullOrEmpty(_apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        }
        _httpClient.Timeout = TimeSpan.FromSeconds(8);
        _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://github.com/cyber-rye/agent-cfo");
        _httpClient.DefaultRequestHeaders.Add("X-Title", "AgentCFO");
    }

    public async Task<string> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct = default)
    {
        var result = await TryCompleteAsync(systemPrompt, userPrompt, ct);
        return result ?? "Analysis completed. Unable to generate detailed reasoning at this time.";
    }

    public async Task<string?> TryCompleteAsync(string systemPrompt, string userPrompt, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_apiKey) && string.IsNullOrEmpty(_xiaomiApiKey))
        {
            _logger.LogWarning("No LLM API keys configured — falling back to deterministic mode");
            return null;
        }

        // Tier 1: Nemotron Super 120B via OpenRouter (paid)
        if (!string.IsNullOrEmpty(_apiKey))
        {
            var result = await CallOpenRouterAsync(_primaryModel, systemPrompt, userPrompt, ct);
            if (result is not null) return result;
        }

        // Tier 3: Xiaomi MiMo v2.5 Pro (direct API)
        if (!string.IsNullOrEmpty(_xiaomiApiKey))
        {
            _logger.LogWarning("OpenRouter models failed, trying Xiaomi MiMo");
            var result = await CallXiaomiAsync(systemPrompt, userPrompt, ct);
            if (result is not null) return result;
        }

        _logger.LogError("All LLM providers failed — falling back to deterministic mode");
        return null;
    }

    private async Task<string?> CallOpenRouterAsync(string model, string systemPrompt, string userPrompt, CancellationToken ct)
    {
        try
        {
            var request = new
            {
                model,
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                max_tokens = 1024,
                temperature = 0.3,
            };

            var response = await _httpClient.PostAsJsonAsync("chat/completions", request, JsonOptions, ct);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("OpenRouter {Model} returned {Status}: {Error}", model, (int)response.StatusCode, errorBody);
                return null;
            }

            var completion = await response.Content.ReadFromJsonAsync<OpenRouterResponse>(JsonOptions, ct);
            var content = completion?.Choices?.FirstOrDefault()?.Message?.Content;

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("OpenRouter {Model} returned empty content", model);
                return null;
            }

            _logger.LogInformation("LLM response from OpenRouter/{Model}: {Length} chars", model, content.Length);
            return content.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenRouter call failed for {Model}", model);
            return null;
        }
    }

    private async Task<string?> CallXiaomiAsync(string systemPrompt, string userPrompt, CancellationToken ct)
    {
        try
        {
            using var xiaomiClient = new HttpClient();
            xiaomiClient.BaseAddress = new Uri(_xiaomiBaseUrl.TrimEnd('/') + "/");
            xiaomiClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _xiaomiApiKey);
            xiaomiClient.Timeout = TimeSpan.FromSeconds(15);

            var request = new
            {
                model = _xiaomiModel,
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                max_tokens = 1024,
                temperature = 0.3,
            };

            _logger.LogInformation("Calling Xiaomi MiMo at {Base}/chat/completions (model: {Model})", _xiaomiBaseUrl, _xiaomiModel);
            var response = await xiaomiClient.PostAsJsonAsync("chat/completions", request, JsonOptions, ct);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Xiaomi MiMo returned {Status}: {Error}", (int)response.StatusCode, errorBody);
                return null;
            }

            var completion = await response.Content.ReadFromJsonAsync<XiaomiResponse>(JsonOptions, ct);
            var choice = completion?.Choices?.FirstOrDefault()?.Message;
            // Xiaomi returns reasoning in reasoning_content, not content
            var content = !string.IsNullOrWhiteSpace(choice?.Content) ? choice.Content : choice?.ReasoningContent;

            if (!string.IsNullOrWhiteSpace(content))
            {
                _logger.LogInformation("LLM response from Xiaomi/{Model}: {Length} chars", _xiaomiModel, content.Length);
                return content.Trim();
            }
            _logger.LogWarning("Xiaomi MiMo returned empty content");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xiaomi MiMo call failed");
            return null;
        }
    }
}

// Response DTOs for OpenRouter / OpenAI-compatible API
internal class OpenRouterResponse
{
    public List<Choice>? Choices { get; set; }
}

internal class Choice
{
    public MessageContent? Message { get; set; }
}

internal class MessageContent
{
    public string? Content { get; set; }
    public string? ReasoningContent { get; set; }
}

// Xiaomi returns reasoning in a separate field
internal class XiaomiResponse
{
    public List<XiaomiChoice>? Choices { get; set; }
}

internal class XiaomiChoice
{
    public XiaomiMessageContent? Message { get; set; }
}

internal class XiaomiMessageContent
{
    public string? Content { get; set; }
    public string? ReasoningContent { get; set; }
}
