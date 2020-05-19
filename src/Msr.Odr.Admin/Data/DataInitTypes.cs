using System;
using System.Collections.Generic;
using System.Text;

namespace Msr.Odr.Admin.Data
{
    [Flags]
    public enum DataInitTypes
    {
        None = 0x00,
        Domains = 0x01,
        Licenses = 0x02,
        FAQs = 0x04,
        Email = 0x08,
        ARM = 0x10,
        DatasetOwners = 0x20
    }
}
