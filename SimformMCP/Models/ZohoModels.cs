public class CreateTaskRequest
{
    public string ProjectId { get; set; } = "";
    public string TaskListId { get; set; } = "";
    public string TaskName { get; set; } = "";
    public string? Description { get; set; }
    public string? AssigneeEmail { get; set; }
    public string? DueDate { get; set; }
    public string? Priority { get; set; }
}

public class CreateProjectRequest
{
    public string ProjectName { get; set; } = "";
    public string? Description { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
    public string? OwnerEmail { get; set; }
}

public class UpdateTaskRequest
{
    public string ProjectId { get; set; } = "";
    public string TaskId { get; set; } = "";
    public string? TaskName { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? DueDate { get; set; }
}

public class ZohoResult
{
    public bool Success { get; set; }
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Url { get; set; }
    public string? Message { get; set; }
}