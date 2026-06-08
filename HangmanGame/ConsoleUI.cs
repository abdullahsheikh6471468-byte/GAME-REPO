using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HangmanGame.Models;

namespace HangmanGame.Helpers
{
    public static class ConsoleUI
    {
        // ── Colour palette
        private static readonly ConsoleColor ColourTitle = ConsoleColor.Cyan;
        private static readonly ConsoleColor ColourSuccess = ConsoleColor.Green;
        private static readonly ConsoleColor ColourDanger = ConsoleColor.Red;
        private static readonly ConsoleColor ColourWarning = ConsoleColor.Yellow;
        private static readonly ConsoleColor ColourInfo = ConsoleColor.White;
        private static readonly ConsoleColor ColourMuted = ConsoleColor.DarkGray;
        private static readonly ConsoleColor ColourAccent = ConsoleColor.Magenta;

        // ── Utility helpers

        public static void Write(string text, ConsoleColor colour)
        {
            Console.ForegroundColor = colour;
            Console.Write(text);
            Console.ResetColor();
        }

        public static void WriteLine(string text, ConsoleColor colour)
        {
            Write(text + "\n", colour);
        }

        public static void Divider(char ch = '=', int width = 55)
        {
            WriteLine(new string(ch, width), ColourMuted);
        }

        public static void BlankLine() { Console.WriteLine(); }

        // ── Welcome / title screens

        public static void ShowWelcomeScreen()
        {
            Console.Clear();
            try { Console.OutputEncoding = System.Text.Encoding.UTF8; } catch { }

            Divider('=');
            WriteLine("", ColourTitle);
            WriteLine("    ██╗  ██╗ █████╗ ███╗   ██╗ ██████╗ ███╗   ███╗ █████╗ ███╗  ", ColourTitle);
            WriteLine("    ██║  ██║██╔══██╗████╗  ██║██╔════╝ ████╗ ████║██╔══██╗████╗ ", ColourTitle);
            WriteLine("    ███████║███████║██╔██╗ ██║██║  ███╗██╔████╔██║███████║██╔██╗", ColourTitle);
            WriteLine("    ██╔══██║██╔══██║██║╚██╗██║██║   ██║██║╚██╔╝██║██╔══██║██║╚██", ColourTitle);
            WriteLine("    ██║  ██║██║  ██║██║ ╚████║╚██████╔╝██║ ╚═╝ ██║██║  ██║██║ ╚█", ColourTitle);
            WriteLine("    ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝ ╚═════╝ ╚═╝     ╚═╝╚═╝  ╚═╝╚═╝  ", ColourTitle);
            WriteLine("", ColourTitle);
            Divider('=');
            WriteLine("          [DEAD HANGMAN GAME]", ColourAccent);
            Divider('=');
            BlankLine();
        }

        public static void ShowWelcomePlayer(string name)
        {
            BlankLine();
            WriteLine("  Welcome, " + name + "! Let's play Hangman.", ColourSuccess);
            BlankLine();
        }

        // ── Game board 

        public static void DrawGameBoard(
            int wrongGuesses,
            char[] maskedWord,
            HashSet<char> guessedLetters,
            string category,
            string hint)
        {
            Console.Clear();
            Divider('=');
            WriteLine("  [DEAD HANGMAN GAME]", ColourTitle);
            Divider('=');

            // ASCII art
            string[] frame = HangmanArt.GetFrame(wrongGuesses);
            BlankLine();
            ConsoleColor artColour = (wrongGuesses == HangmanArt.MaxWrongGuesses)
                                     ? ColourDanger : ColourWarning;
            foreach (string line in frame)
                WriteLine("    " + line, artColour);
            BlankLine();

            // Attempts hearts
            int remaining = HangmanArt.MaxWrongGuesses - wrongGuesses;
            Write("  Attempts Left : ", ColourMuted);
            for (int i = 0; i < HangmanArt.MaxWrongGuesses; i++)
                Write(i < remaining ? "<3 " : "   ", i < remaining ? ColourDanger : ColourMuted);
            BlankLine();
            BlankLine();

            // Category & hint
            Write("  Category : ", ColourMuted);
            WriteLine(category, ColourAccent);
            Write("  Hint     : ", ColourMuted);
            WriteLine(hint, ColourInfo);
            BlankLine();

            // Masked word
            Write("  Word     : ", ColourMuted);
            foreach (char ch in maskedWord)
            {
                if (ch == ' ') Write("  ", ColourInfo);
                else if (ch == '_') Write("_ ", ColourWarning);
                else Write(ch + " ", ColourSuccess);
            }
            BlankLine();
            BlankLine();

            // Guessed letters
            Write("  Guessed  : ", ColourMuted);
            char[] sorted = guessedLetters.ToArray();
            Array.Sort(sorted);
            if (sorted.Length == 0)
            {
                WriteLine("(none yet)", ColourMuted);
            }
            else
            {
                foreach (char ch in sorted)
                    Write(ch + " ", ColourAccent);
                BlankLine();
            }

            Divider('=');
        }

        // ── Input prompts 

        public static string PromptPlayerName()
        {
            while (true)
            {
                Write("  Enter your name : ", ColourInfo);
                string input = (Console.ReadLine() ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(input) && input.Length <= 100)
                    return input;
                WriteLine("  [!] Name cannot be empty (max 100 characters). Try again.", ColourWarning);
            }
        }

        public static char PromptGuess(HashSet<char> already)
        {
            while (true)
            {
                Write("  Guess a letter  : ", ColourInfo);
                string raw = (Console.ReadLine() ?? "").Trim().ToLower();

                if (raw.Length != 1)
                {
                    WriteLine("  [!] Please enter exactly ONE letter.", ColourWarning);
                    continue;
                }

                char ch = raw[0];

                if (!char.IsLetter(ch))
                {
                    WriteLine("  [!] Invalid input - letters only (a-z).", ColourWarning);
                    continue;
                }

                if (already.Contains(ch))
                {
                    WriteLine("  [!] You already guessed '" + ch + "'. Try a different letter.", ColourWarning);
                    continue;
                }

                return ch;
            }
        }

        // ── Feedback messages

        public static void ShowCorrectGuess(char letter)
        {
            WriteLine("  [+] Great! '" + letter + "' is in the word!", ColourSuccess);
            Pause(900);
        }

        public static void ShowWrongGuess(char letter, int wrongCount)
        {
            int left = HangmanArt.MaxWrongGuesses - wrongCount;
            WriteLine("  [-] '" + letter + "' is not in the word. " + left + " attempt(s) left.", ColourDanger);
            Pause(900);
        }

        // ── End-of-game screens 

        public static void ShowVictory(string word)
        {
            Console.Clear();
            Divider('=');
            BlankLine();
            WriteLine("  ************************************", ColourSuccess);
            WriteLine("  *                                  *", ColourSuccess);
            WriteLine("  *      YOU WON THIS GAME!  :)      *", ColourSuccess);
            WriteLine("  *                                  *", ColourSuccess);
            WriteLine("  ************************************", ColourSuccess);
            BlankLine();
            Write("  The word was : ", ColourMuted);
            WriteLine(word.ToUpper(), ColourSuccess);
            Divider('=');
        }

        public static void ShowDefeat(string word)
        {
            Console.Clear();
            Divider('=');
            BlankLine();

            string[] frame = HangmanArt.GetFrame(HangmanArt.MaxWrongGuesses);
            foreach (string line in frame)
                WriteLine("    " + line, ColourDanger);

            BlankLine();
            WriteLine("  ************************************", ColourDanger);
            WriteLine("  *                                  *", ColourDanger);
            WriteLine("  *     YOU LOST THIS GAME!  :(      *", ColourDanger);
            WriteLine("  *                                  *", ColourDanger);
            WriteLine("  ************************************", ColourDanger);
            BlankLine();
            Write("  The word was : ", ColourMuted);
            WriteLine(word.ToUpper(), ColourAccent);
            Divider('=');
        }

        // ── Main menu 

        public static int ShowMainMenu(string playerName)
        {
            BlankLine();
            Divider('-');
            WriteLine("  Player : " + playerName, ColourAccent);
            Divider('-');
            WriteLine("  [1]  New Game", ColourInfo);
            WriteLine("  [2]  My Game History", ColourInfo);
            WriteLine("  [3]  My Performance Report", ColourInfo);
            WriteLine("  [4]  Global Leaderboard", ColourInfo);
            WriteLine("  [5]  Change Player", ColourInfo);
            WriteLine("  [6]  Exit", ColourMuted);
            Divider('-');
            Write("  Your choice : ", ColourInfo);

            while (true)
            {
                string input = (Console.ReadLine() ?? "").Trim();
                int choice;
                if (int.TryParse(input, out choice) && choice >= 1 && choice <= 6)
                    return choice;
                Write("  [!] Enter 1-6 : ", ColourWarning);
            }
        }

        public static bool AskPlayAgain()
        {
            BlankLine();
            Write("  Play again? (Y / N) : ", ColourInfo);
            while (true)
            {
                string input = (Console.ReadLine() ?? "").Trim().ToUpper();
                if (input == "Y") return true;
                if (input == "N") return false;
                Write("  Please enter Y or N : ", ColourWarning);
            }
        }

        // ── Reporting ────────────────────────────────────────────────

        public static void ShowGameHistory(string playerName, List<GameSession> sessions)
        {
            Console.Clear();
            Divider('=');
            WriteLine("  Game History -- " + playerName, ColourTitle);
            Divider('=');

            if (sessions.Count == 0)
            {
                WriteLine("  No games recorded yet.", ColourMuted);
            }
            else
            {
                string header = string.Format("  {0,-4} {1,-20} {2,-18} {3,-8} {4,-6} {5}",
                    "#", "Date", "Word", "Result", "Wrong", "Guesses");
                WriteLine(header, ColourMuted);
                Divider('-');

                int num = 1;
                foreach (GameSession s in sessions)
                {
                    ConsoleColor colour = s.Result == GameResult.Win ? ColourSuccess : ColourDanger;
                    string resultLbl = s.Result == GameResult.Win ? "WIN" : "LOSS";
                    string row = string.Format("  {0,-4} {1,-20} {2,-18} {3,-8} {4,-6} {5}",
                        num,
                        s.PlayedAt.ToString("yyyy-MM-dd HH:mm"),
                        s.SelectedWord,
                        resultLbl,
                        s.WrongGuesses,
                        s.LettersGuessed);
                    WriteLine(row, colour);
                    num++;
                }
            }

            Divider('=');
            PressAnyKey();
        }

        public static void ShowPerformanceReport(PlayerStats stats)
        {
            Console.Clear();
            Divider('=');
            WriteLine("  Performance Report -- " + stats.PlayerName, ColourTitle);
            Divider('=');

            PrintStat("Total Games Played", stats.TotalGames.ToString());
            PrintStat("Wins", stats.Wins.ToString());
            PrintStat("Losses", stats.Losses.ToString());
            PrintStat("Win Rate", stats.WinPercentage + "%");
            PrintStat("Avg Wrong Guesses", stats.AvgWrongGuesses.ToString("F1"));
            PrintStat("Best Game (min wrong)", stats.TotalGames > 0 ? stats.BestGame.ToString() : "N/A");
            PrintStat("Last Played", stats.LastPlayed.HasValue
                                               ? stats.LastPlayed.Value.ToString("yyyy-MM-dd HH:mm")
                                               : "Never");

            // Win/Loss bar
            BlankLine();
            Write("  Win/Loss Bar : ", ColourMuted);
            if (stats.TotalGames > 0)
            {
                int barWidth = 30;
                int winBars = (int)Math.Round((double)stats.Wins / stats.TotalGames * barWidth);
                Write(new string('#', winBars), ColourSuccess);
                Write(new string('-', barWidth - winBars), ColourDanger);
                BlankLine();
            }
            else
            {
                WriteLine("(no data)", ColourMuted);
            }

            Divider('=');
            PressAnyKey();
        }

        public static void ShowLeaderboard(List<LeaderboardEntry> entries)
        {
            Console.Clear();
            Divider('=');
            WriteLine("  Global Leaderboard  (Top 10)", ColourTitle);
            Divider('=');

            if (entries.Count == 0)
            {
                WriteLine("  No data available yet.", ColourMuted);
            }
            else
            {
                string header = string.Format("  {0,-5} {1,-20} {2,-7} {3,-6} {4,-8} {5,-8} {6}",
                    "Rank", "Player", "Games", "Wins", "Losses", "Win%", "Avg Wrong");
                WriteLine(header, ColourMuted);
                Divider('-');

                foreach (LeaderboardEntry e in entries)
                {
                    ConsoleColor colour = e.Rank == 1 ? ConsoleColor.Yellow
                                       : e.Rank == 2 ? ConsoleColor.Gray
                                       : e.Rank == 3 ? ConsoleColor.DarkYellow
                                       : ColourInfo;

                    string medal = e.Rank == 1 ? "[1]"
                                 : e.Rank == 2 ? "[2]"
                                 : e.Rank == 3 ? "[3]"
                                 : string.Format("[{0}]", e.Rank);

                    string row = string.Format("  {0,-5} {1,-20} {2,-7} {3,-6} {4,-8} {5,-8} {6:F1}",
                        medal,
                        e.PlayerName,
                        e.TotalGames,
                        e.Wins,
                        e.Losses,
                        e.WinPercentage.ToString("F1") + "%",
                        e.AvgWrongGuesses);
                    WriteLine(row, colour);
                }
            }

            Divider('=');
            PressAnyKey();
        }

        // ── Status / error messages ──────────────────────────────────

        public static void ShowDbConnectionError()
        {
            BlankLine();
            Divider('-');
            WriteLine("  [!] DATABASE CONNECTION FAILED", ColourDanger);
            WriteLine("  The game will run in offline mode.", ColourWarning);
            WriteLine("  History and leaderboard features are disabled.", ColourWarning);
            Divider('-');
            BlankLine();
            Pause(2000);
        }

        public static void ShowDbSaveSuccess()
        {
            WriteLine("  [OK] Game saved to database.", ColourMuted);
            Pause(800);
        }

        public static void ShowError(string message)
        {
            WriteLine("  [X] Error: " + message, ColourDanger);
            Pause(1500);
        }

        public static void ShowExitMessage(string name)
        {
            Console.Clear();
            Divider('=');
            WriteLine("  Thanks for playing, " + name + "! Goodbye.", ColourAccent);
            Divider('=');
            BlankLine();
        }

        // ── Internal helpers ─────────────────────────────────────────

        private static void PrintStat(string label, string value)
        {
            Write(string.Format("  {0,-28}: ", label), ColourMuted);
            WriteLine(value, ColourInfo);
        }

        private static void PressAnyKey()
        {
            BlankLine();
            Write("  Press any key to return to menu...", ColourMuted);
            Console.ReadKey(true);
        }

        private static void Pause(int ms)
        {
            Thread.Sleep(ms);
        }
    }
}
