﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVCConfigurator.Domain.Models
{
    public class Order : EntityBase
    {
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public decimal Price { get; set; }
        public DateTime DeliveryDate { get; set; }
        public bool IsReady { get; set; }
        public User User { get; set; }

    }
}
