using IWantApp.Domain.Users;

namespace IWantApp.Endpoints.Clients;

public class ClientGet
{
    public static string Template => "/clients";
    public static string[] Methods => new string[] { HttpMethod.Get.ToString() };
    public static Delegate Handle => Action;

    public static async Task<IResult> Action(HttpContext http)
    {   
        var user = http.User;

        var result = new
        {
            Id = user.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value,
            Name = user.Claims.First(c => c.Type == "Name").Value,
            Cpf = user.Claims.FirstOrDefault(c => c.Type == "Cpf")?.Value,
        };

        if (result.Cpf.IsNullOrEmpty())
        {
            return Results.Problem(title: "CPF não encontrado. Verifique se este usuário é um cliente.", statusCode: 400);
        }

        return Results.Ok(result);
    }
}
