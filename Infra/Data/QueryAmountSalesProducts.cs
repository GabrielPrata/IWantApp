namespace IWantApp.Infra.Data;

public class QueryAmountSalesProducts
{
    private readonly IConfiguration Configuration;

    public QueryAmountSalesProducts(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public async Task<IEnumerable<ProductSoldResponse>> Execute()
    {
        var db = new SqlConnection(Configuration["ConnectionString:IWantDb"]);

        var query =
            @"SELECT
                P.Id,
                P.Name,
                COUNT(*) AS TotalVenda
            FROM
                Products P
            JOIN
                OrderProducts V ON P.Id = V.ProductsId
            GROUP BY
                P.Id, P.Name
            ORDER BY
                TotalVenda DESC
            ";

        return await db.QueryAsync<ProductSoldResponse>(query);
    }
}
