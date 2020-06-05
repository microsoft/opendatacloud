// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Rewrite;
using Microsoft.Net.Http.Headers;
using System;
using System.Net;

namespace Msr.Odr.Web
{
    public class DomainRewriteRule : IRule
    {
        private string CanonicalDomain { get; }

        public DomainRewriteRule(string canonicalDomain)
        {
            CanonicalDomain = canonicalDomain;
        }

        public void ApplyRule(RewriteContext context)
        {
            var request = context.HttpContext.Request;
            var host = request.Host;
            if (host.Host.Equals(CanonicalDomain, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = RuleResult.ContinueRules;
                return;
            }

            string newPath = $"https://{CanonicalDomain}{request.PathBase}{request.Path}{request.QueryString}";
            var response = context.HttpContext.Response;
            response.StatusCode = (int)HttpStatusCode.Found;
            response.Headers[HeaderNames.Location] = newPath;
            context.Result = RuleResult.EndResponse;
        }
    }
}
