using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IWantApp.Endpoints.Employees;

public class EmployeePost
{
    //Já atribuo um valor ao criar a propriedade
    public static string Template => "/employees";

    //Seto os métodos permitidos para esta endpoint
    public static string[] Methods => new string[] { HttpMethod.Post.ToString() };

    public static Delegate Handle => Action;

    //UserManager gerencia IdentityUser
    public static IResult Action(EmployeeRequest employeeRequest, HttpContext http, UserManager<IdentityUser> userManager)
    {
        var newUser = new IdentityUser { UserName = employeeRequest.Email, Email = employeeRequest.Email };
        var userId = http.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

        //Passo o objeto do usuário e seu password
        var result = userManager.CreateAsync(newUser, employeeRequest.Password).Result;

        if (!result.Succeeded)
        {
            return Results.ValidationProblem(result.Errors.ConvertToProblemDetails());
        }

        var userClaims = new List<Claim> {
            new Claim("EmployeeCode", employeeRequest.EmployeeCode),
            new Claim("Name", employeeRequest.Name),
            new Claim("CreatedBy", userId)
        };

        var claimResult = userManager.AddClaimsAsync(newUser, userClaims).Result;

        if (!claimResult.Succeeded)
        {
            return Results.BadRequest(result.Errors.First());

        }

        return Results.Created($"/employees/{newUser.Id}", newUser.Id);
    }
}
