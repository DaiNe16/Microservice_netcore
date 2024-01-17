using AutoMapper;
using Mango.Services.ShoppingCartAPI;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Extension;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Mango.Services.ShoppingCartAPI.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Mango.Services.ShoppingCartAPI.Utility;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.RabbitMQSender;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Register connection_string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'MovieDBContext' not found.")));

//Config stripe
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));

//Register Mapper
IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<BackendApiAuthenticationHttpClientHandler>();

//Config Product Client
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("Product", u => u.BaseAddress =
new Uri(builder.Configuration["ApiSettings:ServiceUrls:ProductAPI"])).AddHttpMessageHandler<BackendApiAuthenticationHttpClientHandler>();

//Config Coupon Client
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddHttpClient("Coupon", u => u.BaseAddress =
new Uri(builder.Configuration["ApiSettings:ServiceUrls:CouponAPI"])).AddHttpMessageHandler<BackendApiAuthenticationHttpClientHandler>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSingleton<IRabbitMQCartMessageSender, RabbitMQCartMessageSender>();


builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference= new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            }, new string[]{}
        }
    });
});

builder.AddAppAuthetication();

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

ApplyMigration();

app.Run();

void ApplyMigration()
{
    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (_db.Database.GetPendingMigrations().Count() > 0)
        {
            _db.Database.Migrate();
        }
    }
}