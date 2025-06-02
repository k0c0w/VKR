using System.Buffers;
using Common;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
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
                     u.id
                   , u.email
                   , array_agg(utur.role_type) AS roles
              FROM users u
              JOIN users_to_user_roles utur ON utur.user_id = u.id
             WHERE u.email = @email
             GROUP BY u.id, u.email;
        """;
        var emailParam = new NpgsqlParameter("email", filter.Email);
        
        var conOpeningResult = await GetOpenedConnectionAsync(ct);
        if (conOpeningResult.IsFailure)
        {
            return Result.Fail<User, ErrorMessage>(ErrorMessage.RepositorySpecificErrors.GetError);
        }

        await using var con = conOpeningResult.Value!;
        await using var command = new NpgsqlCommand(sql, con);
        command.Parameters.Add(emailParam);
        try
        {
            await using var reader = await command.ExecuteReaderAsync(ct);

            if (!await reader.ReadAsync(ct))
            {
                return Result.Fail<User, ErrorMessage>(ErrorMessage.EntityNotfoundError);
            }

            var id = reader.GetGuid(0);
            var email = reader.GetString(1);
            var roles = ((short[])reader.GetValue(2))
                .Cast<UserRole>()
                .ToArray();

            var user = User.CreateExisting(id, email, roles);
            
            return Result.Ok<User, ErrorMessage>(user);
        }
        catch (NpgsqlException ex)
        {
            LogError(ex);
            return Result.Fail<User, ErrorMessage>(ErrorMessage.RepositorySpecificErrors.GetError);
        }
    }

    public async Task<ResultWithError<ErrorMessage>> AddAsync(User user, CancellationToken ct)
    {
        try
        {
            var connOpenRes = await GetOpenedConnectionAsync(ct);
            if (connOpenRes.IsFailure)
            {
                return ResultWithError.Fail(ErrorMessage.RepositorySpecificErrors.AddError);
            }

            await using var conn = connOpenRes.Value!;
            await using var batchCommand = new NpgsqlBatch(conn);
            batchCommand.BatchCommands.Add(GetInsertUserCommand(user));
            batchCommand.BatchCommands.Add(GetInsertRolesCommand(user));

            var rowsAffected = await batchCommand.ExecuteNonQueryAsync(ct);

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

    private static NpgsqlBatchCommand GetInsertUserCommand(User user)
    {
        const string insertUserSql = "INSERT INTO users(id, email) VALUES (@id, @email);";

        var command = new NpgsqlBatchCommand(insertUserSql);
        command.Parameters.Add(new("@id", user.Id));
        command.Parameters.Add(new("@email", user.Email));

        return command;
    }

    private static NpgsqlBatchCommand GetInsertRolesCommand(User user)
    {
        const string insertUserRolesSqlBase = """
            INSERT INTO users_to_user_roles(user_id, role_type)
            VALUES 
        """;

        string[]? parameters = default;
        try
        {
            parameters = ArrayPool<string>.Shared.Rent(user.Roles.Count);
            using var sb = new ValueStringBuilder();
            sb.Append(insertUserRolesSqlBase);
            for (var i = 0; i < user.Roles.Count; i++)
            {
                sb.Append('(');
                sb.Append("@id");
                sb.Append(",@");
                var paramName = $"role{i + 1}";
                sb.Append(paramName);
                parameters[i] = paramName;

                if (i != user.Roles.Count - 1)
                {
                    sb.Append(',');
                }
            }

            sb.Append(';');
            var insertRolesSql = sb.ToString();

            var command = new NpgsqlBatchCommand(insertRolesSql);
            command.Parameters.Add(new("id", user.Id));
            foreach (var (i, role) in user.Roles.Index())
            {
                command.Parameters.Add(new(parameters[i], (short)role));
            }

            return command;
        }
        finally
        {
            if (parameters != default)
            {
                ArrayPool<string>.Shared.Return(parameters);
            }
        }
    }
}