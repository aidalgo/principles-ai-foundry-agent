# Principles Agent - C# Guide

This README provides instructions on how to set up and run the C# version of the Principles Agent application. This sample demonstrates how to create and orchestrate multiple agents using Azure AI Foundry with C#, creating a multi-agent system to answer questions about company principles in both English and Brazilian Portuguese.

## Prerequisites

- .NET 8.0 SDK or later
- Azure account with appropriate permissions
- Azure AI Project resource configured

## Setup Instructions

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd principles-agent
   ```

2. **Configure application settings**

   Create or modify the `appsettings.json` file in the `src/csharp` directory by copying from `appsettings.example.json`:

   ```bash
   cd src/csharp
   cp appsettings.example.json appsettings.json
   ```

   Then edit `appsettings.json` with your Azure AI Project details:

   ```json
   {
     "ProjectEndpoint": "https://your-agent-resource.services.ai.azure.com/api/projects/your-project-name",
     "ModelDeploymentName": "your-model-name",
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft": "Warning",
         "Microsoft.Hosting.Lifetime": "Information"
       }
     },
     "AllowedHosts": "*"
   }
   ```

## Running the Application

Build and run the C# application:

```bash
cd src/csharp
dotnet build
dotnet run
```

## What the Application Does

The application performs the following functions:

1. **Authentication**: Connects to Azure AI Project service using DefaultAzureCredential

2. **Data Processing**: Reads principles documents in both English and Portuguese from the `data/` directory

3. **Agent Creation**: Creates three specialized agents:
   - `principles_expert`: Answers questions about principles in English
   - `principles_expert_pt_br`: Answers questions about principles in Portuguese 
   - `principles_orchestrator`: Coordinates between the specialized agents based on query language

4. **Query Demonstration**: Runs sample queries in both languages to demonstrate functionality
   - English: "List the company's principles"
   - Portuguese: "Liste os princípios da empresa"

5. **Cleanup**: Contains code to delete agents (commented out by default)

## Project Structure

- `Program.cs`: Main application entry point and logic
- `csharp.csproj`: Project file with dependencies and configuration
- `appsettings.json`: Configuration file for Azure connection details
- `data/`: Directory containing the principles text files
  - `principles-en-US.txt`: English language principles document
  - `principles-pt-BR.txt`: Portuguese language principles document

## Implementation Details

The C# implementation:

1. Uses the `Azure.AI.Agents.Persistent` library for interacting with Azure AI agents
2. Uses `Azure.Identity` for Azure authentication
3. Loads configuration from `appsettings.json`
4. Creates agents that include the principles text directly in their instructions
5. Sets up connected agent tools to allow the orchestrator to delegate to specialized agents
6. Demonstrates how to create and manage agent threads and runs

## Azure AI Foundry Features Demonstrated

This C# implementation showcases these Azure AI Foundry capabilities:

- Using the .NET SDK for Azure AI Agents to create and manage agents
- Integrating agents with standard .NET configuration patterns
- Creating specialized agents with domain-specific knowledge
- Implementing orchestrator patterns for cross-agent coordination
- Managing agent state through threads and runs
- Handling async operations with agent processing

## Troubleshooting

- **Authentication Issues**: If you encounter authentication problems, ensure you're logged in with the Azure CLI using `az login`

- **Missing Configuration**: Verify that your `appsettings.json` file exists and contains the correct values

- **File Not Found Errors**: Verify that the principles text files exist in the expected `data/` directory and that they are properly configured to be copied to the output directory

- **Azure AI Project Errors**: Ensure your Azure AI Project service is properly configured and that you have the necessary permissions

## Notes

- The application creates temporary agents for demonstration purposes
- The cleanup code is commented out by default; uncomment it if you want to delete the agents after running
- The principles documents should contain the company's principles in their respective languages
- The agent system uses Azure AI to understand and respond to questions about the principles

## License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.
