# ☠ Dead Hangman Game

## Project Overview

**Dead Hangman Game** is a console-based implementation of the classic Hangman word-guessing game, developed in C# with MySQL database integration. Players can engage in interactive gameplay while tracking their performance statistics and competing on a global leaderboard.

---

## 📋 Project Information

| Detail | Information |
|--------|-------------|
| **Student Name** | Muhammad Abdullah |
| **Roll Number** | 2025-GA-118 |
| **Language** | C# (.NET Framework 4.7.2) |
| **Database** | MySQL |
| **Project Type** | Console Application |

---

## ✨ Features

### Core Gameplay
- **Interactive Word Guessing**: Players guess letters to reveal hidden words
- **ASCII Hangman Art**: Visual representation of game progress
- **Category & Hints**: Each word comes with a category and hint
- **Difficulty Tracking**: Monitor attempts and wrong guesses

### Player Management
- **Player Registration**: Create and login with unique player names
- **Persistent Sessions**: All gameplay is recorded in the database
- **Multi-Player Support**: Switch between different player accounts

### Statistics & Analytics
- **Game History**: View all past games with results and details
- **Performance Report**: Track personal statistics including:
  - Total games played
  - Win/Loss ratio
  - Win percentage
  - Average wrong guesses
  - Best game performance
  - Last played date
- **Global Leaderboard**: Compete with other players and rank against them

### Database Features
- **Graceful Offline Mode**: Game runs without database if connection unavailable
- **Automatic Player Creation**: New players are auto-created on login
- **Session Persistence**: Every game result is saved with full details

---

## 🛠️ Requirements

### Software
- **.NET Framework 4.7.2** or higher
- **MySQL Server** (MySQL Workbench 8.0 recommended)
- **Visual Studio 2019+** (for development)

### NuGet Packages
- `MySql.Data 9.7.0` - MySQL database connector
- `Google.Protobuf 3.32.0` - Protocol Buffer support
- `BouncyCastle.Cryptography 2.6.2` - Cryptography support
- Additional dependencies for compression and async operations

---

## 📦 Installation & Setup

### Step 1: Clone or Extract Project
```bash
Extract the project files to your desired location
```

### Step 2: Set Up MySQL Database
Create the required database schema:

```sql
-- Create database
CREATE DATABASE hangman_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE hangman_db;

-- Create players table
CREATE TABLE players (
    player_id INT AUTO_INCREMENT PRIMARY KEY,
    player_name VARCHAR(100) NOT NULL UNIQUE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Create game_sessions table
CREATE TABLE game_sessions (
    session_id INT AUTO_INCREMENT PRIMARY KEY,
    player_id INT NOT NULL,
    selected_word VARCHAR(100) NOT NULL,
    result ENUM('Win', 'Loss') NOT NULL,
    attempts_used INT NOT NULL,
    wrong_guesses INT NOT NULL,
    letters_guessed VARCHAR(26),
    played_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (player_id) REFERENCES players(player_id)
);

-- Create leaderboard view
CREATE VIEW leaderboard AS
SELECT 
    ROW_NUMBER() OVER (ORDER BY 
        (SUM(CASE WHEN result = 'Win' THEN 1 ELSE 0 END) / COUNT(*)) DESC,
        (SUM(CASE WHEN result = 'Win' THEN 1 ELSE 0 END)) DESC
    ) AS rank,
    p.player_name,
    COUNT(*) AS total_games,
    SUM(CASE WHEN result = 'Win' THEN 1 ELSE 0 END) AS wins,
    SUM(CASE WHEN result = 'Loss' THEN 1 ELSE 0 END) AS losses,
    ROUND((SUM(CASE WHEN result = 'Win' THEN 1 ELSE 0 END) / COUNT(*)) * 100, 2) AS win_percentage,
    ROUND(AVG(wrong_guesses), 2) AS avg_wrong_guesses,
    MAX(played_at) AS last_played
FROM game_sessions gs
JOIN players p ON gs.player_id = p.player_id
GROUP BY p.player_id, p.player_name;
```

### Step 3: Configure Database Connection
Edit `DatabaseConfig.cs`:
```csharp
public const string Server = "localhost";      // Your MySQL server
public const int Port = 3306;                   // MySQL port
public const string Database = "hangman_db";    // Database name
public const string UserId = "root";            // MySQL username
public const string Password = "your_password"; // MySQL password
```

### Step 4: Build & Run
```bash
# Open solution in Visual Studio
HangmanGame.slnx

# Build (Ctrl+Shift+B)
# Run (F5 or Ctrl+F5)
```

---

## 🎮 How to Play

### Main Menu
```
1. Play Game       - Start a new game
2. Game History    - View past games
3. Performance     - Check personal statistics
4. Leaderboard     - See global rankings
5. Change Player   - Switch to another player
6. Exit            - Quit the application
```

### Gameplay
1. **Guess Letters**: Enter one letter at a time
2. **Correct Guess**: Letter is revealed in the word
3. **Wrong Guess**: Hangman drawing progresses
4. **Win Condition**: Reveal all letters before reaching 6 wrong guesses
5. **Loss Condition**: Make 6 wrong guesses and the hangman is complete

### Example Game Flow
```
Category: ANIMAL
Hint: Hops and has long ears
Current Word: l _ _ _ _ _
Guessed Letters: l
Wrong Guesses: 0 / 6
Enter your guess: a
```

---

## 📂 Project Structure

```
HangmanGame/
├── Program.cs                 # Application entry point
├── GameController.cs          # Main game orchestrator (runs menus & flow)
├── GameEngine.cs              # Core game logic & word guessing mechanics
├── ConsoleUI.cs               # All console output & user input handling
├── Models.cs                  # Data classes (GameSession, PlayerStats, etc.)
├── DatabaseManager.cs         # Database operations & queries
├── DatabaseConfig.cs          # Connection configuration (EDIT THIS)
├── WordRepository.cs          # Embedded word list with categories & hints
├── HangmanArt.cs              # ASCII art for hangman stages
├── HangmanGame.csproj         # Visual Studio project file
├── App.config                 # Application configuration
├── Properties/                # Assembly metadata
├── bin/                       # Compiled output
├── obj/                       # Build artifacts
├── packages/                  # NuGet dependencies
└── README.md                  # This file
```

---

## 🏗️ Architecture Overview

### Layered Design
```
┌─────────────────────────────────────┐
│   Console UI Layer (ConsoleUI.cs)   │ ← User Interface
├─────────────────────────────────────┤
│  Game Services Layer                │
│  ├─ GameController (orchestration)  │ ← Business Logic
│  └─ GameEngine (game mechanics)     │
├─────────────────────────────────────┤
│  Data Layer (DatabaseManager.cs)    │ ← Persistence
├─────────────────────────────────────┤
│  Models & Helpers                   │ ← Domain Objects
└─────────────────────────────────────┘
```

### Key Classes

| Class | Responsibility |
|-------|-----------------|
| `GameController` | Orchestrates entire game flow, player login, menu navigation |
| `GameEngine` | Implements core word guessing logic, win/loss detection |
| `DatabaseManager` | CRUD operations, player management, statistics queries |
| `ConsoleUI` | All console output, user input prompts, game display |
| `WordRepository` | Maintains word list with categories and hints |
| `Models` | GameSession, PlayerStats, LeaderboardEntry, GameResult enum |

### Data Flow
```
Player Input (ConsoleUI)
    ↓
GameController (routes to appropriate action)
    ↓
GameEngine / DatabaseManager (process logic/data)
    ↓
Models (return structured data)
    ↓
ConsoleUI (display results)
```

---

## 🗄️ Database Schema

### players table
| Column | Type | Notes |
|--------|------|-------|
| player_id | INT (PK) | Auto-increment |
| player_name | VARCHAR(100) | Unique player identifier |
| created_at | DATETIME | Account creation timestamp |

### game_sessions table
| Column | Type | Notes |
|--------|------|-------|
| session_id | INT (PK) | Auto-increment |
| player_id | INT (FK) | References players table |
| selected_word | VARCHAR(100) | The word that was guessed |
| result | ENUM | 'Win' or 'Loss' |
| attempts_used | INT | Total guesses made |
| wrong_guesses | INT | Number of incorrect guesses |
| letters_guessed | VARCHAR(26) | Comma-separated guessed letters |
| played_at | DATETIME | When the game was played |

### leaderboard VIEW
Aggregates player statistics with ranking based on win percentage and total wins.

---

## ⚙️ Configuration

### Offline Mode
If MySQL connection is unavailable, the game:
- ✅ Still runs normally with full gameplay
- ❌ Cannot save player data or view statistics
- ⚠️ Shows a graceful connection error message

### Customization Points

#### Add Custom Words
Edit `WordRepository.cs` to add new words:
```csharp
entries.Add(new WordEntry { 
    Word = "BUTTERFLY", 
    Category = "INSECT", 
    Hint = "Beautiful winged creature" 
});
```

#### Adjust Game Difficulty
In `GameEngine.cs`:
```csharp
private int _wrongGuesses;  // Modify max in HangmanArt.MaxWrongGuesses
```

#### Change Connection Details
Edit `DatabaseConfig.cs` to point to your MySQL server.

---

## 🧪 Testing Checklist

- [ ] Database connection successful
- [ ] New player can be created
- [ ] Game initializes with random word
- [ ] Correct letter guesses reveal the letter
- [ ] Wrong guesses increment counter
- [ ] Game ends on 6 wrong guesses (loss)
- [ ] Game ends when word is revealed (win)
- [ ] Game result is saved to database
- [ ] History view shows past games
- [ ] Performance stats are calculated correctly
- [ ] Leaderboard ranks players correctly
- [ ] Player can switch between accounts
- [ ] Game works in offline mode (without database)

---

## 🐛 Troubleshooting

### Issue: "Cannot connect to database"
**Solution**: Check DatabaseConfig.cs credentials and ensure MySQL server is running

### Issue: "Table doesn't exist"
**Solution**: Run the SQL schema creation script from Step 2 of Installation

### Issue: "Special characters not displaying"
**Solution**: Ensure your console supports UTF-8 and MySQL uses utf8mb4 charset

### Issue: "Application crashes on startup"
**Solution**: Check that all NuGet packages are installed; run `nuget restore`

---

## 📊 Game Statistics Explanation

### Win Percentage
```
(Total Wins / Total Games) × 100
```

### Average Wrong Guesses
```
Sum of all wrong guesses / Total games played
```

### Best Game
Minimum number of wrong guesses in a single game

### Leaderboard Ranking
Ranked primarily by win percentage, secondarily by total wins.

---

## 📝 Notes

- Game automatically reveals the first letter as a built-in hint
- Each player name is unique; system prevents duplicate registrations
- All timestamps are recorded in UTC
- Word list includes diverse categories (Animals, Foods, Places, etc.)
- Console title shows current game status
- Application handles graceful shutdown

---

## 🔒 Security Notes

- Passwords in DatabaseConfig.cs should be changed before production deployment
- Database user should have limited privileges (SELECT, INSERT, UPDATE only)
- Consider using connection pooling for high-volume deployments
- Input validation is implemented for player names and guesses

---

## 📄 License & Attribution

This is an academic project developed as coursework for a Computer Science program.

---

## 🤝 Contributing

For improvements or bug fixes:
1. Test thoroughly before submission
2. Maintain code formatting and comments
3. Update this README if features change
4. Ensure database queries are optimized

---

## 📧 Contact

**Student**: Muhammad Abdullah  
**Roll Number**: 2025-GA-118  
**Project**: Dead Hangman Game - A Console-Based Hangman with MySQL Integration

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | June 2026 | Initial release with core features |

---

**Last Updated**: June 2026  
**Status**: ✅ Complete and Functional
