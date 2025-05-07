#Region
#AutoIt3Wrapper_Icon=..\Downloads\Logo-Xholding_X.ico
#AutoIt3Wrapper_Outfile=XPRINT.Exe
#EndRegion
#Region
#pragma compile(FileDescription, "XPRINT")
#pragma compile(FileVersion, "1.15")
#pragma compile(InternalName, "XPRINT")
#pragma compile(OriginalFilename, "XPRINT.exe")
#pragma compile(ProductName, "XPRINT")
#pragma compile(ProductVersion, "1.15")
#EndRegion

#include <GUIConstantsEx.au3>
#include <WindowsConstants.au3>
#include <FileConstants.au3>
#include <Array.au3>
#include <File.au3>
#include <FileSend.au3>
#include <GuiIPAddress.au3>
#include <EditConstants.au3>
#include <GuiListView.au3>
#include <MsgBoxConstants.au3>
#include <GuiHeader.au3> ; Aggiunto per gestire le intestazioni
#include <Constants.au3>
#include <WindowsConstants.au3>
#include <HeaderConstants.au3>
#include <InetConstants.au3>
#include <WinAPIFiles.au3>
#include <WinHTTP.au3>



#cs ----------------------------------------------------------------------------

 AutoIt Version: 3.3.16.1
 Author:         Riccardo Gabetti
 Company: 		 Pernix
 Version:		 1.15.0
 Date: 07/05/2025

#ce ----------------------------------------------------------------------------

#Region
Global $hGUI
Global $List_1, $Button_1, $Button_2, $Button_3, $Button_4, $Button_5, $IP_1
Global $miSelectFolder, $miSetFileType, $miResetValues, $miExit, $miCheckUpdates, $miUseDefaultExtensions
Global $sPrinterIPAddress = ""
Global $sDirectory = @ScriptDir
Global $FileType = ""
Global $hGUI_2, $SBS_HORZ, $SBS_VERT, $Edit_1, $SB_LINEUP, $SB_LINEDOWN
Global $bUseDefaultExtensions = ""
Global $aDefaultExtensions[5] = ["prn", "jca", "txt", "zpl", "pgl"]
Global $iPort = "9100"
Global $btnGestioneStampanti, $iniFile
Global $txtModello, $txtIP, $txtCliente, $lst, $HDN_ITEMCLICKA
Global $GitHubVar = "1337Intersect/XPRINT"
; URL del file di definizione della versione su GitHub
Global $sVersionFileURL = "https://raw.githubusercontent.com/" & $GitHubVar &"/refs/heads/main/version.txt"
; URL base per il download dell'eseguibile
Global $sDownloadBaseURL = "https://github.com/" & $GitHubVar &"/releases/download/"

; La versione attuale del programma
Global $sVersioneAttuale = "1.15.0"

; Inizializza il file INI delle stampanti
$iniFile = @AppDataDir & "\stampanti.ini"

;------------------------------------------------------
; Title...........:	_guiCreate
; Description.....:	Create the main GUI
;------------------------------------------------------
Func _guiCreate()
    $hGUI = GUICreate("XPRINT 1.15.0", 498, 285, Default, Default)

    $List_1 = GUICtrlCreateList("", 10, 10, 331, 226)
    $Button_1 = GUICtrlCreateButton("Aggiorna", 345, 55, 141, 31)
    $Button_2 = GUICtrlCreateButton("Stampa", 345, 90, 141, 31)
	$Button_3 = GUICtrlCreateButton("Visualizza/Stampa", 345, 125, 141, 31)
	$Button_4 = GUICtrlCreateButton("Apri pagina web", 345, 160, 141, 31)
	$Button_5 = GUICtrlCreateButton("Gestione Stampanti", 345, 195, 141, 31)
	$miUseDefaultExtensions = GUICtrlCreateCheckbox("Usa estensioni predefinite", 10, 233, 141, 20)
	GUICtrlSetState($miUseDefaultExtensions, $bUseDefaultExtensions ? $GUI_CHECKED : $GUI_UNCHECKED)

    Local $Label_1 = GUICtrlCreateLabel("IP Stampante :", 345, 10, 106, 11)
    $IP_1 = _GUICtrlIpAddress_Create($hGUI, 346, 30, 136, 21)
    _GUICtrlIpAddress_Set($IP_1, $sPrinterIPAddress)

    ; Crea il menu
    Local $hMenu = GUICtrlCreateMenu("Impostazioni")
	$miSelectFolder = GUICtrlCreateMenuItem("Seleziona Cartella", $hMenu)
	$miSetFileType = GUICtrlCreateMenuItem("Imposta tipo file", $hMenu)
    GUICtrlCreateMenuItem("", $hMenu, 0) ; Separatore
	$miExit = GUICtrlCreateMenuItem("Esci", $hMenu)

	; Creare il menu "Strumenti"
	Local $hMenuStrumenti = GUICtrlCreateMenu("Strumenti")
	$miResetValues = GUICtrlCreateMenuItem("Reimposta Valori", $hMenuStrumenti)
	$miCheckUpdates = GUICtrlCreateMenuItem("Controlla Aggiornamenti", $hMenuStrumenti) ; Aggiungi questa riga

    ; Aggiungi un input per il tipo di file (nascosto inizialmente)
    Global $Input_FileType = GUICtrlCreateInput($FileType, -1, -1, 1, 1, $GUI_HIDE)

	GUISetState(@SW_SHOW, $hGUI)
    WinMove($hGUI, "", (@DesktopWidth - 498) / 2, (@DesktopHeight - 251) / 2)

EndFunc
#EndRegion

_main()

Func _main()

	; Carica i valori dal file di configurazione
	LoadConfig()
	ControllaAggiornamenti(True) ; True = modalità silenziosa, controlla ma avvisa solo se c'è un aggiornamento
	_guiCreate()
    GUISetState(@SW_SHOWNORMAL)

	; Inizializza il file di configurazione
    Local $sConfigFile = @AppDataDir & "\config.ini"
    If Not FileExists($sConfigFile) Then
        IniWrite($sConfigFile, "Config", "PrinterIPAddress", "")
        IniWrite($sConfigFile, "Config", "Directory", "")
        IniWrite($sConfigFile, "Config", "FileType", "")
		IniWrite($sConfigFile, "Config", "UseDefaultExtensions", "")
    EndIf

LoadFiles()

	; Inizializza $sPrinterIPAddress se non è stato impostato
    If $sPrinterIPAddress = "" Then
        $sPrinterIPAddress = _GUICtrlIpAddress_Get($IP_1)
    EndIf

    While 1
        Switch GUIGetMsg()
			Case $GUI_EVENT_CLOSE
				SaveConfig()
                ExitLoop
			Case $Button_1
                LoadFiles()
            Case $Button_2
                PrintSelectedFile()
			Case $miSelectFolder
				SelectDestinationFolder()
			Case $miSetFileType
				SetFileType()
			Case $miResetValues
                ResetValues()
			Case $miExit
				SaveConfig()
				Exit
			Case $Button_3
				ReadSelectedFile()
			Case $Button_4
				OpenWebPage()
			Case $miUseDefaultExtensions
				ToggleDefaultExtensions()
			Case $Button_5
				GestioneStampanti()
			Case $miCheckUpdates
				ControllaAggiornamenti()
        EndSwitch
    WEnd
EndFunc   ;==>_main

; Funzione per caricare i file nella List_1
Func LoadFiles()
    GUICtrlSetData($List_1, "")

    ; Inizializza l'array dei file in base alle condizioni della checkbox
    Local $aFileList
    If $bUseDefaultExtensions Then
        $aFileList = _FileListToArray($sDirectory, "*", $FLTA_FILES)
    Else
        $aFileList = _FileListToArray($sDirectory, "*" & $FileType, $FLTA_FILES)
    EndIf

    ; Verifica se sono stati trovati file
    If Not @error Then
        ; Pulisci la List_1
        GUICtrlSetData($List_1, "")

        ; Aggiungi i file validi all'elenco
        If $bUseDefaultExtensions Then
            ; Loop per gestire le estensioni predefinite quando la checkbox è attiva
            For $i = 1 To $aFileList[0]
                Local $sFilePath = $sDirectory & "\" & $aFileList[$i]
                If FileExists($sFilePath) Then
                    ; Ottieni l'estensione del file
                    Local $sFileExtension = StringLower(StringRegExpReplace($aFileList[$i], ".*\.(.+)", "$1"))

                    ; Se la checkbox è attiva, aggiungi solo i file con estensioni predefinite
                    If _ArraySearch($aDefaultExtensions, $sFileExtension) <> -1 Then
                        GUICtrlSetData($List_1, $aFileList[$i])
                    EndIf
                EndIf
            Next
        Else
            ; Loop per gestire l'estensione specificata quando la checkbox è disattivata
            For $i = 1 To $aFileList[0]
                Local $sFilePath = $sDirectory & "\" & $aFileList[$i]
                If FileExists($sFilePath) Then
                    GUICtrlSetData($List_1, $aFileList[$i])
                EndIf
            Next
        EndIf
    Else
        MsgBox($MB_ICONERROR, "Errore", "Nessun file trovato nella directory specificata.")
    EndIf
EndFunc

Func PrintSelectedFile()
    ; Ottieni il nome del file selezionato dalla lista
    Local $sSelectedFileName = GUICtrlRead($List_1)

    ; Verifica se un file è stato selezionato
    If $sSelectedFileName <> "" Then
        ; Costruisci il percorso completo del file selezionato
        Local $sSelectedFilePath = $sDirectory & "\" & $sSelectedFileName

        ; Verifica se il file selezionato esiste
        If FileExists($sSelectedFilePath) Then
            ; Esegui il ping all'indirizzo IP della stampante prima di procedere
            Local $sIPAddress = _GUICtrlIpAddress_Get($IP_1)
            If Ping($sIPAddress, 1000) Then
                ; Invia il file alla stampante utilizzando _FileSend
                If _FileSend($sSelectedFilePath, $sIPAddress, $iPort) = 1 Then
                    ; Verifica se il FileType è ".spl" prima di eliminare il file
                    If $FileType = ".spl" Then
                        ; Elimina il file SPL dopo una stampa riuscita
                        If FileDelete($sSelectedFilePath) Then
                            ; Aggiorna la lista dei file dopo aver eliminato il file
							LoadFiles()
                        Else
                            MsgBox($MB_ICONERROR, "Errore", "Impossibile eliminare il file.")
                        EndIf
                    EndIf
                Else
                    MsgBox($MB_ICONERROR, "Errore", "Errore durante l'invio del file alla stampante.")
                EndIf
            Else
                MsgBox($MB_ICONERROR, "Errore", "L'indirizzo IP della stampante non risponde.")
            EndIf
        Else
            MsgBox($MB_ICONERROR, "Errore", "Il file selezionato non esiste nel percorso specificato.")
        EndIf
    Else
        MsgBox($MB_ICONERROR, "Errore", "Nessun file selezionato. Seleziona un file dalla lista prima di stampare.")
    EndIf
EndFunc

; Funzione per gestire il menu "Seleziona Cartella"
Func SelectDestinationFolder()
    Local $sNewFolder = FileSelectFolder("Seleziona una cartella", $sDirectory, 3)
    If @error Then
        ; L'utente ha annullato la selezione
        Return
    EndIf
    $sDirectory = $sNewFolder
	LoadFiles()
	SaveConfig() ; Salva i valori nel file di configurazione
EndFunc

; Funzione per gestire il menu "Imposta tipo file"
Func SetFileType()
    Local $sNewFileType = InputBox("Imposta tipo di file", "Inserisci il nuovo tipo di file (incluso il punto, ad esempio .spl):", $FileType)
    If @error Then
        ; L'utente ha annullato l'input
        Return
    EndIf
    $FileType = $sNewFileType
	LoadFiles()
	SaveConfig() ; Salva i valori nel file di configurazione
EndFunc

Func SaveConfig()
    Local $sConfigFile = @AppDataDir & "\config.ini"
    IniWrite($sConfigFile, "Config", "PrinterIPAddress", _GUICtrlIpAddress_Get($IP_1))
    IniWrite($sConfigFile, "Config", "Directory", $sDirectory)
    IniWrite($sConfigFile, "Config", "FileType", $FileType)
    IniWrite($sConfigFile, "Config", "UseDefaultExtensions", $bUseDefaultExtensions)
EndFunc

Func LoadConfig()
    Local $sConfigFile = @AppDataDir & "\config.ini"
    If FileExists($sConfigFile) Then
        Local $sPrinterIPAddressTemp = IniRead($sConfigFile, "Config", "PrinterIPAddress", "")
        If $sPrinterIPAddressTemp <> "" Then
            $sPrinterIPAddress = $sPrinterIPAddressTemp
        EndIf
        Local $sDirectoryTemp = IniRead($sConfigFile, "Config", "Directory", "")
        If $sDirectoryTemp <> "" Then
            $sDirectory = $sDirectoryTemp
        EndIf
        Local $sFileTypeTemp = IniRead($sConfigFile, "Config", "FileType", "")
        If $sFileTypeTemp <> "" Then
            $FileType = $sFileTypeTemp
        EndIf
        ; Leggi il valore di "UseDefaultExtensions" e convertilo da stringa a booleano
        Local $sUseDefaultExtensionsTemp = IniRead($sConfigFile, "Config", "UseDefaultExtensions", "")
        If $sUseDefaultExtensionsTemp <> "" Then
            $bUseDefaultExtensions = StringToBool($sUseDefaultExtensionsTemp)
        Else
            ; Imposta un valore predefinito se non è presente nel file di configurazione
            $bUseDefaultExtensions = False
        EndIf
    EndIf
EndFunc

Func StringToBool($sString)
    Return StringUpper($sString) = "TRUE"
EndFunc

Func ResetValues()
    $sPrinterIPAddress = "" ; Reimposta il valore a vuoto o al valore predefinito
    _GUICtrlIpAddress_Set($IP_1, "0.0.0.0") ; Imposta temporaneamente a un valore vuoto
    _GUICtrlIpAddress_Set($IP_1, $sPrinterIPAddress) ; Reimposta il campo IP al valore reimpostato
    $sDirectory = "" ; Reimposta il valore a vuoto o al valore predefinito
    $FileType = "" ; Reimposta il valore a vuoto o al valore predefinito
    SaveConfig() ; Salva i valori reimpostati nel file di configurazione
    ; Aggiungi eventuali altre reimpostazioni dei valori necessari
	LoadFiles()
EndFunc

Func ReadSelectedFile()
    ; Ottieni il nome del file selezionato dalla lista
    Local $sSelectedFileName = GUICtrlRead($List_1)

    ; Verifica se un file è stato selezionato
    If $sSelectedFileName <> "" Then
        ; Costruisci il percorso completo del file selezionato
        Local $sSelectedFilePath = $sDirectory & "\" & $sSelectedFileName

        ; Verifica se il file selezionato esiste
        If FileExists($sSelectedFilePath) Then
			; Leggi il contenuto completo del file
			Local $sFileContent = FileRead($sSelectedFilePath, 60000)

			; Sostituisci tutti i Line Feed con Carriage Return + Line Feed
			$sFileContent = StringReplace($sFileContent, @LF, @CRLF)

			; Crea una nuova finestra per modificare il contenuto del file
			Local $hEditPopup = GUICreate("Modifica del file", 600, 480)
			Local $Edit_1 = GUICtrlCreateEdit($sFileContent, 10, 10, 580, 400, BitOR($WS_VSCROLL, $ES_MULTILINE))
            Local $Button_Print = GUICtrlCreateButton("Stampa", 10, 420, 80, 30)

            GUISetState(@SW_SHOW, $hEditPopup)

            ; Loop per la finestra del popup
            While 1
                Switch GUIGetMsg()
                    Case $GUI_EVENT_CLOSE
                        GUIDelete($hEditPopup) ; Chiudi la finestra del popup
                        ExitLoop
                    Case $Button_Print
                        ; Ottieni il nuovo contenuto dall'editor
                        Local $sNewContent = GUICtrlRead($Edit_1)

                        ; Stampa il contenuto senza salvare il file
                        _PrintContent($sNewContent)

                        ; Chiudi la finestra del popup
                        GUIDelete($hEditPopup)
                        ExitLoop
                EndSwitch
            WEnd
        Else
            MsgBox($MB_ICONERROR, "Errore", "Il file selezionato non esiste nel percorso specificato.")
        EndIf
    Else
        MsgBox($MB_ICONERROR, "Errore", "Nessun file selezionato. Seleziona un file dalla lista prima di visualizzare.")
    EndIf
EndFunc

; Funzione per la stampa del contenuto
Func _PrintContent($sContent)
    ; Crea un file temporaneo
    Local $sTempFile = @TempDir & "\PrintTemp.txt"
    FileWrite($sTempFile, $sContent)

    ; Verifica se il file temporaneo esiste
    If FileExists($sTempFile) Then
        ; Esegui il ping all'indirizzo IP della stampante prima di procedere
        Local $sIPAddress = _GUICtrlIpAddress_Get($IP_1)
        If Ping($sIPAddress, 1000) Then
            ; Invia il file alla stampante utilizzando _FileSend
            If _FileSend($sTempFile, $sIPAddress, $iPort) = 1 Then
                ; Operazione di stampa riuscita
            Else
                MsgBox($MB_ICONERROR, "Errore", "Errore durante l'invio del file alla stampante.")
            EndIf
        Else
            MsgBox($MB_ICONERROR, "Errore", "L'indirizzo IP della stampante non risponde.")
        EndIf

        ; Elimina il file temporaneo
        FileDelete($sTempFile)
    Else
        MsgBox($MB_ICONERROR, "Errore", "Il file temporaneo non esiste nel percorso specificato.")
    EndIf
EndFunc

Func OpenWebPage()
    ; Ottieni l'indirizzo IP della stampante
    Local $sIPAddress = _GUICtrlIpAddress_Get($IP_1)
	If Ping($sIPAddress, 1000) Then
		; Apri il browser web con l'indirizzo della stampante
		If $sIPAddress <> "" Then
			ShellExecute("http://" & $sIPAddress)
		Else
			MsgBox($MB_ICONERROR, "Errore", "Indirizzo IP della stampante non specificato.")
		EndIf
	Else
		MsgBox($MB_ICONERROR, "Errore", "L'indirizzo IP della stampante non risponde.")
	EndIf
EndFunc

Func ToggleDefaultExtensions()
    $bUseDefaultExtensions = Not $bUseDefaultExtensions
    GUICtrlSetState($miUseDefaultExtensions, $bUseDefaultExtensions ? $GUI_CHECKED : $GUI_UNCHECKED)
    LoadFiles()
    SaveConfig()
EndFunc

; ================ FUNZIONI GESTIONE STAMPANTI ================

Func GestioneStampanti()
    Local $gui = GUICreate("Gestione Stampanti", 550, 430)

    GUICtrlCreateLabel("Modello:", 10, 10)
    $txtModello = GUICtrlCreateInput("", 80, 10, 150, 20)
    GUICtrlCreateLabel("IP:", 10, 40)
    $txtIP = GUICtrlCreateInput("", 80, 40, 150, 20)
    GUICtrlCreateLabel("Cliente:", 10, 70)
    $txtCliente = GUICtrlCreateInput("", 80, 70, 150, 20)

    Local $btnAggiungi = GUICtrlCreateButton("Aggiungi", 250, 5, 80, 25)
    Local $btnElimina = GUICtrlCreateButton("Elimina selezionata", 340, 5, 130, 25)
	Local $btnModifica = GUICtrlCreateButton("Modifica selezionata", 340, 35, 130, 25)
    Local $btnUsa = GUICtrlCreateButton("Usa questa stampante", 10, 370, 200, 35)

    ; Creiamo la ListView con stile che supporta l'ordinamento
    $lst = GUICtrlCreateListView("Modello|IP|Cliente", 10, 110, 520, 240)
    _GUICtrlListView_SetColumnWidth($lst, 0, 150)
    _GUICtrlListView_SetColumnWidth($lst, 1, 150)
    _GUICtrlListView_SetColumnWidth($lst, 2, 150)

    ; Carica stampanti
    RicaricaListaStampanti()

    GUISetState(@SW_SHOW, $gui)

    While 1
        Local $msg = GUIGetMsg()
        Switch $msg
            Case $GUI_EVENT_CLOSE
                GUIDelete($gui)
                ExitLoop
            Case $btnAggiungi
                AggiungiStampante()
            Case $btnElimina
                EliminaSelezionata()
            Case $btnModifica
                ModificaSelezionata()
            Case $btnUsa
                Local $hListView = GUICtrlGetHandle($lst)
                Local $selIndex = _GUICtrlListView_GetSelectionMark($hListView)

                If $selIndex >= 0 Then
                    ; Ottieni l'IP della stampante selezionata
                    Local $selectedIP = _GUICtrlListView_GetItemText($hListView, $selIndex, 1)

                    ; Imposta l'IP nella finestra principale
                    _GUICtrlIpAddress_Set($IP_1, $selectedIP)
                    $sPrinterIPAddress = $selectedIP
                    SaveConfig()

                    GUIDelete($gui)
                    ExitLoop
                Else
                    MsgBox(48, "Errore", "Nessuna stampante selezionata.")
                EndIf
        EndSwitch
    WEnd
EndFunc

Func AggiungiStampante()
    Local $modello = GUICtrlRead($txtModello)
    Local $ip = GUICtrlRead($txtIP)
    Local $cliente = GUICtrlRead($txtCliente)

    If $modello <> "" And $ip <> "" Then
        ; Generiamo un identificativo unico usando modello e IP
        Local $uniqueID = $modello & "_" & $ip

        ; Verifica se esiste già una combinazione modello+IP
        If IniRead($iniFile, $uniqueID, "Modello", "NonEsiste") <> "NonEsiste" Then
            ; Esiste già una stampante con questa combinazione
            MsgBox($MB_ICONWARNING, "Attenzione", "Una stampante con questi dati esiste già!")
            Return
        EndIf

        ; Aggiunge alla ListView
        GUICtrlCreateListViewItem($modello & "|" & $ip & "|" & $cliente, $lst)

        ; Salva i dati nel file INI usando modello_IP come nome della sezione
        IniWrite($iniFile, $uniqueID, "Modello", $modello)
        IniWrite($iniFile, $uniqueID, "Cliente", $cliente)
        IniWrite($iniFile, $uniqueID, "IP", $ip)

        ; Pulisci i campi di input
        GUICtrlSetData($txtModello, "")
        GUICtrlSetData($txtIP, "")
        GUICtrlSetData($txtCliente, "")
    Else
        MsgBox($MB_ICONWARNING, "Errore", "Inserisci almeno Modello e IP!")
    EndIf
EndFunc

; Funzione per modificare la stampante selezionata
Func ModificaSelezionata()
    Local $hListView = GUICtrlGetHandle($lst)
    Local $selIndex = _GUICtrlListView_GetSelectionMark($hListView)

    If $selIndex >= 0 Then
        ; Ottieni tutti i dati della riga selezionata
        Local $modello = _GUICtrlListView_GetItemText($hListView, $selIndex, 0)
        Local $ip = _GUICtrlListView_GetItemText($hListView, $selIndex, 1)
        Local $cliente = _GUICtrlListView_GetItemText($hListView, $selIndex, 2)

        ; Genera l'ID univoco per la sezione
        Local $uniqueID = $modello & "_" & $ip

        ; Popola i campi di input con i dati della stampante selezionata
        GUICtrlSetData($txtModello, $modello)
        GUICtrlSetData($txtIP, $ip)
        GUICtrlSetData($txtCliente, $cliente)

        ; Chiedi conferma per la modifica
        If MsgBox($MB_YESNO + $MB_ICONQUESTION, "Modifica", "Modificare i dati e premere 'Aggiungi' per salvare." & @CRLF & "Eliminare la vecchia stampante?") = $IDYES Then
            ; Elimina la vecchia stampante
            IniDelete($iniFile, $uniqueID)
            _GUICtrlListView_DeleteItem($hListView, $selIndex)
        EndIf
    Else
        MsgBox($MB_ICONWARNING, "Errore", "Nessuna stampante selezionata.")
    EndIf
EndFunc

; Funzione per caricare gli elementi
Func RicaricaListaStampanti()
    ; Pulisce la lista
    If IsDllStruct($lst) Or IsHWnd($lst) Then
        _GUICtrlListView_DeleteAllItems(GUICtrlGetHandle($lst))
    EndIf

    ; Legge tutte le sezioni del file ini
    Local $sezioni = IniReadSectionNames($iniFile)
    If Not @error Then
        For $i = 1 To $sezioni[0]
            Local $sectionName = $sezioni[$i]
            Local $modello = IniRead($iniFile, $sectionName, "Modello", "")
            Local $ip = IniRead($iniFile, $sectionName, "IP", "")
            Local $cliente = IniRead($iniFile, $sectionName, "Cliente", "")

            ; Aggiungi solo se il modello è valido
            If $modello <> "" Then
                GUICtrlCreateListViewItem($modello & "|" & $ip & "|" & $cliente, $lst)
            EndIf
        Next

        ; Aggiungi l'evento di doppio clic alla ListView
        Local $hWndListView = GUICtrlGetHandle($lst)
    EndIf
EndFunc

Func EliminaSelezionata()
    Local $hListView = GUICtrlGetHandle($lst)
    Local $selIndex = _GUICtrlListView_GetSelectionMark($hListView)

    If $selIndex >= 0 Then
        ; Ottieni tutti i dati della riga selezionata
        Local $modello = _GUICtrlListView_GetItemText($hListView, $selIndex, 0)
        Local $ip = _GUICtrlListView_GetItemText($hListView, $selIndex, 1)

        ; Genera l'ID univoco per la sezione
        Local $uniqueID = $modello & "_" & $ip

        ; Elimina la sezione dal file INI
        IniDelete($iniFile, $uniqueID)

        ; Rimuovi la riga dalla ListView
        _GUICtrlListView_DeleteItem($hListView, $selIndex)

        MsgBox($MB_ICONINFORMATION, "Successo", "Stampante rimossa con successo.")
    Else
        MsgBox($MB_ICONWARNING, "Errore", "Nessuna riga selezionata.")
    EndIf
EndFunc


; Funzione principale per il controllo degli aggiornamenti
Func ControllaAggiornamenti($bSilent = False)

    ; Percorso dove salvare l'aggiornamento
    Local $sTempPath = @TempDir & "\XPRINT_update.exe"

    ; Debug - Mostra la versione corrente
    ConsoleWrite("Versione attuale: " & $sVersioneAttuale & @CRLF)
    ; Inizializza WinHTTP
    Local $hOpen = _WinHttpOpen()
    If @error Then
        If Not $bSilent Then
            MsgBox($MB_ICONERROR, "Errore", "Impossibile inizializzare WinHTTP.")
        EndIf
        Return False
    EndIf

    ; Connessione al server
    Local $hConnect = _WinHttpConnect($hOpen, "raw.githubusercontent.com")
    If @error Then
        _WinHttpCloseHandle($hOpen)
        If Not $bSilent Then
            MsgBox($MB_ICONERROR, "Errore", "Impossibile connettersi al server GitHub.")
        EndIf
        Return False
    EndIf

    ; Percorso da GitHub
    Local $sPath = "/" & $GitHubVar & "/main/version.txt"

    ; Richiesta HTTP
    Local $hRequest = _WinHttpOpenRequest($hConnect, "GET", $sPath)
    If @error Then
        _WinHttpCloseHandle($hConnect)
        _WinHttpCloseHandle($hOpen)
        If Not $bSilent Then
            MsgBox($MB_ICONERROR, "Errore", "Impossibile creare la richiesta HTTP.")
        EndIf
        Return False
    EndIf

    ; Invia la richiesta
    _WinHttpSendRequest($hRequest)
    If @error Then
        _WinHttpCloseHandle($hRequest)
        _WinHttpCloseHandle($hConnect)
        _WinHttpCloseHandle($hOpen)
        If Not $bSilent Then
            MsgBox($MB_ICONERROR, "Errore", "Impossibile inviare la richiesta HTTP.")
        EndIf
        Return False
    EndIf

    ; Ricevi la risposta
    _WinHttpReceiveResponse($hRequest)
    If @error Then
        _WinHttpCloseHandle($hRequest)
        _WinHttpCloseHandle($hConnect)
        _WinHttpCloseHandle($hOpen)
        If Not $bSilent Then
            MsgBox($MB_ICONERROR, "Errore", "Impossibile ricevere la risposta HTTP.")
        EndIf
        Return False
    EndIf

    ; Leggi il contenuto
    Local $sVersioneDisponibile = _WinHttpReadData($hRequest)

    ; Chiudi tutte le handle
    _WinHttpCloseHandle($hRequest)
    _WinHttpCloseHandle($hConnect)
    _WinHttpCloseHandle($hOpen)

    ; Debug - Mostra il contenuto scaricato
    ConsoleWrite("Contenuto scaricato: " & $sVersioneDisponibile & @CRLF)

    ; Rimuovi eventuali spazi o caratteri non desiderati
    $sVersioneDisponibile = StringStripWS($sVersioneDisponibile, $STR_STRIPALL)

    ; Analizza il contenuto del file di versione (formato: "versione|nomefile")
    Local $aVersionInfo = StringSplit($sVersioneDisponibile, "|", $STR_NOCOUNT)

    ; Debug - Mostra il risultato dello split
    ConsoleWrite("Risultato split: " & UBound($aVersionInfo) & " elementi" & @CRLF)

    Local $sNuovaVersione = ""
    Local $sNomeFile = "XPRINT.exe"

    ; Verifica se lo split ha avuto successo
    If @error Or UBound($aVersionInfo) < 1 Then
        ConsoleWrite("Errore nello split del file di versione" & @CRLF)
        If Not $bSilent Then
            MsgBox($MB_ICONERROR, "Errore", "Formato file di versione non valido.")
        EndIf
        Return False
    EndIf

    ; Assegna i valori dalle parti separate
    $sNuovaVersione = $aVersionInfo[0]
    ConsoleWrite("Nuova versione rilevata: " & $sNuovaVersione & @CRLF)

    ; Se è specificato un nome di file nel file di versione, usalo
    If UBound($aVersionInfo) > 1 Then
        $sNomeFile = $aVersionInfo[1]
        ConsoleWrite("Nome file: " & $sNomeFile & @CRLF)
    EndIf

    ; Confronta le versioni
    If _ConfrontaVersioni($sVersioneAttuale, $sNuovaVersione) < 0 Then
        ; Una nuova versione è disponibile
        Local $sMsg = "È disponibile la versione " & $sNuovaVersione & " di XPRINT." & @CRLF & @CRLF & "Versione attuale: " & $sVersioneAttuale & @CRLF & "Vuoi aggiornare adesso?"

        If $bSilent Or MsgBox($MB_YESNO + $MB_ICONINFORMATION, "Aggiornamento Disponibile", $sMsg) = $IDYES Then
            ; Costruisci l'URL di download
            Local $sDownloadURL = $sDownloadBaseURL & "v" & $sNuovaVersione & "/" & $sNomeFile
            ConsoleWrite("URL download: " & $sDownloadURL & @CRLF)

            ; Mostra una barra di progresso per il download
            Local $hDownloadProgressGUI = GUICreate("Download Aggiornamento", 300, 100)
            Local $idProgressBar = GUICtrlCreateProgress(10, 40, 280, 20)
            Local $idStatusLabel = GUICtrlCreateLabel("Download in corso...", 10, 20, 280, 20)
            GUISetState(@SW_SHOW, $hDownloadProgressGUI)

            ; Scarica il nuovo eseguibile
            Local $hDownload = InetGet($sDownloadURL, $sTempPath, $INET_FORCERELOAD, $INET_DOWNLOADBACKGROUND)

            ; Monitora il progresso del download
            Do
                ; Aggiorna la barra di progresso
                Local $iBytes = InetGetInfo($hDownload, $INET_DOWNLOADREAD)
                Local $iTotalBytes = InetGetInfo($hDownload, $INET_DOWNLOADSIZE)

                If $iTotalBytes > 0 Then
                    GUICtrlSetData($idProgressBar, Int($iBytes * 100 / $iTotalBytes))
                    GUICtrlSetData($idStatusLabel, "Download in corso... " & _FormatBytes($iBytes) & " / " & _FormatBytes($iTotalBytes))
                EndIf

                Sleep(100)
            Until InetGetInfo($hDownload, $INET_DOWNLOADCOMPLETE)

            ; Chiudi la GUI di download
            GUIDelete($hDownloadProgressGUI)

            ; Verifica se il download è avvenuto con successo
            If InetGetInfo($hDownload, $INET_DOWNLOADSUCCESS) Then
                ; Verifica che il file scaricato esista e abbia dimensioni > 0
                If FileExists($sTempPath) And FileGetSize($sTempPath) > 0 Then
                    ; Crea un batch per l'installazione dell'aggiornamento
                    Local $sBatchFile = @TempDir & "\XPRINT_updater.bat"
                    Local $hBatchFile = FileOpen($sBatchFile, $FO_OVERWRITE)

                    FileWrite($hBatchFile, "@echo off" & @CRLF)
                    FileWrite($hBatchFile, "echo Attendere, aggiornamento di XPRINT in corso..." & @CRLF)
                    FileWrite($hBatchFile, "ping 127.0.0.1 -n 3 > nul" & @CRLF) ; Attendi un po'
                    FileWrite($hBatchFile, "if exist """ & @ScriptFullPath & """ (" & @CRLF)
                    FileWrite($hBatchFile, "    del /f /q """ & @ScriptFullPath & """" & @CRLF)
                    FileWrite($hBatchFile, ")" & @CRLF)
                    FileWrite($hBatchFile, "if exist """ & $sTempPath & """ (" & @CRLF)
                    FileWrite($hBatchFile, "    copy /y """ & $sTempPath & """ """ & @ScriptFullPath & """" & @CRLF)
                    FileWrite($hBatchFile, "    del /f /q """ & $sTempPath & """" & @CRLF)
                    FileWrite($hBatchFile, ")" & @CRLF)
                    FileWrite($hBatchFile, "start """" """ & @ScriptFullPath & """" & @CRLF)
                    FileWrite($hBatchFile, "del ""%~f0""" & @CRLF) ; Auto-elimina il batch

                    FileClose($hBatchFile)

                    ; Esegui il batch e termina questo programma
                    MsgBox($MB_ICONINFORMATION, "Aggiornamento", "XPRINT verrà chiuso e aggiornato. Al termine, verrà riavviato automaticamente.")
                    ShellExecute($sBatchFile, "", "", "", @SW_HIDE)
                    Exit
                Else
                    MsgBox($MB_ICONERROR, "Errore", "File di aggiornamento non valido o danneggiato.")
                    FileDelete($sTempPath) ; Elimina il file scaricato se non valido
                    Return False
                EndIf
            Else
                ; Errore nel download
                MsgBox($MB_ICONERROR, "Errore", "Impossibile scaricare l'aggiornamento. Riprova più tardi." & @CRLF & "URL: " & $sDownloadURL)
                Return False
            EndIf
        EndIf
    Else
        ; Nessun aggiornamento disponibile
        If Not $bSilent Then
            MsgBox($MB_ICONINFORMATION, "Informazione", "Stai già utilizzando l'ultima versione di XPRINT.")
        EndIf
    EndIf

    Return True
EndFunc

; Funzione per confrontare due stringhe di versione
Func _ConfrontaVersioni($sVersione1, $sVersione2)
    ; Debugging
    ConsoleWrite("Versione1: " & $sVersione1 & @CRLF)
    ConsoleWrite("Versione2: " & $sVersione2 & @CRLF)

    ; Verifica se le stringhe sono valide
    If $sVersione1 = "" Or $sVersione2 = "" Then
        ConsoleWrite("Errore: Le stringhe di versione sono vuote" & @CRLF)
        Return 0
    EndIf

    ; Pulisci le stringhe di versione (rimuovi eventuali spazi bianchi)
    $sVersione1 = StringStripWS($sVersione1, $STR_STRIPALL)
    $sVersione2 = StringStripWS($sVersione2, $STR_STRIPALL)

    ; Dividi le stringhe di versione nei numeri delle componenti
    Local $aV1 = StringSplit($sVersione1, ".", $STR_NOCOUNT)
    Local $aV2 = StringSplit($sVersione2, ".", $STR_NOCOUNT)

    ; Verifica se lo split ha funzionato
    If @error Or UBound($aV1) < 1 Or UBound($aV2) < 1 Then
        ConsoleWrite("Errore nello split delle versioni" & @CRLF)

        ; Confronto diretto delle stringhe come fallback
        If $sVersione1 = $sVersione2 Then Return 0
        If $sVersione1 < $sVersione2 Then Return -1
        If $sVersione1 > $sVersione2 Then Return 1
        Return 0
    EndIf

    ; Debug degli array
    ConsoleWrite("Array V1: " & _ArrayToString($aV1, ", ") & @CRLF)
    ConsoleWrite("Array V2: " & _ArrayToString($aV2, ", ") & @CRLF)

    ; Confronta le componenti una per una
    Local $iMaxComp = UBound($aV1) > UBound($aV2) ? UBound($aV1) : UBound($aV2)

    For $i = 0 To $iMaxComp - 1
        Local $iComp1 = $i < UBound($aV1) ? Int($aV1[$i]) : 0
        Local $iComp2 = $i < UBound($aV2) ? Int($aV2[$i]) : 0

        ConsoleWrite("Confronto componente " & $i & ": " & $iComp1 & " vs " & $iComp2 & @CRLF)

        If $iComp1 < $iComp2 Then
            Return -1 ; Versione1 < Versione2
        ElseIf $iComp1 > $iComp2 Then
            Return 1 ; Versione1 > Versione2
        EndIf
    Next

    Return 0 ; Le versioni sono uguali
EndFunc

; Funzione per formattare i byte in una stringa leggibile
Func _FormatBytes($iBytes)
    Local $iKB = 1024
    Local $iMB = $iKB * 1024

    If $iBytes < $iKB Then
        Return $iBytes & " B"
    ElseIf $iBytes < $iMB Then
        Return Round($iBytes / $iKB, 2) & " KB"
    Else
        Return Round($iBytes / $iMB, 2) & " MB"
    EndIf
EndFunc

; Funzione per creare un file di definizione della versione
Func _CreaFileVersione($sPath, $sVersion, $sFilename = "XPRINT.exe")
    Local $hFile = FileOpen($sPath, $FO_OVERWRITE)
    If $hFile = -1 Then
        Return SetError(1, 0, False)
    EndIf

    FileWrite($hFile, $sVersion & "|" & $sFilename)
    FileClose($hFile)

    Return True
EndFunc
