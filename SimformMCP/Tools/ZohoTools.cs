using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
public class ZohoTools
{
    private readonly ZohoService _zoho;

    public ZohoTools(ZohoService zoho)
    {
        _zoho = zoho;
    }

    [McpServerTool]
    [Description("Get all projects from Zoho. Call this first to find the projectId before creating tasks.")]
    public async Task<string> GetProjects()
    {
        var projects = await _zoho.GetProjectsAsync();

        if (!projects.Any())
            return "No projects found in Zoho.";

        return string.Join("\n", projects.Select(p => $"• ID: {p.Id} | Name: {p.Name}"));
    }

    [McpServerTool]
    [Description("Get all task lists inside a Zoho project. Call this to find taskListId before creating a task.")]
    public async Task<string> GetTaskLists(
        [Description("Zoho Project ID — get this from GetProjects first")] string projectId)
    {
        var lists = await _zoho.GetTaskListsAsync(projectId);

        if (!lists.Any())
            return $"No task lists found for project {projectId}.";

        return string.Join("\n", lists.Select(l => $"• ID: {l.Id} | Name: {l.Name}"));
    }

    [McpServerTool]
    [Description("Create a new task in Zoho Projects under a specific task list.")]
    public async Task<string> CreateTask(
        [Description("Zoho Project ID")]                          string projectId,
        [Description("Task List ID inside the project")]          string taskListId,
        [Description("Name of the task")]                         string taskName,
        [Description("Optional task description")]                string? description = null,
        [Description("Assignee email address")]                   string? assigneeEmail = null,
        [Description("Due date in YYYY-MM-DD format")]            string? dueDate = null,
        [Description("Priority: high, medium, or low")]           string? priority = null)
    {
        var result = await _zoho.CreateTaskAsync(new CreateTaskRequest
        {
            ProjectId     = projectId,
            TaskListId    = taskListId,
            TaskName      = taskName,
            Description   = description,
            AssigneeEmail = assigneeEmail,
            DueDate       = dueDate,
            Priority      = priority
        });

        return result.Success
            ? $"✅ {result.Message}\n🔗 {result.Url}"
            : $"❌ Failed: {result.Message}";
    }

    [McpServerTool]
    [Description("Create a brand new project in Zoho Projects.")]
    public async Task<string> CreateProject(
        [Description("Project name")]                string projectName,
        [Description("Project description")]         string? description = null,
        [Description("Start date in YYYY-MM-DD")]    string? startDate = null,
        [Description("End date in YYYY-MM-DD")]      string? endDate = null,
        [Description("Owner email address")]         string? ownerEmail = null)
    {
        var result = await _zoho.CreateProjectAsync(new CreateProjectRequest
        {
            ProjectName = projectName,
            Description = description,
            StartDate   = startDate,
            EndDate     = endDate,
            OwnerEmail  = ownerEmail
        });

        return result.Success
            ? $"✅ {result.Message} | Project ID: {result.Id}"
            : $"❌ Failed: {result.Message}";
    }

    [McpServerTool]
    [Description("Update an existing task in Zoho. Only provide the fields you want to change.")]
    public async Task<string> UpdateTask(
        [Description("Zoho Project ID")]                       string projectId,
        [Description("Zoho Task ID")]                          string taskId,
        [Description("New task name")]                         string? taskName = null,
        [Description("New description")]                       string? description = null,
        [Description("Status: open, inprogress, or closed")]   string? status = null,
        [Description("Priority: high, medium, or low")]        string? priority = null,
        [Description("New due date in YYYY-MM-DD")]            string? dueDate = null)
    {
        var result = await _zoho.UpdateTaskAsync(new UpdateTaskRequest
        {
            ProjectId   = projectId,
            TaskId      = taskId,
            TaskName    = taskName,
            Description = description,
            Status      = status,
            Priority    = priority,
            DueDate     = dueDate
        });

        return result.Success
            ? $"✅ {result.Message}"
            : $"❌ Failed: {result.Message}";
    }
}