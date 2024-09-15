using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;

namespace AdoWorkItemValidator.Console.Services;

public class Ado
{

   /* private readonly Uri _uri;
    private readonly string _personalAccessToken;
*/
    /// <summary>
    ///     Initializes a new instance of the <see cref="Ado" /> class.
    /// </summary>
    /// <param name="orgName">
    ///     An organization in Azure DevOps Services.
    /// </param>
    /// <param name="personalAccessToken">
    ///     A Personal Access Token for ADO. Needs the following permissions:
    ///     * ??? />.
    /// </param>
   /* public Ado(string orgName, string personalAccessToken)
    {
        _uri = new Uri("https://dev.azure.com/" + orgName);
        _personalAccessToken = personalAccessToken;
    }
    
    public async Task<IList<WorkItem>> QueryOpenWorkItems(string project, string teamName, int sprintOffset = 0)
    {
        var credentials = new VssBasicCredential(string.Empty, _personalAccessToken);

        var wiql = new Wiql()
        {
            Query = "Select [Id] " +
                    "From WorkItems " +
                    "Where [Work Item Type] = 'User Story' " +
                    "And [System.TeamProject] = '" + project + "' " +
                    "And [System.State] <> 'Closed' " +
                    "And [System.IterationPath] =  @currentIteration('[" + project + "]\\"+teamName+"')+" + sprintOffset
        };

        // create instance of work item tracking http client
        using (var httpClient = new WorkItemTrackingHttpClient(_uri, credentials))
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
                "Microsoft.VSTS.Scheduling.StoryPoints",
                "System.IterationPath",
            };

            // get work items for the ids found in query
            return await httpClient.GetWorkItemsAsync(ids, fields, result.AsOf).ConfigureAwait(false);
        }
    }*/
}