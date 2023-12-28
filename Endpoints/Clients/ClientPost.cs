using IWantApp.Domain.Users;

namespace IWantApp.Endpoints.Clients;

public class ClientPost
{
    //Já atribuo um valor ao criar a propriedade
    public static string Template => "/clients";

    //Seto os métodos permitidos para esta endpoint
    public static string[] Methods => new string[] { HttpMethod.Post.ToString() };

    public static Delegate Handle => Action;

    //UserManager gerencia IdentityUser
    [AllowAnonymous]
    //public static async Task<IResult> Action(EmployeeRequest employeeRequest, HttpContext http, UserManager<IdentityUser> userManager)
    public static async Task<IResult> Action(ClientRequest clientRequest, UserCreator userCreator)
    {
        //var newUser = new IdentityUser { UserName = clientRequest.Email, Email = clientRequest.Email };

        ////Passo o objeto do usuário e seu password
        //var result = await userManager.CreateAsync(newUser, clientRequest.Password);

        //if (!result.Succeeded)
        //{
        //    return Results.ValidationProblem(result.Errors.ConvertToProblemDetails());
        //}

        var userClaims = new List<Claim> {
            new Claim("Cpf", clientRequest.Cpf),
            new Claim("Name", clientRequest.Name),
        };

        //var claimResult = await userManager.AddClaimsAsync(newUser, userClaims);

        //if (!claimResult.Succeeded)
        //{
        //    return Results.BadRequest(result.Errors.First());

        //}

        (IdentityResult identity, string userId) result = await userCreator.Create(clientRequest.Email, clientRequest.Password, userClaims);
        if (!result.identity.Succeeded)
        {
            return Results.BadRequest(result.identity.Errors.First());

        }

        return Results.Created($"/clients/{result.userId}", result.userId);
    }
}
