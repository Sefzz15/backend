namespace backend.Models
{
    public class Customer
    {
        public int c_id { get; set; }// Primary Key
        public string first_name { get; set; } = "";
        public string last_name { get; set; } = "";
        public string email { get; set; } = "";
        public string? phone { get; set; }
        public string? address { get; set; }
        public string city { get; set; } = "";

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}