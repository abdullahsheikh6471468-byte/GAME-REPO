// ============================================================
// File        : WordRepository.cs
// Namespace   : HangmanGame.Services
// Purpose     : Word bank and random word selection.
//               Contains 120+ words across multiple categories.
// ============================================================

using System;
using System.Collections.Generic;
using System.Text;

namespace HangmanGame.Services
{
    /// <summary>
    /// Encapsulates the word bank.
    /// Each entry holds a Word, Category, and a raw Hint sentence.
    /// The DisplayHint property returns a smart, multi-word hint
    /// built by BuildSmartHint() so the player always sees at least
    /// 3 meaningful clue words drawn from the beginning, middle and
    /// end of the original hint phrase.
    /// </summary>
    public class WordRepository
    {
        // ── Inner class ───────────────────────────────────────────────
        public class WordEntry
        {
            public string Word { get; private set; }
            public string Category { get; private set; }

            // Raw hint exactly as written in the word bank.
            private string RawHint { get; set; }

            // Processed hint shown to the player (≥3 meaningful words).
            public string Hint { get; private set; }

            public WordEntry(string word, string category, string hint)
            {
                Word = word;
                Category = category;
                RawHint = hint;
                Hint = BuildSmartHint(hint);
            }

      
            private static string BuildSmartHint(string rawHint)
            {
                if (string.IsNullOrWhiteSpace(rawHint))
                    return rawHint;

                // ── Step 1 : tokenise ────────────────────────────────
                // Remove common punctuation but keep hyphens inside words
                string cleaned = rawHint
                    .Replace(",", " ")
                    .Replace(".", " ")
                    .Replace(";", " ")
                    .Replace(":", " ")
                    .Replace("(", " ")
                    .Replace(")", " ")
                    .Replace("\"", " ")
                    .Replace("'", "");

                string[] tokens = cleaned.Split(
                    new char[] { ' ', '\t' },
                    StringSplitOptions.RemoveEmptyEntries);

                int totalTokens = tokens.Length;

                // If the hint is already 3 words or fewer, return as-is
                if (totalTokens <= 3)
                    return rawHint.Trim();

                // ── Step 2 : classify tokens ─────────────────────────
                // Stopwords: articles, prepositions, conjunctions, aux verbs, etc.
                // These are useful for grammar but weak as standalone clues.
                HashSet<string> stopwords = new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    "a", "an", "the", "of", "in", "on", "at", "to",
                    "for", "by", "as", "is", "are", "was", "were",
                    "be", "been", "being", "has", "have", "had",
                    "do", "does", "did", "will", "would", "could",
                    "should", "may", "might", "shall", "can",
                    "and", "or", "but", "nor", "so", "yet",
                    "with", "from", "into", "onto", "upon",
                    "its", "it", "this", "that", "these", "those",
                    "not", "no", "nor", "also", "both", "either",
                    "each", "than", "then", "when", "which", "who",
                    "how", "what", "where", "very", "just", "only"
                };

                List<int> meaningfulIdx = new List<int>();
                List<int> fillerIdx = new List<int>();

                for (int i = 0; i < totalTokens; i++)
                {
                    string t = tokens[i];
                    bool isMeaningful = t.Length >= 4 &&
                                       !stopwords.Contains(t);
                    if (isMeaningful)
                        meaningfulIdx.Add(i);
                    else
                        fillerIdx.Add(i);
                }

                // ── Step 3 : pick spread of meaningful words ─────────
                const int TARGET_WORDS = 3;
                List<int> chosen = new List<int>();

                int mCount = meaningfulIdx.Count;

                if (mCount >= TARGET_WORDS)
                {
                    // Always include the first meaningful word (beginning)
                    chosen.Add(meaningfulIdx[0]);

                    // Include the last meaningful word (end) if distinct
                    int lastIdx = meaningfulIdx[mCount - 1];
                    if (lastIdx != meaningfulIdx[0])
                        chosen.Add(lastIdx);


                    int needed = TARGET_WORDS - chosen.Count;
                    if (needed > 0 && mCount >= 3)
                    {
                        // Middle bucket: indices 1 through mCount-2
                        int bucketSize = mCount - 2; // slots available
                        for (int slot = 0; slot < needed && slot < bucketSize; slot++)
                        {
                            // Evenly spaced pick inside the middle bucket
                            int pickPos = 1 + (int)Math.Round(
                                (double)slot * (bucketSize - 1) /
                                Math.Max(1, needed - 1));
                            int candidate = meaningfulIdx[pickPos];
                            if (!chosen.Contains(candidate))
                                chosen.Add(candidate);
                        }
                    }
                }
                else
                {
                    // Fewer meaningful words than target — take all of them
                    chosen.AddRange(meaningfulIdx);
                }

                // ── Step 4 : top up with fillers if still short ───────
                // Use filler words that sit closest to under-represented
                // parts of the sentence (prefer first and last fillers).
                if (chosen.Count < TARGET_WORDS && fillerIdx.Count > 0)
                {
                    // Sort fillers to prefer boundary words
                    List<int> sortedFillers = new List<int>(fillerIdx);
                    // Take from start, then end, then middle of filler list
                    int fi = 0, bi = sortedFillers.Count - 1;
                    bool fromFront = true;
                    while (chosen.Count < TARGET_WORDS && fi <= bi)
                    {
                        int pick = fromFront ? sortedFillers[fi++]
                                             : sortedFillers[bi--];
                        fromFront = !fromFront;
                        if (!chosen.Contains(pick))
                            chosen.Add(pick);
                    }
                }

                // ── Step 5 : restore original reading order ───────────
                chosen.Sort();

                // ── Step 6 : build output string ──────────────────────
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < chosen.Count; i++)
                {
                    if (i > 0) sb.Append(" ... ");
                    sb.Append(tokens[chosen[i]]);
                }

                // Capitalise first letter
                if (sb.Length > 0)
                    sb[0] = char.ToUpper(sb[0]);

                return sb.ToString();
            }
        }

        // ── Word bank (120 entries) ───────────────────────────────────
        private static readonly List<WordEntry> _wordBank = new List<WordEntry>
        {
            // Animals
            new WordEntry("elephant",     "Animals",     "Largest land animal"),
            new WordEntry("giraffe",      "Animals",     "Tallest living terrestrial animal"),
            new WordEntry("crocodile",    "Animals",     "Ancient reptile with powerful jaws"),
            new WordEntry("chimpanzee",   "Animals",     "Our closest primate relative"),
            new WordEntry("rhinoceros",   "Animals",     "Horn-nosed African giant"),
            new WordEntry("flamingo",     "Animals",     "Pink wading bird"),
            new WordEntry("penguin",      "Animals",     "Flightless bird of the Antarctic"),
            new WordEntry("chameleon",    "Animals",     "Reptile that changes colour"),
            new WordEntry("porcupine",    "Animals",     "Rodent covered in sharp quills"),
            new WordEntry("platypus",     "Animals",     "Egg-laying mammal with a duck bill"),

            // Technology
            new WordEntry("algorithm",    "Technology",  "Step-by-step problem-solving procedure"),
            new WordEntry("database",     "Technology",  "Organised collection of structured data"),
            new WordEntry("encryption",   "Technology",  "Process of encoding information securely"),
            new WordEntry("framework",    "Technology",  "Reusable software platform structure"),
            new WordEntry("interface",    "Technology",  "Contract for class behaviour in OOP"),
            new WordEntry("polymorphism", "Technology",  "OOP concept: many forms of a method"),
            new WordEntry("inheritance",  "Technology",  "OOP mechanism: child inherits parent"),
            new WordEntry("abstraction",  "Technology",  "Hiding complexity behind simple APIs"),
            new WordEntry("bandwidth",    "Technology",  "Data transfer rate of a network"),
            new WordEntry("compiler",     "Technology",  "Translates source code to machine code"),
            new WordEntry("debugger",     "Technology",  "Tool for finding code errors"),
            new WordEntry("microchip",    "Technology",  "Tiny integrated circuit"),
            new WordEntry("semiconductor","Technology",  "Material with controllable conductivity"),
            new WordEntry("repository",   "Technology",  "Version-controlled code storage"),
            new WordEntry("constructor",  "Technology",  "Special method that initialises objects"),

            // Countries
            new WordEntry("pakistan",     "Countries",   "Nation founded in 1947 in South Asia"),
            new WordEntry("australia",    "Countries",   "Continent that is also a country"),
            new WordEntry("argentina",    "Countries",   "South American nation, famous for tango"),
            new WordEntry("indonesia",    "Countries",   "World's largest archipelago nation"),
            new WordEntry("kazakhstan",   "Countries",   "Largest landlocked country in the world"),
            new WordEntry("mozambique",   "Countries",   "East African coastal nation"),
            new WordEntry("switzerland",  "Countries",   "Known for watches, chocolate, and Alps"),
            new WordEntry("venezuela",    "Countries",   "Home to Angel Falls"),
            new WordEntry("bangladesh",   "Countries",   "Delta nation in South Asia"),
            new WordEntry("cambodia",     "Countries",   "Home to Angkor Wat temple"),

            // Science
            new WordEntry("photosynthesis","Science",    "How plants convert sunlight to energy"),
            new WordEntry("mitochondria", "Science",     "Powerhouse of the cell"),
            new WordEntry("chromosome",   "Science",     "DNA-carrying structure in cell nucleus"),
            new WordEntry("atmosphere",   "Science",     "Layers of gas surrounding Earth"),
            new WordEntry("hypothesis",   "Science",     "Testable scientific prediction"),
            new WordEntry("laboratory",   "Science",     "Controlled environment for experiments"),
            new WordEntry("telescope",    "Science",     "Instrument for viewing distant objects"),
            new WordEntry("microscope",   "Science",     "Instrument that magnifies tiny things"),
            new WordEntry("earthquake",   "Science",     "Sudden ground shaking from tectonic shift"),
            new WordEntry("lightning",    "Science",     "Electrical discharge during a storm"),
            new WordEntry("evaporation",  "Science",     "Liquid turning into vapour"),
            new WordEntry("constellation","Science",     "Named pattern of stars in the sky"),
            new WordEntry("metabolism",   "Science",     "Chemical processes sustaining life"),
            new WordEntry("neutron",      "Science",     "Neutral particle in an atomic nucleus"),
            new WordEntry("quantum",      "Science",     "Discrete unit of energy"),

            // Sports
            new WordEntry("basketball",   "Sports",      "Five-player ball-and-hoop sport"),
            new WordEntry("volleyball",   "Sports",      "Net sport played with inflated ball"),
            new WordEntry("badminton",    "Sports",      "Racket sport using a shuttlecock"),
            new WordEntry("gymnastics",   "Sports",      "Sport of acrobatic body movements"),
            new WordEntry("wrestling",    "Sports",      "Combat sport of holds and throws"),
            new WordEntry("swimming",     "Sports",      "Aquatic locomotion sport"),
            new WordEntry("marathon",     "Sports",      "26.2-mile running race"),
            new WordEntry("archery",      "Sports",      "Bow-and-arrow target sport"),
            new WordEntry("skateboard",   "Sports",      "Board on four wheels for tricks"),
            new WordEntry("cricket",      "Sports",      "Bat-and-ball sport popular in Pakistan"),

            // Food
            new WordEntry("chocolate",    "Food",        "Sweet made from cocoa beans"),
            new WordEntry("pineapple",    "Food",        "Tropical fruit with spiky crown"),
            new WordEntry("blueberry",    "Food",        "Small dark blue antioxidant fruit"),
            new WordEntry("avocado",      "Food",        "Green creamy fruit used in guacamole"),
            new WordEntry("cinnamon",     "Food",        "Aromatic spice from tree bark"),
            new WordEntry("spaghetti",    "Food",        "Long thin Italian pasta"),
            new WordEntry("cappuccino",   "Food",        "Italian espresso with steamed milk"),
            new WordEntry("watermelon",   "Food",        "Large green fruit with red interior"),
            new WordEntry("raspberry",    "Food",        "Small red berry with hollow centre"),
            new WordEntry("cauliflower",  "Food",        "White vegetable in the broccoli family"),

            // Geography
            new WordEntry("himalaya",     "Geography",   "World's highest mountain range"),
            new WordEntry("amazon",       "Geography",   "World's largest rainforest and river"),
            new WordEntry("volcano",      "Geography",   "Mountain that can erupt lava"),
            new WordEntry("peninsula",    "Geography",   "Land almost surrounded by water"),
            new WordEntry("archipelago",  "Geography",   "Group or chain of islands"),
            new WordEntry("tributary",    "Geography",   "Smaller river feeding a larger one"),
            new WordEntry("equator",      "Geography",   "Imaginary line around Earth's middle"),
            new WordEntry("canyon",       "Geography",   "Deep gorge carved by a river"),
            new WordEntry("glacier",      "Geography",   "Slow-moving mass of ice"),
            new WordEntry("savanna",      "Geography",   "Tropical grassland with few trees"),

            // History
            new WordEntry("revolution",   "History",     "Dramatic political or social upheaval"),
            new WordEntry("civilization", "History",     "Advanced organised human society"),
            new WordEntry("democracy",    "History",     "Government by the people"),
            new WordEntry("renaissance",  "History",     "European cultural rebirth 14th-17th century"),
            new WordEntry("colonialism",  "History",     "Policy of dominating foreign territories"),
            new WordEntry("parliament",   "History",     "Legislative assembly of elected members"),
            new WordEntry("emperor",      "History",     "Ruler of an empire"),
            new WordEntry("constitution", "History",     "Fundamental laws of a nation"),
            new WordEntry("independence", "History",     "Freedom from external control"),
            new WordEntry("liberation",   "History",     "Act of setting someone free"),

            // Professions
            new WordEntry("architect",    "Profession",  "Designer of buildings and structures"),
            new WordEntry("pharmacist",   "Profession",  "Dispenses medicines and drugs"),
            new WordEntry("journalist",   "Profession",  "Researches and reports news stories"),
            new WordEntry("astronaut",    "Profession",  "Person trained for space travel"),
            new WordEntry("surgeon",      "Profession",  "Doctor who performs operations"),
            new WordEntry("engineer",     "Profession",  "Applies science to design solutions"),
            new WordEntry("diplomat",     "Profession",  "Represents a country abroad"),
            new WordEntry("psychologist", "Profession",  "Studies human mind and behaviour"),
            new WordEntry("accountant",   "Profession",  "Manages financial records"),
            new WordEntry("detective",    "Profession",  "Investigates crimes or mysteries"),

            // General
            new WordEntry("adventure",    "General",     "Exciting and daring experience"),
            new WordEntry("beautiful",    "General",     "Pleasing to the senses"),
            new WordEntry("challenge",    "General",     "Difficult task requiring effort"),
            new WordEntry("discovery",    "General",     "Finding something previously unknown"),
            new WordEntry("education",    "General",     "Process of learning and teaching"),
            new WordEntry("friendship",   "General",     "Mutual affection between people"),
            new WordEntry("government",   "General",     "System that rules a state"),
            new WordEntry("happiness",    "General",     "State of feeling pleased and content"),
            new WordEntry("imagination",  "General",     "Ability to form mental pictures"),
            new WordEntry("knowledge",    "General",     "Facts and information acquired"),
            new WordEntry("language",     "General",     "System of human communication"),
            new WordEntry("mysterious",   "General",     "Difficult or impossible to understand"),
            new WordEntry("nightmare",    "General",     "Frightening dream"),
            new WordEntry("opportunity",  "General",     "Favourable set of circumstances"),
            new WordEntry("perseverance", "General",     "Continued effort despite difficulty"),
        };

        // ── Random selection ─────────────────────────────────────────
        private static readonly Random _rng = new Random();

        /// <summary>Returns a random word entry from the bank.</summary>
        public WordEntry GetRandomWord()
        {
            int index = _rng.Next(_wordBank.Count);
            return _wordBank[index];
        }

        /// <summary>Total number of words in the bank.</summary>
        public int Count { get { return _wordBank.Count; } }
    }
}
