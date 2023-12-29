var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSqlServer<ApplicationDbContext>(builder.Configuration["ConnectionString:IWantDb"]);

//Configuração do Log no banco de dados

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
    .WriteTo.Console()
    .WriteTo.MSSqlServer(
        context.Configuration["ConnectionString:IWantDb"],
        sinkOptions: new MSSqlServerSinkOptions()
        {
            AutoCreateSqlDatabase = true,
            TableName = "LogAPI",
        });
});

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

    options.AddPolicy("CpfPolicy", p =>
       p.RequireAuthenticatedUser().RequireClaim("Cpf")
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
builder.Services.AddScoped<QueryAmountSalesProducts>();
builder.Services.AddScoped<UserCreator>();

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
app.MapMethods(ProductsPost.Template, ProductsPost.Methods, ProductsPost.Handle);
app.MapMethods(EmployeeGetAll.Template, EmployeeGetAll.Methods, EmployeeGetAll.Handle);
app.MapMethods(TokenPost.Template, TokenPost.Methods, TokenPost.Handle);
app.MapMethods(ProductPost.Template, ProductPost.Methods, ProductPost.Handle);
app.MapMethods(ProductGetAll.Template, ProductGetAll.Methods, ProductGetAll.Handle);
app.MapMethods(ProductGetTotalSold.Template, ProductGetTotalSold.Methods, ProductGetTotalSold.Handle);
app.MapMethods(ProductGetShowCase.Template, ProductGetShowCase.Methods, ProductGetShowCase.Handle);
app.MapMethods(ClientPost.Template, ClientPost.Methods, ClientPost.Handle);
app.MapMethods(ClientGet.Template, ClientGet.Methods, ClientGet.Handle);
app.MapMethods(OrderPost.Template, OrderPost.Methods, OrderPost.Handle);
app.MapMethods(OrderGet.Template, OrderGet.Methods, OrderGet.Handle);

app.UseExceptionHandler("/error");
app.Map("/error", (HttpContext http) =>
{
    //Pego qual foi o erro que aconteceu
    var error = http.Features?.Get<IExceptionHandlerFeature>().Error;

    if (error != null)
    {
        if(error is SqlException)
        {
            return Results.Problem(title: "An error ocurred in the system database", statusCode: 500);
        }
        else if (error is BadHttpRequestException)
        {
            return Results.Problem(title: "Error to convert data to other type. See all the information sent", statusCode: 500);
        }
    }

    return Results.Problem(title: "An error ocurred", statusCode: 500);
});

app.Run();