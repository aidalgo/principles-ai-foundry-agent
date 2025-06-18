# Principles Agent Application

This is the main repository for the Principles Agent application, a multi-agent system designed to answer questions about company principles in both English and Brazilian Portuguese.

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
