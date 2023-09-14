using Azure.Core;
using Azure.ResourceManager.AppService;
using Azure.ResourceManager.AppService.Models;
using Azure.ResourceManager.Models;
using Cdk.Core;

namespace Cdk.Websites
{
    public enum Runtime
    {
        Node,
        Dotnetcore
    }

    public class WebSite : Resource<WebSiteData>
    {
        public const string ResourceTypeName = "Microsoft.Web/sites";
        private static string GetName(string? name) => name is null ? $"webSite-{Infrastructure.Seed}" : $"{name}-{Infrastructure.Seed}";

        public WebSite(Resource? scope, string resourceName, AppServicePlan appServicePlan, Runtime runtime, string runtimeVersion, string version = "2021-02-01", AzureLocation? location = default)
            : base(scope, GetName(resourceName), ResourceTypeName, version, ArmAppServiceModelFactory.WebSiteData(
                name: GetName(resourceName),
                location: GetLocation(location),
                resourceType: ResourceTypeName,
                kind: "app,linux",
                appServicePlanId: appServicePlan.Id,
                siteConfig: ArmAppServiceModelFactory.SiteConfigProperties(
                    linuxFxVersion: $"{runtime.ToString().ToLower()}|{runtimeVersion}",
                    isAlwaysOn: true,
                    ftpsState: AppServiceFtpsState.FtpsOnly,
                    minTlsVersion: "1.2",
                    appCommandLine: runtime == Runtime.Dotnetcore ? string.Empty : "./entrypoint.sh -o ./env-config.js && pm2 serve /home/site/wwwroot --no-daemon --spa",
                    cors: new AppServiceCorsSettings()
                    {
                        AllowedOrigins = 
                        {
                            "https://portal.azure.com",
                            "https://ms.portal.azure.com"
                        }
                    }) ,
                isHttpsOnly: true,
                identity: new ManagedServiceIdentity(ManagedServiceIdentityType.SystemAssigned)))
        {
            ModuleDependencies.Add(appServicePlan);
        }
    }
}
