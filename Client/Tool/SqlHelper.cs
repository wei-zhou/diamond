namespace Home
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class SqlHelper
    {
        public static async Task<DbConnection> CreateConnectionAsync(string name, bool open)
        {
            Debug.Assert(!string.IsNullOrEmpty(name));

            var configuration = ConfigurationManager.ConnectionStrings[name];
            var factory = DbProviderFactories.GetFactory(configuration.ProviderName);
            var connection = factory.CreateConnection();
            connection.ConnectionString = configuration.ConnectionString;
            if (open)
            {
                await connection.OpenAsync();
            }

            return connection;
        }

        public static DbCommand CreateCommand(this DbConnection connection, string commandText)
        {
            Debug.Assert(connection != null);
            Debug.Assert(!string.IsNullOrEmpty(commandText));

            var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;
            return command;
        }

        public static DbCommand AddParameter(this DbCommand command, string name, DbType type, int size, object value)
        {
            Debug.Assert(command != null);
            Debug.Assert(!string.IsNullOrEmpty(name));
            Debug.Assert(Enum.IsDefined(typeof(DbType), type));

            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.DbType = type;
            parameter.Size = size;
            parameter.Value = value;
            command.Parameters.Add(parameter);
            return command;
        }

        public static async Task ExecuteNonQuery(this DbConnection connection, string commandText, IEnumerable<DbParameter> parameters = null)
        {
            Debug.Assert(connection != null);
            Debug.Assert(!string.IsNullOrEmpty(commandText));

            var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;
            AddParameters(command, parameters);
            await command.ExecuteNonQueryAsync();
        }

        public static async Task<DbDataReader> ExecuteReaderAsync(this DbConnection connection, string commandText, IEnumerable<DbParameter> parameters = null, CommandBehavior behavior = CommandBehavior.Default)
        {
            Debug.Assert(connection != null);
            Debug.Assert(!string.IsNullOrEmpty(commandText));

            var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = CommandType.Text;
            AddParameters(command, parameters);
            return await command.ExecuteReaderAsync(behavior);
        }

        private static void AddParameters(DbCommand command, IEnumerable<DbParameter> parameters)
        {
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters.ToArray());
            }
        }
    }
}
