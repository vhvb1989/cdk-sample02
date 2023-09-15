using Cdk.Core;

namespace Cdk.Sql
{
    public class ConnectionString
    {
        public string Value { get; }

        internal SqlDatabase Database { get; }

        internal Resource Password { get; }

        public ConnectionString(SqlDatabase database, Resource password, string userName)
        {
            Database = database;
            Password = password;
            Value = $"Server=${{{database.Scope!.Name}.properties.fullyQualifiedDomainName}}; Database=${{{database.Name}.name}}; User={userName}; Password=${{{password.Name}}}";
        }
    }
}
