using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Globalization;
using Microsoft.Owin.Cors;
using System.Web.Http.Cors;


[assembly: OwinStartup(typeof(AEPWebAPI.Startup))]

namespace AEPWebAPI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // 如需如何設定應用程式的詳細資訊，請參閱  http://go.microsoft.com/fwlink/?LinkID=316888
            //  JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();
            // 如需如何設定應用程式的詳細資訊，請參閱  http://go.microsoft.com/fwlink/?LinkID=316888


            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);



            var config = Register();



            app.UseWebApi(config);


            //var hubConfiguration = new HubConfiguration();
            //hubConfiguration.EnableDetailedErrors = true;

            //app.MapSignalR(hubConfiguration);

            //PerformanceEngine performanceEngine = new PerformanceEngine(1000);
            //Task.Factory.StartNew(async () => await performanceEngine.OnPerformanceMonitor());

            // Web API 設定和服務
            //var cors = new EnableCorsAttribute("*", "*", "*")
            //{
            //    SupportsCredentials = true
            //};
            //config.EnableCors(cors); 
            // Web API configuration and services



        }
        public HttpConfiguration Register()
        {
            var config = new HttpConfiguration();

            // Web API routes
            config.MapHttpAttributeRoutes();


            //config.Filters.Add(new AuthorizeAttribute());

            // config.EnableCors();

            //var cors = new EnableCorsAttribute("http://localhost:56794, http://localhost:4200", "*", "*") { SupportsCredentials = true };
            var cors = new EnableCorsAttribute("*", "*", "*") { SupportsCredentials = true };
            config.EnableCors(cors);


            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            // Web API 路由
          //  config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );



            //// config.Filters.Add(new LoggingFilterAttribute());
            //config.MapHttpAttributeRoutes();
            //var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            //jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();


            //var json = config.Formatters.JsonFormatter;
            //json.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
            ////json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            return config;

        }
    }
}
