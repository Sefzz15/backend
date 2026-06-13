using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace backend.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController(OrderService orderService, AppDbContext context, OrderDetailService orderDetailService)
    : ControllerBase
{
    private readonly OrderDetailService _orderDetailService = orderDetailService;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAllOrders()
    {
        IEnumerable<Order> orders = await orderService.GetAllOrders();

        var result = orders.Select(order => new
        {
            order.Oid,
            order.Uid,
            Date = order.Date.ToString("d/M/yyyy HH:mm:ss")
        });

        return Ok(result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderInput input)
    {
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // Step 1: Check Stock
            List<int> productIds = input.OrderDetails.Select(od => od.Pid).ToList();
            Dictionary<int, Product> products = await context.Products
                .Where(p => productIds.Contains(p.Pid))
                .ToDictionaryAsync(p => p.Pid);

            foreach (OrderDetailInput item in input.OrderDetails)
            {
                if (!products.ContainsKey(item.Pid))
                    return BadRequest(new { message = $"Product with ID {item.Pid} does not exist." });

                if (products[item.Pid].Stock < item.Quantity)
                    return BadRequest(new { message = $"Not enough stock for product {products[item.Pid].Pname}." });
            }

            // Step 2: Create Order
            Order order = new Order
            {
                Uid = input.Uid,
                Date = DateTime.Now
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            // Step 3: Create OrderDetails and Update Stock
            foreach (OrderDetailInput item in input.OrderDetails)
            {
                context.OrderDetails.Add(new OrderDetail
                {
                    Oid = order.Oid,
                    Pid = item.Pid,
                    Quantity = item.Quantity
                });

                products[item.Pid].Stock -= item.Quantity;
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(order);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        Order? order = await orderService.GetOrderById(id);
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

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        return Ok(order);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] Order order)
    {
        if (id != order.Oid) return BadRequest();
        await orderService.UpdateOrder(order);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        await orderService.DeleteOrder(id);
        return NoContent();
    }
}