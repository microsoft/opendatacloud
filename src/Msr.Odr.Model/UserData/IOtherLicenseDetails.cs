using System;
using System.Collections.Generic;
using System.Text;

namespace Msr.Odr.Model.UserData
{
    public interface IOtherLicenseDetails
    {
        string OtherLicenseContentHtml { get; }
        string OtherLicenseFileContent { get; }
        string OtherLicenseFileContentType { get; }
        string OtherLicenseFileName { get; }
        string OtherLicenseAdditionalInfoUrl { get; }
        string OtherLicenseName { get; }
        NominationLicenseType NominationLicenseType { get; }
    }
}
