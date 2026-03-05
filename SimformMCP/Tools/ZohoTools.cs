using ModelContextProtocol.Server;
using System.ComponentModel;

[McpServerToolType]
public class ZohoTools
{
    private readonly ZohoService _zoho;
    public ZohoTools(ZohoService zoho) => _zoho = zoho;

    // ══════════════════════════════════════════════════════
    // PROJECTS
    // ══════════════════════════════════════════════════════

    [McpServerTool]
    [Description("Get all projects from Zoho. Call this first to find projectId.")]
    public async Task<string> GetProjects()
    {
        var projects = await _zoho.GetProjectsAsync();
        return projects.Any()
            ? string.Join("\n", projects.Select(p => $"• ID: {p.Id} | Name: {p.Name}"))
            : "No projects found.";
    }

    [McpServerTool]
    [Description("Create a new Zoho project.")]
    public async Task<string> CreateProject(
        [Description("Project name")]             string projectName,
        [Description("Description")]              string? description = null,
        [Description("Start date YYYY-MM-DD")]    string? startDate   = null,
        [Description("End date YYYY-MM-DD")]      string? endDate     = null,
        [Description("Owner email")]              string? ownerEmail  = null)
    {
        var result = await _zoho.CreateProjectAsync(new CreateProjectRequest
        {
            ProjectName = projectName,
            Description = description,
            StartDate   = startDate,
            EndDate     = endDate,
            OwnerEmail  = ownerEmail
        });
        return result.Success ? $"✅ {result.Message} | ID: {result.Id}" : $"❌ {result.Message}";
    }

    [McpServerTool]
    [Description("Update an existing Zoho project. Only provide fields to change.")]
    public async Task<string> UpdateProject(
        [Description("Project ID")]                          string  projectId,
        [Description("New name")]                            string? projectName = null,
        [Description("New description")]                     string? description = null,
        [Description("New start date YYYY-MM-DD")]           string? startDate   = null,
        [Description("New end date YYYY-MM-DD")]             string? endDate     = null,
        [Description("Status: active or archived")]          string? status      = null)
    {
        var result = await _zoho.UpdateProjectAsync(new UpdateProjectRequest
        {
            ProjectId   = projectId,
            ProjectName = projectName,
            Description = description,
            StartDate   = startDate,
            EndDate     = endDate,
            Status      = status
        });
        return result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
    }

    [McpServerTool]
    [Description("Permanently delete a Zoho project by ID.")]
    public async Task<string> DeleteProject(
        [Description("Project ID to delete")] string projectId)
    {
        var result = await _zoho.DeleteProjectAsync(projectId);
        return result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
    }

    // ══════════════════════════════════════════════════════
    // TASK LISTS
    // ══════════════════════════════════════════════════════

    [McpServerTool]
    [Description("Get all task lists in a Zoho project.")]
    public async Task<string> GetTaskLists(
        [Description("Project ID")] string projectId)
    {
        var lists = await _zoho.GetTaskListsAsync(projectId);
        return lists.Any()
            ? string.Join("\n", lists.Select(l => $"• ID: {l.Id} | Name: {l.Name}"))
            : $"No task lists found for project {projectId}.";
    }

    [McpServerTool]
    [Description("Create a new task list inside a Zoho project.")]
    public async Task<string> CreateTaskList(
        [Description("Project ID")]      string projectId,
        [Description("Task list name")]  string taskListName)
    {
        var result = await _zoho.CreateTaskListAsync(new CreateTaskListRequest
        {
            ProjectId    = projectId,
            TaskListName = taskListName
        });
        return result.Success ? $"✅ {result.Message} | ID: {result.Id}" : $"❌ {result.Message}";
    }

    [McpServerTool]
    [Description("Rename an existing task list in a Zoho project.")]
    public async Task<string> UpdateTaskList(
        [Description("Project ID")]       string  projectId,
        [Description("Task List ID")]     string  taskListId,
        [Description("New name")]         string? taskListName = null)
    {
        var result = await _zoho.UpdateTaskListAsync(new UpdateTaskListRequest
        {
            ProjectId    = projectId,
            TaskListId   = taskListId,
            TaskListName = taskListName
        });
        return result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
    }

    [McpServerTool]
    [Description("Delete a task list from a Zoho project.")]
    public async Task<string> DeleteTaskList(
        [Description("Project ID")]    string projectId,
        [Description("Task List ID")]  string taskListId)
    {
        var result = await _zoho.DeleteTaskListAsync(projectId, taskListId);
        return result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
    }

    // ══════════════════════════════════════════════════════
    // TASKS
    // ══════════════════════════════════════════════════════

    [McpServerTool]
    [Description("Get all tasks inside a task list.")]
    public async Task<string> GetTasks(
        [Description("Project ID")]   string projectId,
        [Description("TaskList ID")]  string taskListId)
    {
        var tasks = await _zoho.GetTasksAsync(projectId, taskListId);
        return tasks.Any()
            ? string.Join("\n", tasks.Select(t => $"• ID: {t.Id} | Name: {t.Name}"))
            : "No tasks found in this task list.";
    }

    [McpServerTool]
    [Description("Create a new task inside a task list.")]
    public async Task<string> CreateTask(
        [Description("Project ID")]                string  projectId,
        [Description("Task List ID")]              string  taskListId,
        [Description("Task name")]                 string  taskName,
        [Description("Description")]               string? description   = null,
        [Description("Assignee email")]            string? assigneeEmail = null,
        [Description("Due date YYYY-MM-DD")]       string? dueDate       = null,
        [Description("Priority: high/medium/low")] string? priority      = null)
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
        return result.Success ? $"✅ {result.Message}\n🔗 {result.Url}" : $"❌ {result.Message}";
    }

    [McpServerTool]
    [Description("Update an existing task. Only provide fields to change.")]
    public async Task<string> UpdateTask(
        [Description("Project ID")]                          string  projectId,
        [Description("Task ID")]                             string  taskId,
        [Description("New name")]                            string? taskName    = null,
        [Description("New description")]                     string? description = null,
        [Description("Status: open/inprogress/closed")]      string? status      = null,
        [Description("Priority: high/medium/low")]           string? priority    = null,
        [Description("New due date YYYY-MM-DD")]             string? dueDate     = null)
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
        return result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
    }

    [McpServerTool]
    [Description("Delete a task from a Zoho project.")]
    public async Task<string> DeleteTask(
        [Description("Project ID")] string projectId,
        [Description("Task ID")]    string taskId)
    {
        var result = await _zoho.DeleteTaskAsync(projectId, taskId);
        return result.Success ? $"✅ {result.Message}" : $"❌ {result.Message}";
    }
}