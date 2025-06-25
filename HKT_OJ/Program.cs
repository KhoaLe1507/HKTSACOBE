using HKT_OJ.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HKT_OJ.Services;


namespace HKT_OJ
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Thêm CORS service
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp",
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:3000") // địa chỉ FE
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });
            builder.Services.AddControllers();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var jwtConfig = builder.Configuration.GetSection("JwtSettings");
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtConfig["Issuer"],
                        ValidAudience = jwtConfig["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["SecretKey"]))
                    };
                });

            builder.Services.AddAuthorization();


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "HKT-OJ API",
                    Version = "v1"
                });
            });
            builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddSingleton<Judge0Service>();
            builder.Services.AddScoped<GeminiService>();

            var app = builder.Build();

            // Tạm thời bật Swagger ở mọi môi trường, không chỉ Development
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "HKT-OJ API v1");
                c.RoutePrefix = string.Empty;
            });
            app.UseCors("AllowReactApp");
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
