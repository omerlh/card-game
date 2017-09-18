using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reactive.Linq;
using MFRC522;
using static System.Reactive.Linq.Observable;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ICardReader, CardReader>();
            services.AddSingleton<IObservable<string>>(provider => {
                 var reader = provider.GetService<ICardReader>();
                 return Interval(TimeSpan.FromMilliseconds(100))
                 .Select(_ => {
                     if (reader.IsCardAvailable())
                     {
                         return reader.GetCardId();
                     }
                     return null;
                 }).Where(cardId => cardId != null)
                 .Select(cardId => BitConverter.ToString(cardId));

            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            // Accept web socket requests
            app.UseWebSockets();
            // handle web socket requests
            app.UseMiddleware<WebSocketMiddleware>(); 
        }
    }
}
