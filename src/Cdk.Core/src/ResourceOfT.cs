using Azure.ResourceManager.Models;

namespace Cdk.Core
{
    public abstract class Resource<T> : Resource
        where T : ResourceData
    {
        public new T Properties { get; }

        protected Resource(Resource? scope, string version, T properties)
            : base(scope, version, properties)
        {
            Properties = properties;
        }
    }
}
