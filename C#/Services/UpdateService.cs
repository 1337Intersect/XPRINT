// Services/UpdateService.cs
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using XPRINT.Models;
using XPRINT.Views;

namespace XPRINT.Services
{
    public class UpdateService
    {
        private readonly AppSettings _settings;
        private readonly string _githubRepo;
        private readonly string _tempPath;

        public UpdateService(AppSettings settings, string githubRepo = "1337Intersect/XPRINT")
        {
            _settings = settings;
            _githubRepo = githubRepo;
            _tempPath = Path.Combine(Path.GetTempPath(), "XPRINT_update.exe");
        }

        // Controlla gli aggiornamenti (modalità silenziosa opzionale)
        public async Task<bool> CheckForUpdates(bool silent = false)
        {
            try
            {
                // URL del file di versione su GitHub
                string versionFileUrl = $"https://raw.githubusercontent.com/{_githubRepo}/main/version.txt";

                // Scarica il contenuto del file di versione
                string versionContent;
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                    versionContent = await client.GetStringAsync(versionFileUrl);
                }

                // Pulisci e analizza il contenuto
                versionContent = versionContent.Trim();
                string[] versionInfo = versionContent.Split('|');

                if (versionInfo.Length < 1)
                {
                    if (!silent)
                    {
                        MessageBox.Show("Formato file di versione non valido.", "Errore",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return false;
                }

                string newVersion = versionInfo[0];
                string fileName = versionInfo.Length > 1 ? versionInfo[1] : "XPRINT.exe";

                // Confronta le versioni
                if (CompareVersions(_settings.CurrentVersion, newVersion) < 0)
                {
                    // Verifica se l'utente ha scelto di saltare questa versione
                    if (newVersion == _settings.SkipVersion)
                    {
                        return false;
                    }

                    // Nuova versione disponibile
                    if (silent)
                    {
                        // Mostra finestra di aggiornamento moderna
                        bool updateConfirmed = await ShowUpdateWindow(newVersion);
                        if (!updateConfirmed)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // Chiedi conferma all'utente
                        var result = MessageBox.Show(
                            $"È disponibile la versione {newVersion} di XPRINT.\n\n" +
                            $"Versione attuale: {_settings.CurrentVersion}\n" +
                            "Vuoi aggiornare adesso?",
                            "Aggiornamento Disponibile",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Information);

                        if (result != MessageBoxResult.Yes)
                        {
                            return false;
                        }
                    }

                    // URL di download
                    string downloadUrl = $"https://github.com/{_githubRepo}/releases/download/v{newVersion}/{fileName}";

                    // Scarica l'aggiornamento con progress bar
                    bool downloadSuccess = await DownloadUpdate(downloadUrl);

                    if (downloadSuccess)
                    {
                        // Crea uno script batch per l'aggiornamento
                        CreateUpdateBatch();

                        // Informa l'utente
                        MessageBox.Show(
                            "XPRINT verrà chiuso e aggiornato. Al termine, verrà riavviato automaticamente.",
                            "Aggiornamento",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        // Esegui lo script batch
                        Process.Start(Path.Combine(Path.GetTempPath(), "XPRINT_updater.bat"));

                        // Termina l'applicazione
                        Application.Current.Shutdown();
                        return true;
                    }
                    else
                    {
                        if (!silent)
                        {
                            MessageBox.Show(
                                "Impossibile scaricare l'aggiornamento. Riprova più tardi.",
                                "Errore",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                        return false;
                    }
                }
                else
                {
                    // Nessun aggiornamento disponibile
                    if (!silent)
                    {
                        MessageBox.Show(
                            "Stai già utilizzando l'ultima versione di XPRINT.",
                            "Informazione",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (!silent)
                {
                    MessageBox.Show(
                        $"Errore durante il controllo degli aggiornamenti: {ex.Message}",
                        "Errore",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                return false;
            }
        }

        // Confronta due stringhe di versione
        private int CompareVersions(string version1, string version2)
        {
            // Assicurati che le stringhe non siano vuote
            if (string.IsNullOrEmpty(version1) || string.IsNullOrEmpty(version2))
            {
                return 0;
            }

            // Rimuovi spazi bianchi
            version1 = version1.Trim();
            version2 = version2.Trim();

            // Dividi le stringhe nei numeri delle componenti
            string[] v1Parts = version1.Split('.');
            string[] v2Parts = version2.Split('.');

            // Determina il numero massimo di componenti
            int maxLength = Math.Max(v1Parts.Length, v2Parts.Length);

            // Confronta le componenti una per una
            for (int i = 0; i < maxLength; i++)
            {
                int v1Component = i < v1Parts.Length ? int.Parse(v1Parts[i]) : 0;
                int v2Component = i < v2Parts.Length ? int.Parse(v2Parts[i]) : 0;

                if (v1Component < v2Component)
                {
                    return -1;
                }
                else if (v1Component > v2Component)
                {
                    return 1;
                }
            }

            // Le versioni sono uguali
            return 0;
        }

        // Scarica l'aggiornamento con progress bar
        private async Task<bool> DownloadUpdate(string url)
        {
            try
            {
                // Crea una finestra di progresso
                Window progressWindow = new Window
                {
                    Title = "Download Aggiornamento",
                    Width = 350,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize
                };

                // Crea i controlli per la finestra
                Grid grid = new Grid { Margin = new Thickness(10) };
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                TextBlock statusText = new TextBlock
                {
                    Text = "Download in corso...",
                    Margin = new Thickness(0, 0, 0, 10)
                };
                Grid.SetRow(statusText, 0);

                ProgressBar progressBar = new ProgressBar
                {
                    Height = 20,
                    Minimum = 0,
                    Maximum = 100
                };
                Grid.SetRow(progressBar, 1);

                grid.Children.Add(statusText);
                grid.Children.Add(progressBar);
                progressWindow.Content = grid;

                // Mostra la finestra
                progressWindow.Show();

                // Inizializza HttpClient
                using (HttpClient client = new HttpClient())
                {
                    // Abilita il download in modalità streaming
                    using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        // Ottieni la dimensione totale del file
                        long totalBytes = response.Content.Headers.ContentLength ?? -1L;

                        // Apri lo stream di risposta
                        using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            // Crea un buffer per la lettura
                            byte[] buffer = new byte[8192];
                            bool fileOk = false;

                            // Apri il file di destinazione
                            using (FileStream fileStream = new FileStream(_tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                long totalBytesRead = 0;
                                int bytesRead;

                                // Leggi il file a blocchi
                                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                {
                                    await fileStream.WriteAsync(buffer, 0, bytesRead);

                                    // Aggiorna il contatore dei byte letti
                                    totalBytesRead += bytesRead;

                                    // Calcola e mostra il progresso
                                    if (totalBytes > 0)
                                    {
                                        double progressPercentage = (double)totalBytesRead / totalBytes * 100;
                                        progressBar.Value = progressPercentage;
                                        statusText.Text = $"Download in corso... {FormatBytes(totalBytesRead)} / {FormatBytes(totalBytes)}";
                                    }
                                    else
                                    {
                                        statusText.Text = $"Download in corso... {FormatBytes(totalBytesRead)}";
                                    }
                                }

                                fileOk = true;
                            }

                            // Chiudi la finestra di progresso
                            progressWindow.Close();

                            // Verifica che il file sia stato scaricato con successo
                            if (fileOk && File.Exists(_tempPath) && new FileInfo(_tempPath).Length > 0)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il download: {ex.Message}", "Errore",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        // Crea un file batch per l'installazione dell'aggiornamento
        private void CreateUpdateBatch()
        {
            string batchPath = Path.Combine(Path.GetTempPath(), "XPRINT_updater.bat");

            try
            {
                using (StreamWriter writer = new StreamWriter(batchPath))
                {
                    writer.WriteLine("@echo off");
                    writer.WriteLine("echo Attendere, aggiornamento di XPRINT in corso...");
                    writer.WriteLine("ping 127.0.0.1 -n 3 > nul"); // Attendi un po'
                    writer.WriteLine($"if exist \"{Process.GetCurrentProcess().MainModule.FileName}\" (");
                    writer.WriteLine($"    del /f /q \"{Process.GetCurrentProcess().MainModule.FileName}\"");
                    writer.WriteLine(")");
                    writer.WriteLine($"if exist \"{_tempPath}\" (");
                    writer.WriteLine($"    copy /y \"{_tempPath}\" \"{Process.GetCurrentProcess().MainModule.FileName}\"");
                    writer.WriteLine($"    del /f /q \"{_tempPath}\"");
                    writer.WriteLine(")");
                    writer.WriteLine($"start \"\" \"{Process.GetCurrentProcess().MainModule.FileName}\"");
                    writer.WriteLine("del \"%~f0\""); // Auto-elimina il batch
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la creazione del batch di aggiornamento: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Formatta i byte in una stringa leggibile
        private string FormatBytes(long bytes)
        {
            const long KB = 1024;
            const long MB = KB * 1024;

            if (bytes < KB)
            {
                return $"{bytes} B";
            }
            else if (bytes < MB)
            {
                return $"{bytes / (double)KB:0.00} KB";
            }
            else
            {
                return $"{bytes / (double)MB:0.00} MB";
            }
        }

        // Mostra la finestra di aggiornamento moderna
        private async Task<bool> ShowUpdateWindow(string newVersion)
        {
            // Crea una TaskCompletionSource per gestire il risultato
            var tcs = new TaskCompletionSource<bool>();

            // Crea la nuova finestra di aggiornamento
            string releaseInfoUrl = $"https://github.com/{_githubRepo}/releases/tag/v{newVersion}";
            var updateWindow = new UpdateNotificationWindow(_settings.CurrentVersion, newVersion, releaseInfoUrl);

            // Gestisci gli eventi
            updateWindow.UpdateRequested += (s, e) => tcs.SetResult(true);
            updateWindow.SkipVersionRequested += (s, e) =>
            {
                // Salva la versione da saltare
                _settings.SkipVersion = newVersion;
                tcs.SetResult(false);
            };
            updateWindow.RemindLaterRequested += (s, e) => tcs.SetResult(false);

            // Gestisci la chiusura della finestra
            updateWindow.Closed += (s, e) =>
            {
                if (!tcs.Task.IsCompleted)
                {
                    tcs.SetResult(false);
                }
            };

            // Mostra la finestra
            updateWindow.Show();

            // Attendi il risultato
            return await tcs.Task;
        }
    }
}