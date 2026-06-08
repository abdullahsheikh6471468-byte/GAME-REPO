// ============================================================
// File        : Models.cs
// Namespace   : HangmanGame.Models
// Purpose     : All plain-data model classes / enums used
//               across the application.
// ============================================================

using System;

namespace HangmanGame.Models
{
    // ── Enumerations ─────────────────────────────────────────────

    /// <summary>Possible outcomes of a completed game.</summary>
    public enum GameResult { Win, Loss }

    // ── Domain Models ─────────────────────────────────────────────

    /// <summary>Represents a completed game session (maps to game_sessions table).</summary>
    public class GameSession
    {
        public int SessionId { get; set; }
        public int PlayerId { get; set; }
        public string SelectedWord { get; set; } = string.Empty;
        public GameResult Result { get; set; }
        public int AttemptsUsed { get; set; }
        public int WrongGuesses { get; set; }
        public string LettersGuessed { get; set; } = string.Empty;
        public DateTime PlayedAt { get; set; } = DateTime.Now;
    }

    /// <summary>Aggregate performance stats for a single player.</summary>
    public class PlayerStats
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int TotalGames { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double WinPercentage { get; set; }
        public double AvgWrongGuesses { get; set; }
        public int BestGame { get; set; }   // fewest wrong guesses
        public DateTime? LastPlayed { get; set; }
    }

    /// <summary>One row in the global leaderboard view.</summary>
    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int TotalGames { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public double WinPercentage { get; set; }
        public double AvgWrongGuesses { get; set; }
        public DateTime? LastPlayed { get; set; }
    }
}
