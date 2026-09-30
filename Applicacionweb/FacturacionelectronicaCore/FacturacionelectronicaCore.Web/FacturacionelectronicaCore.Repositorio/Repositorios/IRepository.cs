using EstacionesServicio.Repositorio.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EstacionesServicio.Repositorio.Repositorios
{
    public interface IRepository<TEntity> where TEntity : Entity
    {

        Task<IEnumerable<TEntity>> Listar();
        TEntity Obtener(Guid guid);
        Task Agregar(IEnumerable<TEntity> isla);
        Task Modificar(IEnumerable<TEntity> isla);
        Task Borrar(Guid guid);
    }
}
