using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace WorkItemValidator.AzureDevOps.Actions;

public class WorkItemTags
{
    private readonly Uri _uri;
    private readonly string _personalAccessToken;

    public WorkItemTags(string orgName, string personalAccessToken)
    {
        _uri = new Uri("https://dev.azure.com/" + orgName);
        _personalAccessToken = personalAccessToken;
    }
    
    public async Task UpdateAsync(string workItemId, IEnumerable<string> addTags, IEnumerable<string> removeTags)
    {
        var credentials = new VssBasicCredential(string.Empty, _personalAccessToken);
        using (var httpClient = new WorkItemTrackingHttpClient(_uri, credentials))       //TODO: Use httpClient to constructor. No need to make one for each call. Remember to dispose it
        {
            // Fetch the work item
            var workItem = await httpClient.GetWorkItemAsync(int.Parse(workItemId), expand: WorkItemExpand.Fields).ConfigureAwait(false);

            // Combine existing tags with new tags
            var existingTags = workItem.Fields.TryGetValue("System.Tags", out var tags) ? tags.ToString().Split(';').Select(tag => tag.Trim()).ToList() : new List<string>();
            var updatedTags = existingTags.Except(removeTags).Union(addTags).Distinct().ToList();  // TODO: Union already makes tags destinct, so don't call the Distinct() function.
            
            // Create a patch document to update the tags
            var patchDocument = new JsonPatchDocument();
            patchDocument.Add(new JsonPatchOperation
            {
                Operation = Operation.Replace,
                Path = "/fields/System.Tags",
                Value = string.Join(";", updatedTags)
            });

            // Update the work item
            await httpClient.UpdateWorkItemAsync(patchDocument, int.Parse(workItemId)).ConfigureAwait(false);
        }
    }
}