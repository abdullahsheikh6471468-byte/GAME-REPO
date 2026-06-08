// ============================================================
// File        : HangmanArt.cs
// Namespace   : HangmanGame.Helpers
// Purpose     : ASCII art hangman figure for each stage (0-4).
// ============================================================

using System;

namespace HangmanGame.Helpers
{
    /// <summary>
    /// Static helper that returns multi-line ASCII art strings
    /// for each stage of the hangman scaffold.
    /// Stages: 0=Gallows, 1=Head, 2=Body, 3=Arms, 4=Legs (DEAD)
    /// </summary>
    public static class HangmanArt
    {
        // ── Art frames ───────────────────────────────────────────────
        private static readonly string[] Stage0 = new string[]
        {
            "  +======+  ",
            "  |      |  ",
            "  |         ",
            "  |         ",
            "  |         ",
            "  |         ",
            "==+======   ",
        };

        private static readonly string[] Stage1 = new string[]
        {
            "  +======+  ",
            "  |      |  ",
            "  |      O  ",
            "  |         ",
            "  |         ",
            "  |         ",
            "==+======   ",
        };

        private static readonly string[] Stage2 = new string[]
        {
            "  +======+  ",
            "  |      |  ",
            "  |      O  ",
            "  |      |  ",
            "  |      |  ",
            "  |         ",
            "==+======   ",
        };

        private static readonly string[] Stage3 = new string[]
        {
            "  +======+  ",
            "  |      |  ",
            "  |      O  ",
            "  |     /|\\",
            "  |      |  ",
            "  |         ",
            "==+======   ",
        };

        private static readonly string[] Stage4 = new string[]
        {
            "  +======+  ",
            "  |      |  ",
            "  |      O  ",
            "  |     /|\\",
            "  |      |  ",
            "  |     / \\",
            "==+======   ",
        };

        private static readonly string[][] AllStages = new string[][]
        {
            Stage0, Stage1, Stage2, Stage3, Stage4
        };

        // ── Public API ───────────────────────────────────────────────

        /// <summary>Returns the ASCII art lines for the given wrong guess count.</summary>
        public static string[] GetFrame(int wrongGuesses)
        {
            int clamped = wrongGuesses < 0 ? 0 : (wrongGuesses > 4 ? 4 : wrongGuesses);
            return AllStages[clamped];
        }

        /// <summary>Maximum number of wrong guesses allowed.</summary>
        public const int MaxWrongGuesses = 4;
    }
}
