// Models/AppSettings.cs
using System;
using System.Collections.Generic;

namespace XPRINT.Models
{
    public class AppSettings
    {
        // Equivalenti alle variabili globali in AutoIt
        public string PrinterIpAddress { get; set; }
        public string Directory { get; set; }
        public string FileType { get; set; }
        public bool UseDefaultExtensions { get; set; }
        public int Port { get; set; }
        public string CurrentVersion { get; set; }
        public string SkipVersion { get; set; }

        // Equivalente a $aDefaultExtensions in AutoIt
        public List<string> DefaultExtensions { get; set; }

        public AppSettings()
        {
            PrinterIpAddress = "0.0.0.0";
            Directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            FileType = "";
            UseDefaultExtensions = false;
            Port = 9100;
            CurrentVersion = "2.0.0";
            SkipVersion = "";
            DefaultExtensions = new List<string> { "prn", "jca", "txt", "zpl", "pgl" };
        }
    }
}