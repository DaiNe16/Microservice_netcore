using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Mango.Services.AuthAPI.Extension
{
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder AddAppAuthetication(this WebApplicationBuilder builder)
        {
            var _configuration = builder.Configuration;
            //.GetSection("ApiSettings");

            //var secret = settingsSection.GetValue<string>("Secret");
            //var issuer = settingsSection.GetValue<string>("Issuer");
            //var audience = settingsSection.GetValue<string>("Audience");

            // Retrieve values from configuration
            string secret = _configuration["ApiSettings:JwtOptions:Secret"];
            string issuer = _configuration["ApiSettings:JwtOptions:Issuer"];
            string audience = _configuration["ApiSettings:JwtOptions:Audience"];

            var key = Encoding.ASCII.GetBytes(secret);


            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    ValidateAudience = true
                };
            });

            return builder;
        }
    }
}
