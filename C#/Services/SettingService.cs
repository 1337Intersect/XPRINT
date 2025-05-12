// Services/SettingsService.cs
using System;
using System.IO;
using System.Xml.Linq;
using XPRINT.Models;

namespace XPRINT.Services
{
    public class SettingsService
    {
        private readonly string _configFilePath;

        public SettingsService()
        {
            // Ottieni la cartella AppData
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string xprintFolder = Path.Combine(appDataFolder, "XPRINT");

            // Crea la cartella se non esiste
            if (!Directory.Exists(xprintFolder))
            {
                Directory.CreateDirectory(xprintFolder);
            }

            // Percorso del file di configurazione
            _configFilePath = Path.Combine(xprintFolder, "config.xml");
        }

        // Salva le impostazioni su un file XML
        public bool SaveSettings(AppSettings settings)
        {
            try
            {
                XDocument doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("Settings",
                        new XElement("PrinterIPAddress", settings.PrinterIpAddress),
                        new XElement("Directory", settings.Directory),
                        new XElement("FileType", settings.FileType),
                        new XElement("UseDefaultExtensions", settings.UseDefaultExtensions.ToString()),
                        new XElement("Port", settings.Port.ToString()),
                        new XElement("CurrentVersion", settings.CurrentVersion),
                        new XElement("SkipVersion", settings.SkipVersion)
                    )
                );

                doc.Save(_configFilePath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il salvataggio delle impostazioni: {ex.Message}");
                return false;
            }
        }

        // Carica le impostazioni da un file XML
        public bool LoadSettings(AppSettings settings)
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    return false;
                }

                XDocument doc = XDocument.Load(_configFilePath);
                XElement root = doc.Root;

                // Carica ogni impostazione se presente
                settings.PrinterIpAddress = GetElementValue(root, "PrinterIPAddress", settings.PrinterIpAddress);
                settings.Directory = GetElementValue(root, "Directory", settings.Directory);
                settings.FileType = GetElementValue(root, "FileType", settings.FileType);
                settings.UseDefaultExtensions = bool.TryParse(GetElementValue(root, "UseDefaultExtensions", settings.UseDefaultExtensions.ToString()), out bool useDefaultExtensions) ? useDefaultExtensions : settings.UseDefaultExtensions;
                settings.Port = int.TryParse(GetElementValue(root, "Port", settings.Port.ToString()), out int port) ? port : settings.Port;
                settings.SkipVersion = GetElementValue(root, "SkipVersion", settings.SkipVersion);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il caricamento delle impostazioni: {ex.Message}");
                return false;
            }
        }

        // Metodo di utilità per recuperare il valore di un elemento XML
        private string GetElementValue(XElement parent, string elementName, string defaultValue)
        {
            XElement element = parent?.Element(elementName);
            return element != null ? element.Value : defaultValue;
        }
    }
}