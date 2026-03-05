using System.Text.Json;

public class ZohoAuthService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _http;
    // Cache the token in memory so we don't call Zoho on every single request
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public ZohoAuthService(IConfiguration config, IHttpClientFactory factory)
    {
        _config = config;
        _http = factory.CreateClient();
    }

    public async Task<string> GetAccessTokenAsync()
    {
        // Still valid? Return cached token (5 min safety buffer)
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
            return _cachedToken;

        // Token expired — go get a new one from Zoho
        var response = await _http.PostAsync(
            "https://accounts.zoho.in/oauth/v2/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"]    = "refresh_token",
                ["client_id"]     = _config["Zoho:ClientId"]!,
                ["client_secret"] = _config["Zoho:ClientSecret"]!,
                ["refresh_token"] = _config["Zoho:RefreshToken"]!
            })
        );

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        _cachedToken = json.GetProperty("access_token").GetString()!;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(
            json.GetProperty("expires_in").GetInt32()
        );

        return _cachedToken;
    }
}