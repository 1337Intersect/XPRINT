// App.xaml.cs
using System;
using System.IO;
using System.Windows;
using XPRINT.Models;
using XPRINT.Services;
using XPRINT.ViewModels;
using XPRINT.Views;

namespace XPRINT
{
    public partial class App : Application
    {
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            // Crea e configura le impostazioni dell'applicazione
            var settings = new AppSettings();

            // Carica le impostazioni da un file di configurazione
            LoadSettingsFromFile(settings);

            // Crea il ViewModel principale con le impostazioni
            var viewModel = new MainViewModel(settings);

            // Crea e mostra la finestra principale
            var mainWindow = new MainWindow
            {
                DataContext = viewModel
            };

            mainWindow.Show();

            // Controlla gli aggiornamenti in background all'avvio
            await viewModel.CheckForUpdatesOnStartup();
        }

        private void LoadSettingsFromFile(AppSettings settings)
        {
            var settingsService = new SettingsService();

            if (!settingsService.LoadSettings(settings))
            {
                // Se il caricamento fallisce, usa i valori predefiniti
                settings.PrinterIpAddress = "0.0.0.0";
                settings.Directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                settings.FileType = "";
                settings.UseDefaultExtensions = false;
                settings.Port = 9100;
                settings.CurrentVersion = VersionInfo.CurrentVersion; // Usa la versione centralizzata
                settings.SkipVersion = "";

                settingsService.SaveSettings(settings);
            }
        }
    }
}