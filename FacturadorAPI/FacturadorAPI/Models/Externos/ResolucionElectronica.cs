﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturadorAPI.Models.Externos
{
    public class ResolucionElectronica
    {
        public string id { get; set; }
        public string name { get; set; }
        public string prefix { get; set; }
        public string invoiceText { get; set; }
        public bool isDefault { get; set; }
    }
}
