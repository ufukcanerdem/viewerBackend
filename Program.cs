
using AutodeskViewerAPI.Services;
using AutodeskViewerAPI.Settings;
using MongoDB.Driver;

namespace AutodeskViewerAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            builder.Services.Configure<MongoDBSettings>(
            builder.Configuration.GetSection("MongoDBSettings"));

            builder.Services.AddSingleton<IMongoClient, MongoClient>(sp =>
                new MongoClient(builder.Configuration.GetValue<string>("MongoDBSettings:ConnectionString")));

            //Add services to the container.
            builder.Services.AddSingleton<CommentsMongoDBService>();


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseCors("AllowAllOrigins");

            // Configure the HTTP request pipeline.
            //!!!Comment below if to enable swagger on production
            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI();
            //}

            //app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
