﻿using IWantApp.Endpoints.Clients;

namespace IWantApp.Endpoints.Employees;

public class ProductsPost
{
    //Já atribuo um valor ao criar a propriedade
    public static string Template => "/employees";

    //Seto os métodos permitidos para esta endpoint
    public static string[] Methods => new string[] { HttpMethod.Post.ToString() };

    public static Delegate Handle => Action;

    //UserManager gerencia IdentityUser
    //public static async Task<IResult> Action(EmployeeRequest employeeRequest, HttpContext http, UserManager<IdentityUser> userManager)
    public static async Task<IResult> Action(EmployeeRequest employeeRequest, HttpContext http, UserCreator userCreator)
    {
        //var newUser = new IdentityUser { UserName = employeeRequest.Email, Email = employeeRequest.Email };
        var userId = http.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

        ////Passo o objeto do usuário e seu password
        //var result = await userManager.CreateAsync(newUser, employeeRequest.Password);

        //if (!result.Succeeded)
        //{
        //    return Results.ValidationProblem(result.Errors.ConvertToProblemDetails());
        //}

        var userClaims = new List<Claim> {
            new Claim("EmployeeCode", employeeRequest.EmployeeCode),
            new Claim("Name", employeeRequest.Name),
            new Claim("CreatedBy", userId)
        };

        //var claimResult = await userManager.AddClaimsAsync(newUser, userClaims);

        //if (!claimResult.Succeeded)
        //{
        //    return Results.BadRequest(result.Errors.First());

        //}

        (IdentityResult identity, string userId) result = await userCreator.Create(employeeRequest.Email, employeeRequest.Password, userClaims);
        if (!result.identity.Succeeded)
        {
            return Results.BadRequest(result.identity.Errors.First());

        }

        return Results.Created($"/employees/{result.userId}", result.userId);
    }
}