using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

public class ZohoService
{
    private readonly IConfiguration _config;
    private readonly ZohoAuthService _auth;
    private readonly HttpClient _http;

    private string PortalId => _config["Zoho:PortalId"]!;
    private string BaseUrl => $"https://projectsapi.zoho.in/restapi/portal/{PortalId}";

    public ZohoService(IConfiguration config, ZohoAuthService auth, IHttpClientFactory factory)
    {
        _config = config;   
        _auth = auth;
        _http = factory.CreateClient();
    }

    private async Task AuthorizeAsync()
    {
        var token = await _auth.GetAccessTokenAsync();
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Zoho-oauthtoken", token);
    }

    public async Task<List<ZohoResult>> GetProjectsAsync()
    {
        await AuthorizeAsync();

        var json = await _http.GetFromJsonAsync<JsonElement>($"{BaseUrl}/projects/");
        var results = new List<ZohoResult>();

        foreach (var p in json.GetProperty("projects").EnumerateArray())
        {
            results.Add(new ZohoResult
            {
                Id      = p.GetProperty("id_string").GetString(),
                Name    = p.GetProperty("name").GetString(),
                Success = true
            });
        }

        return results;
    }

    public async Task<List<ZohoResult>> GetTaskListsAsync(string projectId)
    {
        await AuthorizeAsync();

        var json = await _http.GetFromJsonAsync<JsonElement>(
            $"{BaseUrl}/projects/{projectId}/tasklists/"
        );
        var results = new List<ZohoResult>();

        foreach (var tl in json.GetProperty("tasklists").EnumerateArray())
        {
            results.Add(new ZohoResult
            {
                Id      = tl.GetProperty("id_string").GetString(),
                Name    = tl.GetProperty("name").GetString(),
                Success = true
            });
        }

        return results;
    }

    public async Task<ZohoResult> CreateTaskAsync(CreateTaskRequest req)
    {
        await AuthorizeAsync();

        var url = $"{BaseUrl}/projects/{req.ProjectId}/tasklists/{req.TaskListId}/tasks/";

        var body = new Dictionary<string, string>
        {
            ["name"] = req.TaskName
        };
        if (req.Description   != null) body["description"]        = req.Description;
        if (req.DueDate       != null) body["end_date"]            = req.DueDate;
        if (req.Priority      != null) body["priority"]            = req.Priority;
        if (req.AssigneeEmail != null) body["person_responsible"]  = req.AssigneeEmail;

        var response = await _http.PostAsync(url, new FormUrlEncodedContent(body));
        var json     = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (!response.IsSuccessStatusCode)
            return new ZohoResult { Success = false, Message = json.ToString() };

        var task = json.GetProperty("tasks")[0];
        return new ZohoResult
        {
            Success = true,
            Id      = task.GetProperty("id_string").GetString(),
            Url     = task.GetProperty("link").GetProperty("self").GetProperty("url").GetString(),
            Message = $"Task '{req.TaskName}' created successfully!"
        };
    }

    public async Task<ZohoResult> CreateProjectAsync(CreateProjectRequest req)
    {
        await AuthorizeAsync();

        var body = new Dictionary<string, string>
        {
            ["name"] = req.ProjectName
        };
        if (req.Description != null) body["description"] = req.Description;
        if (req.StartDate   != null) body["start_date"]  = req.StartDate;
        if (req.EndDate     != null) body["end_date"]     = req.EndDate;
        if (req.OwnerEmail  != null) body["owner"]        = req.OwnerEmail;

        var response = await _http.PostAsync($"{BaseUrl}/projects/", new FormUrlEncodedContent(body));
        var json     = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (!response.IsSuccessStatusCode)
            return new ZohoResult { Success = false, Message = json.ToString() };

        var project = json.GetProperty("projects")[0];
        return new ZohoResult
        {
            Success = true,
            Id      = project.GetProperty("id_string").GetString(),
            Name    = project.GetProperty("name").GetString(),
            Message = $"Project '{req.ProjectName}' created successfully!"
        };
    }

    public async Task<ZohoResult> UpdateTaskAsync(UpdateTaskRequest req)
    {
        await AuthorizeAsync();

        var url  = $"{BaseUrl}/projects/{req.ProjectId}/tasks/{req.TaskId}/";
        var body = new Dictionary<string, string>();

        if (req.TaskName    != null) body["name"]        = req.TaskName;
        if (req.Description != null) body["description"] = req.Description;
        if (req.Status      != null) body["status"]      = req.Status;
        if (req.Priority    != null) body["priority"]    = req.Priority;
        if (req.DueDate     != null) body["end_date"]    = req.DueDate;

        var response = await _http.PostAsync(url, new FormUrlEncodedContent(body));
        var json     = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (!response.IsSuccessStatusCode)
            return new ZohoResult { Success = false, Message = json.ToString() };

        return new ZohoResult
        {
            Success = true,
            Id      = req.TaskId,
            Message = $"Task '{req.TaskId}' updated successfully!"
        };
    }
}