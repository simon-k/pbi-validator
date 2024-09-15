namespace WorkItemValidator.Domain;

public class AnalyzerResult
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required bool Passed { get; set; }
    public required string Message { get; set; }

    public string PassedText => Passed ? "Yes" : "No";
    
    public override string ToString()
    {
        return $"Id: {Id}, Title: {Title}, Passed: {Passed}, Message: {Message}";
    }
}