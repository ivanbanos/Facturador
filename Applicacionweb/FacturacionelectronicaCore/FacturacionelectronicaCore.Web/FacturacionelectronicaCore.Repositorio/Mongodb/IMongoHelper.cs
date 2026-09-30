using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Mongodb
{
    public interface IMongoHelper
    {
        Task<List<T>> GetAllDocuments<T>(string dbName, string collectionName);
        Task<List<T>> GetFilteredDocuments<T>(string dbName, string collectionName, FilterDefinition<T> filter);
        Task<IEnumerable<T>> GetFilteredDocuments<T>(string dbName, string collectionName, IEnumerable<FilterDefinition<T>> filter);
        Task UpdateDocument<T>(string dbName, string collectionName, FilterDefinition<T> filter, UpdateDefinition<T> document);
        Task ReplaceDocument<T>(string dbName, string collectionName, FilterDefinition<T> filter, T document);
        Task CreateDocument<T>(string dbName, string collectionName, T document);
        Task DeleteDocument<T>(string dbName, string collectionName, FilterDefinition<T> filter);
    }
}
