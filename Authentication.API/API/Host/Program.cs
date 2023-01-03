namespace Host
{
    using Microsoft.AspNetCore.Builder;

    using Web;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddWeb(builder.Configuration);

            var app = builder.Build();

            app.UseWeb();
            app.MapEndpoints();

            app.Run();
        }
    }
}