using backend.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/orderdetails")]
public class OrderDetailsController : ControllerBase
{
    private readonly OrderDetailService _orderDetailService;

    public OrderDetailsController(OrderDetailService orderDetailService)
    {
        _orderDetailService = orderDetailService;

    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrderDetails()
    {
        var orderDetails = await _orderDetailService.GetAllOrderDetails();
        return Ok(orderDetails);
    }

}