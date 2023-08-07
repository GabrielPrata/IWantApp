namespace IWantApp.Domain.Products;

public class Product : Entity
{
    //O Guid gera uma espécie de hash para os Id's e isso torna a aplicação mais segura porém menos performática
    public string Name { get; set; }
    public Category Category { get; set; }
    public string Description { get; set; }
    public bool HasStock { get; set; }
}
