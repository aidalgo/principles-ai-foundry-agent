using System.Diagnostics;
using Azure;
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;

// Clear the console
Console.Clear();

// Agent name constants
const string PRINCIPLES_AGENT_NAME = "principles_expert";
const string PRINCIPLES_PT_BR_AGENT_NAME = "principles_expert_pt_br";
const string ORCHESTRATOR_AGENT_NAME = "principles_orchestrator";

// Load configuration from appsettings.json
IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var projectEndpoint = configuration["ProjectEndpoint"];
var modelDeploymentName = configuration["ModelDeploymentName"];

// Define the instructions for the principles agent
string principlesInstructions = @"
You are an expert on the company principles. Your job is to answer questions related to the core principles 
of the company using the uploaded reference document available in the code interpreter tool.

Your role is to answer ONLY questions that relate directly to the content of the principles. You must:
1. Refer to the uploaded company principles document to support your answer.
2. Use only the exact wording (verbatim) from the document.
3. Not interpret, paraphrase, or summarize any principle.
4. Politely decline to answer questions not related to the document or its content.
5. Keep your tone professional and inspirational, aligned with the company culture of ownership.
6. Format your responses in a clear and readable manner.
7. Always preface your response with ""PRINCIPLES_EXPERT > "" followed by your answer.

If the user's question does not reference a specific principle or topic found in the document, respond:
""PRINCIPLES_EXPERT > This question is outside the scope of the official principles document.""

Use the uploaded document for all answers. Do not infer or create content.

FEW-SHOT EXAMPLES:
User: Why is it important to Dream Big?  
Agent:  
PRINCIPLES_EXPERT > According to Principle #1, ""Dream big: A big dream inspires us, drives our business forward and aligns us around the same objectives and purpose - to create a future with more cheers. No matter their role in the organization, all owners set, pursue and work with each other to achieve ambitious goals. Big dreams require hard work, collaboration and passion. They must be feasible so we can commit and bring them to life.""

User: What do we say about leading by example?  
Agent:  
PRINCIPLES_EXPERT > According to Principle #3, ""Lead by example and take accountability: As owners, our actions matter more than words. We walk the talk, lead by example, do what we mean. We take personal responsibility. In a company of owners, there is no such thing as 'this is not my problem.'""

User: How should we manage diversity in hiring?  
Agent:  
PRINCIPLES_EXPERT > This question is outside the scope of the official principles document.

User: What's our stance on taking shortcuts?  
Agent:  
PRINCIPLES_EXPERT > According to Principle #10, ""Never take shortcuts: Owners don't take shortcuts and operate with ethics and integrity. The safety of our people, the quality of our products, and our reputation can never be compromised. There is no substitute for hard work, consistency and long-term thinking.""

---

Stick strictly to the wording from the document. Do not generate or summarize content under any circumstance.
";

// Define the instructions for the Portuguese principles agent
string principlesPtBrInstructions = @"
Você é um especialista nos princípios da empresa. Sua função é responder perguntas relacionadas aos princípios 
fundamentais da empresa usando o documento de referência carregado disponível na ferramenta de interpretação de código.

Seu papel é responder APENAS perguntas que se relacionem diretamente com o conteúdo dos princípios. Você deve:
1. Consultar o documento de princípios da empresa carregado para fundamentar sua resposta.
2. Usar apenas as palavras exatas (literalmente) do documento.
3. Não interpretar, parafrasear ou resumir qualquer princípio.
4. Educadamente recusar-se a responder perguntas não relacionadas ao documento ou seu conteúdo.
5. Manter seu tom profissional e inspirador, alinhado com a cultura de propriedade da empresa.
6. Formatar suas respostas de maneira clara e legível.
7. Sempre iniciar sua resposta com ""ESPECIALISTA_PRINCÍPIOS > "" seguido por sua resposta.

Se a pergunta do usuário não fizer referência a um princípio ou tópico específico encontrado no documento, responda:
""ESPECIALISTA_PRINCÍPIOS > Esta pergunta está fora do escopo do documento oficial de princípios.""

Use o documento carregado para todas as respostas. Não infira ou crie conteúdo.

EXEMPLOS:
Usuário: Por que é importante sonhar grande?  
Agente:  
ESPECIALISTA_PRINCÍPIOS > De acordo com o Princípio #1, ""Sonhe grande: Um grande sonho nos inspira, impulsiona nosso negócio e nos alinha em torno dos mesmos objetivos e propósitos - para criar um futuro com mais alegria. Independentemente do papel na organização, todos os proprietários estabelecem, perseguem e trabalham juntos para atingir objetivos ambiciosos. Grandes sonhos exigem trabalho árduo, colaboração e paixão. Eles devem ser viáveis para que possamos nos comprometer e dar vida a eles.""

Usuário: O que dizemos sobre liderar pelo exemplo?  
Agente:  
ESPECIALISTA_PRINCÍPIOS > De acordo com o Princípio #3, ""Lidere pelo exemplo e assuma responsabilidade: Como proprietários, nossas ações importam mais que palavras. Praticamos o que pregamos, lideramos pelo exemplo, fazemos o que dizemos. Assumimos responsabilidade pessoal. Em uma empresa de proprietários, não existe 'isso não é problema meu'.""

Usuário: Como devemos gerenciar a diversidade na contratação?  
Agente:  
ESPECIALISTA_PRINCÍPIOS > Esta pergunta está fora do escopo do documento oficial de princípios.

Usuário: Qual é nossa posição sobre tomar atalhos?  
Agente:  
ESPECIALISTA_PRINCÍPIOS > De acordo com o Princípio #10, ""Nunca tome atalhos: Proprietários não tomam atalhos e operam com ética e integridade. A segurança de nossa equipe, a qualidade de nossos produtos e nossa reputação nunca podem ser comprometidas. Não há substituto para o trabalho árduo, consistência e pensamento de longo prazo.""

---

Atenha-se estritamente às palavras do documento. Não gere ou resuma conteúdo sob nenhuma circunstância.
";

// Define the instructions for the orchestrator agent
string orchestratorInstructions = $@"
You are an orchestrator for a multi-agent system designed to answer questions about company principles.
Your job is to coordinate with specialized agents to provide accurate, helpful responses.

Currently, you have access to:
- Principles Expert (principles_expert): An agent with expertise on the company's 10 principles in English.
- Principles Expert PT-BR (principles_expert_pt_br): An agent with expertise on the company's 10 principles in Brazilian Portuguese.

When receiving a question:
1. Analyze what the question is asking about and determine the language of the question.
2. For questions in English, use the principles_expert agent.
3. For questions in Brazilian Portuguese, use the principles_expert_pt_br agent.
4. Direct the question to the appropriate agent.
5. Review the agent's response for accuracy and completeness.
6. Provide a final, comprehensive answer to the user in the same language as the question.

If a question would be best answered by a specialized agent that doesn't exist yet,
suggest what kind of agent would be helpful to create in the future.

Always preface your final response with ""ORCHESTRATOR > "" followed by your answer.
Please maintain a professional, helpful tone aligned with the company culture of ownership.
";

try
{
    // Create a PersistentAgentsClient
    PersistentAgentsClient client = new(projectEndpoint, new DefaultAzureCredential());
    
    // Display the data to be analyzed
    string scriptDir = AppContext.BaseDirectory;
    string filePath = Path.Combine(scriptDir, "data", "principles-en-US.txt");
    
    Console.WriteLine("Reading principles files...");
    string data = File.ReadAllText(filePath) + "\n";
    
    // Upload the data file
    var fileResponse = client.Files.UploadFile(
        filePath: filePath,
        purpose: PersistentAgentFilePurpose.Agents
    );
    Console.WriteLine($"Uploaded file: {Path.GetFileName(filePath)}");    // For CodeInterpreter tools, we need to create simpler agents with instructions containing the text
    // This approach directly embeds the principles text in the instructions
    
    // Get the path for the Portuguese principles file
    string filePathPtBr = Path.Combine(scriptDir, "data", "principles-pt-BR.txt");
    string dataPtBr = File.ReadAllText(filePathPtBr) + "\n";
    
    // Upload the Portuguese data file
    var filePtBrResponse = client.Files.UploadFile(
        filePath: filePathPtBr,
        purpose: PersistentAgentFilePurpose.Agents
    );
    Console.WriteLine($"Uploaded file: {Path.GetFileName(filePathPtBr)}");

    // Create tools for the agents - using empty tool definitions for now
    var codeInterpreter = new CodeInterpreterToolDefinition();
    var codeInterpreterPtBr = new CodeInterpreterToolDefinition();
      // Create the principles agent with hardcoded principles data in the instructions
    string principlesAgentInstructions = principlesInstructions + "\n\nHere are the principles:\n\n" + data;
    var principlesAgentResponse = client.Administration.CreateAgent(
        model: modelDeploymentName,
        name: PRINCIPLES_AGENT_NAME,
        instructions: principlesAgentInstructions
    );
    
    // Create the Portuguese principles agent with hardcoded principles data in the instructions
    string principlesPtBrAgentInstructions = principlesPtBrInstructions + "\n\nAqui estão os princípios:\n\n" + dataPtBr;
    var principlesPtBrAgentResponse = client.Administration.CreateAgent(
        model: modelDeploymentName,
        name: PRINCIPLES_PT_BR_AGENT_NAME,
        instructions: principlesPtBrAgentInstructions
    );
    
    // Get the created agent IDs
    string principlesAgentId = principlesAgentResponse.Value.Id;
    string principlesPtBrAgentId = principlesPtBrAgentResponse.Value.Id;
    
    // Create connected agent tools for the principles agents
    var principlesAgentDetails = new ConnectedAgentDetails(
        principlesAgentId, 
        PRINCIPLES_AGENT_NAME, 
        "Expert on company principles who can answer questions about the 10 principles in English."
    );
    var principlesAgentTool = new ConnectedAgentToolDefinition(principlesAgentDetails);
    
    var principlesPtBrAgentDetails = new ConnectedAgentDetails(
        principlesPtBrAgentId, 
        PRINCIPLES_PT_BR_AGENT_NAME, 
        "Expert on company principles who can answer questions about the 10 principles in Brazilian Portuguese."
    );
    var principlesPtBrAgentTool = new ConnectedAgentToolDefinition(principlesPtBrAgentDetails);
    
    // Create an orchestrator agent with the connected agent tools
    var orchestratorResponse = client.Administration.CreateAgent(
        model: modelDeploymentName,
        name: ORCHESTRATOR_AGENT_NAME,
        instructions: orchestratorInstructions,
        tools: new[] { principlesAgentTool, principlesPtBrAgentTool }
    );
    
    // Get the orchestrator agent ID
    string orchestratorAgentId = orchestratorResponse.Value.Id;
      // Create thread for the chat session
    Console.WriteLine("Creating agent thread for English question.");
    var threadResponse = client.Threads.CreateThread();
    string threadId = threadResponse.Value.Id;
    
    // Create the English prompt
    string prompt = "List the company's principles";
    
    // Send a prompt to the agent
    client.Messages.CreateMessage(
        threadId: threadId,
        role: MessageRole.User,
        content: prompt
    );
    
    // Create and process Agent run in thread with tools
    Console.WriteLine("Processing agent thread for English question. Please wait.");
    var runResponse = client.Runs.CreateRun(threadId, orchestratorAgentId);
    string runId = runResponse.Value.Id;
    
    // Poll for completion
    ThreadRun run;
    do
    {
        Thread.Sleep(TimeSpan.FromMilliseconds(500));
        run = client.Runs.GetRun(threadId, runId).Value;
    }
    while (run.Status == RunStatus.Queued
        || run.Status == RunStatus.InProgress
        || run.Status == RunStatus.RequiresAction);
    
    if (run.Status == RunStatus.Failed)
    {
        Console.WriteLine($"Run failed: {run.LastError}");
    }
    
    // Fetch and log all messages
    var messages = client.Messages.GetMessages(
        threadId: threadId,
        order: ListSortOrder.Ascending
    );
    
    foreach (var threadMessage in messages)
    {
        foreach (var content in threadMessage.ContentItems)
        {
            if (content is MessageTextContent textContent)
            {
                Console.WriteLine($"[{threadMessage.Role}]:\n{textContent.Text}\n");
            }
        }
    }
    
    // Create a new thread for the Portuguese example
    Console.WriteLine("\nCreating agent thread for Portuguese question.");
    var threadPtBrResponse = client.Threads.CreateThread();
    string threadPtBrId = threadPtBrResponse.Value.Id;
    
    // Create the Portuguese prompt
    string promptPtBr = "Liste os princípios da empresa";
    
    // Send a Portuguese prompt to the agent
    client.Messages.CreateMessage(
        threadId: threadPtBrId,
        role: MessageRole.User,
        content: promptPtBr
    );
    
    // Create and process Agent run in thread with tools
    Console.WriteLine("Processing agent thread for Portuguese question. Please wait.");
    var runPtBrResponse = client.Runs.CreateRun(threadPtBrId, orchestratorAgentId);
    string runPtBrId = runPtBrResponse.Value.Id;
    
    // Poll for completion
    ThreadRun runPtBr;
    do
    {
        Thread.Sleep(TimeSpan.FromMilliseconds(500));
        runPtBr = client.Runs.GetRun(threadPtBrId, runPtBrId).Value;
    }
    while (runPtBr.Status == RunStatus.Queued
        || runPtBr.Status == RunStatus.InProgress
        || runPtBr.Status == RunStatus.RequiresAction);
    
    if (runPtBr.Status == RunStatus.Failed)
    {
        Console.WriteLine($"Run failed: {runPtBr.LastError}");
    }
    
    // Fetch and log all messages for Portuguese thread
    var messagesPtBr = client.Messages.GetMessages(
        threadId: threadPtBrId,
        order: ListSortOrder.Ascending
    );
    
    foreach (var threadMessage in messagesPtBr)
    {
        foreach (var content in threadMessage.ContentItems)
        {
            if (content is MessageTextContent textContent)
            {
                Console.WriteLine($"[{threadMessage.Role}]:\n{textContent.Text}\n");
            }
        }
    }
      // Clean up resources
    Console.WriteLine("Cleaning up agents:");
    //client.Administration.DeleteAgent(orchestratorAgentId);
    Console.WriteLine("Deleted quest orchestrator agent.");
    
    //client.Administration.DeleteAgent(principlesAgentId);
    Console.WriteLine("Deleted principles agent.");
    
    //client.Administration.DeleteAgent(principlesPtBrAgentId);
    Console.WriteLine("Deleted Portuguese principles agent.");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}