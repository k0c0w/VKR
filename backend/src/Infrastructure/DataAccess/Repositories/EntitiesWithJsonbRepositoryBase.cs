using Newtonsoft.Json;
using Npgsql;

namespace DataAccess.Repositories;

internal abstract class EntitiesWithJsonbRepositoryBase(NpgsqlDataSource dataSource, JsonSerializer serializer) 
    : PostgresRepositoryBase(dataSource)
{
    protected string Serialize<TModel>(TModel model) where TModel : notnull
        => JsonConvert.SerializeObject(model, Formatting.None);
}