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

    [HttpGet("formatted")]
    public async Task<IActionResult> GetAllOrderDetailsFormatted()
    {
        var orderDetails = await _orderDetailService.GetAllOrderDetails();

        var result = orderDetails.Select(od => new
        {
            oid = od.Oid,
            date = od.Order.Date.ToString("d/M/yyyy HH:mm:ss"),
            productName = od.Product.Pname,
            quantity = od.Quantity,
            price = od.Product.Price,
            order = new
            {
                user = new
                {
                    uname = od.Order.User.Uname
                }
            }
        });
        return Ok(result);
    }
}