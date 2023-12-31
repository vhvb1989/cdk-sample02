﻿using Cdk.Core;
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
            connectionString.Database.Scope!.Resources.Add(this);
            connectionString.Database.Scope!.ResourceReferences.Add(Scope!);
            connectionString.Database.Scope!.ResourceReferences.Add(connectionString.Password);
            connectionString.Database.Scope!.ModuleDependencies.Add(scope!);
            Scope?.Resources.Remove(this);
        }

        private static string GetName(string? name) => name is null ? $"kvs-{Infrastructure.Seed}" : $"{name}-{Infrastructure.Seed}";
    }
}
