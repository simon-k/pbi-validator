# pbi-validator
This is a console application that validates work items in Azure DevOps. It is used to ensure that work items meet the Definition of Ready (DOR) criteria. 

# How to run this 
Right now the application is not configurable and the input is hardcoded. So when cloning this repo, be sure to change the hardcoded values in the `Program.cs` file.

To run the application, you need to have the following environment variables set:

`OPENAI_API_KEY` an API key for OpenAI.

`AZURE_DEVOPS_PAT` a Personal Access Token for Azure DevOps. Permission to read and write work items is required.

The Definiton of Ready (DOR) is found in the DOR.md file.

# TODO
* Fix all the TODOs in the code.
* Make the application configurable and take input as arguments
* Call Analyzer in parallel. When doing so make sure that it can actually run in parallel and the results are not mixed up.
