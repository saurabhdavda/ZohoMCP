using System.Net.Http.Headers;
using System.Text.Json;

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

    private static string? ConvertDate(string? date)
    {
        if (date == null) return null;
        if (DateTime.TryParse(date, out var d))
            return d.ToString("MM-dd-yyyy");
        return date;
    }

    private static string? ConvertPriority(string? priority) => priority?.ToLower() switch
    {
        "high" => "High",
        "medium" => "Medium",
        "low" => "Low",
        "none" => "None",
        _ => null
    };

    private async Task AuthorizeAsync()
    {
        var token = await _auth.GetAccessTokenAsync();
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Zoho-oauthtoken", token);
    }
    
    // PROJECTS

    public async Task<List<ZohoResult>> GetProjectsAsync()
    {
        await AuthorizeAsync();
        var json = await _http.GetFromJsonAsync<JsonElement>($"{BaseUrl}/projects/");
        var results = new List<ZohoResult>();

        foreach (var p in json.GetProperty("projects").EnumerateArray())
            results.Add(new ZohoResult
            {
                Id = p.GetProperty("id_string").GetString(),
                Name = p.GetProperty("name").GetString(),
                Success = true
            });

        return results;
    }

    public async Task<ZohoResult> CreateProjectAsync(CreateProjectRequest req)
    {
        await AuthorizeAsync();

        var body = new Dictionary<string, string> { ["name"] = req.ProjectName };
        if (req.Description != null) body["description"] = req.Description;
        if (req.StartDate != null) body["start_date"] = ConvertDate(req.StartDate)!;
        if (req.EndDate != null) body["end_date"] = ConvertDate(req.EndDate)!;
        if (req.OwnerEmail != null) body["owner"] = req.OwnerEmail;

        var response = await _http.PostAsync($"{BaseUrl}/projects/", new FormUrlEncodedContent(body));
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (!response.IsSuccessStatusCode)
            return new ZohoResult { Success = false, Message = json.ToString() };

        var project = json.GetProperty("projects")[0];
        return new ZohoResult
        {
            Success = true,
            Id = project.GetProperty("id_string").GetString(),
            Name = project.GetProperty("name").GetString(),
            Message = $"Project '{req.ProjectName}' created successfully!"
        };
    }

    public async Task<ZohoResult> UpdateProjectAsync(UpdateProjectRequest req)
    {
        await AuthorizeAsync();

        var body = new Dictionary<string, string>();
        if (req.ProjectName != null) body["name"] = req.ProjectName;
        if (req.Description != null) body["description"] = req.Description;
        if (req.StartDate != null) body["start_date"] = ConvertDate(req.StartDate)!;
        if (req.EndDate != null) body["end_date"] = ConvertDate(req.EndDate)!;
        if (req.Status != null) body["status"] = req.Status;

        var response = await _http.PostAsync(
            $"{BaseUrl}/projects/{req.ProjectId}/",
            new FormUrlEncodedContent(body));
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (!response.IsSuccessStatusCode)
            return new ZohoResult { Success = false, Message = json.ToString() };

        return new ZohoResult { Success = true, Id = req.ProjectId, Message = "Project updated successfully!" };
    }

    public async Task<ZohoResult> DeleteProjectAsync(string projectId)
    {
        await AuthorizeAsync();

        var response = await _http.DeleteAsync($"{BaseUrl}/projects/{projectId}/");

        return response.IsSuccessStatusCode
            ? new ZohoResult { Success = true, Message = $"Project '{projectId}' deleted successfully!" }
            : new ZohoResult { Success = false, Message = $"Delete failed: {response.StatusCode}" };
    }

    // TASK LISTS

    public async Task<List<ZohoResult>> GetTaskListsAsync(string projectId)
    {
        await AuthorizeAsync();
        var json = await _http.GetFromJsonAsync<JsonElement>($"{BaseUrl}/projects/{projectId}/tasklists/");
        var results = new List<ZohoResult>();

        foreach (var tl in json.GetProperty("tasklists").EnumerateArray())
            results.Add(new ZohoResult
            {
                Id = tl.GetProperty("id_string").GetString(),
                Name = tl.GetProperty("name").GetString(),
                Success = true
            });

        return results;
    }

    public async Task<ZohoResult> CreateTaskListAsync(CreateTaskListRequest req)
    {
        await AuthorizeAsync();

        var body = new Dictionary<string, string> { ["name"] = req.TaskListName };
        var response = await _http.PostAsync(
            $"{BaseUrl}/projects/{req.ProjectId}/tasklists/",
            new FormUrlEncodedContent(body));
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (!response.IsSuccessStatusCode)
            return new ZohoResult { Success = false, Message = json.ToString() };

        var tl = json.GetProperty("tasklists")[0];
        return new ZohoResult
        {
            Success = true,
            Id = tl.GetProperty("id_string").GetString(),
            Message = $"Task list '{req.TaskListName}' created successfully!"
        };
    }

    public async Task<ZohoResult> UpdateTaskListAsync(UpdateTaskListRequest req)
    {
        await AuthorizeAsync();

        var body = new Dictionary<string, string>
        {
            ["flag"] = "internal"
        };
        if (req.TaskListName != null) body["name"] = req.TaskListName;

        var response = await _http.PostAsync(
            $"{BaseUrl}/projects/{req.ProjectId}/tasklists/{req.TaskListId}/",
            new FormUrlEncodedContent(body));
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (!response.IsSuccessStatusCode)
            return new ZohoResult { Success = false, Message = json.ToString() };

        return new ZohoResult { Success = true, Id = req.TaskListId, Message = "Task list updated successfully!" };
    }

    public async Task<ZohoResult> DeleteTaskListAsync(string projectId, string taskListId)
    {
        await AuthorizeAsync();

        var response = await _http.DeleteAsync(
            $"{BaseUrl}/projects/{projectId}/tasklists/{taskListId}/");

        return response.IsSuccessStatusCode
            ? new ZohoResult { Success = true, Message = $"Task list '{taskListId}' deleted successfully!" }
            : new ZohoResult { Success = false, Message = $"Delete failed: {response.StatusCode}" };
    }

    // TASKS
    public async Task<List<ZohoResult>> GetTasksAsync(string projectId, string taskListId)
    {
        await AuthorizeAsync();
        var json = await _http.GetFromJsonAsync<JsonElement>(
            $"{BaseUrl}/projects/{projectId}/tasklists/{taskListId}/tasks/");
        var results = new List<ZohoResult>();

        foreach (var t in json.GetProperty("tasks").EnumerateArray())
            results.Add(new ZohoResult
            {
                Id = t.GetProperty("id_string").GetString(),
                Name = t.GetProperty("name").GetString(),
                Success = true
            });

        return results;
    }

    public async Task<ZohoResult> CreateTaskAsync(CreateTaskRequest req)
    {
        await AuthorizeAsync();

        var body = new Dictionary<string, string>
        {
            ["name"] = req.TaskName,
            ["tasklist_id"] = req.TaskListId   // pass tasklist as body param instead
        };
        if (req.Description != null) body["description"] = req.Description;
        if (req.DueDate != null) body["end_date"] = ConvertDate(req.DueDate)!;
        if (req.Priority != null) body["priority"] = ConvertPriority(req.Priority)!;
        if (req.AssigneeEmail != null) body["person_responsible"] = req.AssigneeEmail;

        var response = await _http.PostAsync(
            $"{BaseUrl}/projects/{req.ProjectId}/tasks/",
            new FormUrlEncodedContent(body));

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (!response.IsSuccessStatusCode)
            return new ZohoResult { Success = false, Message = json.ToString() };

        var task = json.GetProperty("tasks")[0];
        return new ZohoResult
        {
            Success = true,
            Id = task.GetProperty("id_string").GetString(),
            Url = task.GetProperty("link").GetProperty("self").GetProperty("url").GetString(),
            Message = $"Task '{req.TaskName}' created successfully!"
        };
    }

    public async Task<ZohoResult> UpdateTaskAsync(UpdateTaskRequest req)
    {
        await AuthorizeAsync();

        var body = new Dictionary<string, string>();
        if (req.TaskName != null) body["name"] = req.TaskName;
        if (req.Description != null) body["description"] = req.Description;
        if (req.Status != null) body["status"] = req.Status;
        if (req.Priority != null) body["priority"] = ConvertPriority(req.Priority)!;
        if (req.DueDate != null) body["end_date"] = ConvertDate(req.DueDate)!;

        var response = await _http.PostAsync(
            $"{BaseUrl}/projects/{req.ProjectId}/tasks/{req.TaskId}/",
            new FormUrlEncodedContent(body));
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        if (!response.IsSuccessStatusCode)
            return new ZohoResult { Success = false, Message = json.ToString() };

        return new ZohoResult { Success = true, Id = req.TaskId, Message = $"Task '{req.TaskId}' updated successfully!" };
    }

    public async Task<ZohoResult> DeleteTaskAsync(string projectId, string taskId)
    {
        await AuthorizeAsync();

        var response = await _http.DeleteAsync(
            $"{BaseUrl}/projects/{projectId}/tasks/{taskId}/");

        return response.IsSuccessStatusCode
            ? new ZohoResult { Success = true, Message = $"Task '{taskId}' deleted successfully!" }
            : new ZohoResult { Success = false, Message = $"Delete failed: {response.StatusCode}" };
    }
}