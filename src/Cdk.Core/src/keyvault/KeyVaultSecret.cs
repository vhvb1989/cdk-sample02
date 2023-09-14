using Cdk.Core;
using Azure.ResourceManager.KeyVault;
using Azure.ResourceManager.KeyVault.Models;
using Cdk.Sql;

namespace Cdk.KeyVault
{
    public class KeyVaultSecret : Resource<KeyVaultSecretData>
    {
        private const string ResourceTypeName = "Microsoft.KeyVault/vaults/secrets";

        public KeyVaultSecret(Resource? scope, string name, string version = "2023-02-01")
            : base(scope, GetName(name), ResourceTypeName, version, ArmKeyVaultModelFactory.KeyVaultSecretData(
                name: GetName(name),
                resourceType: ResourceTypeName,
                properties: ArmKeyVaultModelFactory.SecretProperties(
                    value: Guid.Empty.ToString())
                ))
        {
        }

        public KeyVaultSecret(Resource? scope, string name, ConnectionString connectionString, string version = "2023-02-01")
            : base(scope, GetName(name), ResourceTypeName, version, ArmKeyVaultModelFactory.KeyVaultSecretData(
                name: GetName(name),
                resourceType: ResourceTypeName,
                properties: ArmKeyVaultModelFactory.SecretProperties(
                    value: connectionString.Value)
                ))
        {
            ResourceReferences.Add(connectionString.Database);
            ResourceReferences.Add(connectionString.Database.Scope!);
        }

        private static string GetName(string? name) => name is null ? $"kvs-{Infrastructure.Seed}" : $"{name}-{Infrastructure.Seed}";
    }
}
