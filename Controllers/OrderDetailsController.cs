using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Services;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailsController : ControllerBase
    {
        private readonly IOrderDetailsService _orderDetailsService;

        public OrderDetailsController(IOrderDetailsService orderDetailsService)
        {
            _orderDetailsService = orderDetailsService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetOrderDetails()
        {
            var orderDetails = await _orderDetailsService.GetOrderDetailsAsync();
            return Ok(orderDetails);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDetail>> GetOrderDetail(int id)
        {
            var orderDetail = await _orderDetailsService.GetOrderDetailByIdAsync(id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return Ok(orderDetail);
        }

        [HttpPost]
        public async Task<ActionResult<OrderDetail>> PostOrderDetail(OrderDetail orderDetail)
        {
            var createdOrderDetail = await _orderDetailsService.AddOrderDetailAsync(orderDetail);
            return CreatedAtAction(nameof(GetOrderDetail), new { id = createdOrderDetail.o_details_id }, createdOrderDetail);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrderDetail(int id, OrderDetail orderDetail)
        {
            var success = await _orderDetailsService.UpdateOrderDetailAsync(id, orderDetail);
            if (!success)
            {
                return BadRequest();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderDetail(int id)
        {
            var success = await _orderDetailsService.DeleteOrderDetailAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
