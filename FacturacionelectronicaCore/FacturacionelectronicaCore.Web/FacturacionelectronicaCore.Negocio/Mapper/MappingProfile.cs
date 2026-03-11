using AutoMapper;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;

namespace EstacionesServicio.Negocio.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Modelo.Usuario, Repositorio.Entities.Usuario>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Repositorio.Entities.Factura, FacturacionelectronicaCore.Negocio.Modelo.Factura>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Repositorio.Entities.OrdenDeDespacho, FacturacionelectronicaCore.Negocio.Modelo.OrdenDeDespacho>();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.OrdenDeDespacho, FacturacionelectronicaCore.Repositorio.Entities.OrdenDeDespacho>();
            CreateMap<FacturacionelectronicaCore.Repositorio.Entities.Resolucion, FacturacionelectronicaCore.Negocio.Modelo.Resolucion>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Repositorio.Entities.CreacionResolucion, FacturacionelectronicaCore.Negocio.Modelo.CreacionResolucion>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Repositorio.Entities.Tercero, FacturacionelectronicaCore.Negocio.Modelo.Tercero>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Repositorio.Entities.Canastilla, FacturacionelectronicaCore.Negocio.Modelo.Canastilla>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.Tercero, TerceroInput>().ReverseMap();
            CreateMap<Modelo.OrdenesDeDespachoGuids, Repositorio.Entities.OrdenesDeDespachoGuids>();
            CreateMap<Modelo.FacturasEntity, FacturasEntity>();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.Estacion, FacturacionelectronicaCore.Repositorio.Entities.Estacion>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.FacturaFechaReporte, FacturacionelectronicaCore.Repositorio.Entities.FacturaFechaReporte>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.FacturaCanastilla, FacturacionelectronicaCore.Repositorio.Entities.FacturaCanastilla>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.CanastillaFactura, FacturacionelectronicaCore.Repositorio.Entities.CanastillaFactura>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.FormasPagos, FacturacionelectronicaCore.Repositorio.Entities.FormasPagos>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.Turno, FacturacionelectronicaCore.Repositorio.Entities.Turno>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.TurnoSurtidor, FacturacionelectronicaCore.Repositorio.Entities.TurnoSurtidor>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.CupoAutomotor, FacturacionelectronicaCore.Repositorio.Entities.CupoAutomotor>().ReverseMap();
            CreateMap<FacturacionelectronicaCore.Negocio.Modelo.CupoCliente, FacturacionelectronicaCore.Repositorio.Entities.CupoCliente>().ReverseMap();
        }
    }
}
