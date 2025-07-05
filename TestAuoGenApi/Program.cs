
using Microsoft.Data.SqlClient;
using System.Data;
using TestAutoGen.Dal;

namespace TestAuoGenApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var test = new RegisterRepositories();
            test.Register(builder.Services);
            builder.Services.AddScoped<DalBase>();
            //TODO: Add sql connection string
            builder.Services.AddSingleton<IDbConnection>(_ => new SqlConnection(""));

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

            app.Run();
        }
    }
}
