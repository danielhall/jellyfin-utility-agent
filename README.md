# Jellyfin Utility Agent

A fun personal project that creates an AI-powered agent to interact with your Jellyfin media server. This agent provides an interactive conversational interface to help you discover and get movie suggestions based on your personal media library.

## What is This?

This is a .NET console application that uses AI agents to provide intelligent assistance for your Jellyfin media server. The agent can analyze your movie collection and provide personalized recommendations, making it easier to decide what to watch. You can have continuous conversations with the agent, and it will remember the context of your previous messages.

## Technologies Used

- **.NET 10.0** - Modern C# framework
- **Microsoft.Agents.AI** - AI agent framework for building conversational agents
- **Microsoft.Extensions.AI** - AI abstraction layer for .NET
- **GitHub Models API** - AI model inference through GitHub's API
- **Jellyfin API** - Integration with Jellyfin media server
- **RestSharp** - HTTP client for API calls

## Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- A running [Jellyfin](https://jellyfin.org/) media server
- A GitHub account with access to [GitHub Models](https://github.com/marketplace/models)

### Installation

1. **Clone the repository**
   ```bash
   git clone <your-repo-url>
   cd jellyfin-utility-agent
   ```

2. **Configure your environment**
   
   Copy the example environment file:
   ```bash
   cp .env.example .env
   ```

3. **Edit `.env` with your credentials**
   
   Open the `.env` file and fill in the required values:
   ```bash
   # GitHub Models API Configuration
   GITHUB_TOKEN=your_github_personal_access_token
   GITHUB_ENDPOINT=https://models.inference.ai.azure.com
   
   # Jellyfin Media Server Configuration
   JELLYFIN_URL=http://your-jellyfin-server:8096
   JELLYFIN_USERNAME=your_jellyfin_username
   JELLYFIN_PASSWORD=your_jellyfin_password
   ```

   **Getting a GitHub Token:**
   - Go to [GitHub Settings → Developer settings → Personal access tokens](https://github.com/settings/tokens)
   - Generate a new token with access to GitHub Models
   - Copy the token to your `.env` file

4. **Restore dependencies**
   ```bash
   dotnet restore
   ```

5. **Build the project**
   ```bash
   dotnet build
   ```

6. **Run the agent**
   ```bash
   dotnet run
   ```

## Features

- **Continuous Conversation** - Interactive chat interface that maintains conversation context
- **Search Library** - Search for movies, TV shows, and other media by name, genre, or media type
- **Genre Discovery** - Browse all available genres and find content by specific genres
- **Recently Added** - See what's new in your library
- **Detailed Information** - Get comprehensive details about any movie or show
- **Favorites** - View your favorite content
- **Year Search** - Find content from specific years
- **AI-Powered Suggestions** - Get intelligent recommendations based on your preferences
- **Natural Language Interaction** - Talk to the agent in plain English
- **Context-Aware** - The agent remembers your conversation and can handle follow-up questions

## How It Works

The application uses the Microsoft Agents AI framework to create an intelligent agent that:
1. Connects to your Jellyfin server and retrieves your movie library
2. Uses GitHub Models (powered by GPT models) to understand your requests
3. Analyzes your library and provides personalized recommendations
4. Maintains conversation context for natural multi-turn interactions

## Example Usage

Once running, the agent will greet you and wait for your input. You can have a continuous conversation by asking questions like:

**You:** "What genres do I have?"
**Assistant:** Lists all available genres in your library

**You:** "Show me some horror movies"
**Assistant:** Displays horror movies with ratings and descriptions

**You:** "What about the second one?"
**Assistant:** Provides detailed information about that specific movie

**You:** "What was added recently?"
**Assistant:** Shows recently added content

**You:** "Find something from the 1980s"
**Assistant:** Lists movies and shows from that decade

Type `exit` or `quit` to end the conversation.

## Security Notes

- Never commit your `.env` file to version control
- Keep your GitHub token and Jellyfin credentials secure
- The `.env` file is already included in `.gitignore`

## Project Structure

```
jellyfin-utility-agent/
├── Program.cs                      # Main application entry point with conversation loop
├── Services/
│   └── Jellyfin/
│       ├── JellyfinClient.cs       # Jellyfin API integration
│       └── JellyfinTools.cs        # AI agent tool functions
├── .env.example                    # Environment configuration template
└── JellyfinUtilityAgent.csproj     # Project configuration
```

## Available Tools

The agent has access to the following tools to help you:

- **GetAllMoviesAsync** - Retrieve complete list of all movies
- **SearchLibraryAsync** - Search by name with optional media type and genre filters
- **GetAllGenresAsync** - Get all available genres, optionally filtered by media type
- **GetMoviesByGenreAsync** - Find movies in a specific genre, sorted by rating
- **GetRecentlyAddedAsync** - See what's been recently added to the library
- **GetItemDetailsAsync** - Get detailed information about a specific item
- **GetFavoritesAsync** - View favorite content
- **GetItemsByYearAsync** - Find content from a specific production year

## Contributing

This is a personal project, but feel free to fork it and adapt it for your own needs!

## Acknowledgments

- [Jellyfin](https://jellyfin.org/) - The amazing open-source media server
- [Microsoft Agents AI](https://github.com/microsoft/agents) - AI agent framework
- [GitHub Models](https://github.com/marketplace/models) - Easy access to AI models
