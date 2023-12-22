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

//Para diminuir a seguran�a exigida para a senha
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
    //Configurando a pol�tica de seguran�a default
    options.FallbackPolicy = new AuthorizationPolicyBuilder()

    //Deve ter essa autentica��o
    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)

    //Deve ser obrigat�rio o usu�rio estar autenticado
    .RequireAuthenticatedUser()

    .Build();

    //Pol�ticas de seguran�a personalizadas
    options.AddPolicy("EmployeePolicy", p =>
        //O usu�rio deve estar autenticado
        p.RequireAuthenticatedUser()
        //Para que um usu�rio possa acessar um endpoint com pol�tica de empregado
        //ele deve ter o EmployeeCode
        .RequireClaim("EmployeeCode")
    );

    options.AddPolicy("Employee099Policy", p =>
        //Al�m de validar que a claim existe, eu valido tamb�m o valor do claim
        p.RequireAuthenticatedUser().RequireClaim("EmployeeCode", "099")
    );
});

builder.Services.AddAuthentication(x =>
{
    //Configura��es defaults
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    //Digo que estou trabalhando com Jwt
}).AddJwtBearer(options =>
{
    //Essas valida��es servem para verificar se o token que eu estou recebendo � o token que eu estou esperando
    //A classe TokenValidationParameters � quem faz essa verifica��o
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateActor = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        //Propiedade para setar quanto tempo irei aceitar um token expirado
        //Por padr�o vem setado 5 minutos
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

//Services � tudo o que est� dispon�vel para o ASP.NET usar
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