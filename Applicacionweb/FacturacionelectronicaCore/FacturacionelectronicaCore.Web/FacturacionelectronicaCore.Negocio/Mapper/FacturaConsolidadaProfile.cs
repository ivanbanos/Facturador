using AutoMapper;
using EntitiesFC = FacturacionelectronicaCore.Repositorio.Entities;

namespace FacturacionelectronicaCore.Negocio.Mapper
{
    public class FacturaConsolidadaProfile : Profile
    {
        public FacturaConsolidadaProfile()
        {
            // Mapeo entre FacturaConsolidada (Entity) y FacturaConsolidada (Modelo)
            CreateMap<EntitiesFC.FacturaConsolidada, Modelo.FacturaConsolidada>()
                .ForMember(dest => dest.Totales, opt => opt.MapFrom(src => src.Totales))
                .ForMember(dest => dest.ResumenCombustibles, opt => opt.MapFrom(src => src.ResumenCombustibles))
                .ForMember(dest => dest.OrdenesDetalle, opt => opt.Ignore()); // Se llena en el servicio

            CreateMap<Modelo.FacturaConsolidada, EntitiesFC.FacturaConsolidada>()
                .ForMember(dest => dest.Totales, opt => opt.MapFrom(src => src.Totales))
                .ForMember(dest => dest.ResumenCombustibles, opt => opt.MapFrom(src => src.ResumenCombustibles));

            // Mapeo entre TotalesFactura (Entity) y TotalesFactura (Modelo)
            CreateMap<EntitiesFC.TotalesFactura, Modelo.TotalesFactura>().ReverseMap();

            // Mapeo entre ResumenCombustible (Entity) y DetallePorCombustible (Modelo) - simplified mapping
            CreateMap<EntitiesFC.ResumenCombustible, Modelo.TotalesFactura>()
                .ForMember(dest => dest.SubTotal, opt => opt.MapFrom(src => src.SubTotal))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total));

            // Mapeo entre OrdenDeDespacho (Entities) y OrdenDeDespacho (Modelo)
            CreateMap<EntitiesFC.OrdenDeDespacho, Modelo.OrdenDeDespacho>();
            CreateMap<Modelo.OrdenDeDespacho, EntitiesFC.OrdenDeDespacho>();

            // Mapeo entre Tercero (Entities) y Tercero (Modelo)
            CreateMap<EntitiesFC.Tercero, Modelo.Tercero>().ReverseMap();
        }
    }
}