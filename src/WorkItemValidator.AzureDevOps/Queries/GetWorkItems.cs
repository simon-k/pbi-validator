using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;

namespace WorkItemValidator.AzureDevOps.Queries;

internal class GetWorkItems
{
    private readonly Uri _uri;
    private readonly string _personalAccessToken;

    public GetWorkItems(string orgName, string personalAccessToken)
    {
        _uri = new Uri("https://dev.azure.com/" + orgName);
        _personalAccessToken = personalAccessToken;
    }
    
    public async Task<IList<WorkItem>> OpenWorkItemsAsync(string project, string teamName, string type, int sprintOffset = 0)
    {
        var credentials = new VssBasicCredential(string.Empty, _personalAccessToken);

        var wiql = new Wiql()
        {
            Query = "Select [Id] " +
                    "From WorkItems " +
                    "Where [Work Item Type] = '" + type + "' " +
                    "And [System.TeamProject] = '" + project + "' " +
                    "And [System.State] <> 'Closed' " +
                    "And [System.IterationPath] =  @currentIteration('[" + project + "]\\"+teamName+"')+" + sprintOffset
        };

        // create instance of work item tracking http client
        using (var httpClient = new WorkItemTrackingHttpClient(_uri, credentials))          //TODO: Use httpClient to constructor. No need to make one for each call. Remember to dispose it
        {
            // execute the query to get the list of work items in the results
            var result = await httpClient.QueryByWiqlAsync(wiql).ConfigureAwait(false);
            var ids = result.WorkItems.Select(item => item.Id).ToArray();

            // some error handling
            if (ids.Length == 0)
            {
                return Array.Empty<WorkItem>();
            }

            // build a list of the fields we want to see
            var fields = new[]
            {
                "System.Id", 
                "System.Title", 
                "System.State", 
                "System.Description", 
                "Microsoft.VSTS.Common.AcceptanceCriteria", 
                "System.WorkItemType", 
                "Microsoft.VSTS.Scheduling.Effort", //"Microsoft.VSTS.Scheduling.StoryPoints",
                "System.IterationPath",
            };

            // get work items for the ids found in query
            return await httpClient.GetWorkItemsAsync(ids, fields, result.AsOf).ConfigureAwait(false);
        }
    }
}