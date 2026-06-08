using System;
using HangmanGame.Database;
using HangmanGame.Helpers;

namespace HangmanGame.Services
{

    public class GameController
    {
        // ── Dependencies ─────────────────────────────────────────────
        private readonly DatabaseManager _db;
        private readonly WordRepository _wordRepo;
        private readonly bool _dbAvailable;

        // ── Player state ─────────────────────────────────────────────
        private string _playerName = string.Empty;
        private int _playerId = -1;

        // ── Constructor ──────────────────────────────────────────────
        public GameController()
        {
            _db = new DatabaseManager();
            _wordRepo = new WordRepository();
            _dbAvailable = _db.TestConnection();

            if (!_dbAvailable)
                ConsoleUI.ShowDbConnectionError();
        }

        // ── Entry point ──────────────────────────────────────────────

        /// <summary>Starts the application.</summary>
        public void Run()
        {
            ConsoleUI.ShowWelcomeScreen();
            LoginPlayer();
            MainMenuLoop();
        }

        // ── Player login ─────────────────────────────────────────────

        private void LoginPlayer()
        {
            _playerName = ConsoleUI.PromptPlayerName();

            if (_dbAvailable)
            {
                try
                {
                    _playerId = _db.GetOrCreatePlayer(_playerName);
                }
                catch (Exception ex)
                {
                    ConsoleUI.ShowError($"Could not save player: {ex.Message}");
                    _playerId = -1;
                }
            }

            ConsoleUI.ShowWelcomePlayer(_playerName);
        }

        // ── Main menu loop ───────────────────────────────────────────

        private void MainMenuLoop()
        {
            bool running = true;

            while (running)
            {
                int choice = ConsoleUI.ShowMainMenu(_playerName);

                switch (choice)
                {
                    case 1: PlayGame(); break;
                    case 2: ShowHistory(); break;
                    case 3: ShowPerformance(); break;
                    case 4: ShowLeaderboard(); break;
                    case 5: ChangePlayer(); break;
                    case 6: running = false; break;
                }
            }

            ConsoleUI.ShowExitMessage(_playerName);
        }

        // ── Game flow ────────────────────────────────────────────────

        private void PlayGame()
        {
            bool keepPlaying = true;

            while (keepPlaying)
            {
                var engine = new GameEngine(_wordRepo);
                var session = engine.Play(_playerId);

                // Persist result
                if (_dbAvailable && _playerId > 0)
                {
                    try
                    {
                        _db.SaveGameSession(session);
                        ConsoleUI.ShowDbSaveSuccess();
                    }
                    catch (Exception ex)
                    {
                        ConsoleUI.ShowError($"Could not save result: {ex.Message}");
                    }
                }

                keepPlaying = ConsoleUI.AskPlayAgain();
            }
        }

        // ── Report screens ───────────────────────────────────────────

        private void ShowHistory()
        {
            if (!RequireDb()) return;

            try
            {
                var history = _db.GetPlayerHistory(_playerId);
                ConsoleUI.ShowGameHistory(_playerName, history);
            }
            catch (Exception ex)
            {
                ConsoleUI.ShowError(ex.Message);
            }
        }

        private void ShowPerformance()
        {
            if (!RequireDb()) return;

            try
            {
                var stats = _db.GetPlayerStats(_playerId, _playerName);
                ConsoleUI.ShowPerformanceReport(stats);
            }
            catch (Exception ex)
            {
                ConsoleUI.ShowError(ex.Message);
            }
        }

        private void ShowLeaderboard()
        {
            if (!RequireDb()) return;

            try
            {
                var board = _db.GetLeaderboard();
                ConsoleUI.ShowLeaderboard(board);
            }
            catch (Exception ex)
            {
                ConsoleUI.ShowError(ex.Message);
            }
        }

        // ── Player change ────────────────────────────────────────────

        private void ChangePlayer()
        {
            ConsoleUI.ShowWelcomeScreen();
            LoginPlayer();
        }

        // ── Guard helper ─────────────────────────────────────────────

        private bool RequireDb()
        {
            if (_dbAvailable && _playerId > 0) return true;
            ConsoleUI.ShowError(
                "This feature requires a database connection. " +
                "Check DatabaseConfig.cs and ensure MySQL is running.");
            return false;
        }
    }
}
