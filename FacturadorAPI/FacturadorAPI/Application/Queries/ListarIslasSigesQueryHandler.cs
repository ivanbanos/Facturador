using FacturadorAPI.Models;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Queries
{
    public class ListarIslasSigesQueryHandler : IRequestHandler<ListarIslasSigesQuery, IEnumerable<IslaSiges>>
    {
        private readonly ILogger<ListarIslasSigesQueryHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;

        public ListarIslasSigesQueryHandler(ILogger<ListarIslasSigesQueryHandler> logger, IDataBaseHandler databaseHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
        }

        public async Task<IEnumerable<IslaSiges>> Handle(ListarIslasSigesQuery request, CancellationToken cancellationToken)
        {
            var caras = await _databaseHandler.ListarCarasSigues(cancellationToken);
            return caras.GroupBy(x => x.IdIsla).Select(p => new IslaSiges() { Id= p.FirstOrDefault().IdIsla, Isla = p.FirstOrDefault().Isla });
        }
    }
}
