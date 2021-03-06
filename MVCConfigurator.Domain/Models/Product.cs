﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCConfigurator.Domain.Models
{
    public class Product : EntityBase
    {
        public string Name { get; set; }
        public string ProductCode { get; set; }
        public string ImagePath { get; set; }
        public virtual IList<Part> Parts { get; set; }
        public virtual ProductCategory Category { get; set; }
    }
}
