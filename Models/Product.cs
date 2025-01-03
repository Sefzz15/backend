namespace backend.Models
{
    public class Product
    {
        public int p_id { get; set; }

        public string p_name { get; set; } = "";

        public string? description { get; set; }

        public decimal price { get; set; }

        public int stock_quantity { get; set; }
    }
}
