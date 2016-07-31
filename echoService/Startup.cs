using System.Web.Http;
using Owin;
using echoService.Controllers;
using Microsoft.ServiceFabric.Data;
using Microsoft.WindowsAzure.Storage.Table;

namespace echoService
{
    public static class Startup
    {
        public static CloudTable Table { get; internal set; }

        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public static void ConfigureApp(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            config.Properties["Table"] = Table;

            config.Routes.MapHttpRoute(
                name: "ConsoleApi",
                routeTemplate: "{controller}/echo/{channel}/{category}/{message}",
                defaults: new {
                    channel = RouteParameter.Optional,
                    category = RouteParameter.Optional,
                    message = RouteParameter.Optional
                }
            );

            config.Routes.MapHttpRoute(
                name: "CanvasApi",
                routeTemplate: "{controller}/echo/{channel}/{category}/{message}",
                defaults: new
                {
                    channel = RouteParameter.Optional,
                    category = RouteParameter.Optional,
                    message = RouteParameter.Optional
                }
            );
            appBuilder.UseWebApi(config);
        }
    }
}
