using Azure.ResourceManager.Models;

namespace Cdk.Core
{
    internal abstract class SubResource : Resource
    {
        public SubResource(Resource scope, string version, ResourceData properties)
            : base(scope, version, properties)
        {
        }
    }
}
