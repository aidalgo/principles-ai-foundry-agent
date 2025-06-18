# Principles Agent - Python Guide

This README provides instructions on how to set up and run the Python version of the Principles Agent application, which creates a multi-agent system to answer questions about company principles in both English and Brazilian Portuguese.

## Prerequisites

- Python 3.8 or later
- Azure account with appropriate permissions
- Azure AI Project resource configured

## Setup Instructions

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd principles-agent
   ```

2. **Install dependencies**

   Navigate to the Python directory and install the required packages:

   ```bash
   cd src/python
   pip install -r requirements.txt
   ```

3. **Configure environment variables**

   Create a `.env` file in the `src/python` directory with the following content:

   ```
   PROJECT_ENDPOINT=your-azure-ai-project-endpoint
   MODEL_DEPLOYMENT_NAME=your-model-deployment-name  # e.g., gpt-4o
   ```

   Replace the placeholder values with your actual Azure AI Project endpoint and model deployment name.

## Running the Application

Execute the principles-agents.py script from the Python directory:

```bash
cd src/python
python principles-agents.py
```

## What the Application Does

The application performs the following functions:

1. **Authentication**: Connects to Azure AI Project service using DefaultAzureCredential

2. **Data Upload**: Uploads principles documents in both English and Portuguese from the `data/` directory

3. **Agent Creation**: Creates three specialized agents:
   - `principles_expert`: Answers questions about principles in English
   - `principles_expert_pt_br`: Answers questions about principles in Portuguese 
   - `principles_orchestrator`: Coordinates between the specialized agents based on query language

4. **Query Demonstration**: Runs sample queries in both languages to demonstrate functionality
   - English: "List the company's principles"
   - Portuguese: "Liste os princípios da empresa"

5. **Cleanup**: Deletes all created agents when done

## Project Structure

- `principles-agents.py`: Main application script that creates and manages the agents
- `requirements.txt`: Python dependencies
- `data/`: Directory containing the principles text files
  - `principles-en-US.txt`: English language principles document
  - `principles-pt-BR.txt`: Portuguese language principles document

## Implementation Details

The Python implementation:

1. Uses the `azure.ai.projects` library for creating and managing AI projects
2. Uses the `azure.ai.agents.models` for agent-specific operations
3. Uses `DefaultAzureCredential` for Azure authentication
4. Loads environment variables from a `.env` file
5. Uploads principles documents to make them available to the agents via code interpreter tools
6. Creates connected agent tools to enable the orchestrator to delegate to specialized agents

## Troubleshooting

- **Authentication Issues**: If you encounter authentication problems, ensure you're logged in with the Azure CLI using `az login`

- **Missing Environment Variables**: If the application fails to start, check that your `.env` file exists and contains the correct values

- **File Not Found Errors**: Verify that the principles text files exist in the expected `data/` directory

- **Azure AI Project Errors**: Ensure your Azure AI Project service is properly configured and that you have the necessary permissions

## Notes

- The application creates temporary agents for demonstration purposes and cleans them up afterward
- The principles documents should contain the company's principles in their respective languages
- The agent system uses Azure AI to understand and respond to questions about the principles

## License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.
