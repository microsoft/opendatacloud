using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Msr.Odr.Api
{
    /// <summary>
    /// Routing path constants
    /// </summary>
    public static class RouteConstants
    {
        /// <summary>
        /// The base route
        /// </summary>
        public const string BaseRoute = "";

        /// <summary>
        /// The license controller route
        /// </summary>
        public const string LicenseControllerRoute = BaseRoute + "licenses";

        /// <summary>
        /// The file controller route
        /// </summary>
        public const string FileControllerRoute = DatasetControllerRoute + "{datasetid}/files";

        /// <summary>
        /// The dataset controller route
        /// </summary>
        public const string DatasetControllerRoute = BaseRoute + "datasets";

        /// <summary>
        /// The dataset edits controller route
        /// </summary>
        public const string DatasetEditsControllerRoute = BaseRoute + "dataset-edits";

        /// <summary>
        /// The Deploy to Azure controller route
        /// </summary>
        public const string AzureDeployControllerRoute = BaseRoute + "azure-deploy";

        /// <summary>
        /// The dataset nominations controller route
        /// </summary>
        public const string DatasetNominationsControllerRoute = BaseRoute + "dataset-nominations";

        /// <summary>
        /// The dataset issues controller route
        /// </summary>
        public const string DatasetIssuesControllerRoute = BaseRoute + "dataset-issues";

        /// <summary>
        /// The feedback (general issue) controller route
        /// </summary>
        public const string FeedbackControllerRoute = BaseRoute + "feedback";

        /// <summary>
        /// The user data controller route
        /// </summary>
        public const string UserDataControllerRoute = BaseRoute + "user";

        /// <summary>
        /// The tags controller route
        /// </summary>
        public const string TagsControllerRoute = BaseRoute + "tags";

        /// <summary>
        /// The domains controller route
        /// </summary>
        public const string DomainsControllerRoute = BaseRoute + "domains";

        /// <summary>
        /// The FAQs controller route
        /// </summary>
        public const string FAQsControllerRoute = BaseRoute + "faqs";

        /// <summary>
        /// The file types controller route
        /// </summary>
        public const string FileTypesControllerRoute = BaseRoute + "file-types";
	}
}
