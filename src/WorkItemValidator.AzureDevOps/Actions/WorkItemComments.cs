using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;

namespace WorkItemValidator.AzureDevOps.Actions;

public class WorkItemComments
{
    private readonly Uri _uri;
    private readonly string _project;
    private readonly string _personalAccessToken;

    public WorkItemComments(string project, string orgName, string personalAccessToken)
    {
        _uri = new Uri("https://dev.azure.com/" + orgName);
        _project = project;
        _personalAccessToken = personalAccessToken;
    }
    
    public async Task AddAsync(string workItemId, string title, string message)
    {
        var htmlTitle = $"<div><b>{title}</b></div>";
        var htmlMessage = $"<div>{message.Replace(Environment.NewLine, "</div><div>")}</div>";        
        
        var credentials = new VssBasicCredential(string.Empty, _personalAccessToken);
        using (var httpClient = new WorkItemTrackingHttpClient(_uri, credentials))       //TODO: Use httpClient to constructor. No need to make one for each call. Remember to dispose it
        {
            // TODO: There mus be a better way to get the latest comment. This is not efficient.
            var workItemComments = httpClient.GetCommentsAsync(int.Parse(workItemId)).Result;
            var latestComment = workItemComments.Comments.OrderByDescending(c => c.Revision).FirstOrDefault();
            //Console.WriteLine("Latest comment for " + workItemId + ": " + latestComment.Text);
            
            //TODO: If latest comment has the same title as this new comment then skip the comment
            
            //TODO: Add comment to work item
            var comment = new CommentCreate
            {
                Text = htmlTitle + htmlMessage
            };
            await httpClient.AddCommentAsync(comment, _project, int.Parse(workItemId));
        }
    }
}