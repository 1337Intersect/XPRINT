using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XPRINT.Models
{
    public static class VersionInfo
    {
        // Qui definisci la versione corrente dell'applicazione
        // Aggiorna questi valori prima di ogni nuovo rilascio
        public const string CurrentVersion = "2.0.0";

        // Data dell'ultimo rilascio (utile per scopi informativi)
        public const string ReleaseDate = "12/05/2025";

        // Descrizione della release corrente (opzionale)
        public const string ReleaseNotes = "Migrazione a C# con nuova interfaccia utente";

        // Informazioni complete sulla versione
        public static string FullVersionInfo =>
            $"v{CurrentVersion} ({ReleaseDate})";
    }
}
