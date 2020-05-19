using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Msr.Odr.Model;
using Msr.Odr.Services;
using Msr.Odr.WebAdminPortal.Users;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Msr.Odr.WebAdminPortal.Controllers
{
    [Produces("application/json")]
    [Route("api/assets")]
    [Authorize(Policy = PolicyNames.MustBeInAdminGroup)]
    public class AssetsController : Controller
    {
        private string DatasetUtilStorageUrl => Configuration["assets:datasetutil"];

        private IConfiguration Configuration { get; }
        private UserDataStorageService UserDataStorage { get; }

        public AssetsController(
            IConfiguration configuration,
            UserDataStorageService userDataStorage)
        {
            Configuration = configuration;
            UserDataStorage = userDataStorage;
        }

        [HttpGet("auth-token")]
        public IActionResult GetAuthenticationToken()
        {
            string authToken = GetAuthorizationBearerToken();
            if (authToken == null)
            {
                return StatusCode((int)HttpStatusCode.BadRequest, "Authentication header was not found.");
            }

            Response.Cookies.Append(
                Constants.AssetsAuthTokenName,
                authToken,
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(1),
                    Path = "/api/assets",
                    Secure = true,
                });
            Response.Cookies.Append(
                Constants.AssetsAuthTokenName,
                authToken,
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(1),
                    Path = "/api/dataset-nominations",
                    Secure = true,
                });
            return Ok();
        }

        [HttpGet("datasetutil")]
        public async Task<IActionResult> DatasetUtil(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var readStream = await UserDataStorage.OpenDatasetUtil(DatasetUtilStorageUrl);
            return File(readStream, "application/octet-stream", "DatasetUtil.msi");
        }

        [HttpGet("dataset-import/{id:guid}")]
        public async Task<IActionResult> DatasetImportProperties([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var importProps = await UserDataStorage.GetDatasetImportPropertiesForNomination(id, cancellationToken);
            if(importProps == null)
            {
                return NotFound();
            }

            var jsonText = JsonConvert.SerializeObject(importProps, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            });
            var contentBytes = Encoding.UTF8.GetBytes(jsonText);
            return File(contentBytes, "application/json", "dataset-upload.json");
        }

        private string GetAuthorizationBearerToken()
        {
            string headerText = Request.Headers["authorization"].FirstOrDefault() ?? string.Empty;
            var values = headerText.Split(' ');
            if (values.Length != 2 || String.Compare(values[0], "bearer", StringComparison.OrdinalIgnoreCase) != 0)
            {
                return null;
            }

            return values[1];
        }
    }
}