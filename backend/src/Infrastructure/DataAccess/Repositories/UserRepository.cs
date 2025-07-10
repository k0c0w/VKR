using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using NpgsqlTypes;
using ResultMonad;

namespace DataAccess.Repositories;

public class UserRepository(
    NpgsqlDataSource dataSource,
    ILogger<IUserRepository>? logger = default)
    : PostgresRepositoryBase(dataSource, logger ?? NullLogger<IUserRepository>.Instance), IUserRepository
{
    public async Task<Result<User, ErrorMessage>> GetAsync(IUserRepository.GetUserFilter filter, CancellationToken ct)
    {
        const string sql = """
            SELECT 
                      email
                    , roles
              FROM users
             WHERE email = @Email;
        """;

        var emailParam = new NpgsqlParameter("Email", filter.Email);

        var connectionResult = await GetOpenedConnectionAsync(ct);
        if (connectionResult.IsFailure)
        {
            return Result.Fail<User, ErrorMessage>(ErrorMessage.RepositorySpecificErrors.GetError);
        }

        await using var connection = connectionResult.Value!;
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(emailParam);

        try
        {
            await using var reader = await command.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                return Result.Fail<User, ErrorMessage>(ErrorMessage.EntityNotfoundError);
            }

            var email = reader.GetString(0);
            var roles = ((short[])reader.GetValue(1))
                .Cast<UserRole>()
                .ToArray();

            var user = User.CreateExisting(email, roles);

            return Result.Ok<User, ErrorMessage>(user);
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);
            return Result.Fail<User, ErrorMessage>(ErrorMessage.RepositorySpecificErrors.GetError);
        }
    }

    
    public async Task<Result<User[], ErrorMessage>> GetAllAsync(CancellationToken ct)
    {
        const string sql = """
                               SELECT 
                                         email
                                       , roles
                                 FROM users;
                           """;

        var result = new List<User>();
        
        var connectionResult = await GetOpenedConnectionAsync(ct);
        if (connectionResult.IsFailure)
        {
            return Result.Fail<User[], ErrorMessage>(ErrorMessage.RepositorySpecificErrors.GetError);
        }

        await using var connection = connectionResult.Value!;
        await using var command = new NpgsqlCommand(sql, connection);

        try
        {
            await using var reader = await command.ExecuteReaderAsync(ct);

            while (await reader.ReadAsync(ct))
            {

                var email = reader.GetString(0);
                var roles = ((short[])reader.GetValue(1))
                    .OrderBy(x => x)
                    .Cast<UserRole>()
                    .ToArray();

                var user = User.CreateExisting(email, roles);
                result.Add(user);
            }

            return Result.Ok<User[], ErrorMessage>(result.ToArray());
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);
            return Result.Fail<User[], ErrorMessage>(ErrorMessage.RepositorySpecificErrors.GetError);
        }
    }

    public async Task<ResultWithError<ErrorMessage>> AddAsync(User user, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO users (email, roles)
            VALUES (@Email, @Roles);
        """;

        var connectionResult = await GetOpenedConnectionAsync(ct);
        if (connectionResult.IsFailure)
        {
            return ResultWithError.Fail(ErrorMessage.RepositorySpecificErrors.AddError);
        }

        await using var connection = connectionResult.Value!;
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Email", user.Email);
        command.Parameters.AddWithValue("Roles", NpgsqlDbType.Array | NpgsqlDbType.Smallint, user.Roles
            .Cast<short>()
            .OrderBy(x => x)
            .ToArray());

        try
        {
            var rowsAffected = await command.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0
                ? ResultWithError.Ok<ErrorMessage>()
                : ResultWithError.Fail(ErrorMessage.RepositorySpecificErrors.AddError);
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);
            return ResultWithError.Fail(ErrorMessage.RepositorySpecificErrors.AddError);
        }
    }

    public async Task<ResultWithError<ErrorMessage>> UpdateAsync(User user, CancellationToken ct)
    {
        const string sql = """
                               UPDATE users
                                  SET roles = @Roles
                                WHERE email = @Email;
                           """;

        var connectionResult = await GetOpenedConnectionAsync(ct);
        if (connectionResult.IsFailure)
        {
            return ResultWithError.Fail(connectionResult.Error);
        }

        await using var connection = connectionResult.Value!;
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("Email", user.Email);
        command.Parameters.AddWithValue("Roles", NpgsqlDbType.Array | NpgsqlDbType.Smallint, user.Roles
            .Cast<short>()
            .OrderBy(x => x)
            .ToArray());

        try
        {
            var rowsAffected = await command.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0
                ? ResultWithError.Ok<ErrorMessage>()
                : ResultWithError.Fail(ErrorMessage.EntityNotfoundError);
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);
            return ResultWithError.Fail(ErrorMessage.DomainError($"Возникла ошибка при обновлении пользователя {user.Email}."));
        }
    }

    public async Task<ResultWithError<ErrorMessage>> RemoveAsync(User user, CancellationToken ct)
    {
        const string sql = """
                               DELETE FROM users
                                WHERE email = @Email;
                           """;

        var emailParam = new NpgsqlParameter("Email", user.Email);

        var connectionResult = await GetOpenedConnectionAsync(ct);
        if (connectionResult.IsFailure)
        {
            return ResultWithError.Fail(connectionResult.Error);
        }

        await using var connection = connectionResult.Value!;
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(emailParam);
        try
        {
            var rowsAffected = await command.ExecuteNonQueryAsync(ct);
            return rowsAffected > 0
                ? ResultWithError.Ok<ErrorMessage>()
                : ResultWithError.Fail(ErrorMessage.EntityNotfoundError);
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);
            return ResultWithError.Fail(ErrorMessage.ValidationError($"Пользователь {user.Email} не был удален."));
        }
    }
    
    public async Task<Result<bool, ErrorMessage>> AnyUserWithRole(UserRole role, CancellationToken ct)
    {
        const string sql = """
                               SELECT EXISTS (
                                   SELECT 1
                                     FROM users
                                    WHERE @Role = ANY(roles)
                               ) AS user_exists;
                           """;

        var roleParam = new NpgsqlParameter("Role", (short)role);

        var connectionResult = await GetOpenedConnectionAsync(ct);
        if (connectionResult.IsFailure)
        {
            return Result.Fail<bool, ErrorMessage>(connectionResult.Error);
        }

        await using var connection = connectionResult.Value!;
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.Add(roleParam);

        try
        {
            var exists = (bool)(await command.ExecuteScalarAsync(ct))!;
            return Result.Ok<bool, ErrorMessage>(exists);
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);
            return Result.Fail<bool, ErrorMessage>(ErrorMessage.RepositorySpecificErrors.GetError);
        }
    }
}
