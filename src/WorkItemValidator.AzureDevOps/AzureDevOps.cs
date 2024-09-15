using WorkItemValidator.AzureDevOps.Mappers;
using WorkItemValidator.AzureDevOps.Queries;
using WorkItem = WorkItemValidator.Domain.WorkItem;
using WorkItemValidator.AzureDevOps.Actions;

namespace WorkItemValidator.AzureDevOps;

public class AzureDevOps(string orgName, string personalAccessToken)
{
    public async Task<IList<WorkItem>> GetWorkItemsAsync(string project, string teamName)
    {
        var query = new GetWorkItems(orgName, personalAccessToken);     //TODO: Get this from DI
        var workItems = await query.OpenWorkItemsAsync(project, teamName, "Product Backlog Item");
        return workItems.Select(workItem => workItem.ToDomain()).ToList();
    }
    
    public async Task UpdateTagsAsync(string workItemId, string tagToAdd, string tagToRemove)
    {
        var action = new WorkItemTags(orgName, personalAccessToken);    //TODO: Get this from DI
        await action.UpdateAsync(workItemId, new List<string> { tagToAdd }, new List<string> { tagToRemove });
    }

    public async Task AddCommentAsync(string project, string workItemId, string title, string comment)
    {
        var action = new WorkItemComments(project, orgName, personalAccessToken);    //TODO: Get this from DI
        await action.AddAsync(workItemId, title, comment);
    }
}