using Cdk.Core;
using Azure.Core;
using Azure.Core.Serialization;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources;
using System.Text;

namespace Cdk.ResourceManager
{
    public class ResourceGroup : Resource<ResourceGroupData>, IModelSerializable<ResourceGroup>
    {
        public override string Name { get; } = $"rg{Infrastructure.Seed.Replace("-","")}";

        public ResourceGroup(Resource scope, string? name = default, string version = "2023-07-01", AzureLocation? location = default)
            : base(scope, version, ResourceManagerModelFactory.ResourceGroupData(
                name: name is null ? $"rg-{Infrastructure.Seed}" : $"{name}-{Infrastructure.Seed}",
                resourceType: "Microsoft.Resources/resourceGroups",
                tags: new Dictionary<string,string> {{"azd-env-name",Environment.GetEnvironmentVariable("AZURE_ENV_NAME")}},
                location: location ?? Environment.GetEnvironmentVariable("AZURE_LOCATION") ?? AzureLocation.WestUS))
        {
        }

        BinaryData IModelSerializable<ResourceGroup>.Serialize(ModelSerializerOptions options)
        {
            using var stream = new MemoryStream();
            stream.Write(Encoding.UTF8.GetBytes($"targetScope = 'subscription'{Environment.NewLine}{Environment.NewLine}"));

            WriteParameters(stream);

            stream.Write(Encoding.UTF8.GetBytes($"resource {Name} '{Properties.ResourceType}@{Version}' = {{{Environment.NewLine}"));
            stream.Write(ModelSerializer.Serialize(Properties, options));
            stream.Write(Encoding.UTF8.GetBytes($"}}{Environment.NewLine}"));

            foreach (var resource in Resources)
            {
                stream.Write(Encoding.UTF8.GetBytes(Environment.NewLine));
                stream.Write(ModelSerializer.Serialize(resource, "bicep-module"));
            }

            return new BinaryData(stream.GetBuffer().AsMemory(0, (int)stream.Position));
        }

        ResourceGroup IModelSerializable<ResourceGroup>.Deserialize(BinaryData data, ModelSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
