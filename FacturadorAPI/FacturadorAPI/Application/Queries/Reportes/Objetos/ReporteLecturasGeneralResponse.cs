namespace FacturadorAPI.Application.Queries.Reportes.Objetos
{
    public class ReporteLecturasGeneralResponse
    {
        public ReporteLecturasGeneralResponse(string nombre, string nIT)
        {
            Nombre = nombre;
            NIT = nIT;
        }

        public List<ReporteTurnoItem> reporteTurnoItem { get; set; } = new List<ReporteTurnoItem>();
        public string Nombre { get; }
        public string NIT { get; }
    }

    public class ReporteTurnoItem
    {
        public DateTime Fecha { get; set; }
        public int IdTurno { get; set; }
        public string Isla { get; set; }
        public string Manguera { get; set; }
        public string Combustible { get; set; }
        public string LecturaInicial { get; set; }
        public string LecturaFinal { get; set; }
    }
}
