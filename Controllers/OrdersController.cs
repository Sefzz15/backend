using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly OrderDetailService _orderDetailService;
    private readonly AppDbContext _context;

    public OrderController(OrderService orderService, AppDbContext context, OrderDetailService orderDetailService)

    {
        _orderService = orderService;
        _orderDetailService = orderDetailService;
        _context = context;

    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrders();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _orderService.GetOrderById(id);
        if (order == null) return NotFound();
        return Ok(order);
    }

[HttpPost]
public async Task<IActionResult> CreateOrder([FromBody] Order order)
{
    // Check if order and order items are valid (uncomment validation if needed)
    // if (order == null  order.OrderItems == null  !order.OrderItems.Any())
    // {
    //     return BadRequest("Invalid order request.");
    // }

    order.Date = DateTime.UtcNow; // Set order date

    try
    {
        // Set OrderId for each OrderDetail and handle stock check and creation
        var orderDetails = await _orderDetailService.CreateOrderDetailsAsync(order);

        // Set the OrderId for each OrderDetail to the Oid of the Order
        foreach (var orderDetail in orderDetails)
        {
            orderDetail.Oid = order.Oid;  // Ensure OrderId is set to the Order's Oid
        }

        // Add the order to the context
        _context.Orders.Add(order);

        // Add the order items to the context
        _context.OrderDetails.AddRange(orderDetails);

        // Save all changes to the database
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Order created successfully!", OrderId = order.Oid });
    }
    catch (ArgumentException ex)
    {
        return BadRequest(ex.Message);  // Handle validation failures
    }
}

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order order)
    {
        if (id != order.Oid) return BadRequest();
        await _orderService.UpdateOrder(order);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        await _orderService.DeleteOrder(id);
        return NoContent();
    }
}