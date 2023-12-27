namespace IWantApp.Endpoints.Security;

public class TokenPost
{
    //Já atribuo um valor ao criar a propriedade
    public static string Template => "/token";

    //Seto os métodos permitidos para esta endpoint
    public static string[] Methods => new string[] { HttpMethod.Post.ToString() };

    public static Delegate Handle => Action;

    [AllowAnonymous]
    //UserManager gerencia IdentityUser
    public async static Task<IResult> Action(
        LoginRequest loginRequest,
        IConfiguration configuration,
        UserManager<IdentityUser> userManager,
        ILogger<TokenPost> log,
        IWebHostEnvironment environment
        )
    {
        log.LogInformation("Getting token");
        log.LogWarning("Warning");
        log.LogError("Error");
        log.LogCritical("Critical");
        log.LogDebug("Debug");
        log.LogTrace("Trace");

        var user = await userManager.FindByEmailAsync(loginRequest.Email);

        if (user == null)
            return Results.BadRequest();

        //Por ser um método assíncrono, utilizo o .Result
        var validation = await userManager.CheckPasswordAsync(user, loginRequest.Password);
        if (!validation)
            return Results.BadRequest();

        var claims = await userManager.GetClaimsAsync(user);

        //Monto meu Token
        var subject = new ClaimsIdentity(new Claim[]
        {
                //Primeira coisa que passo no token é o email, que é possível ser lido por quem receber este token
                new Claim(ClaimTypes.Email, loginRequest.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
        });

        subject.AddClaims(claims);

        var key = Encoding.ASCII.GetBytes(configuration["JwtBearerTokenSettings:SecretKey"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {

            Subject = subject,
            SigningCredentials =
            new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Audience = configuration["JwtBearerTokenSettings:Audience"],
            Issuer = configuration["JwtBearerTokenSettings:Issuer"],
            //Validação de ambiente para gerar o token
            Expires = environment.IsDevelopment() || environment.IsStaging() ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow.AddMinutes(3),
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return Results.Ok(new
        {
            token = tokenHandler.WriteToken(token)
        });
    }
}
