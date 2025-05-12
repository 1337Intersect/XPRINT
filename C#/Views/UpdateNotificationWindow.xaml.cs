using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace XPRINT.Views
{
    public partial class UpdateNotificationWindow : Window
    {
        public string CurrentVersion { get; set; }
        public string UpdateVersion { get; set; }
        public string ReleaseInfoUrl { get; set; }

        // Eventi per le azioni
        public event EventHandler UpdateRequested;
        public event EventHandler SkipVersionRequested;
        public event EventHandler RemindLaterRequested;

        public UpdateNotificationWindow(string currentVersion, string updateVersion, string releaseInfoUrl)
        {
            InitializeComponent();

            CurrentVersion = currentVersion;
            UpdateVersion = updateVersion;
            ReleaseInfoUrl = releaseInfoUrl;

            // Imposta il DataContext
            DataContext = this;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Chiudi senza fare nulla (equivalente a Remind Later)
            RemindLaterRequested?.Invoke(this, EventArgs.Empty);
            Close();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateRequested?.Invoke(this, EventArgs.Empty);
            Close();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            SkipVersionRequested?.Invoke(this, EventArgs.Empty);
            Close();
        }

        private void RemindButton_Click(object sender, RoutedEventArgs e)
        {
            RemindLaterRequested?.Invoke(this, EventArgs.Empty);
            Close();
        }

        private void ReleaseInfo_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Apri il browser con l'URL delle note di rilascio
                Process.Start(new ProcessStartInfo
                {
                    FileName = ReleaseInfoUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Impossibile aprire le note di rilascio: {ex.Message}",
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Permette di trascinare la finestra
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
}