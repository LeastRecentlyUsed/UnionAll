using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Diagnostics;
using AutoMapper;
using DataFork.DataStore.Services;
using DataFork.API.Models;


namespace DataFork.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureStagingServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(
                opt => opt.UseNpgsql(Configuration.GetConnectionString("forkstagedatabase"))
                );

            CommonConfiguration(services);
        }

        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(
                opt => opt.UseSqlite(Configuration.GetConnectionString("forkdevdatabase"))
                );

            CommonConfiguration(services);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            ILoggerFactory logFactory, DataContext dataContext)
        {
            //logFactory.AddConsole(); -- added by default in core 2.0 onwards

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    // for global exception handling (across all controllers) to save code duplication.
                    appBuilder.Run(async context =>
                    {
                        var exHandlerFeat = context.Features.Get<IExceptionHandlerFeature>();
                        if (exHandlerFeat != null)
                        {
                            var logger = logFactory.CreateLogger("Global Exception Logger");
                            logger.LogError(500, exHandlerFeat.Error, exHandlerFeat.Error.Message);
                        }

                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later.");

                    });
                });
            }

            AutoMapper.Mapper.Initialize(config =>
            {
                ApiMapping(config);

            });

            app.UseMvc();
        }


        // create attribute mapping between the database domain objects and API model objects.
        private static void ApiMapping(IMapperConfigurationExpression config)
        {
            config.CreateMap<KeyValuePair<int, string>, NodePairsDto>()
                .ForMember(dest => dest.NodeId, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.NodeName, opt => opt.MapFrom(src => src.Value));

            config.CreateMap<NodeCreateDto, Domain.Node>();
            config.CreateMap<NodeUpdateDto, Domain.Node>();

            config.CreateMap<Domain.Node, NodeDto>();
            config.CreateMap<Domain.Node, NodeWithLinksDto>();

            config.CreateMap<Domain.Vector, VectorDto>();
            config.CreateMap<Domain.Vector, VectorWithLinksDto>();

            config.CreateMap<VectorCreateDto, Domain.Vector>();
            config.CreateMap<VectorUpdateDto, Domain.Vector>();
        }


        // create common configure services 
        private static void CommonConfiguration(IServiceCollection services)
        {
            // MVC setup for the content type input/output formats (Json is added already by default) includes XML.
            // An unknown format returns HTTP status code 406.
            services.AddMvc(setupAction =>
            {
                setupAction.ReturnHttpNotAcceptable = true;
                setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                //setupAction.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());

                // also add custom media types to allow vendor specific json output
                var jsonOutFormats = setupAction.OutputFormatters
                    .OfType<JsonOutputFormatter>().FirstOrDefault();

                if (jsonOutFormats != null)
                {
                    jsonOutFormats.SupportedMediaTypes.Add("application/vnd.fork.v1+json");
                }
            });

            services.AddScoped<INodeRepository, NodeRepository>();
            services.AddScoped<IVectorRepository, VectorRepository>();

            // add facility to access the context for an action within a controller
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // add facility to automatically generate URL links for REST headers (for the 
            // action context that is in scope).
            services.AddScoped<IUrlHelper, UrlHelper>(implementationFactory =>
            {
                var actionCtx = implementationFactory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionCtx);
            });
        }
    }
}
