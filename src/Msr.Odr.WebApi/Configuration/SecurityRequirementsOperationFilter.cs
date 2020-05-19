using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Msr.Odr.Api.Configuration
{
    /// <summary>
    /// Provides security details to swagger
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter" />
    /// <see href="https://github.com/domaindrivendev/Swashbuckle.AspNetCore" />
    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        /// <summary>
        /// The authorization options
        /// </summary>
        private readonly IOptions<AuthorizationOptions> authorizationOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityRequirementsOperationFilter" /> class.
        /// </summary>
        /// <param name="authorizationOptions">The authorization options.</param>
        public SecurityRequirementsOperationFilter(IOptions<AuthorizationOptions> authorizationOptions)
        {
            this.authorizationOptions = authorizationOptions;
        }

        /// <summary>
        /// Applies the specified operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="context">The context.</param>
        public void Apply(Operation operation, OperationFilterContext context)
        {
            //var controllerPolicies = context.ApiDescription.ControllerAttributes()
            //    .OfType<AuthorizeAttribute>()
            //    .Select(attr => attr.Policy);
            var controllerPolicies = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Select(attr => attr.Policy);
            //var actionPolicies = context.ApiDescription.ActionAttributes()
            //    .OfType<AuthorizeAttribute>()
            //    .Select(attr => attr.Policy);
            var actionPolicies = context.MethodInfo.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>()
                .Select(attr => attr.Policy);

            var filters = context.ApiDescription.ActionDescriptor.FilterDescriptors
                .Select(t => t.Filter)
                .OfType<Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter>();
            var filterPolicies = filters.Select(f => f.Policy)
				.Where(t => t != null)
				.SelectMany(t => t.Requirements)
                .Where(t => t != null)
                .Select(t => t.ToString());
			
			if (filters.Count() > 0)
            {
                if (!operation.Responses.ContainsKey("401"))
                {
                    operation.Responses.Add("401", new Response { Description = "Unauthorized. The user must authenticate." });
                }

                if (!operation.Responses.ContainsKey("403"))
                {
                    operation.Responses.Add("403", new Response { Description = "Forbidden. The user is not allowed" });
                }

                if (filterPolicies.Any())
                {
                    operation.Security = new List<IDictionary<string, IEnumerable<string>>>();
                    operation.Security.Add(
                        new Dictionary<string, IEnumerable<string>>
                        {
							{ "oauth2", filterPolicies.ToList() }
                        });
                }

                return;
            }

            var policies = controllerPolicies.Union(actionPolicies).Distinct().Where(t => t != null);
            if (policies != null && policies.Count() > 0)
            {
                var requiredClaimTypes = policies
                    .Select(x => this.authorizationOptions.Value.GetPolicy(x))
                    .SelectMany(x => x.Requirements)
                    .OfType<ClaimsAuthorizationRequirement>()
                    .Select(x => x.ClaimType);

                if (requiredClaimTypes.Any())
                {
                    operation.Responses.Add("401", new Response { Description = "Unauthorized" });
                    operation.Responses.Add("403", new Response { Description = "Forbidden" });

                    operation.Security = new List<IDictionary<string, IEnumerable<string>>>();
                    operation.Security.Add(
                        new Dictionary<string, IEnumerable<string>>
                        {
							{ "oauth2", requiredClaimTypes }
                        });
                }
            }
        }
    }
}
