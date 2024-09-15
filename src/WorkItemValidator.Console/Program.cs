using Spectre.Console;
using WorkItemValidator.Analyzer;
using WorkItemValidator.AzureDevOps;
using WorkItemValidator.Domain;

//TODO: Bootstrap all the objects needed.
var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new Exception("Create an environment variable named 'OPENAI_API_KEY' with your OpenAI API key");
var azureDevOpsPat = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT") ?? throw new Exception("Create an environment variable named 'AZURE_DEVOPS_PAT' with your Azure DevOps Personal Access Token");

Console.Clear();
Console.WriteLine("Validate the work items against the Definition of Done");

var workItems = await GetWorkItems();
var results = await GetResults(workItems);

//TODO: Extract
var table = new Table();
table.AddColumn("[green]ID[/]");
table.AddColumn("[green]Title[/]");
table.AddColumn("[green]DoR Pass[/]");
table.AddColumn("[green]Message[/]");
var index = 0;
foreach (var result in results)
{
    var color = index++ % 2 == 0 ? "blue" : "skyblue2";
    table.AddRow(
        new Markup($"[{color}]{result.Id}[/]"), 
        new Markup($"[{color}]{result.Title}[/]"), 
        new Markup($"[{color}]{result.PassedText}[/]"), 
        new Markup($"[{color}]{result.Message}[/]"));
    
    if (results.Count > index)
        table.AddRow("","","","");
}
AnsiConsole.Write(table);

//TODO: Extract
var failedCount = results.Count(result => result.Passed == false);
AnsiConsole.MarkupLine(failedCount > 0
    ? $":broken_heart: [red]{failedCount} of {results.Count} work items failed[/]"
    : ":party_popper: [green]all work items passed[/]");

await TagWorkItems(results);
await CommentWorkItems(results);

return;

async Task CommentWorkItems(IList<AnalyzerResult> results)
{
    //TODO Add project and team name to ado constructor
    var ado = new AzureDevOps("Simon-k", azureDevOpsPat);
    foreach (var result in results)
    {
        //Generate a title and a message
        var title = result.Passed ? "DoR Passed" : "DoR Failed";
        await ado.AddCommentAsync("dor-test", result.Id, title, result.Message);
    }
    AnsiConsole.MarkupLine(":speech_balloon: [green]Work items commented[/]");
}


async Task TagWorkItems(IList<AnalyzerResult> results)
{
    await AnsiConsole.Status().Spinner(Spinner.Known.Star).StartAsync(
        "Tagging work items...", async _ => { 
            //TODO: make org and project name and team name configurable
            var ado = new AzureDevOps("Simon-k", azureDevOpsPat);
            foreach (var result in results)
            {
                var tagToAdd = result.Passed ? "DoR Passed" : "DoR Failed";
                var tagToRemove = result.Passed ? "DoR Failed" : "DoR Passed";
                await ado.UpdateTagsAsync(result.Id, tagToAdd, tagToRemove);    
            }
        });
    AnsiConsole.MarkupLine(":bookmark: [green]Work items tagged[/]");
}

async Task<IList<WorkItem>> GetWorkItems()
{
    return await AnsiConsole.Status().Spinner(Spinner.Known.Star).StartAsync(
        "Getting work items from Azure DevOps...",
        _ => { 
            var ado = new AzureDevOps("Simon-k", azureDevOpsPat);
            return ado.GetWorkItemsAsync("dor-test", "dor-test-team");
        });
}

async Task<IList<AnalyzerResult>> GetResults(IList<WorkItem> workItems)
{
    var dor = new DefinitionOfReady(openAiApiKey);
    var analyzerResults = new List<AnalyzerResult>();
    
    await AnsiConsole.Progress()
        .Columns([
            new SpinnerColumn(Spinner.Known.Star),  
            new TaskDescriptionColumn(),            
            new ProgressBarColumn(),        
            new PercentageColumn(),         
            new RemainingTimeColumn()
        ])
        .AutoClear(true)
        .AutoRefresh(true) 
        .StartAsync(async context =>
        {
            var progress = context.AddTask("Analyze Work Items");
            progress.MaxValue = workItems.Count();
            foreach (var workItem in workItems)
            {
                //TODO: Are these done once at a time? If not, it might fuck up the chat history...
                var result = await dor.AnalyzeAsync(workItem);
                analyzerResults.Add(result);
                progress.Value++;
            }
        });
    
    return analyzerResults;
}

