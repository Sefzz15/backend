namespace backend.Models
{
    public class OrderDetail
    {
        public int o_details_id { get; set; } // o_details_id

        public int o_id { get; set; } // o_id

        public int p_id { get; set; } // p_id

        public int quantity { get; set; } // quantity

        public decimal price { get; set; } // price

        // Navigation Properties
        public required Order Order { get; set; } // `required` modifier

        public required Product Product { get; set; } // `required` modifier
    }
}
