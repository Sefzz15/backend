namespace backend.Models
{
    public class OrderDetail
    {
        public int o_details_id { get; set; } 

        public int o_id { get; set; } 

        public int p_id { get; set; } 

        public int quantity { get; set; } 

        public decimal price { get; set; } 

        // Navigation Properties
        public required Order Order { get; set; } 

        public required Product Product { get; set; }
    }
}
