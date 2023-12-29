namespace IWantApp.Endpoints.Orders;

public class OrderPost
{
    public static string Template => "/orders";
    public static string[] Methods => new string[] { HttpMethod.Post.ToString() };
    public static Delegate Handle => Action;

    [Authorize(Policy = "CpfPolicy")]
   public static async Task<IResult> Action(OrderRequest orderRequest, HttpContext http, ApplicationDbContext context)
    {
        var clientId = http.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var clientName = http.User.Claims.First(c => c.Type == "Name").Value;

        var products = new List<Product>();

        //Uma forma de fazer as validações. Não é a melhor opção pois a classe de pedidos já possui as suas validações
        //if (orderRequest.ProductIds == null || !orderRequest.ProductIds.Any())
        //    return Results.Problem(title: "Produto obrigatório para pedido.", statusCode: 400);

        //if(string.IsNullOrEmpty(orderRequest.DeliveryAddress))
        //    return Results.Problem(title: "Endereço de entrega obrigatório", statusCode: 400);
        List<Product> productsFound = null;
        if(orderRequest.ProductIds != null || orderRequest.ProductIds.Any())
            productsFound = context.Products.Where(p => orderRequest.ProductIds.Contains(p.Id)).ToList();
        
        var order = new Order(clientId, clientName, productsFound, orderRequest.DeliveryAddress);
        if (!order.IsValid)
        {
            return Results.ValidationProblem(order.Notifications.ConvertToProblemDetails());
        }


        //O foreach abaixo não é recomendada pois a cada produto eu realizo uma consulta no banco de dados
        //foreach (var item in orderRequest.ProductIds)
        //{

        //    var product = context.Products.First(p => p.Id == item);
        //    products.Add(product);
        //}
        //Para resolver este problema, faço da seguinte forma



        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();


        return Results.Created($"/orders/{order.Id}", order.Id);
    }
}
