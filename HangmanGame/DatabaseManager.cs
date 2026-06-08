// ============================================================
// File        : DatabaseManager.cs
// Namespace   : HangmanGame.Database
// Purpose     : Data-access layer.  All MySQL queries live here.
//               Compatible with C# 7.3+ and .NET Framework / .NET 8.
// ============================================================

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;
using HangmanGame.Models;

namespace HangmanGame.Database
{
    /// <summary>
    /// Responsible for every database interaction:
    /// - Connection testing
    /// - Player upsert (insert-or-fetch)
    /// - Saving game sessions
    /// - Fetching history and leaderboard
    /// </summary>
    public class DatabaseManager
    {
        // ── Private state ────────────────────────────────────────────
        private readonly string _connectionString;

        // ── Constructor ──────────────────────────────────────────────
        public DatabaseManager()
        {
            _connectionString = DatabaseConfig.ConnectionString;
        }

        // ============================================================
        // CONNECTION HELPERS
        // ============================================================

        /// <summary>Opens and returns a new MySqlConnection.</summary>
        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        /// <summary>
        /// Tests connectivity. Returns true if the database is
        /// reachable, false otherwise.
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    return conn.State == ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        // ============================================================
        // PLAYER OPERATIONS
        // ============================================================

        /// <summary>
        /// Returns the player_id for the given name.
        /// If the player does not exist, inserts a new record first.
        /// </summary>
        public int GetOrCreatePlayer(string playerName)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();

                // Try to find existing player
                const string selectSql =
                    "SELECT player_id FROM players WHERE player_name = @name LIMIT 1;";

                using (MySqlCommand cmd = new MySqlCommand(selectSql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", playerName);
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                        return Convert.ToInt32(result);
                }

                // Not found - insert new player
                const string insertSql =
                    "INSERT INTO players (player_name) VALUES (@name);";

                using (MySqlCommand cmd = new MySqlCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", playerName);
                    cmd.ExecuteNonQuery();
                    return (int)cmd.LastInsertedId;
                }
            }
        }

        // ============================================================
        // GAME SESSION OPERATIONS
        // ============================================================

        /// <summary>Persists a completed game session to the database.</summary>
        public void SaveGameSession(GameSession session)
        {
            const string sql = @"
                INSERT INTO game_sessions
                    (player_id, selected_word, game_result,
                     attempts_used, wrong_guesses, letters_guessed, played_at)
                VALUES
                    (@playerId, @word, @result,
                     @attempts, @wrong, @letters, @playedAt);";

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@playerId", session.PlayerId);
                    cmd.Parameters.AddWithValue("@word", session.SelectedWord);
                    cmd.Parameters.AddWithValue("@result", session.Result.ToString());
                    cmd.Parameters.AddWithValue("@attempts", session.AttemptsUsed);
                    cmd.Parameters.AddWithValue("@wrong", session.WrongGuesses);
                    cmd.Parameters.AddWithValue("@letters", session.LettersGuessed);
                    cmd.Parameters.AddWithValue("@playedAt", session.PlayedAt);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ============================================================
        // HISTORY / REPORTING QUERIES
        // ============================================================

        /// <summary>Returns all game sessions for a specific player.</summary>
        public List<GameSession> GetPlayerHistory(int playerId)
        {
            const string sql = @"
                SELECT session_id, player_id, selected_word, game_result,
                       attempts_used, wrong_guesses, letters_guessed, played_at
                FROM   game_sessions
                WHERE  player_id = @playerId
                ORDER  BY played_at DESC;";

            List<GameSession> sessions = new List<GameSession>();

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@playerId", playerId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sessions.Add(new GameSession
                            {
                                SessionId = reader.GetInt32("session_id"),
                                PlayerId = reader.GetInt32("player_id"),
                                SelectedWord = reader.GetString("selected_word"),
                                Result = (GameResult)Enum.Parse(typeof(GameResult), reader.GetString("game_result")),
                                AttemptsUsed = reader.GetInt32("attempts_used"),
                                WrongGuesses = reader.GetInt32("wrong_guesses"),
                                LettersGuessed = reader.GetString("letters_guessed"),
                                PlayedAt = reader.GetDateTime("played_at")
                            });
                        }
                    }
                }
            }

            return sessions;
        }

        /// <summary>Returns aggregate statistics for a player.</summary>
        public PlayerStats GetPlayerStats(int playerId, string playerName)
        {
            const string sql = @"
                SELECT
                    COUNT(*)                                                    AS total_games,
                    SUM(CASE WHEN game_result='Win'  THEN 1 ELSE 0 END)        AS wins,
                    SUM(CASE WHEN game_result='Loss' THEN 1 ELSE 0 END)        AS losses,
                    ROUND(AVG(wrong_guesses), 2)                               AS avg_wrong,
                    MIN(wrong_guesses)                                         AS best_wrong,
                    MAX(played_at)                                             AS last_played
                FROM game_sessions
                WHERE player_id = @playerId;";

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@playerId", playerId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int total = reader.GetInt32("total_games");
                            int wins = reader.GetInt32("wins");

                            return new PlayerStats
                            {
                                PlayerId = playerId,
                                PlayerName = playerName,
                                TotalGames = total,
                                Wins = wins,
                                Losses = reader.GetInt32("losses"),
                                WinPercentage = total > 0 ? Math.Round((double)wins / total * 100, 1) : 0,
                                AvgWrongGuesses = reader.IsDBNull(reader.GetOrdinal("avg_wrong"))
                                                    ? 0 : reader.GetDouble("avg_wrong"),
                                BestGame = reader.IsDBNull(reader.GetOrdinal("best_wrong"))
                                                    ? 0 : reader.GetInt32("best_wrong"),
                                LastPlayed = reader.IsDBNull(reader.GetOrdinal("last_played"))
                                                    ? (DateTime?)null
                                                    : reader.GetDateTime("last_played")
                            };
                        }
                    }
                }
            }

            return new PlayerStats { PlayerId = playerId, PlayerName = playerName };
        }

        /// <summary>Returns the global leaderboard (top 10 players by wins).</summary>
        public List<LeaderboardEntry> GetLeaderboard()
        {
            const string sql = @"
                SELECT player_name, total_games, wins, losses,
                       win_percentage, avg_wrong_guesses, last_played
                FROM   leaderboard
                LIMIT  10;";

            List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        int rank = 1;
                        while (reader.Read())
                        {
                            entries.Add(new LeaderboardEntry
                            {
                                Rank = rank++,
                                PlayerName = reader.GetString("player_name"),
                                TotalGames = reader.GetInt32("total_games"),
                                Wins = reader.GetInt32("wins"),
                                Losses = reader.GetInt32("losses"),
                                WinPercentage = reader.GetDouble("win_percentage"),
                                AvgWrongGuesses = reader.GetDouble("avg_wrong_guesses"),
                                LastPlayed = reader.IsDBNull(reader.GetOrdinal("last_played"))
                                                    ? (DateTime?)null
                                                    : reader.GetDateTime("last_played")
                            });
                        }
                    }
                }
            }

            return entries;
        }
    }
}
