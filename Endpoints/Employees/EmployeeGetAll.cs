using IWantApp.Infra.Data;
using Microsoft.AspNetCore.Authorization;

namespace IWantApp.Endpoints.Employees;

public class EmployeeGetAll
{
    //Já atribuo um valor ao criar a propriedade
    public static string Template => "/employees";

    //Seto os métodos permitidos para esta endpoint
    public static string[] Methods => new string[] { HttpMethod.Get.ToString() };

    public static Delegate Handle => Action;

    //O Identity não é muito bom para lidar com uma grande "massa de dados", pois pode gerar problemas de performance
    //Normalmente para necessidades gerais utilo o EF Core. E para necessidades específicas, onde eu preciso de
    //performance utilizo o Dapper. Abaixo, é um caso onde eu necessito de performance

    //Toda vez que no endpoint eu quiser pegar informações do appsettings.json, basta injetar o IConfiguration
    //que o ASP.NET já entende que eu quero o acesso as configurações da minha aplicação
    [Authorize(Policy = "Employee099Policy")]
    public static IResult Action(int? page, int? rows, QueryAllUsersWithClaimName query)
    {
        //Utilizo o .Value pois o valor pode ser nulo, isso evita de dar pau no sistema
        return Results.Ok(query.Execute(page.Value, rows.Value));
    }
}
