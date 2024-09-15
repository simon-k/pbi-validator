namespace WorkItemValidator.Domain;

public class WorkItem
{
    public required string Id { get; set; }  //TODO: Should the ID be an int?
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string AcceptCriteria { get; set; } = string.Empty;
    public int? Estimate { get; set; }
    public string SprintName { get; set; } = string.Empty;
    public string WorkItemType { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty; 
}