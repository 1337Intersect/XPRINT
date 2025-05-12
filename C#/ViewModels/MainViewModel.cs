// ViewModels/MainViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XPRINT.Models;
using XPRINT.Services;
using XPRINT.Views;
using Forms = System.Windows.Forms;

namespace XPRINT.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // Servizi
        private readonly FileService _fileService;
        private readonly UpdateService _updateService;
        private readonly SettingsService _settingsService;
        private readonly AppSettings _settings;


        // Proprietà per l'interfaccia
        private ObservableCollection<string> _files;
        public ObservableCollection<string> Files
        {
            get => _files;
            set
            {
                _files = value;
                OnPropertyChanged();
            }
        }

        private string _selectedFile;
        public string SelectedFile
        {
            get => _selectedFile;
            set
            {
                _selectedFile = value;
                OnPropertyChanged();
            }
        }

        private bool _useDefaultExtensions;
        public bool UseDefaultExtensions
        {
            get => _useDefaultExtensions;
            set
            {
                _useDefaultExtensions = value;
                _settings.UseDefaultExtensions = value;
                SaveSettings();
                RefreshFiles();
                OnPropertyChanged();
            }
        }

        // Segmenti dell'indirizzo IP
        private string _ipSegment1 = "0";
        public string IpSegment1
        {
            get => _ipSegment1;
            set
            {
                if (int.TryParse(value, out int result) && result >= 0 && result <= 255)
                {
                    _ipSegment1 = value;
                    UpdateIpAddress();
                    OnPropertyChanged();
                }
            }
        }

        private string _ipSegment2 = "0";
        public string IpSegment2
        {
            get => _ipSegment2;
            set
            {
                if (int.TryParse(value, out int result) && result >= 0 && result <= 255)
                {
                    _ipSegment2 = value;
                    UpdateIpAddress();
                    OnPropertyChanged();
                }
            }
        }

        private string _ipSegment3 = "0";
        public string IpSegment3
        {
            get => _ipSegment3;
            set
            {
                if (int.TryParse(value, out int result) && result >= 0 && result <= 255)
                {
                    _ipSegment3 = value;
                    UpdateIpAddress();
                    OnPropertyChanged();
                }
            }
        }

        private string _ipSegment4 = "0";
        public string IpSegment4
        {
            get => _ipSegment4;
            set
            {
                if (int.TryParse(value, out int result) && result >= 0 && result <= 255)
                {
                    _ipSegment4 = value;
                    UpdateIpAddress();
                    OnPropertyChanged();
                }
            }
        }

        // Informazioni di stato
        private string _statusText = "Pronto";
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        public string CurrentDirectory => $"Directory: {_settings.Directory}";
        public string VersionInfo => $"Versione: {_settings.CurrentVersion}";

        // Comandi
        public ICommand RefreshCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand ViewPrintCommand { get; }
        public ICommand OpenWebPageCommand { get; }
        public ICommand ManagePrintersCommand { get; }
        public ICommand SelectFolderCommand { get; }
        public ICommand SetFileTypeCommand { get; }
        public ICommand ResetValuesCommand { get; }
        public ICommand CheckUpdatesCommand { get; }
        public ICommand ExitCommand { get; }

        // Costruttore
        public MainViewModel(AppSettings settings)
        {
            _settings = settings;
            _fileService = new FileService(_settings);
            _updateService = new UpdateService(_settings);
            _settingsService = new SettingsService();

            // Inizializza le collezioni
            Files = new ObservableCollection<string>();

            // Imposta i valori iniziali dalle impostazioni
            _useDefaultExtensions = _settings.UseDefaultExtensions;
            SetIpSegmentsFromAddress(_settings.PrinterIpAddress);

            // Inizializza i comandi
            RefreshCommand = new RelayCommand(RefreshFiles);
            PrintCommand = new RelayCommand(PrintSelectedFile);
            ViewPrintCommand = new RelayCommand(ViewAndPrintFile);
            OpenWebPageCommand = new RelayCommand(OpenWebPage);
            ManagePrintersCommand = new RelayCommand(OpenPrinterManagement);
            SelectFolderCommand = new RelayCommand(SelectFolder);
            SetFileTypeCommand = new RelayCommand(SetFileType);
            ResetValuesCommand = new RelayCommand(ResetValues);
            CheckUpdatesCommand = new RelayCommand(CheckForUpdates);
            ExitCommand = new RelayCommand(() => Application.Current.Shutdown());

            // Carica subito i file
            RefreshFiles();
        }

        // Implementazione dei metodi per i comandi
        public void RefreshFiles()
        {
            Files.Clear();
            var files = _fileService.GetFiles();
            foreach (var file in files)
            {
                Files.Add(file);
            }

            StatusText = $"Caricati {files.Count} file";
            OnPropertyChanged(nameof(CurrentDirectory));
        }

        private async void PrintSelectedFile()
        {
            if (string.IsNullOrEmpty(SelectedFile))
            {
                MessageBox.Show("Seleziona un file da stampare", "Nessun file selezionato",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StatusText = "Stampa in corso...";
            bool success = await _fileService.PrintFile(SelectedFile);

            if (success)
            {
                StatusText = $"File {SelectedFile} stampato con successo";

                // Se abbiamo eliminato un file SPL, aggiorniamo la lista
                if (Path.GetExtension(SelectedFile).Equals(".spl", StringComparison.OrdinalIgnoreCase))
                {
                    RefreshFiles();
                }
            }
            else
            {
                StatusText = "Errore durante la stampa";
                MessageBox.Show("Si è verificato un errore durante la stampa", "Errore",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ViewAndPrintFile()
        {
            if (string.IsNullOrEmpty(SelectedFile))
            {
                MessageBox.Show("Seleziona un file da visualizzare", "Nessun file selezionato",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string filePath = Path.Combine(_settings.Directory, SelectedFile);
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Il file selezionato non esiste", "File non trovato",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Leggi il contenuto del file
            string content = File.ReadAllText(filePath);

            // Crea una finestra di dialogo per visualizzare e modificare il contenuto
            var dialog = new Window
            {
                Title = $"Modifica: {SelectedFile}",
                Width = 700,
                Height = 500,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var grid = new Grid();
            dialog.Content = grid;

            // Definisci le righe per editor e pulsanti
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Crea l'editor di testo
            var textEditor = new TextBox
            {
                Text = content,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(10)
            };
            Grid.SetRow(textEditor, 0);
            grid.Children.Add(textEditor);

            // Crea il pannello per i pulsanti
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10, 0, 10, 10)
            };
            Grid.SetRow(buttonPanel, 1);
            grid.Children.Add(buttonPanel);

            // Pulsante Stampa
            var printButton = new Button
            {
                Content = "Stampa",
                Padding = new Thickness(15, 5, 15, 5),
                Margin = new Thickness(0, 0, 10, 0)
            };
            printButton.Click += async (s, e) =>
            {
                // Stampa il contenuto modificato
                bool success = await _fileService.PrintContent(textEditor.Text);
                if (success)
                {
                    StatusText = "Contenuto stampato con successo";
                    dialog.Close();
                }
                else
                {
                    StatusText = "Errore durante la stampa";
                    MessageBox.Show("Si è verificato un errore durante la stampa", "Errore",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            buttonPanel.Children.Add(printButton);

            // Pulsante Annulla
            var cancelButton = new Button
            {
                Content = "Annulla",
                Padding = new Thickness(15, 5, 15, 5)
            };
            cancelButton.Click += (s, e) => dialog.Close();
            buttonPanel.Children.Add(cancelButton);

            // Mostra la finestra di dialogo
            dialog.ShowDialog();
        }

        private void OpenWebPage()
        {
            string ipAddress = _settings.PrinterIpAddress;

            if (string.IsNullOrEmpty(ipAddress) || ipAddress == "0.0.0.0")
            {
                MessageBox.Show("Inserisci un indirizzo IP valido", "IP non valido",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Avvia il browser predefinito con l'URL della stampante
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"http://{ipAddress}",
                    UseShellExecute = true
                });

                StatusText = $"Apertura pagina web: http://{ipAddress}";
            }
            catch (Exception ex)
            {
                StatusText = "Errore durante l'apertura della pagina web";
                MessageBox.Show($"Errore: {ex.Message}", "Errore",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenPrinterManagement()
        {
            // Crea un'istanza del servizio stampanti
            var printerService = new PrinterService();

            // Definisci l'azione da eseguire quando viene selezionata una stampante
            Action<string> onPrinterSelected = (ipAddress) =>
            {
                // Imposta l'IP della stampante selezionata
                _settings.PrinterIpAddress = ipAddress;

                // Aggiorna i campi IP
                SetIpSegmentsFromAddress(ipAddress);

                // Salva le impostazioni
                SaveSettings();

                StatusText = $"Stampante IP {ipAddress} selezionata";
            };

            // Crea un'istanza del ViewModel per la gestione stampanti
            var viewModel = new PrinterManagementViewModel(printerService, onPrinterSelected);

            // Crea e mostra la finestra di gestione stampanti
            var window = new PrinterManagementWindow(viewModel);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        // Metodo SelectFolder senza dipendenze esterne
        private void SelectFolder()
        {
            try
            {
                // Utilizziamo lo standard WPF OpenFileDialog come selettore cartelle
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Seleziona una cartella",
                    CheckFileExists = false,
                    CheckPathExists = true,
                    FileName = "Seleziona questa cartella",
                    ValidateNames = false
                };

                // Imposta la directory iniziale se disponibile
                if (!string.IsNullOrEmpty(_settings.Directory) && Directory.Exists(_settings.Directory))
                {
                    dialog.InitialDirectory = _settings.Directory;
                }

                // Mostra il dialogo e processa il risultato
                if (dialog.ShowDialog() == true)
                {
                    // Ottieni il percorso della directory (rimuovi il nome del file fittizio)
                    string selectedPath = System.IO.Path.GetDirectoryName(dialog.FileName);

                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        _settings.Directory = selectedPath;
                        SaveSettings();
                        RefreshFiles();
                        OnPropertyChanged(nameof(CurrentDirectory));
                        StatusText = $"Cartella selezionata: {selectedPath}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante la selezione della cartella: {ex.Message}",
                               "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetFileType()
        {
            // Crea una finestra di dialogo personalizzata per impostare il tipo di file
            var dialog = new Window
            {
                Title = "Imposta tipo file",
                Width = 350,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize
            };

            var grid = new Grid
            {
                Margin = new Thickness(10)
            };
            dialog.Content = grid;

            // Definisci le righe per l'input e i pulsanti
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Etichetta
            var label = new TextBlock
            {
                Text = "Inserisci il nuovo tipo di file (incluso il punto, ad esempio .spl):",
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(label, 0);
            grid.Children.Add(label);

            // Campo di input
            var textBox = new TextBox
            {
                Text = _settings.FileType,
                Margin = new Thickness(0, 0, 0, 20)
            };
            Grid.SetRow(textBox, 1);
            grid.Children.Add(textBox);

            // Pannello pulsanti
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetRow(buttonPanel, 2);
            grid.Children.Add(buttonPanel);

            // Pulsante OK
            var okButton = new Button
            {
                Content = "OK",
                Padding = new Thickness(15, 5, 15, 5),
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };
            okButton.Click += (s, e) =>
            {
                _settings.FileType = textBox.Text;
                SaveSettings();
                RefreshFiles();
                dialog.Close();
            };
            buttonPanel.Children.Add(okButton);

            // Pulsante Annulla
            var cancelButton = new Button
            {
                Content = "Annulla",
                Padding = new Thickness(15, 5, 15, 5),
                IsCancel = true
            };
            cancelButton.Click += (s, e) => dialog.Close();
            buttonPanel.Children.Add(cancelButton);

            // Mostra la finestra di dialogo
            dialog.ShowDialog();
        }

        private void ResetValues()
        {
            if (MessageBox.Show("Sei sicuro di voler reimpostare tutti i valori?", "Conferma",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // Reimposta i valori
                _settings.PrinterIpAddress = "0.0.0.0";
                _settings.Directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                _settings.FileType = "";
                _settings.UseDefaultExtensions = false;

                // Aggiorna l'interfaccia
                SetIpSegmentsFromAddress(_settings.PrinterIpAddress);
                _useDefaultExtensions = _settings.UseDefaultExtensions;
                OnPropertyChanged(nameof(UseDefaultExtensions));
                OnPropertyChanged(nameof(CurrentDirectory));

                // Salva le impostazioni
                SaveSettings();

                // Aggiorna la lista dei file
                RefreshFiles();

                StatusText = "Valori reimpostati";
            }
        }

        private async void CheckForUpdates()
        {
            StatusText = "Controllo aggiornamenti in corso...";
            bool updateFound = await _updateService.CheckForUpdates(false);

            if (!updateFound)
            {
                StatusText = "Nessun nuovo aggiornamento disponibile";
            }
        }

        // Metodo per il controllo automatico (silenzioso) degli aggiornamenti
        public async Task CheckForUpdatesOnStartup()
        {
            await _updateService.CheckForUpdates(true);
        }

        // Metodi di utilità
        private void UpdateIpAddress()
        {
            _settings.PrinterIpAddress = $"{_ipSegment1}.{_ipSegment2}.{_ipSegment3}.{_ipSegment4}";
            SaveSettings();
        }

        private void SetIpSegmentsFromAddress(string ipAddress)
        {
            string[] segments = ipAddress.Split('.');
            if (segments.Length == 4)
            {
                _ipSegment1 = segments[0];
                _ipSegment2 = segments[1];
                _ipSegment3 = segments[2];
                _ipSegment4 = segments[3];

                OnPropertyChanged(nameof(IpSegment1));
                OnPropertyChanged(nameof(IpSegment2));
                OnPropertyChanged(nameof(IpSegment3));
                OnPropertyChanged(nameof(IpSegment4));
            }
        }

        private void SaveSettings()
        {
            if (_settingsService.SaveSettings(_settings))
            {
                StatusText = "Impostazioni salvate";
            }
            else
            {
                StatusText = "Errore durante il salvataggio delle impostazioni";
            }

            // Aggiorna le proprietà correlate alle impostazioni
            OnPropertyChanged(nameof(CurrentDirectory));
            OnPropertyChanged(nameof(VersionInfo));
        }

        // Metodo pubblico per permettere il caricamento delle impostazioni dall'esterno
        public bool LoadSettings()
        {
            bool result = _settingsService.LoadSettings(_settings);

            if (result)
            {
                // Aggiorna l'interfaccia utente con i valori caricati
                _useDefaultExtensions = _settings.UseDefaultExtensions;
                OnPropertyChanged(nameof(UseDefaultExtensions));

                SetIpSegmentsFromAddress(_settings.PrinterIpAddress);

                // Aggiorna le altre proprietà
                OnPropertyChanged(nameof(CurrentDirectory));
                OnPropertyChanged(nameof(VersionInfo));

                // Carica i file in base alle nuove impostazioni
                RefreshFiles();
            }

            return result;
        }

        // Implementazione INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}