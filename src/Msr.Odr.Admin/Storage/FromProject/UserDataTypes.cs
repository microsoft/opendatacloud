using System.Runtime.Serialization;

namespace Msr.Odr.Model.UserData
{
    /// <summary>
    /// The datatype for the stored user data documents.
    /// </summary>
    public enum UserDataTypes
    {
        /// <summary>
        /// Unknown or unspecified
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Dataset nomination document.
        /// </summary>
        [EnumMember(Value = "dataset-nomination")]
        DatasetNomination = 1,

        /// <summary>
        /// Dataset issue document.
        /// </summary>
        [EnumMember(Value = "dataset-issue")]
        DatasetIssue = 2,

        /// <summary>
        /// User has accepted specific dataset license.
        /// </summary>
        [EnumMember(Value = "accepted-license")]
        AcceptedLicense = 3,

        /// <summary>
        /// User is deploying a dataset to Azure
        /// </summary>
        [EnumMember(Value = "deployment")]
        Deployment = 4,
    }
}