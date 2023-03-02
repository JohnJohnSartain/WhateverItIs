using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Service.ArtificialIntelligence;

public interface IOpenAi
{
    Task<OpenAiResponse> QueryAsync(string query, int tokens);
}

public class OpenAi : IOpenAi
{
    public IConfiguration Configuration { get; set; }
    public IHttpClientFactory HttpClientFactory { get; set; }
    public string OpenAiApiConnectionString { get; set; }
    public string OpenAiApiKey { get; set; }

    public OpenAi(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        Configuration = configuration;
        HttpClientFactory = httpClientFactory;

        OpenAiApiConnectionString = Configuration["ConnectionString:OpenAi"];
        OpenAiApiKey = Configuration["Key:OpenAi"];
    }

    public async Task<OpenAiResponse> QueryAsync(string query, int tokens)
    {
        var client = HttpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", OpenAiApiKey);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var myObject = new
        {
            prompt = query,
            max_tokens = tokens,
        };

        var content = JsonContent.Create(myObject);

        var httpResponseMessage = await client.PostAsync(OpenAiApiConnectionString, content);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

            return await JsonSerializer.DeserializeAsync<OpenAiResponse>(contentStream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        throw new Exception("Failed to deserialize open ai response");
    }
}

public class OpenAiResponse
{
    public string Id { get; set; }
    public string Object { get; set; }
    public int Created { get; set; }
    public string Model { get; set; }
    public List<Choice> Choices { get; set; }
    public Usage Usage { get; set; }
}

public class Choice
{
    public string Text { get; set; }
    public int Index { get; set; }
    public object Logprobs { get; set; }
    public string Finish_reason { get; set; }
}

public class Usage
{
    public int Prompt_tokens { get; set; }
    public int Completion_tokens { get; set; }
    public int Total_tokens { get; set; }
}