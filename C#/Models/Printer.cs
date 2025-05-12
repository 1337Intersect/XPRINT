// Models/Printer.cs
using System;
using System.Collections.Generic;

namespace XPRINT.Models
{
    public class Printer
    {
        public string Model { get; set; }
        public string IpAddress { get; set; }
        public string Customer { get; set; }

        // ID univoco basato su modello e IP
        public string UniqueId => $"{Model}_{IpAddress}";

        public Printer()
        {
            Model = string.Empty;
            IpAddress = "0.0.0.0";
            Customer = string.Empty;
        }

        public Printer(string model, string ipAddress, string customer)
        {
            Model = model;
            IpAddress = ipAddress;
            Customer = customer;
        }
    }
}