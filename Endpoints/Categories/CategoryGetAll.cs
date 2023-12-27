namespace IWantApp.Endpoints.Categories;

public class CategoryGetAll
{
    //Já atribuo um valor ao criar a propriedade
    public static string Template => "/categories";

    //Seto os métodos permitidos para esta endpoint
    public static string[] Methods => new string[] { HttpMethod.Get.ToString() };

    public static Delegate Handle => Action;

    public static IResult Action(ApplicationDbContext context)
    {
        var categories = context.Categories.ToList();

        //Aqui, eu passo como category response para que as informações de log (Edited/Created by e on)
        //não sejam exibidas para os usuários
        var response = categories.Select(c => new CategoryResponse(c.Id, c.Name, c.Active));

        return Results.Ok(response);
    }
}
