using Cdk.Core;
using Azure.Core;
using Azure.ResourceManager.Sql;
using Azure.ResourceManager.Sql.Models;

namespace Cdk.Sql
{
    public class SqlServer : Resource<SqlServerData>
    {
        public override string Name { get; } = $"sql{Infrastructure.Seed}";

        public SqlServer(Resource? scope, string name, string? version = default, AzureLocation? location = default)
            : base(scope, version ?? "2022-08-01-preview", ArmSqlModelFactory.SqlServerData(
                name: name is null ? $"sql-{Infrastructure.Seed}" : $"{name}-{Infrastructure.Seed}",
                location: location ?? Environment.GetEnvironmentVariable("AZURE_LOCATION") ?? AzureLocation.WestUS,
                resourceType: "Microsoft.Sql/servers",
                version: "12.0",
                minimalTlsVersion: "1.2",
                publicNetworkAccess: ServerNetworkAccessFlag.Enabled,
                administratorLogin: "sqladmin",
                administratorLoginPassword: Guid.Empty.ToString()))
        {
        }
    }
}
