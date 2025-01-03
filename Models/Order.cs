namespace backend.Models
{
    public enum OrderStatus 
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled,
        Refunded,
        Failed,
        OnHold
    }

    public class Order
    {
        public int o_id { get; set; } // o_id

        public int c_id { get; set; } // c_id

        public DateTime o_date { get; set; } // o_date

        public decimal total_amount { get; set; } // total_amount

        public OrderStatus  status { get; set; } = OrderStatus.Pending; // status with default

        public required Customer Customer { get; set; } // `required` modifier

        public required ICollection<OrderDetail> OrderDetails { get; set; } // `required` modifier
    }
}
