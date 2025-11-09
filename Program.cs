using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using OpenAI;
using DotNetEnv;

Env.Load("./.env");

var github_endpoint = Environment.GetEnvironmentVariable("GITHUB_ENDPOINT") ?? throw new InvalidOperationException("GITHUB_ENDPOINT is not set.");
var github_model_id = Environment.GetEnvironmentVariable("GITHUB_MODEL_ID") ?? "gpt-4o-mini";
var github_token = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? throw new InvalidOperationException("GITHUB_TOKEN is not set.");

var openAIOptions = new OpenAIClientOptions()
{
    Endpoint = new Uri(github_endpoint)  // Set custom endpoint for GitHub Models API
};

var openAIClient = new OpenAIClient(new ApiKeyCredential(github_token), openAIOptions);

const string AGENT_NAME = "MovieSuggestionAgent";

const string AGENT_INSTRUCTIONS = @"You are a helpful AI Agent that can help suggest movies to watch based on what's available on the user's Jellyfin media server.";

AIAgent agent = new OpenAIClient(new ApiKeyCredential(github_token), openAIOptions)
    .GetChatClient(github_model_id)  // Get chat client for the specified AI model
    .CreateAIAgent(
        name: AGENT_NAME,             // Set agent identifier for logging and tracking
        instructions: AGENT_INSTRUCTIONS,  // Comprehensive behavior and personality instructions
        tools: [AIFunctionFactory.Create((Func<Task<List<string>>>)JellyfinTools.GetAllMovies)]  // Register tool functions
    );

AgentThread thread = agent.GetNewThread();


Console.WriteLine(await agent.RunAsync("Find a spooky movie for me to watch tonight.", thread));

