// ViewModels/PrinterManagementViewModel.cs
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using XPRINT.Models;
using XPRINT.Services;

namespace XPRINT.ViewModels
{
    public class PrinterManagementViewModel : INotifyPropertyChanged
    {
        private readonly PrinterService _printerService;
        private readonly Action<string> _onPrinterSelected;

        // Proprietà per i campi di input
        private string _model;
        public string Model
        {
            get => _model;
            set
            {
                _model = value;
                OnPropertyChanged();
            }
        }

        private string _ipAddress;
        public string IpAddress
        {
            get => _ipAddress;
            set
            {
                _ipAddress = value;
                OnPropertyChanged();
            }
        }

        private string _customer;
        public string Customer
        {
            get => _customer;
            set
            {
                _customer = value;
                OnPropertyChanged();
            }
        }

        // Collezione di stampanti
        private ObservableCollection<Printer> _printers;
        public ObservableCollection<Printer> Printers
        {
            get => _printers;
            set
            {
                _printers = value;
                OnPropertyChanged();
            }
        }

        // Stampante selezionata
        private Printer _selectedPrinter;
        public Printer SelectedPrinter
        {
            get => _selectedPrinter;
            set
            {
                _selectedPrinter = value;
                OnPropertyChanged();
            }
        }

        // Comandi
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand UseSelectedPrinterCommand { get; }

        // Costruttore
        public PrinterManagementViewModel(PrinterService printerService, Action<string> onPrinterSelected)
        {
            _printerService = printerService;
            _onPrinterSelected = onPrinterSelected;

            // Inizializza la collezione di stampanti
            Printers = new ObservableCollection<Printer>();

            // Inizializza i campi di input
            Model = "";
            IpAddress = "";
            Customer = "";

            // Imposta i comandi
            AddCommand = new RelayCommand(AddPrinter, CanAddPrinter);
            DeleteCommand = new RelayCommand(DeletePrinter, CanExecuteSelectedPrinterCommand);
            UpdateCommand = new RelayCommand(UpdatePrinter, CanExecuteSelectedPrinterCommand);
            UseSelectedPrinterCommand = new RelayCommand(UseSelectedPrinter, CanExecuteSelectedPrinterCommand);

            // Carica le stampanti
            LoadPrinters();
        }

        // Carica tutte le stampanti
        private void LoadPrinters()
        {
            Printers.Clear();
            var printers = _printerService.GetAllPrinters();
            foreach (var printer in printers)
            {
                Printers.Add(printer);
            }
        }

        // Aggiunge una nuova stampante
        private void AddPrinter()
        {
            var newPrinter = new Printer(Model, IpAddress, Customer);

            if (_printerService.AddPrinter(newPrinter))
            {
                // Aggiunta riuscita
                Printers.Add(newPrinter);
                ClearInputFields();
            }
            else
            {
                // Stampante già esistente
                MessageBox.Show("Una stampante con questi dati esiste già!", "Attenzione",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Aggiorna una stampante esistente
        private void UpdatePrinter()
        {
            var oldUniqueId = SelectedPrinter.UniqueId;
            var updatedPrinter = new Printer(Model, IpAddress, Customer);

            // Chiedi conferma all'utente
            var result = MessageBox.Show(
                "Modificare i dati e premere 'Aggiungi' per salvare.\nEliminare la vecchia stampante?",
                "Modifica", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Aggiorna i campi di input con i dati della stampante selezionata
                Model = SelectedPrinter.Model;
                IpAddress = SelectedPrinter.IpAddress;
                Customer = SelectedPrinter.Customer;

                // Elimina la vecchia stampante
                _printerService.DeletePrinter(oldUniqueId);

                // Aggiorna la collezione
                LoadPrinters();
            }
        }

        // Elimina una stampante
        private void DeletePrinter()
        {
            if (_printerService.DeletePrinter(SelectedPrinter.UniqueId))
            {
                // Rimuovi dalla collezione
                Printers.Remove(SelectedPrinter);
                SelectedPrinter = null;

                MessageBox.Show("Stampante rimossa con successo.", "Successo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Impossibile eliminare la stampante.", "Errore",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Usa la stampante selezionata
        private void UseSelectedPrinter()
        {
            if (SelectedPrinter != null)
            {
                _onPrinterSelected(SelectedPrinter.IpAddress);

                // Chiudi la finestra
                var window = Application.Current.Windows[1];
                window.Close();
            }
        }

        // Pulisce i campi di input
        private void ClearInputFields()
        {
            Model = "";
            IpAddress = "";
            Customer = "";
        }

        // Controlla se è possibile aggiungere una stampante
        private bool CanAddPrinter()
        {
            return !string.IsNullOrEmpty(Model) && !string.IsNullOrEmpty(IpAddress);
        }

        // Controlla se è selezionata una stampante
        private bool CanExecuteSelectedPrinterCommand()
        {
            return SelectedPrinter != null;
        }

        // Implementazione di INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}