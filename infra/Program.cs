using Azure.ResourceManager.Resources.Models;
using Cdk.Core;
using Cdk.KeyVault;
using Cdk.ResourceManager;
using Cdk.Resources;
using Cdk.Sql;
using Cdk.Websites;

// Defaults coming from env:
// AZURE_ENV_NAME
// AZURE_LOCATION
// AZURE_SUBSCRIPTION_ID
// AZURE_PRINCIPAL_ID
// AZURE_TENANT_ID

// New ResourceGroup for infra
var resourceGroup = new ResourceGroup();

// ** KeyVault
var keyVault = new KeyVault(resourceGroup, "kv");

// ** Secret for SQL - Password
var sqlAdminPasswordParam = new Parameter("sqlAdminPassword", "SQL Server administrator password", isSecure: true);
var sqlAdminSecret = new KeyVaultSecret(keyVault, "sqlAdminPassword");
sqlAdminSecret.AssignParameter(nameof(sqlAdminSecret.Properties.Properties.Value), sqlAdminPasswordParam);

var dbUserPasswordParam = new Parameter("dbUserPassword", "Database password", isSecure: true);
var dbPasswordSecret = new KeyVaultSecret(keyVault, "databasePassword");
dbPasswordSecret.AssignParameter(nameof(dbPasswordSecret.Properties.Properties.Value), dbUserPasswordParam);

// SQL Server
var sqlServer = new SqlServer(resourceGroup, "sql");
sqlServer.AssignParameter(nameof(sqlServer.Properties.AdministratorLoginPassword), sqlAdminPasswordParam);

// SQL Server config - Database / Firewall / deployment
var sqlDb = new SqlDatabase(sqlServer);
new KeyVaultSecret(keyVault, "connectionString", sqlDb.GetConnectionString(dbPasswordSecret));
new SqlFirewallRule(sqlServer, "sqlRules");
new DeploymentScript(resourceGroup, "cliScript", sqlDb, dbUserPasswordParam, sqlAdminPasswordParam);

// ** Service plan
var appServicePlan = new AppServicePlan(resourceGroup, "appServicePlan");

// web - app
var webApp = new WebSite(resourceGroup, "web", appServicePlan, Runtime.Node, "18-lts");
webApp.Properties.Tags.Add("azd-service-name","web");
new WebSiteConfigLogs(webApp, "webLogs");

// api - api
var apiApp = new WebSite(resourceGroup, "api", appServicePlan, Runtime.Dotnetcore, "6.0");

apiApp.Properties.Tags.Add("azd-service-name","api");
new WebSiteConfigLogs(apiApp, "apiLogs");

// Handle output
string path = args.Length > 0 ? args[0] : "./infra";
new Infra().ToBicep(path);


class Infra : Infrastructure {
}
