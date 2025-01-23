using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Order
    {
        [Key]
        public int oid { get; set; }
        public int cid { get; set; }

        [ForeignKey(nameof(cid))]
        public Customer Customer { get; set; } // Navigation property

        public int pid { get; set; }

        [ForeignKey(nameof(pid))]
        public Product Product { get; set; } // Navigation property

        public DateTime o_date { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int quantity { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal price { get; set; }
    }
}