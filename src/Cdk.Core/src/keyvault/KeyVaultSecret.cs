using Cdk.Core;
using Azure.ResourceManager.KeyVault;
using Azure.ResourceManager.KeyVault.Models;

namespace Cdk.KeyVault
{
    public class KeyVaultSecret : Resource<KeyVaultSecretData>
    {
        public override string Name { get; }

        public KeyVaultSecret(Resource? scope, string name, string version = "2023-02-01")
            : base(scope, version, ArmKeyVaultModelFactory.KeyVaultSecretData(
                name: name is null ? $"kvs-{Infrastructure.Seed}" : $"{name}-{Infrastructure.Seed}",
                resourceType: "Microsoft.KeyVault/vaults/secrets",
                properties: ArmKeyVaultModelFactory.SecretProperties(
                    value: Guid.Empty.ToString())
                ))
        {
            Name = name is null ? $"kvs{Infrastructure.Seed}" : $"{name}{Infrastructure.Seed}";
        }
    }
}
