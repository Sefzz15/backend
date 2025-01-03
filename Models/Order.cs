using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Order
    {
        public int o_id { get; set; } // o_id

        public int c_id { get; set; } // c_id

        public DateTime o_date { get; set; } // o_date

        public decimal total_amount { get; set; } // total_amount

        public string status { get; set; } = "Pending"; // status

        public required Customer Customer { get; set; } // `required` modifier

        public required ICollection<OrderDetail> OrderDetails { get; set; } // `required` modifier
    }
}
