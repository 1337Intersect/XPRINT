// Services/FileService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using XPRINT.Models;

namespace XPRINT.Services
{
    public class FileService
    {
        private readonly AppSettings _settings;

        public FileService(AppSettings settings)
        {
            _settings = settings;
        }

        // Equivalente a LoadFiles() in AutoIt
        public List<string> GetFiles()
        {
            var result = new List<string>();

            try
            {
                if (!Directory.Exists(_settings.Directory))
                {
                    return result;
                }

                // Ottieni tutti i file nella directory
                string[] allFiles = Directory.GetFiles(_settings.Directory);

                if (_settings.UseDefaultExtensions)
                {
                    // Filtra per estensioni predefinite
                    result = allFiles
                        .Where(f => _settings.DefaultExtensions.Contains(
                            Path.GetExtension(f).TrimStart('.').ToLower()))
                        .Select(Path.GetFileName)
                        .ToList();
                }
                else if (!string.IsNullOrEmpty(_settings.FileType))
                {
                    // Filtra per tipo di file specificato
                    string extension = _settings.FileType.TrimStart('.');
                    result = allFiles
                        .Where(f => Path.GetExtension(f).TrimStart('.').Equals(extension, StringComparison.OrdinalIgnoreCase))
                        .Select(Path.GetFileName)
                        .ToList();
                }
                else
                {
                    // Nessun filtro, restituisci tutti i file
                    result = allFiles.Select(Path.GetFileName).ToList();
                }
            }
            catch (Exception ex)
            {
                // Log dell'errore (in un'applicazione reale useresti un vero sistema di log)
                Console.WriteLine($"Errore durante il caricamento dei file: {ex.Message}");
            }

            return result;
        }

        // Equivalente a PrintSelectedFile() in AutoIt
        public async Task<bool> PrintFile(string fileName)
        {
            try
            {
                string filePath = Path.Combine(_settings.Directory, fileName);

                if (!File.Exists(filePath))
                {
                    return false;
                }

                // Leggi il file come byte array
                byte[] fileData = File.ReadAllBytes(filePath);

                // Verifica connessione alla stampante (simulazione del ping)
                if (await IsPrinterReachable(_settings.PrinterIpAddress))
                {
                    // Invia dati alla stampante
                    bool success = await SendToPrinter(fileData, _settings.PrinterIpAddress, _settings.Port);

                    // Se è un file SPL e la stampa è andata a buon fine, eliminalo
                    if (success && Path.GetExtension(filePath).Equals(".spl", StringComparison.OrdinalIgnoreCase))
                    {
                        File.Delete(filePath);
                    }

                    return success;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la stampa: {ex.Message}");
                return false;
            }
        }

        // Sostituisce _PrintContent() in AutoIt
        public async Task<bool> PrintContent(string content)
        {
            try
            {
                // Crea un file temporaneo
                string tempFile = Path.Combine(Path.GetTempPath(), "PrintTemp.txt");
                File.WriteAllText(tempFile, content);

                if (await IsPrinterReachable(_settings.PrinterIpAddress))
                {
                    // Leggi il file temporaneo e invialo alla stampante
                    byte[] fileData = File.ReadAllBytes(tempFile);
                    bool success = await SendToPrinter(fileData, _settings.PrinterIpAddress, _settings.Port);

                    // Elimina il file temporaneo
                    File.Delete(tempFile);

                    return success;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la stampa del contenuto: {ex.Message}");
                return false;
            }
        }

        // Metodo per inviare dati alla stampante (equivalente a _FileSend in AutoIt)
        private async Task<bool> SendToPrinter(byte[] data, string ipAddress, int port)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(ipAddress, port);

                    using (NetworkStream stream = client.GetStream())
                    {
                        await stream.WriteAsync(data, 0, data.Length);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante l'invio alla stampante: {ex.Message}");
                return false;
            }
        }

        // Metodo per verificare se la stampante è raggiungibile
        private async Task<bool> IsPrinterReachable(string ipAddress)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    // Timeout di 1 secondo (1000ms) come nel tuo Ping in AutoIt
                    var connectTask = client.ConnectAsync(ipAddress, _settings.Port);
                    var timeoutTask = Task.Delay(1000);

                    // Attendi che la connessione avvenga o che scada il timeout
                    var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                    // Se la connessione è avvenuta prima del timeout
                    return completedTask == connectTask && !connectTask.IsFaulted;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}