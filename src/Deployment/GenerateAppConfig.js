const {
  readFile: readFileCallback,
  writeFile: writeFileCallback,
  access: accessCallback,
  F_OK,
} = require("fs");
const { join } = require("path");
const { promisify } = require("util");

const configFile = join(__dirname, "AppConfig.json");
const rootPath = join(__dirname, "../..");

const readFile = promisify(readFileCallback);
const writeFile = promisify(writeFileCallback);

main()
  .then(() => console.log("Finished."))
  .catch((err) => console.error(err));

async function main() {
  if (await fileExists(configFile)) {
    await updateConfigurationFiles();
  } else {
    await generateNewFile();
  }
}

async function generateNewFile() {
  const prefix = process.argv[2] || "xyz";
  const data = {
    databaseAccountName: `${prefix}-db`,
    searchAccountName: `${prefix}-search`,
    datasetStorageName: `${prefix}datasets01`,
    applicationStorageName: `${prefix}appstorage`,
    webPlanName: `${prefix}-web-plan`,
    webAppName: `${prefix}-web-app`,
    webApiName: `${prefix}-web-api`,
    webAdminName: `${prefix}-web-admin`,
    batchAccountName: `${prefix}batch`,
    consumptionPlanName: `${prefix}-email-plan`,
    emailFxnName: `${prefix}-email-fxn`,
    fromEmail: "Name <name@example.com>",
    toEmail: [
      "Name One <name.one@example.com>",
      "Name Two <name.two@example.com>",
    ],
    sendGridApiKey: "[API-KEY]",
    appInsightsName: `${prefix}-insights`,
    keyVaultName: `${prefix}-keyvault`,
    domainName: `${prefix}-web-app.azurewebsites.net`,
    b2cTenant: "[B2C-TENANT-NAME]",
    b2cWebAudience: "[B2C-WEB-CLIENT-ID]",
    b2cWebPolicy: "[B2C-WEB-POLICY-NAME]",
    b2cAdminAudience: "[B2C-ADMIN-CLIENT-ID]",
    b2cAdminPolicy: "[B2C-ADMIN-POLICY-NAME]",
    authorizedAdminUsers: ["one@example.com", "two@example.com"],
  };
  const content = JSON.stringify(data, null, 2);
  await writeFile(configFile, content, "utf8");
  console.log("Created new configuration file:");
  console.log(configFile);
}

async function updateConfigurationFiles() {
  console.log("Updating project configuration file");
  const content = await readFile(configFile, "utf8");
  const config = JSON.parse(content);
  const configMap = [
    [
      "src/Deployment/templates/azuredeploy.parameters.json",
      transformJsonFile(updateAzureDeployParameters),
    ],
    [
      "src/Msr.Odr.Web/appsettings.json",
      transformJsonFile(updateWebAppSettings),
    ],
    [
      "src/Msr.Odr.Web/appsettings.development.json",
      transformJsonFile(updateWebAppDevelopmentSettings),
    ],
    [
      "src/Msr.Odr.Web/appsettings.production.json",
      transformJsonFile(updateWebAppProductionSettings),
    ],
    [
      "src/Msr.Odr.WebApi/appsettings.json",
      transformJsonFile(updateWebApiSettings),
    ],
    [
      "src/Msr.Odr.WebApi/appsettings.development.json",
      transformJsonFile(updateWebApiDevelopmentSettings),
    ],
    [
      "src/Msr.Odr.WebApi/appsettings.production.json",
      transformJsonFile(updateWebApiProductionSettings),
    ],
    [
      "src/Msr.Odr.WebAdminPortal/appsettings.json",
      transformJsonFile(updateWebAdminSettings),
    ],
    [
      "src/Msr.Odr.WebAdminPortal/appsettings.development.json",
      transformJsonFile(updateWebAdminDevelopmentSettings),
    ],
    [
      "src/Msr.Odr.WebAdminPortal/appsettings.production.json",
      transformJsonFile(updateWebAdminProductionSettings),
    ],
  ];

  for ([relativeFile, transformFn] of configMap) {
    await transformFn(relativeFile, config);
  }
}

function updateAzureDeployParameters(config, data) {
  data.parameters = {
    databaseAccountName: {
      value: config.databaseAccountName,
    },
    searchAccountName: {
      value: config.searchAccountName,
    },
    datasetStorageName: {
      value: config.datasetStorageName,
    },
    applicationStorageName: {
      value: config.applicationStorageName,
    },
    webPlanName: {
      value: config.webPlanName,
    },
    webAppName: {
      value: config.webAppName,
    },
    webApiName: {
      value: config.webApiName,
    },
    webApiAllowedOrigins: {
      value: [
        "http://localhost:53048",
        config.webAppName,
        "https://portal.azure.com",
      ],
    },
    webAdminName: {
      value: config.webAdminName,
    },
    batchAccountName: {
      value: config.batchAccountName,
    },
    consumptionPlanName: {
      value: config.consumptionPlanName,
    },
    emailFxnName: {
      value: config.emailFxnName,
    },
    fromEmail: {
      value: config.fromEmail,
    },
    toEmail: {
      value: config.toEmail.join(";"),
    },
    sendGridApiKey: {
      value: config.sendGridApiKey,
    },
    appInsightsName: {
      value: config.appInsightsName,
    },
    keyVaultName: {
      value: config.keyVaultName,
    },
    authorizedAdminUsers: {
      value: config.authorizedAdminUsers.join(";"),
    },
  };
  return data;
}

function updateWebAppSettings(config, data) {
  data.azureAD = {
    tenant: config.b2cTenant,
    audience: config.b2cWebAudience,
    policy: config.b2cWebPolicy,
  };
  data.SiteMap = `https://${config.domainName}`;
  return data;
}
function updateWebAppDevelopmentSettings(config, data) {
  return data;
}
function updateWebAppProductionSettings(config, data) {
  data.apiBaseUrl = `https://${config.webApiName}.azurewebsites.net/`;
  return data;
}

function updateWebApiSettings(config, data) {
  data.azureAD = {
    tenant: config.b2cTenant,
    audience: config.b2cWebAudience,
    policy: config.b2cWebPolicy,
  };
  data.SiteMap = `https://${config.domainName}`;
  return data;
}
function updateWebApiDevelopmentSettings(config, data) {
  data.keyVaultUrl = `https://${config.keyVaultName}.vault.azure.net/`;
  return data;
}
function updateWebApiProductionSettings(config, data) {
  data.keyVaultUrl = `https://${config.keyVaultName}.vault.azure.net/`;
  data.WebServer.URL = `https://${config.webApiName}.azurewebsites.net/`;
  return data;
}

function updateWebAdminSettings(config, data) {
  data.azureAD = {
    tenant: config.b2cTenant,
    audience: config.b2cAdminAudience,
    policy: config.b2cAdminPolicy,
  };
  return data;
}
function updateWebAdminDevelopmentSettings(config, data) {
  data.keyVaultUrl = `https://${config.keyVaultName}.vault.azure.net/`;
  data.Assets.DatasetUtil = `https://${config.applicationStorageName}.blob.core.windows.net/application-assets/DatasetUtil.msi`;
  data.authorizedAdminUsers = config.authorizedAdminUsers.join(";");
  return data;
}
function updateWebAdminProductionSettings(config, data) {
  data.keyVaultUrl = `https://${config.keyVaultName}.vault.azure.net/`;
  data.Assets.DatasetUtil = `https://${config.applicationStorageName}.blob.core.windows.net/application-assets/DatasetUtil.msi`;
  return data;
}

function fileExists(fileName) {
  return new Promise((resolve) => {
    accessCallback(fileName, F_OK, (err) => {
      resolve(!err);
    });
  });
}

function transformJsonFile(transformFn) {
  return async (relativeFile, config) => {
    console.log(` - ${relativeFile}`);
    const fileName = join(rootPath, relativeFile);
    const exists = await fileExists(fileName);
    let data = {};
    let content;
    if (exists) {
      content = await readFile(fileName, "utf8");
      data = JSON.parse(content);
    }
    data = transformFn(config, data);
    content = JSON.stringify(data, null, 2);
    await writeFile(fileName, content, "utf8");
  };
}
