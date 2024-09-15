namespace WorkItemValidator.AzureDevOps.Mappers;

public static class WorkItemMapperExtensions
{
    public static WorkItemValidator.Domain.WorkItem ToDomain(this Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem workItem)
    {
        //TODO: This can be done better!
        return new WorkItemValidator.Domain.WorkItem
        {
            Id = workItem.Fields["System.Id"].ToString() ?? throw new ArgumentException("Expected to get a System.Id field from a work item"),
            Title = workItem.Fields["System.Title"].ToString() ?? throw new ArgumentException("Expected to get a System.Title field from a work item"),
            Description = GetOptionalField(workItem, "System.Description", string.Empty),
            AcceptCriteria = GetOptionalField(workItem, "Microsoft.VSTS.Common.AcceptanceCriteria", string.Empty),
            Estimate = GetOptionalField(workItem, "Microsoft.VSTS.Scheduling.Effort"),  //TODO: Support story points.
            SprintName = workItem.Fields["System.IterationPath"].ToString() ?? string.Empty,
            WorkItemType = workItem.Fields["System.WorkItemType"].ToString() ?? string.Empty,
            State = workItem.Fields["System.State"].ToString() ?? string.Empty,
        };
    }
    
    private static int? GetOptionalField(Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem workItem, string fieldName)
    {
        workItem.Fields.TryGetValue(fieldName, out var fieldValue);
        if (fieldValue == null)
        {
            return null;
        }
        
        return int.Parse(fieldValue.ToString()!);
    } 
    
    private static string GetOptionalField(Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem workItem, string fieldName, string defaultValue)
    {
        workItem.Fields.TryGetValue(fieldName, out var fieldValue);
        return fieldValue?.ToString() ?? defaultValue;
    } 
}