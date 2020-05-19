using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Msr.Odr.WebApi.ViewModels
{
    public class CurrentUserDetails
    {
        public bool IsAuthenticated { get; set; }
        public bool CanNominateDataset { get; set; }
    }
}
