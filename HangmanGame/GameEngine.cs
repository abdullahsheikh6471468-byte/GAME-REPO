// ============================================================
// File        : GameEngine.cs
// Namespace   : HangmanGame.Services
// Purpose     : Core game loop and state management.
// ============================================================

using System;
using System.Collections.Generic;
using System.Linq;
using HangmanGame.Helpers;
using HangmanGame.Models;

namespace HangmanGame.Services
{
    /// <summary>
    /// Manages a single game session:
    /// word selection, letter guessing, win/loss detection.
    /// Returns a completed GameSession when done.
    /// </summary>
    public class GameEngine
    {
        // ── Dependencies ─────────────────────────────────────────────
        private readonly WordRepository _wordRepo;

        // ── State ────────────────────────────────────────────────────
        private string _targetWord = string.Empty;
        private string _category = string.Empty;
        private string _hint = string.Empty;
        private char[] _maskedWord = new char[0];
        private HashSet<char> _guessedLetters = new HashSet<char>();
        private int _wrongGuesses;
        private int _totalAttempts;

        // ── Constructor ──────────────────────────────────────────────
        public GameEngine(WordRepository wordRepository)
        {
            _wordRepo = wordRepository;
        }

        // ── Public API ───────────────────────────────────────────────

        /// <summary>
        /// Runs a complete game and returns the session record.
        /// </summary>
        public GameSession Play(int playerId)
        {
            InitialiseGame();

            while (true)
            {
                ConsoleUI.DrawGameBoard(
                    _wrongGuesses,
                    _maskedWord,
                    _guessedLetters,
                    _category,
                    _hint);

                // ── Check terminal states ──
                if (_wrongGuesses >= HangmanArt.MaxWrongGuesses)
                {
                    ConsoleUI.ShowDefeat(_targetWord);
                    return BuildSession(playerId, GameResult.Loss);
                }

                if (IsWordRevealed())
                {
                    ConsoleUI.ShowVictory(_targetWord);
                    return BuildSession(playerId, GameResult.Win);
                }

                // ── Get guess ──
                char guess = ConsoleUI.PromptGuess(_guessedLetters);
                _guessedLetters.Add(guess);
                _totalAttempts++;

                if (_targetWord.Contains(guess))
                {
                    RevealLetter(guess);
                    ConsoleUI.ShowCorrectGuess(guess);
                }
                else
                {
                    _wrongGuesses++;
                    ConsoleUI.ShowWrongGuess(guess, _wrongGuesses);
                }
            }
        }

        // ── Private helpers ──────────────────────────────────────────

        private void InitialiseGame()
        {
            WordRepository.WordEntry entry = _wordRepo.GetRandomWord();
            _targetWord = entry.Word.ToLower();
            _category = entry.Category;
            _hint = entry.Hint;
            _guessedLetters = new HashSet<char>();
            _wrongGuesses = 0;
            _totalAttempts = 0;

            // Build initial masked array (reveal first letter as built-in hint)
            _maskedWord = new char[_targetWord.Length];
            for (int i = 0; i < _targetWord.Length; i++)
                _maskedWord[i] = (i == 0) ? _targetWord[i] : '_';

            // Pre-add first letter so it cannot be re-entered
            _guessedLetters.Add(_targetWord[0]);
        }

        private void RevealLetter(char letter)
        {
            for (int i = 0; i < _targetWord.Length; i++)
                if (_targetWord[i] == letter)
                    _maskedWord[i] = letter;
        }

        private bool IsWordRevealed()
        {
            foreach (char c in _maskedWord)
                if (c == '_') return false;
            return true;
        }

        private GameSession BuildSession(int playerId, GameResult result)
        {
            char[] sorted = _guessedLetters.ToArray();
            Array.Sort(sorted);

            return new GameSession
            {
                PlayerId = playerId,
                SelectedWord = _targetWord,
                Result = result,
                AttemptsUsed = _totalAttempts,
                WrongGuesses = _wrongGuesses,
                LettersGuessed = new string(sorted),
                PlayedAt = DateTime.Now
            };
        }
    }
}
