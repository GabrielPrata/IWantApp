namespace IWantApp.Endpoints.Products;

public class ProductGetShowCase
{
    public static string Template => "/products/showcase";
    public static string[] Methods => new string[] { HttpMethod.Get.ToString() };
    public static Delegate Handle => Action;

    [AllowAnonymous]
    public static async Task<IResult> Action(ApplicationDbContext context, int page = 1, int row = 10, string orderBy = "name")
    {
        if (row > 10)
            return Results.Problem(title: "Row with max 10", statusCode: 400);

        //A consulta só vai ser enviada para o banco de dados quando eu der um ToList()
        //AsNoTracking() indica que quando o objeto for consultado, ele não será rastreado na memória
        //pois assim nada é setado na memória, tornando o programa mais performático
        //Sempre que for realizar apenas uma consulta, sem alterar nenhum dado e etc, devo utilizar o AsNoTracking()
        var queryBase = context.Products.AsNoTracking().Include(p => p.Category)
            .Where(p => p.HasStock && p.Category.Active);

        if (orderBy == "name")
            queryBase = queryBase.OrderBy(p => p.Name);

        else if (orderBy == "price")
            queryBase = queryBase.OrderBy(p => p.Price);

        else
            return Results.Problem(title: "Order only by price or name", statusCode: 400);

        //Paginação
        var queryFilter = queryBase.Skip((page - 1) * row).Take(row);

        var products = queryFilter.ToList();


        var results = products.Select(p => new ProductResponse(p.Name, p.Category.Name, p.Description, p.HasStock, p.Active, p.Price));
        return Results.Ok(results);
    }
}