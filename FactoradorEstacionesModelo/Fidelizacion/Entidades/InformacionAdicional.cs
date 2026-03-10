namespace Dominio.Entidades
{
    public class InformacionAdicional
    {
        public int Id { get; set; }
        public string? Telefono { get; set; }
        public string Celular { get; set; }
        public string Direccion { get; set; }
        public int? Estrato { get; set; }
        public int? NumeroHijos { get; set; }
        public int SexoId { get; set; }
        public virtual Sexo? Sexo { get; set; }
        public int CiudadId { get; set; }
        public virtual Ciudad? Ciudad { get; set; }
        public int? ProfesionId { get; set; }
        public virtual Profesion? Profesion { get; set; }
        public int? EmpresaFidelizadoId { get; set; }
        public virtual EmpresaFidelizado? EmpresaFidelizado { get; set; }
        public int UsuarioId { get; set; }
        public int? FidelizadoId { get; set; }

    }
}
