using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FacturacionelectronicaCore.Repositorio.Mongodb
{
    public class MongoHelper : IMongoHelper
    {
        // MongoClient es thread-safe y mantiene el pool de conexiones: debe reutilizarse.
        // Crear uno por consulta abre conexiones nuevas cada vez y degrada mucho el rendimiento.
        private static readonly ConcurrentDictionary<string, MongoClient> Clients = new ConcurrentDictionary<string, MongoClient>();

        private readonly IConfiguration _configuration;
        public MongoHelper(IConfiguration config)
        {
            _configuration = config;
        }
        public IMongoDatabase GetMongoDbInstance(string dbName)
        {
            var connString = _configuration.GetConnectionString("MongoDB");
            var client = Clients.GetOrAdd(connString, cs => new MongoClient(cs));
            return client.GetDatabase(dbName);
        }
        private IMongoCollection<T> GetCollection<T>(string dbName, string collectionName)
        {
            return GetMongoDbInstance(dbName).GetCollection<T>(collectionName);
        }
        public async Task<List<T>> GetAllDocuments<T>(string dbName, string collectionName)
        {
            var collection = GetCollection<T>(dbName, collectionName);
            return await collection.Find(x => true).ToListAsync();
        }
        public async Task<List<T>> GetFilteredDocuments<T>(string dbName, string collectionName, FilterDefinition<T> filter)
        {
            return await GetCollection<T>(dbName, collectionName).Find(filter).ToListAsync();
        }
        public async Task UpdateDocument<T>(string dbName, string collectionName, FilterDefinition<T> filter, UpdateDefinition<T> document)
        {
            await GetCollection<T>(dbName, collectionName).UpdateOneAsync(filter, document);
        }
        public async Task ReplaceDocument<T>(string dbName, string collectionName, FilterDefinition<T> filter, T document)
        {
            await GetCollection<T>(dbName, collectionName).ReplaceOneAsync(filter, document);
        }
        public async Task CreateDocument<T>(string dbName, string collectionName, T document)
        {
            await GetCollection<T>(dbName, collectionName).InsertOneAsync(document);
        }
        public async Task DeleteDocument<T>(string dbName, string collectionName, FilterDefinition<T> filter)
        {
            await GetCollection<T>(dbName, collectionName).DeleteOneAsync(filter);
        }

        public async Task<IEnumerable<T>> GetFilteredDocuments<T>(string dbName, string collectionName, IEnumerable<FilterDefinition<T>> filter)
        {
            FilterDefinition<T> combineFilters = Builders<T>.Filter.And(filter);
            return await GetCollection<T>(dbName, collectionName).Find(combineFilters).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetFilteredDocuments<T>(string dbName, string collectionName, IEnumerable<FilterDefinition<T>> filter, SortDefinition<T> sort)
        {
            FilterDefinition<T> combineFilters = Builders<T>.Filter.And(filter);
            return await GetCollection<T>(dbName, collectionName).Find(combineFilters).Sort(sort).ToListAsync();
        }

        public async Task<long> UpdateMany<T>(string dbName, string collectionName, FilterDefinition<T> filter, UpdateDefinition<T> document)
        {
            var resultado = await GetCollection<T>(dbName, collectionName).UpdateManyAsync(filter, document);
            return resultado.MatchedCount;
        }

        public async Task BulkWrite<T>(string dbName, string collectionName, IEnumerable<WriteModel<T>> operaciones, bool ordenado = true)
        {
            var lista = operaciones.ToList();
            if (!lista.Any())
            {
                return;
            }
            await GetCollection<T>(dbName, collectionName).BulkWriteAsync(lista, new BulkWriteOptions { IsOrdered = ordenado });
        }
    }
}
