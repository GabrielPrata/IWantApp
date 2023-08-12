using IWantApp.Domain.Products;
using IWantApp.Infra.Data;
using Microsoft.AspNetCore.Mvc;

namespace IWantApp.Endpoints.Categories;

public class CategoryPut
{
    //Já atribuo um valor ao criar a propriedade
    //id:guid = defino que o parâmetro esperado é do tipo guid
    public static string Template => "/categories/{id:guid}";

    //Seto os métodos permitidos para esta endpoint
    public static string[] Methods => new string[] { HttpMethod.Put.ToString() };

    public static Delegate Handle => Action;

    public static IResult Action([FromRoute] Guid id, CategoryRequest categoryRequest, ApplicationDbContext context)
    {
        var category = context.Categories.Where(c => c.Id == id).FirstOrDefault();

        //O LINQ não encontrou nenhuma categoria com o ID passado, portanto o valor de category sera null
        if (category == null)
        {
            return Results.NotFound();
        }

        category.EditInfo(categoryRequest.Name, categoryRequest.Active);

        if (!category.IsValid)
        {
            return Results.ValidationProblem(category.Notifications.ConvertToProblemDetails());
        }

        context.SaveChanges();

        return Results.Ok();
    }
}
