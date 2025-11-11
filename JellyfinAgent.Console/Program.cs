using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using OpenAI;
using DotNetEnv;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Load environment variables from .env file
        Env.Load("../.env");

        // Initialize Jellyfin client
        var jellyfinUrl = Environment.GetEnvironmentVariable("JELLYFIN_URL") ?? throw new InvalidOperationException("JELLYFIN_URL is not set.");
        var jellyfinUsername = Environment.GetEnvironmentVariable("JELLYFIN_USERNAME") ?? throw new InvalidOperationException("JELLYFIN_USERNAME is not set.");
        var jellyfinPassword = Environment.GetEnvironmentVariable("JELLYFIN_PASSWORD") ?? throw new InvalidOperationException("JELLYFIN_PASSWORD is not set.");

        var jellyfinClient = new JellyfinClient(jellyfinUrl);
        await jellyfinClient.LoginAsync(jellyfinUsername, jellyfinPassword);

        // Initialize JellyfinTools with the authenticated client using dependency injection
        var jellyfinTools = new JellyfinTools(jellyfinClient);

        // Configure GitHub Models API
        var github_endpoint = Environment.GetEnvironmentVariable("GITHUB_ENDPOINT") ?? throw new InvalidOperationException("GITHUB_ENDPOINT is not set.");
        var github_model_id = Environment.GetEnvironmentVariable("GITHUB_MODEL_ID") ?? "gpt-4o-mini";
        var github_token = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? throw new InvalidOperationException("GITHUB_TOKEN is not set.");

        var openAIOptions = new OpenAIClientOptions()
        {
            Endpoint = new Uri(github_endpoint)
        };

        const string AGENT_NAME = "MovieSuggestionAgent";

        const string AGENT_INSTRUCTIONS = @"You are a helpful AI Agent that can suggest movies and TV shows to watch based on what's available on the user's Jellyfin media server.

You have access to several tools to help users find content:
- Search the library by name, genre, or media type
- Get all available genres
- Find movies by specific genre
- Get recently added content
- Get detailed information about specific items
- View the user's favorites
- Search by production year

Be conversational, friendly, and help users discover great content from their personal library!";

        // Register all available Jellyfin tools
        AIAgent agent = new OpenAIClient(new ApiKeyCredential(github_token), openAIOptions)
            .GetChatClient(github_model_id)
            .CreateAIAgent(
                name: AGENT_NAME,
                instructions: AGENT_INSTRUCTIONS,
                tools: [
                    AIFunctionFactory.Create(jellyfinTools.GetAllMoviesAsync),
                    AIFunctionFactory.Create(jellyfinTools.SearchLibraryAsync),
                    AIFunctionFactory.Create(jellyfinTools.GetAllGenresAsync),
                    AIFunctionFactory.Create(jellyfinTools.GetMoviesByGenreAsync),
                    AIFunctionFactory.Create(jellyfinTools.GetRecentlyAddedAsync),
                    AIFunctionFactory.Create(jellyfinTools.GetItemDetailsAsync),
                    AIFunctionFactory.Create(jellyfinTools.GetFavoritesAsync),
                    AIFunctionFactory.Create(jellyfinTools.GetItemsByYearAsync)
                ]
            );

        AgentThread thread = agent.GetNewThread();

        // Welcome message
        Console.WriteLine("🎬 Jellyfin Movie Assistant");
        Console.WriteLine("===========================");
        Console.WriteLine("Ask me about movies and TV shows in your library!");
        Console.WriteLine("Type 'exit' or 'quit' to end the conversation.\n");

        // Continuous conversation loop
        while (true)
        {
            Console.Write("You: ");
            var userInput = Console.ReadLine();

            // Check for exit commands
            if (string.IsNullOrWhiteSpace(userInput) ||
                userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("\n👋 Thanks for chatting! Enjoy your movie!");
                break;
            }

            try
            {
                // Run the agent with the user's input
                Console.WriteLine("\nAssistant: ");
                var response = await agent.RunAsync(userInput, thread);
                Console.WriteLine(response);
                Console.WriteLine(); // Add spacing between exchanges
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Console.WriteLine("Please try again.\n");
            }
        }
    }
}
