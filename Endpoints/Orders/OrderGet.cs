namespace IWantApp.Endpoints.Orders;

public class OrderGet
{
    public static string Template => "/orders";
    public static string[] Methods => new string[] { HttpMethod.Get.ToString() };
    public static Delegate Handle => Action;

    public static async Task<IResult> Action(ApplicationDbContext context, HttpContext http, string userId)
    {
        var userClaims = http.User.Claims;
        var loggedUserId = userClaims.First(u => u.Type == ClaimTypes.NameIdentifier).Value;

        if (userId != loggedUserId && !userClaims.Any(u=> u.Type == "EmployeeCode"))
            return Results.Problem(title: "Você não possui permissão para vizualizar essas informações.", statusCode: 403);

        var orders = context.Orders
            .Where(o => o.ClientId == userId)
            .Include(o => o.Products)
            .Select(o => new
            {
                o.Id,
                o.Total,
                o.DeliveryAddress,
                Products = o.Products.Select(p => new OrderProduct(p.Id, p.Name, p.Price, p.Description, p.Category.Name)).ToList()

            }
            )
            .AsSingleQuery().ToList();

        var results = orders.Select(o => new OrderResponse(o.Id, o.Total, o.DeliveryAddress, o.Products));
        
        return Results.Ok(results);
    }
}
