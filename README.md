# Principles Agent Application

This is a sample repository demonstrating how to create and orchestrate multiple agents using Azure AI Foundry. The Principles Agent application is a multi-agent system designed to answer questions about company principles in both English and Brazilian Portuguese, showcasing practical patterns for building multi-agent systems with Azure AI services.

## Available Implementations

This repository contains two implementations of the Principles Agent:

1. **[Python Implementation](src/python/README.md)**
   - Uses azure.ai.projects library
   - Detailed setup and running instructions in the Python README

2. **[C# Implementation](src/csharp/README.md)**
   - Uses Azure.AI.Agents.Persistent library
   - Detailed setup and running instructions in the C# README

## Overview

Both implementations create a multi-agent system with:

- An English principles expert
- A Portuguese principles expert
- An orchestrator agent to route questions based on language

The implementations differ in their specific libraries and approaches but achieve the same functionality of demonstrating multi-agent architectures in Azure AI.

## Azure AI Foundry

This sample showcases Azure AI Foundry capabilities including:

- Creating specialized agents with specific roles and knowledge domains
- Implementing orchestration patterns between multiple agents
- Managing agent communication patterns
- Working with multilingual content
- Handling agent life cycle (creation, usage, and cleanup)

These patterns can be adapted for various business applications where complex decision making across multiple domains is required.

## Prerequisites for All Implementations

- Azure account with appropriate permissions
- Azure AI Project resource configured
- Principles documents in both English and Portuguese

## Project Structure

```
principles-agent/
├── src/
│   ├── python/
│   │   ├── README.md           # Python-specific instructions
│   │   ├── principles-agents.py
│   │   ├── requirements.txt
│   │   └── data/
│   │       ├── principles-en-US.txt
│   │       └── principles-pt-BR.txt
│   └── csharp/
│       ├── README.md           # C#-specific instructions
│       ├── Program.cs
│       ├── csharp.csproj
│       ├── appsettings.json
│       └── data/
│           ├── principles-en-US.txt
│           └── principles-pt-BR.txt
└── README.md                   # This main README file
```

## Getting Started

1. Choose your preferred implementation (Python or C#)
2. Follow the specific setup instructions in the respective README file:
   - [Python Setup](src/python/README.md)
   - [C# Setup](src/csharp/README.md)

## Further Information

For implementation-specific details, troubleshooting, and additional notes, please refer to the README in the corresponding directory.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
