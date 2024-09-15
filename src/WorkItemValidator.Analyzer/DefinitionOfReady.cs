using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using WorkItemValidator.Domain;

namespace WorkItemValidator.Analyzer;

public class DefinitionOfReady
{
    private readonly IChatCompletionService _chatService;
    private readonly OpenAIPromptExecutionSettings _settings;
    private readonly Kernel _kernel;
    
    public DefinitionOfReady(string openAiApiKey)
    {
        var handler = new HttpClientHandler();
        handler.CheckCertificateRevocationList = false;
        var client = new HttpClient(handler);
        
        var kernelBuilder = Kernel.CreateBuilder();
        
        _kernel = kernelBuilder
            .AddOpenAIChatCompletion("gpt-4o", openAiApiKey, httpClient: client) 
            .Build();
     
        _kernel.ImportPluginFromType<DefinitionOfReadyPlugin>(); 
        
        _settings = new OpenAIPromptExecutionSettings() {ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions};
        _chatService = _kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<AnalyzerResult> AnalyzeAsync(WorkItem workItem)
    {
        var chatHistory = new ChatHistory("You are a chat bot that knows about software development project management. " +
                                          "You help a team of developers to manage their project. " +
                                          "The team has a definition of ready that is a checklist of tasks that need to be completed in order to start working on a feature. " +
                                          "When you are analyzing a work item, you should check if it follows the definition of ready. " +
                                          "A user story has the form 'As, I, So'. " +
                                          "A risk assessment does not have to be very formal, it can just state the risk and what to do if something happens. " +
                                          "Answer the question with a 'Yes' if the work item follows the Definition of ready. " +
                                          "If the work item does not follow the definition of ready then don't say no, but gve a short description of what is missing. " +
                                          "Do not use the characters '[' and ']' in your answers. ");
        
        var workItemJson = JsonSerializer.Serialize(workItem);
        chatHistory.AddUserMessage(workItemJson);
        chatHistory.AddUserMessage("Does my work item follow the Definition of Ready?");
        var answer = await _chatService.GetChatMessageContentAsync(chatHistory, _settings, _kernel);
        chatHistory.Add(answer);

        return new AnalyzerResult
        {
            Id = workItem.Id,
            Title = workItem.Title,
            Passed = answer.ToString().StartsWith("Yes", StringComparison.InvariantCultureIgnoreCase),
            Message = answer.ToString()
        };
    }
    
    //TODO: Extract to plugins folder
    class DefinitionOfReadyPlugin
    {
        [KernelFunction]
        [Description("Get the Definition of Ready")]
        public string GetDefinitionOfReady()
        {
            //TODO: Read the DoR from the file.
            return """
                   # Definition Of Ready
                   This is the teams list of criteria that a work item must meet before it can be accepted into a sprint.
                   
                   The work item must 
                   * have a user story
                   * have acceptance criteria
                   * have an estimate
                   * have a risk analysis and mitigation efforts
                   """;
        }
        
        [KernelFunction]
        [Description("Get an estimate of a work item")]
        public string GetEstimate(
            [Description("The work item")] WorkItem workItem, 
            [Description("The lowest estimate from the defenition of ready")] int lowerLimit, 
            [Description("The lowest estimate from the defenition of ready")] int upperLimit)
        {
            if (workItem.Estimate == null)
            {
                return "The estimate is missing";
            }
            
            if (workItem.Estimate <= /*lowerLimit*/ 0)   //Gpt40 is not good at numbers... Use the o1 model when it comes out
            {
                return "The estimate is too low";
            } 
            
            if (workItem.Estimate > /*upperLimit*/13)  //Gpt40 is not good at numbers... Use the o1 model when it comes out
            {
                return "The estimate is too high";
            }
            
            return "The estimate is within the limits";
        }
    }
}