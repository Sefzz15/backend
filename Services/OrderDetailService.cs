public class OrderDetailService
{
    private readonly AppDbContext _context;

    public OrderDetailService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderDetail>> CreateOrderDetailsAsync(Order order)
    {
        if (order == null)
        {
            throw new ArgumentException("Η παραγγελία είναι null.");
        }

        if (order.OrderDetails == null || !order.OrderDetails.Any())
        {
            throw new ArgumentException("Η παραγγελία πρέπει να περιέχει τουλάχιστον ένα προϊόν.");
        }

        var orderDetails = new List<OrderDetail>();

        foreach (var detail in order.OrderDetails)
        {
            var product = await _context.Products.FindAsync(detail.ProductId);
            if (product == null)
            {
                throw new ArgumentException($"Το προϊόν με ID {detail.ProductId} δεν βρέθηκε.");
            }

            if (product.Stock < detail.Quantity)
            {
                throw new ArgumentException($"Δεν υπάρχει αρκετό απόθεμα για το προϊόν {product.Pname}.");
            }

            // Αφαίρεση αποθέματος
            product.Stock -= detail.Quantity;

            // Δημιουργία του OrderDetail προς αποθήκευση
            var newOrderDetail = new OrderDetail
            {
                ProductId = detail.ProductId,
                Quantity = detail.Quantity,
                Oid = order.Oid // ορίζεται αργότερα στον controller
            };

            orderDetails.Add(newOrderDetail);
        }

        return orderDetails;
    }

}