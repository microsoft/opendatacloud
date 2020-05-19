using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Msr.Odr.Model.Datasets;

namespace Msr.Odr.Model
{
	public class DatasetSiteMapEntry
    {
        public Guid DatasetId { get; set; }
        public DateTime? Modified { get; set; }
	}
}
