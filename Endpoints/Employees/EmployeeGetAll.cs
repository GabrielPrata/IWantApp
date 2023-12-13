using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IWantApp.Endpoints.Employees;

public class EmployeeGetAll
{
    //Já atribuo um valor ao criar a propriedade
    public static string Template => "/employees";

    //Seto os métodos permitidos para esta endpoint
    public static string[] Methods => new string[] { HttpMethod.Get.ToString() };

    public static Delegate Handle => Action;

    //UserManager gerencia IdentityUser
    public static IResult Action(UserManager<IdentityUser> userManager)
    {
        var users = userManager.Users.ToList();

        var employees = new List<EmployeeResponse>();
        foreach (var user in users)
        {
            var claims = userManager.GetClaimsAsync(user).Result;
            var claimName = claims.FirstOrDefault(c => c.Type == "Name");
            var userName = claimName != null ? claimName.Value : string.Empty;

            employees.Add(new EmployeeResponse(user.Email, userName));
        }

        return Results.Ok(employees);
    }
}
