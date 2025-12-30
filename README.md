# 🎮 LolStatsTracker

**A comprehensive League of Legends statistics tracking application** that automatically monitors your games and provides detailed analytics through a beautiful web dashboard.

![.NET 9](https://img.shields.io/badge/.NET-9.0-purple)
![Blazor WebAssembly](https://img.shields.io/badge/Blazor-WebAssembly-blue)
![WPF](https://img.shields.io/badge/WPF-TrayApp-green)
![License](https://img.shields.io/badge/license-MIT-blue)

---

## 📋 Overview

LolStatsTracker is a full-stack solution consisting of:
- **TrayApp** - A Windows system tray application that connects to the League Client and Riot API to automatically track your matches
- **API Server** - A RESTful backend service for data storage, authentication, and statistics processing
- **Web Client** - A modern Blazor WebAssembly dashboard for visualizing your performance

---

## ✨ Features

### 📊 Dashboard & Analytics
- **Match History** - View detailed information about all your games
- **Champion Statistics** - Track performance per champion with KDA, winrate, and more
- **Win/Loss Streaks** - Monitor your current and best streaks
- **Winrate by Time Analysis** - Discover when you perform best (time of day, day of week)
- **Performance Scores** - Get an overall performance rating for each match
- **Rank Milestones** - Track your ranked progression over time

### 🔔 Automatic Tracking
- **LCU Integration** - Connects directly to League Client Update for real-time game detection
- **Riot API Integration** - Fetches detailed match data from official Riot APIs
- **Background Sync** - Automatically syncs your match history to the cloud
- **System Tray** - Runs silently in the background while you play

### 👤 User Features
- **JWT Authentication** - Secure login with refresh token support
- **Multiple Profiles** - Link multiple League of Legends accounts
- **Season Tracking** - Analyze performance across different ranked seasons

---

## 🏗️ Architecture

```
LolStatsTracker/
├── Client/                  # Blazor WebAssembly Frontend
│   ├── Components/          # Reusable Razor components
│   │   ├── Dashboard/       # Dashboard widgets
│   │   ├── Matches/         # Match display components
│   │   └── Common/          # Shared UI elements
│   ├── Pages/               # Main application pages
│   ├── Services/            # API client services
│   └── Layout/              # App layout components
├── Server/                  # ASP.NET Core Web API
│   ├── Controllers/         # API endpoints
│   ├── Services/            # Business logic
│   ├── Data/                # Entity Framework DbContext
│   └── Migrations/          # Database migrations
├── Shared/                  # Common code library
│   ├── DTOs/                # Data transfer objects
│   ├── Models/              # Entity models
│   ├── Constants/           # Application constants
│   └── Helpers/             # Utility functions
├── TrayApp/                 # WPF System Tray Application
│   ├── Services/            # LCU & Riot API services
│   ├── ViewModels/          # MVVM view models
│   └── Views/               # WPF windows
└── Tests/                   # Unit & integration tests
    ├── LolStatsTracker.API.Tests/
    ├── LolStatsTracker.Client.Tests/
    ├── LolStatsTracker.Shared.Tests/
    └── LolStatsTracker.TrayApp.Tests/
```

---

## 🛠️ Technology Stack

### Frontend (Client)
- **Blazor WebAssembly** - SPA framework
- **MudBlazor 8.x** - Material Design component library
- **Blazored.LocalStorage** - Browser storage for tokens

### Backend (Server)
- **ASP.NET Core 9.0** - Web API framework
- **Entity Framework Core** - ORM with SQLite
- **JWT Bearer Authentication** - Secure token-based auth
- **BCrypt** - Password hashing
- **Swashbuckle** - OpenAPI/Swagger documentation

### Desktop (TrayApp)
- **WPF (.NET 9)** - Windows desktop framework
- **CommunityToolkit.Mvvm** - MVVM pattern support
- **Hardcodet.NotifyIcon.Wpf** - System tray integration
- **Websocket.Client** - LCU WebSocket connection

### Testing
- **xUnit** - Test framework
- **Moq** - Mocking library

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [JetBrains Rider](https://www.jetbrains.com/rider/)
- Windows 10/11 (for TrayApp)
- A Riot Games API Key (for development)

### Configuration

#### 1. Server Configuration
Edit `Server/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=matches.db"
  },
  "Jwt": {
    "Key": "your-secret-key-here",
    "Issuer": "LolStatsTracker",
    "Audience": "LolStatsTracker"
  }
}
```

#### 2. TrayApp Configuration
Edit `TrayApp/appsettings.json`:
```json
{
  "ApiBaseUrl": "https://localhost:7001",
  "RiotApiKey": "RGAPI-xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
}
```

### Running the Application

#### Start the API Server
```bash
cd Server
dotnet run
```
The API will be available at `https://localhost:7001` with Swagger UI at `/swagger`.

#### Start the Web Client
```bash
cd Client
dotnet run
```
The web app will be available at `https://localhost:7002`.

#### Start the TrayApp
```bash
cd TrayApp
dotnet run
```
The application will appear in your system tray.

---

## 📝 API Endpoints

| Controller | Description |
|------------|-------------|
| `/api/auth` | Authentication (login, register, refresh tokens) |
| `/api/matches` | Match history CRUD operations |
| `/api/stats` | Statistics and analytics endpoints |
| `/api/profiles` | User profile management |
| `/api/milestones` | Rank milestone tracking |
| `/api/seasons` | Season management |
| `/api/assets` | DDragon asset proxying |

---

## 🧪 Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test Tests/LolStatsTracker.API.Tests
dotnet test Tests/LolStatsTracker.Shared.Tests
dotnet test Tests/LolStatsTracker.TrayApp.Tests
dotnet test Tests/LolStatsTracker.Client.Tests
```

---

## 📁 Key Components

### TrayApp Services
- **LcuService** - Connects to League Client via lockfile, monitors game events via WebSocket
- **RiotApiService** - Interacts with official Riot Games API for match data
- **ApiSyncService** - Syncs local match data to the cloud server
- **TrayAuthService** - Handles authentication flow for the tray application

### Server Services
- **MatchService** - Match CRUD operations with filtering
- **StatsService** - Complex statistics calculations
- **AuthService** - User authentication and token management
- **ProfileService** - Summoner profile management
- **MilestoneService** - Rank milestone tracking
- **DDragonService** - Champion data from Riot's Data Dragon

### Shared Helpers
- **ChampionStatsHelper** - Calculate champion-specific statistics
- **PerformanceScoreHelper** - Compute match performance scores
- **RankConstants** - Rank tier definitions and conversions

---

## 🎨 Web Pages

| Page | Description |
|------|-------------|
| **Dashboard** | Overview with streaks, time analysis, hardest matchups |
| **Matches** | Paginated match history with filters |
| **Champions** | Champion pool statistics |
| **Login/Register** | Authentication pages |

---

## 🔒 Security

- JWT-based authentication with access and refresh tokens
- BCrypt password hashing with configurable work factor
- Rate limiting protection on API endpoints
- Secure token storage in browser local storage

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## ⚠️ Disclaimer

LolStatsTracker isn't endorsed by Riot Games and doesn't reflect the views or opinions of Riot Games or anyone officially involved in producing or managing Riot Games properties. Riot Games, and all associated properties are trademarks or registered trademarks of Riot Games, Inc.
