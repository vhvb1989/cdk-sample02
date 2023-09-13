using Cdk.ResourceManager;
using Azure.Core.Serialization;

namespace Cdk.Core
{
    public class Infrastructure
    {
        internal static readonly string Seed = Environment.GetEnvironmentVariable("AZURE_ENV_NAME") ?? throw new Exception("No environment variable found named 'AZURE_ENV_NAME'");

        public IList<Resource> Resources { get; }

        public IList<Parameter> Parameters { get; }

        public Infrastructure()
        {
            Resources = new List<Resource>();
            Parameters = new List<Parameter>();
        }

        public void ToBicep(string outputPath = ".")
        {
            outputPath = Path.GetFullPath(outputPath);
            foreach (var resource in Resources)
            {
                WriteBicepFile(outputPath, resource);
            }
        }

        private string GetFilePath(string outputPath, Resource resource)
        {
            string fileName = resource is ResourceGroup ? Path.Combine(outputPath, "main.bicep") : Path.Combine(outputPath, "resources", resource.Name, $"{resource.Name}.bicep");
            Directory.CreateDirectory(Path.GetDirectoryName(fileName)!);
            return fileName;
        }

        private void WriteBicepFile(string outputPath, Resource resource)
        {
            using var stream = new FileStream(GetFilePath(outputPath, resource), FileMode.Create);
            if (resource is ResourceGroup && resource.Parameters.Count == 0)
            {
                foreach (var parameter in Parameters)
                {
                    resource.Parameters.Add(parameter);
                }
            }
            stream.Write(ModelSerializer.Serialize(resource, "bicep"));
            if (resource is ResourceGroup)
            {
                foreach (var child in resource.Resources)
                {
                    WriteBicepFile(outputPath, child);
                }
            }
        }
    }
}