import os
from dotenv import load_dotenv
from pathlib import Path
# Add references
from azure.ai.projects import AIProjectClient
from azure.ai.agents.models import ConnectedAgentTool, MessageRole, ListSortOrder, FilePurpose, CodeInterpreterTool
from azure.identity import DefaultAzureCredential

# Clear the console
os.system('cls' if os.name=='nt' else 'clear')

# Load environment variables from .env file
load_dotenv()
project_endpoint= os.getenv("PROJECT_ENDPOINT")
model_deployment = os.getenv("MODEL_DEPLOYMENT_NAME")

# Agent name constants
PRINCIPLES_AGENT_NAME = "principles_expert"
PRINCIPLES_PT_BR_AGENT_NAME = "principles_expert_pt_br"
ORCHESTRATOR_AGENT_NAME = "principles_orchestrator"

# Define the instructions for the principles agent
principles_instructions = """
You are an expert on the company principles. Your job is to answer questions related to the core principles 
of the company using the uploaded reference document available in the code interpreter tool.

Your role is to answer ONLY questions that relate directly to the content of the principles. You must:
1. Refer to the uploaded company principles document to support your answer.
2. Use only the exact wording (verbatim) from the document.
3. Not interpret, paraphrase, or summarize any principle.
4. Politely decline to answer questions not related to the document or its content.
5. Keep your tone professional and inspirational, aligned with the company culture of ownership.
6. Format your responses in a clear and readable manner.
7. Always preface your response with "PRINCIPLES_EXPERT > " followed by your answer.

If the user's question does not reference a specific principle or topic found in the document, respond:
"PRINCIPLES_EXPERT > This question is outside the scope of the official principles document."

Use the uploaded document for all answers. Do not infer or create content.

FEW-SHOT EXAMPLES:
User: Why is it important to Dream Big?  
Agent:  
PRINCIPLES_EXPERT > According to Principle #1, "Dream big: A big dream inspires us, drives our business forward and aligns us around the same objectives and purpose - to create a future with more cheers. No matter their role in the organization, all owners set, pursue and work with each other to achieve ambitious goals. Big dreams require hard work, collaboration and passion. They must be feasible so we can commit and bring them to life."

User: What do we say about leading by example?  
Agent:  
PRINCIPLES_EXPERT > According to Principle #3, "Lead by example and take accountability: As owners, our actions matter more than words. We walk the talk, lead by example, do what we mean. We take personal responsibility. In a company of owners, there is no such thing as 'this is not my problem.'"

User: How should we manage diversity in hiring?  
Agent:  
PRINCIPLES_EXPERT > This question is outside the scope of the official principles document.

User: What’s our stance on taking shortcuts?  
Agent:  
PRINCIPLES_EXPERT > According to Principle #10, "Never take shortcuts: Owners don’t take shortcuts and operate with ethics and integrity. The safety of our people, the quality of our products, and our reputation can never be compromised. There is no substitute for hard work, consistency and long-term thinking."

---

Stick strictly to the wording from the document. Do not generate or summarize content under any circumstance.
"""

# Define the instructions for the Portuguese principles agent
principles_pt_br_instructions = """
Você é um especialista nos princípios da empresa. Sua função é responder perguntas relacionadas aos princípios 
fundamentais da empresa usando o documento de referência carregado disponível na ferramenta de interpretação de código.

Seu papel é responder APENAS perguntas que se relacionem diretamente com o conteúdo dos princípios. Você deve:
1. Consultar o documento de princípios da empresa carregado para fundamentar sua resposta.
2. Usar apenas as palavras exatas (literalmente) do documento.
3. Não interpretar, parafrasear ou resumir qualquer princípio.
4. Educadamente recusar-se a responder perguntas não relacionadas ao documento ou seu conteúdo.
5. Manter seu tom profissional e inspirador, alinhado com a cultura de propriedade da empresa.
6. Formatar suas respostas de maneira clara e legível.
7. Sempre iniciar sua resposta com "ESPECIALISTA_PRINCÍPIOS > " seguido por sua resposta.

Se a pergunta do usuário não fizer referência a um princípio ou tópico específico encontrado no documento, responda:
"ESPECIALISTA_PRINCÍPIOS > Esta pergunta está fora do escopo do documento oficial de princípios."

Use o documento carregado para todas as respostas. Não infira ou crie conteúdo.

EXEMPLOS:
Usuário: Por que é importante sonhar grande?  
Agente:  
ESPECIALISTA_PRINCÍPIOS > De acordo com o Princípio #1, "Sonhe grande: Um grande sonho nos inspira, impulsiona nosso negócio e nos alinha em torno dos mesmos objetivos e propósitos - para criar um futuro com mais alegria. Independentemente do papel na organização, todos os proprietários estabelecem, perseguem e trabalham juntos para atingir objetivos ambiciosos. Grandes sonhos exigem trabalho árduo, colaboração e paixão. Eles devem ser viáveis para que possamos nos comprometer e dar vida a eles."

Usuário: O que dizemos sobre liderar pelo exemplo?  
Agente:  
ESPECIALISTA_PRINCÍPIOS > De acordo com o Princípio #3, "Lidere pelo exemplo e assuma responsabilidade: Como proprietários, nossas ações importam mais que palavras. Praticamos o que pregamos, lideramos pelo exemplo, fazemos o que dizemos. Assumimos responsabilidade pessoal. Em uma empresa de proprietários, não existe 'isso não é problema meu'."

Usuário: Como devemos gerenciar a diversidade na contratação?  
Agente:  
ESPECIALISTA_PRINCÍPIOS > Esta pergunta está fora do escopo do documento oficial de princípios.

Usuário: Qual é nossa posição sobre tomar atalhos?  
Agente:  
ESPECIALISTA_PRINCÍPIOS > De acordo com o Princípio #10, "Nunca tome atalhos: Proprietários não tomam atalhos e operam com ética e integridade. A segurança de nossa equipe, a qualidade de nossos produtos e nossa reputação nunca podem ser comprometidas. Não há substituto para o trabalho árduo, consistência e pensamento de longo prazo."

---

Atenha-se estritamente às palavras do documento. Não gere ou resuma conteúdo sob nenhuma circunstância.
"""

# Define the instructions for the orchestrator agent
orchestrator_instructions = f"""
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

Always preface your final response with "ORCHESTRATOR > " followed by your answer.
Please maintain a professional, helpful tone aligned with the company culture of ownership.
"""

# Connect to the agents client
project_client = AIProjectClient(
    endpoint=project_endpoint,
    credential=DefaultAzureCredential()  # Use Azure Default Credential for authentication
    #api_version="latest",
)

with project_client:

    # Display the data to be analyzed
    script_dir = Path(__file__).parent  # Get the directory of the script
    file_path = script_dir / 'data/principles-en-US.txt'

    with file_path.open('r') as file:
        data = file.read() + "\n"
        #print(data)

    # Upload the data file and create a CodeInterpreterTool    
    file = project_client.agents.files.upload_and_poll(
        file_path=file_path,
        purpose=FilePurpose.AGENTS
    )
    print(f"Uploaded {file.filename}")

    code_interpreter = CodeInterpreterTool(file_ids=[file.id])
    
    # Get the path for the Portuguese principles file
    file_path_pt_br = script_dir / 'data/principles-pt-BR.txt'

    with file_path_pt_br.open('r') as file:
        data_pt_br = file.read() + "\n"
        #print(data_pt_br)

    # Upload the Portuguese data file and create a CodeInterpreterTool
    file_pt_br = project_client.agents.files.upload_and_poll(
        file_path=file_path_pt_br,
        purpose=FilePurpose.AGENTS
    )
    print(f"Uploaded {file_pt_br.filename}")

    code_interpreter_pt_br = CodeInterpreterTool(file_ids=[file_pt_br.id])

    # Create the principles agent on the Azure AI agent service
    principles_agent = project_client.agents.create_agent(
        model=model_deployment,
        name=PRINCIPLES_AGENT_NAME,
        instructions=principles_instructions,
        tools=code_interpreter.definitions,
        tool_resources=code_interpreter.resources
    )

    # Create the Portuguese principles agent on the Azure AI agent service
    principles_pt_br_agent = project_client.agents.create_agent(
        model=model_deployment,
        name=PRINCIPLES_PT_BR_AGENT_NAME,
        instructions=principles_pt_br_instructions,
        tools=code_interpreter_pt_br.definitions,
        tool_resources=code_interpreter_pt_br.resources
    )

    # Create a connected agent tool for the principles agent
    principles_agent_tool = ConnectedAgentTool(
        id=principles_agent.id, 
        name=PRINCIPLES_AGENT_NAME, 
        description="Expert on company principles who can answer questions about the 10 principles in English."
    )

    # Create a connected agent tool for the Portuguese principles agent
    principles_pt_br_agent_tool = ConnectedAgentTool(
        id=principles_pt_br_agent.id, 
        name=PRINCIPLES_PT_BR_AGENT_NAME, 
        description="Expert on company principles who can answer questions about the 10 principles in Brazilian Portuguese."
    )

    # Create a main agent with the Connected Agent tools
    principles_orchestrator = project_client.agents.create_agent(
        model=model_deployment,
        name=ORCHESTRATOR_AGENT_NAME,
        instructions=orchestrator_instructions,
        tools=[
            principles_agent_tool.definitions[0],
            principles_pt_br_agent_tool.definitions[0],
        ]
    )

    # Create thread for the chat session
    print("Creating agent thread.")
    thread = project_client.agents.threads.create()

    # Create the English quest prompt
    prompt = "List the company's principles"

    # Send a prompt to the agent
    message = project_client.agents.messages.create(
        thread_id=thread.id,
        role=MessageRole.USER,
        content=prompt,
    )

    # Create and process Agent run in thread with tools
    print("Processing agent thread for English question. Please wait.")
    run = project_client.agents.runs.create_and_process(thread_id=thread.id, agent_id=principles_orchestrator.id)

    if run.status == "failed":
        print(f"Run failed: {run.last_error}")

    # Fetch and log all messages
    messages = project_client.agents.messages.list(thread_id=thread.id, order=ListSortOrder.ASCENDING)
    for message in messages:
        if message.text_messages:
            last_msg = message.text_messages[-1]
            print(f"{message.role}:\n{last_msg.text.value}\n")
    
    # Create a new thread for the Portuguese example
    print("\nCreating agent thread for Portuguese question.")
    thread_pt_br = project_client.agents.threads.create()

    # Create the Portuguese quest prompt
    prompt_pt_br = "Liste os princípios da empresa"

    # Send a Portuguese prompt to the agent
    message_pt_br = project_client.agents.messages.create(
        thread_id=thread_pt_br.id,
        role=MessageRole.USER,
        content=prompt_pt_br,
    )

    # Create and process Agent run in thread with tools
    print("Processing agent thread for Portuguese question. Please wait.")
    run_pt_br = project_client.agents.runs.create_and_process(thread_id=thread_pt_br.id, agent_id=principles_orchestrator.id)

    if run_pt_br.status == "failed":
        print(f"Run failed: {run_pt_br.last_error}")

    # Fetch and log all messages for Portuguese thread
    messages_pt_br = project_client.agents.messages.list(thread_id=thread_pt_br.id, order=ListSortOrder.ASCENDING)
    for message in messages_pt_br:
        if message.text_messages:
            last_msg = message.text_messages[-1]
            print(f"{message.role}:\n{last_msg.text.value}\n")
    
    # Delete the Agent when done
    print("Cleaning up agents:")
    project_client.agents.delete_agent(principles_orchestrator.id)
    print("Deleted quest orchestrator agent.")

    # Delete the connected Agents when done
    project_client.agents.delete_agent(principles_agent.id)
    print("Deleted principles agent.")
    
    project_client.agents.delete_agent(principles_pt_br_agent.id)
    print("Deleted Portuguese principles agent.")