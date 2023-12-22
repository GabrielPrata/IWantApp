using IWantApp.Endpoints.Categories;
using IWantApp.Endpoints.Employees;
using IWantApp.Endpoints.Security;
using IWantApp.Infra.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration["ConnectionString:IWantDb"]);

//Para diminuir a segurança exigida para a senha
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredLength = 3;

}).AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthorization(options =>
{
    //Configurando a política de segurança default
    options.FallbackPolicy = new AuthorizationPolicyBuilder()

    //Deve ter essa autenticação
    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)

    //Deve ser obrigatório o usuário estar autenticado
    .RequireAuthenticatedUser()

    .Build();

    //Políticas de segurança personalizadas
    options.AddPolicy("EmployeePolicy", p =>
        //O usuário deve estar autenticado
        p.RequireAuthenticatedUser()
        //Para que um usuário possa acessar um endpoint com política de empregado
        //ele deve ter o EmployeeCode
        .RequireClaim("EmployeeCode")
    );

    options.AddPolicy("Employee099Policy", p =>
        //Além de validar que a claim existe, eu valido também o valor do claim
        p.RequireAuthenticatedUser().RequireClaim("EmployeeCode", "099")
    );
});

builder.Services.AddAuthentication(x =>
{
    //Configurações defaults
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    //Digo que estou trabalhando com Jwt
}).AddJwtBearer(options =>
{
    //Essas validações servem para verificar se o token que eu estou recebendo é o token que eu estou esperando
    //A classe TokenValidationParameters é quem faz essa verificação
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateActor = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        //Propiedade para setar quanto tempo irei aceitar um token expirado
        //Por padrão vem setado 5 minutos
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["JwtBearerTokenSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtBearerTokenSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtBearerTokenSettings:SecretKey"]))
    };
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddScoped<QueryAllUsersWithClaimName>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Services é tudo o que está disponível para o ASP.NET usar
//app eu estou o habilitando para o aplicativo executar
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapMethods(CategoryPost.Template, CategoryPost.Methods, CategoryPost.Handle);
app.MapMethods(CategoryGetAll.Template, CategoryGetAll.Methods, CategoryGetAll.Handle);
app.MapMethods(CategoryPut.Template, CategoryPut.Methods, CategoryPut.Handle);
app.MapMethods(EmployeePost.Template, EmployeePost.Methods, EmployeePost.Handle);
app.MapMethods(EmployeeGetAll.Template, EmployeeGetAll.Methods, EmployeeGetAll.Handle);
app.MapMethods(TokenPost.Template, TokenPost.Methods, TokenPost.Handle);



app.Run();