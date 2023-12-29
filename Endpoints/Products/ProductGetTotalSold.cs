namespace IWantApp.Endpoints.Products;

public class ProductGetTotalSold
{
    public static string Template => "/products/totalsold";
    public static string[] Methods => new string[] { HttpMethod.Get.ToString() };
    public static Delegate Handle => Action;

    [Authorize(Policy = "EmployeePolicy")]
    public static async Task<IResult> Action(QueryAmountSalesProducts query)
    {
        var result = await query.Execute();
        return Results.Ok(result);
    }
}