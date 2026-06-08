// ============================================================
// File        : DatabaseConfig.cs
// Namespace   : HangmanGame.Database
// Purpose     : Centralised database connection configuration.
//               Edit ONLY this file to point at your MySQL server.
// ============================================================

namespace HangmanGame.Database
{
    /// <summary>
    /// Holds all connection-related constants.
    /// Modify Server / Port / Database / Uid / Pwd to match your
    /// MySQL Workbench 8.0 installation.
    /// </summary>
    public static class DatabaseConfig
    {
        // ── Connection parameters ────────────────────────────────────
        public const string Server = "localhost";
        public const int Port = 3306;
        public const string Database = "hangman_db";
        public const string UserId = "root";          // change if needed
        public const string Password = "your_password"; // change to your MySQL root password

        // ── Derived connection string ────────────────────────────────
        public static string ConnectionString =>
            $"Server={Server};Port={Port};Database={Database};" +
            $"Uid={UserId};Pwd={Password};" +
            $"CharSet=utf8mb4;SslMode=None;AllowPublicKeyRetrieval=True;";
    }
}
