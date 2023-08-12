using IWantApp.Domain.Products;
using IWantApp.Infra.Data;

namespace IWantApp.Endpoints.Categories;

public class CategoryPost
{
    //Já atribuo um valor ao criar a propriedade
    public static string Template => "/categories";

    //Seto os métodos permitidos para esta endpoint
    public static string[] Methods => new string[] { HttpMethod.Post.ToString() };

    public static Delegate Handle => Action;

    public static IResult Action(CategoryRequest categoryRequest, ApplicationDbContext context)
    {
        var category = new Category(categoryRequest.Name, "Test", "Test");

        //IsValid é herdado de Notifiable<Notification>
        if(!category.IsValid)
        {
            //Primeiro eu agrupo todas as keys. As keys são os nomes dos campos (da resposta de erro da requisição)
            //Para transforar para dictionary, eu preciso passar a key e dentro da key qual é a listagem
            //Por isso utilizo o .Select(x => x.Message), pois assim retorno apenas o texto da mensagem
            //Em seguida transformo em um array, pois o ValidationProblem() solicita que seja um dictionary e um array de string
            //O codigo abaixo está comentado, pois foi implementado em ProblemDetailsExtensions()
            //var errors = category.Notifications.GroupBy(g => g.Key).ToDictionary(g => g.Key, g => g.Select(x => x.Message).ToArray());
            
            //ConvertToProblemDetails é uma extensão do método Notifications
            return Results.ValidationProblem(category.Notifications.ConvertToProblemDetails());
        }

        context.Categories.Add(category);
        context.SaveChanges();

        return Results.Created($"/categories/{category.Id}", category.Id);
    }
}
