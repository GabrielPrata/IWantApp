namespace IWantApp.Infra.Data;

public class QueryAllUsersWithClaimName
{
    private readonly IConfiguration Configuration;

    public QueryAllUsersWithClaimName(IConfiguration configuration)
    {
            Configuration = configuration;
    }

    public async Task<IEnumerable<EmployeeResponse>> Execute(int page, int rows)
    {
        var db = new SqlConnection(Configuration["ConnectionString:IWantDb"]);

        //O Dapper vai converter as colunas resultantes da consulta abaixo na classe "EmployeeResponse"
        //Faço a paginação pelo próprio SQL
        var query =
            @"SELECT Email, CLAIMVALUE AS Name
            FROM AspNetUsers U INNER JOIN AspNetUserClaims C
            ON U.ID = C.UserId AND ClaimType = 'Name'
            ORDER BY Name
            OFFSET (@page - 1) * @rows ROWS FETCH NEXT @rows ROWS ONLY";

        //Crio um objeto sem nome
        return await db.QueryAsync<EmployeeResponse>(
           query, new { page, rows }
        );
    }
}
