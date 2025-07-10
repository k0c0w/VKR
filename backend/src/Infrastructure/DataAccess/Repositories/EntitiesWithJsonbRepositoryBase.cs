using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;

namespace DataAccess.Repositories;

public abstract class EntitiesWithJsonbRepositoryBase(NpgsqlDataSource dataSource, ILogger logger) 
    : PostgresRepositoryBase(dataSource, logger)
{
    protected string Serialize<TModel>(TModel model) where TModel : notnull
        => JsonConvert.SerializeObject(model, Formatting.None);
}