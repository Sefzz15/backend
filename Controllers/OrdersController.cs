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
    public async Task<ActionResult<IEnumerable<object>>> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrders();

        var result = orders.Select(order => new
        {
            order.Oid,
            order.Uid,
            Date = order.Date.ToString("d/M/yyyy HH:mm:ss")
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _orderService.GetOrderById(id);
        if (order == null) return NotFound();

        var result = new
        {
            order.Oid,
            order.Uid,
            Date = order.Date.ToString("d/M/yyyy HH:mm:ss")
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] Order order)
    {
        order.Date = DateTime.Now;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return Ok(order);
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