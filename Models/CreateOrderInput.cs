public class CreateOrderInput
{
    public int Uid { get; set; }
    public List<OrderDetailInput> OrderDetails { get; set; } = new();
}

public class OrderDetailInput
{
    public int Pid { get; set; }
    public int Quantity { get; set; }
}
