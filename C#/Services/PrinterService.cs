// Services/PrinterService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using XPRINT.Models;

namespace XPRINT.Services
{
    public class PrinterService
    {
        private readonly string _configFilePath;

        public PrinterService()
        {
            // Equivalente della cartella AppDataDir in AutoIt
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string xprintFolder = Path.Combine(appDataFolder, "XPRINT");

            // Crea la cartella se non esiste
            if (!Directory.Exists(xprintFolder))
            {
                Directory.CreateDirectory(xprintFolder);
            }

            // Percorso del file XML di configurazione (invece del file INI)
            _configFilePath = Path.Combine(xprintFolder, "printers.xml");

            // Inizializza il file XML se non esiste
            if (!File.Exists(_configFilePath))
            {
                CreateEmptyPrintersFile();
            }
        }

        // Crea un file XML vuoto per le stampanti
        private void CreateEmptyPrintersFile()
        {
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("Printers")
            );

            doc.Save(_configFilePath);
        }

        // Ottieni tutte le stampanti
        public List<Printer> GetAllPrinters()
        {
            List<Printer> printers = new List<Printer>();

            try
            {
                XDocument doc = XDocument.Load(_configFilePath);

                foreach (XElement printerElement in doc.Root.Elements("Printer"))
                {
                    Printer printer = new Printer
                    {
                        Model = printerElement.Element("Model")?.Value ?? "",
                        IpAddress = printerElement.Element("IP")?.Value ?? "",
                        Customer = printerElement.Element("Customer")?.Value ?? ""
                    };

                    printers.Add(printer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il caricamento delle stampanti: {ex.Message}");
            }

            return printers;
        }

        // Aggiungi una nuova stampante
        public bool AddPrinter(Printer printer)
        {
            try
            {
                // Verifica se la stampante esiste già
                if (PrinterExists(printer.UniqueId))
                {
                    return false;
                }

                XDocument doc = XDocument.Load(_configFilePath);

                XElement newPrinter = new XElement("Printer",
                    new XElement("Model", printer.Model),
                    new XElement("IP", printer.IpAddress),
                    new XElement("Customer", printer.Customer),
                    new XElement("UniqueId", printer.UniqueId)
                );

                doc.Root.Add(newPrinter);
                doc.Save(_configFilePath);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante l'aggiunta della stampante: {ex.Message}");
                return false;
            }
        }

        // Modifica una stampante esistente
        public bool UpdatePrinter(string oldUniqueId, Printer updatedPrinter)
        {
            try
            {
                XDocument doc = XDocument.Load(_configFilePath);

                // Trova l'elemento da aggiornare
                XElement printerElement = doc.Root.Elements("Printer")
                    .FirstOrDefault(p => p.Element("UniqueId")?.Value == oldUniqueId);

                if (printerElement == null)
                {
                    return false;
                }

                // Aggiorna i valori
                printerElement.Element("Model").Value = updatedPrinter.Model;
                printerElement.Element("IP").Value = updatedPrinter.IpAddress;
                printerElement.Element("Customer").Value = updatedPrinter.Customer;
                printerElement.Element("UniqueId").Value = updatedPrinter.UniqueId;

                doc.Save(_configFilePath);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante l'aggiornamento della stampante: {ex.Message}");
                return false;
            }
        }

        // Elimina una stampante esistente
        public bool DeletePrinter(string uniqueId)
        {
            try
            {
                XDocument doc = XDocument.Load(_configFilePath);

                // Trova l'elemento da eliminare
                XElement printerElement = doc.Root.Elements("Printer")
                    .FirstOrDefault(p => p.Element("UniqueId")?.Value == uniqueId);

                if (printerElement == null)
                {
                    return false;
                }

                // Rimuovi l'elemento
                printerElement.Remove();
                doc.Save(_configFilePath);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante l'eliminazione della stampante: {ex.Message}");
                return false;
            }
        }

        // Verifica se una stampante esiste già
        private bool PrinterExists(string uniqueId)
        {
            try
            {
                XDocument doc = XDocument.Load(_configFilePath);

                return doc.Root.Elements("Printer")
                    .Any(p => p.Element("UniqueId")?.Value == uniqueId);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}