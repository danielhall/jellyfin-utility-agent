# Jellyfin Utility Agent

A fun personal project that creates an AI-powered agent to interact with your Jellyfin media server. This agent can help you discover and get movie suggestions based on your personal media library!

## 🎬 What is This?

This is a .NET console application that uses AI agents to provide intelligent assistance for your Jellyfin media server. The agent can analyze your movie collection and provide personalized recommendations, making it easier to decide what to watch.

## 🛠️ Technologies Used

- **.NET 10.0** - Modern C# framework
- **Microsoft.Agents.AI** - AI agent framework for building conversational agents
- **Microsoft.Extensions.AI** - AI abstraction layer for .NET
- **GitHub Models API** - AI model inference through GitHub's API
- **Jellyfin API** - Integration with Jellyfin media server
- **RestSharp** - HTTP client for API calls

## 🚀 Getting Started

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

## 📋 Features

- 🎥 **Movie Discovery** - Get a list of all movies in your Jellyfin library
- 🤖 **AI-Powered Suggestions** - Ask the agent to recommend movies based on your mood or preferences
- 💬 **Natural Language Interaction** - Talk to the agent in plain English
- 🎃 **Context-Aware** - The agent understands context like "Find a spooky movie for me to watch tonight"

## 🔧 How It Works

The application uses the Microsoft Agents AI framework to create an intelligent agent that:
1. Connects to your Jellyfin server and retrieves your movie library
2. Uses GitHub Models (powered by GPT models) to understand your requests
3. Analyzes your library and provides personalized recommendations
4. Maintains conversation context for natural multi-turn interactions

## 🎯 Example Usage

Once running, you can ask the agent questions like:
- "Find a spooky movie for me to watch tonight"
- "What comedies do I have?"
- "Suggest an action movie"
- "What's a good film for a movie night?"

## 🔐 Security Notes

- Never commit your `.env` file to version control
- Keep your GitHub token and Jellyfin credentials secure
- The `.env` file is already included in `.gitignore`

## 📝 Project Structure

```
jellyfin-utility-agent/
├── Program.cs                 # Main application entry point
├── Services/
│   └── Jellyfin/
│       └── JellyfinClient.cs  # Jellyfin API integration
├── .env.example               # Environment configuration template
└── JellyfinUtilityAgent.csproj # Project configuration
```

## 🤝 Contributing

This is a personal project, but feel free to fork it and adapt it for your own needs!

## 📄 License

This is a personal project - use it however you'd like!

## 🎉 Acknowledgments

- [Jellyfin](https://jellyfin.org/) - The amazing open-source media server
- [Microsoft Agents AI](https://github.com/microsoft/agents) - AI agent framework
- [GitHub Models](https://github.com/marketplace/models) - Easy access to AI models

---

**Happy movie watching! 🍿**
