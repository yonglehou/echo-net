using System.Web.Http;
using Owin;

namespace echoService
{
    public static class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public static void ConfigureApp(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{channel}/{message}",
                defaults: new {
                    channel = RouteParameter.Optional,
                    message = RouteParameter.Optional
                }
            );

            appBuilder.UseWebApi(config);
        }
    }
}
