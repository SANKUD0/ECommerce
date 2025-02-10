namespace ECommerce.ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Ajout du Logger dans les services
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            var logger = builder.Logging.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();


            // Charger la configuration correctement
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .Build();


            var reverseProxyConfig = builder.Configuration.GetSection("ReverseProxy");
            // Verifier si la section ReverseProxy existe
            if (!reverseProxyConfig.Exists())
                logger.LogInformation("ERREUR La section 'ReverseProxy est introuvable !'");
            else
            {
                logger.LogInformation("Configuration Reverse Proxy chargée avec succès !");
                logger.LogInformation($"- Nombre de Clusters: {reverseProxyConfig.GetSection("Clusters").GetChildren().Count()}");
                foreach (var cluster in reverseProxyConfig.GetSection("Clusters").GetChildren())
                    logger.LogInformation($"- Nombre de Routes: {reverseProxyConfig.GetSection("Routes").GetChildren().Count()}");
                foreach (var route in reverseProxyConfig.GetSection("Routes").GetChildren())
                    logger.LogInformation($"- Route chargée:{route.Key}");
            }


            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AllowAnonymous", policy =>
                policy.RequireAssertion(_ => true));

                options.AddPolicy("RequireAuthentication", policy =>
                policy.RequireAuthenticatedUser());
            });

            builder.Services
                .AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            logger.LogInformation($"Environement actuel : {builder.Environment.EnvironmentName}");
            var authServiceUrl = builder.Configuration["ReverseProxy:Clusters:auth-cluster:Destinations:local:Address"];
            logger.LogInformation($"API Gateway regirige vers auth-service: {authServiceUrl ?? "NON TROUVÉ !"}\n");

            //Test de la connexion avec les microservices
            app.Use(async (context, next) =>
            {
                var requestPath = context.Request.Path.Value;
                Console.WriteLine($" Requête reûe : {requestPath}");
                await next();
            });

            app.MapReverseProxy();

            app.Run();
        }
    }
}
