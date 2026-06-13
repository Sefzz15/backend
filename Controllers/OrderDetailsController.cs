using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/orderdetails")]
public class OrderDetailsController(OrderDetailService orderDetailService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllOrderDetails()
    {
        IEnumerable<OrderDetail> orderDetails = await orderDetailService.GetAllOrderDetails();
        return Ok(orderDetails);
    }

    [HttpGet("formatted")]
    public async Task<IActionResult> GetAllOrderDetailsFormatted()
    {
        IEnumerable<OrderDetail> orderDetails = await orderDetailService.GetAllOrderDetails();

        var result = orderDetails.Select(od => new
        {
            oid = od.Oid,
            date = od.Order!.Date.ToString("d/M/yyyy HH:mm:ss"),
            productName = od.Product!.Pname,
            quantity = od.Quantity,
            price = od.Product!.Price,
            order = new
            {
                user = new
                {
                    uname = od.Order!.User!.Uname
                }
            }
        });
        return Ok(result);
    }
}