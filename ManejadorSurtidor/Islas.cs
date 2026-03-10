using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ManejadorSurtidor
{
    public class Islas
    {
        private List<string> IslasConectadas = new List<string>();
        public Islas() { IslasConectadas = new List<string>(); }

        public void AgregarIsla(string isla)
        {
            isla = JsonConvert.DeserializeObject<string>(isla);
            if (!IslasConectadas.Contains(isla))
            {
                IslasConectadas.Add(isla);
            }
        }

        public List<string> GetIslasConectadas() { return IslasConectadas; }

    }
}
