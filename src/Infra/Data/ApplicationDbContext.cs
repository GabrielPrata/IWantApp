using Flunt.Notifications;
using IWantApp.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace IWantApp.Infra.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        //Como a classe Entity herda de notification, algumas regras da classe(Possuir chave primaria, por exemplo)
        //Atrapalham a forma de como a minha aplicação é construída. Por isso eu adiciono o builder.Ignore
        builder.Ignore<Notification>();

        builder.Entity<Product>()
            .Property(p => p.Name).IsRequired();

        //Única propriedade do tipo string que pode ter mais de 100 caracteres
        builder.Entity<Product>()
            .Property(p => p.Description).HasMaxLength(255);

        builder.Entity<Category>()
            .Property(c => c.Name).IsRequired();
    }

    //Configurando convenções do EntityFramework
    protected override void ConfigureConventions(ModelConfigurationBuilder configuration)
    {
        //Toda propriedade do tipo string irá ter no máximo 100 caracteres
        configuration.Properties<string>()
            .HaveMaxLength(100);
    }
}