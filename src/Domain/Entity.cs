namespace IWantApp.Domain;

public abstract class Entity
{
    //Ao Instanciar um objeto com esta categoria o Id já é gerado automaticamente
    public Entity()
    {
        Id = Guid.NewGuid();
    }

    public Guid Id { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string EditedBy { get; set; }
    public DateTime EditedOn { get; set; }
}
