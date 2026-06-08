// ============================================================
// File        : Program.cs
// Namespace   : HangmanGame
// Purpose     : Application entry point.
// ============================================================

using System;
using HangmanGame.Services;

namespace HangmanGame
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Set console properties for best rendering
            Console.Title = "☠ Dead Hangman Game";

            try
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.CursorVisible = true;
            }
            catch { /* some terminals don't support these – safe to ignore */ }

            // Bootstrap and run
            var controller = new GameController();
            controller.Run();
        }
    }
}
