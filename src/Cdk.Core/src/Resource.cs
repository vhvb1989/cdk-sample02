using Cdk.ResourceManager;
using Azure.Core.Serialization;
using Azure.ResourceManager.Models;
using System.Text;

namespace Cdk.Core
{
    public abstract class Resource : IModelSerializable<Resource>
    {
        private class EmptyResourceData : ResourceData { }

        private class Subscription : Resource<EmptyResourceData>
        {
            public override string Name => "subscription";

            public Subscription(Resource? scope, string version, EmptyResourceData properties)
                : base(scope, version, properties)
            {
            }
        }

        public static readonly Resource SubscriptionScope = new Subscription(null, string.Empty, new EmptyResourceData());

        protected internal Dictionary<string, string> ParameterOverrides { get; }
        public IList<Parameter> Parameters { get; }

        public IList<Output> Outputs { get; }

        public IList<Resource> Resources { get; }
        public Resource? Scope { get; }
        public ResourceData Properties { get; }
        public string Version { get; }
        public abstract string Name { get; }

        protected Resource(Resource? scope, string version, ResourceData properties)
        {
            Resources = new List<Resource>();
            Scope = scope;
            Scope?.Resources.Add(this);
            Properties = properties;
            Version = version;
            ParameterOverrides = new Dictionary<string, string>();
            Parameters = new List<Parameter>();
            Outputs = new List<Output>();
        }

        private bool IsChildResource => Scope is not null && Scope is not ResourceGroup && !Scope.Equals(SubscriptionScope);

        public void AssignParameter(string propertyName, Parameter parameter)
        {
            ParameterOverrides.Add(propertyName.ToCamelCase(), parameter.Name);
            Parameters.Add(parameter);
        }

        public void AddOutput(string name, string propertyName)
        {
            string? reference = GetReference(Properties.GetType(), propertyName, Name.ToCamelCase());
            if (reference is null)
                throw new ArgumentNullException(nameof(propertyName), $"{propertyName} was not found in the property tree for {Properties.GetType().Name}");
            Outputs.Add(new Output(name, reference));
        }

        private static string? GetReference(Type type, string propertyName, string str)
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                if (property.Name.Equals(propertyName, StringComparison.Ordinal))
                {
                    return $"{str}.{property.Name.ToCamelCase()}";
                }
            }

            //need to check next level
            foreach (var property in properties)
            {
                var result = GetReference(property.PropertyType, propertyName, $"{str}.{property.Name.ToCamelCase()}");
                if (result is not null)
                    return result;
            }

            return null;
        }

        BinaryData IModelSerializable<Resource>.Serialize(ModelSerializerOptions options) => (options.Format.ToString()) switch
        {
            "bicep" => SerializeModule(options),
            "bicep-module" => SerializeModuleReference(options),
            _ => throw new FormatException($"Unsupported format {options.Format}")
        };

        private BinaryData SerializeModuleReference(ModelSerializerOptions options)
        {
            using var stream = new MemoryStream();
            stream.Write(Encoding.UTF8.GetBytes($"module {Name} './resources/{Name}/{Name}.bicep' = {{{Environment.NewLine}"));
            stream.Write(Encoding.UTF8.GetBytes($"  name: '{Properties.Name}'{Environment.NewLine}"));
            stream.Write(Encoding.UTF8.GetBytes($"  scope: {Scope!.Name}{Environment.NewLine}"));
            var parametersToWrite = GetParams(this);
            if (parametersToWrite.Count() > 0)
            {
                stream.Write(Encoding.UTF8.GetBytes($"  params: {{{Environment.NewLine}"));
                foreach (var paramName in parametersToWrite)
                {
                    stream.Write(Encoding.UTF8.GetBytes($"    {paramName}: {paramName}{Environment.NewLine}"));
                }
                stream.Write(Encoding.UTF8.GetBytes($"  }}{Environment.NewLine}"));
            }
            stream.Write(Encoding.UTF8.GetBytes($"}}{Environment.NewLine}"));

            return new BinaryData(stream.GetBuffer().AsMemory(0, (int)stream.Position));
        }

        private static IEnumerable<string> GetParams(Resource resource)
        {
            IEnumerable<string> parameters = resource.ParameterOverrides.Values;
            foreach (var child in resource.Resources)
            {
                parameters = parameters.Concat(GetParams(child));
            }
            return parameters;
        }

        private BinaryData SerializeModule(ModelSerializerOptions options)
        {
            int depth = GetDepth();
            using var stream = new MemoryStream();

            WriteParameters(stream);

            stream.Write(Encoding.UTF8.GetBytes($"resource {Name} '{Properties.ResourceType}@{Version}' = {{{Environment.NewLine}"));

            if (IsChildResource)
            {
                stream.Write(Encoding.UTF8.GetBytes($"  parent: {Scope!.Name}{Environment.NewLine}"));
            }

            WriteLines(0, ModelSerializer.Serialize(Properties, options), stream, this);
            stream.Write(Encoding.UTF8.GetBytes($"}}{Environment.NewLine}"));

            foreach (var resource in Resources)
            {
                int depthToUse = resource is SubResource ? depth : 0;
                stream.Write(Encoding.UTF8.GetBytes(Environment.NewLine));
                WriteLines(depthToUse, ModelSerializer.Serialize(resource, options), stream, resource);
            }

            WriteOutputs(stream);

            return new BinaryData(stream.GetBuffer().AsMemory(0, (int)stream.Position));
        }

        private void WriteOutputs(MemoryStream stream)
        {
            if (Outputs.Count > 0)
                stream.Write(Encoding.UTF8.GetBytes(Environment.NewLine));

            foreach(var output in Outputs)
            {
                stream.Write(Encoding.UTF8.GetBytes($"output {output.Name} string = {output.Value}{Environment.NewLine}"));
            }
        }

        protected void WriteParameters(MemoryStream stream)
        {
            foreach (var parameter in Parameters)
            {
                string defaultValue = parameter.DefaultValue is null ? string.Empty : $" = '{parameter.DefaultValue}'";

                if (parameter.IsSecure)
                    stream.Write(Encoding.UTF8.GetBytes($"@secure(){Environment.NewLine}"));

                stream.Write(Encoding.UTF8.GetBytes($"@description('{parameter.Description}'){Environment.NewLine}"));
                stream.Write(Encoding.UTF8.GetBytes($"param {parameter.Name} string{defaultValue}{Environment.NewLine}{Environment.NewLine}"));
            }
        }

        protected internal static void WriteLines(int depth, BinaryData data, Stream stream, Resource resource)
        {
            string indent = new string(' ', depth * 2);
            string[] lines = data.ToString().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                string lineToWrite = lines[i];

                ReadOnlySpan<char> line = lines[i];
                int start = 0;
                while (line.Length > start && line[start] == ' ')
                {
                    start++;
                }
                line = line.Slice(start);
                int end = line.IndexOf(':');
                if (end > 0)
                {
                    string name = line.Slice(0, end).ToString();
                    if (resource.ParameterOverrides.TryGetValue(name, out var value))
                    {
                        lineToWrite = $"{new string(' ', start)}{name}: {value}";
                    }
                }
                stream.Write(Encoding.UTF8.GetBytes($"{indent}{lineToWrite}{Environment.NewLine}"));
            }
        }

        private int GetDepth()
        {
            Resource? parent = Scope;
            int depth = 0;
            while (parent is not null)
            {
                depth++;
                parent = parent.Scope;
            }
            return depth;
        }

        Resource IModelSerializable<Resource>.Deserialize(BinaryData data, ModelSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
