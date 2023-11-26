using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using blog_api.Data;
using blog_api.Exception;
using blog_api.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<FiasDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("FiasConnection")));
builder.Services.AddControllers(options => options.Filters.Add<ExceptionHandlingFilter>())
    .AddJsonOptions(jsonOptions => { jsonOptions.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
builder.Services.AddRazorPages();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ICommunityService, CommunityService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IPostService, PostService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Scheme: bearer \\{token\\}",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var dbContext = context.HttpContext.RequestServices.GetRequiredService<BlogDbContext>();
                var guidString = context.Principal?.Claims
                    .FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;
                if (guidString == null)
                {
                    context.Fail("Unauthorized");
                    return;
                }

                var minimalIssuedTime = (await
                        dbContext.TokenValidation.FindAsync(Guid.Parse(guidString)))
                    ?.MinimalIssuedTime;
                if (minimalIssuedTime == null)
                {
                    return;
                }

                var tokenIssuedTime = context.Properties.IssuedUtc?.DateTime;
                if (tokenIssuedTime == null || tokenIssuedTime < minimalIssuedTime)
                {
                    context.Fail("Unauthorized");
                }
            }
        };
    });

var app = builder.Build();

app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapControllers();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();