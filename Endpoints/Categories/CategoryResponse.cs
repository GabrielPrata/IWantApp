namespace IWantApp.Endpoints.Categories;

//Record é uma classe que não vai ser alterada após sua instanciação
public record CategoryResponse(Guid Id, string Name, bool Active);