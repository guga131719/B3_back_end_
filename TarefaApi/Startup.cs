using LocalizeApi.Context;
using LocalizeApi.Services;
using LocalizesApi.Services;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using MassTransit;
using MassTransit.RabbitMqTransport;
using System;
using AlunosApi.Models;
using RabbitMQ.Client;
using TarefaApi.RabbitMQ;

namespace LocalizeApi
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
            var segredo = Configuration["TokenJwt:Secret"];

            services.AddControllers();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<ITarefaService, TarefasService>();
            services.AddScoped<IRabitMQProducer, RabitMQProducer>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tarefa.API", Version = "v1" });
            });



            var rabbitMqSettings = new RabbitMqSettings
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/",
                QueueName = "enamilNotification"
            };

            services.AddMassTransit(x =>
            {
                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(new Uri($"rabbitmq://{rabbitMqSettings.HostName}/"), host =>
                    {
                        host.Username(rabbitMqSettings.UserName);
                        host.Password(rabbitMqSettings.Password);
                    });
                    cfg.Publish<MessageNotification>(x => x.ExchangeType = ExchangeType.Fanout);
                }));
            });

            services.AddMassTransitHostedService();
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TarefaApi v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseCors(builder =>
            {
                builder.WithOrigins("http://localhost:4200")
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    public class RabbitMqSettings
    {
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }

        public string QueueName { get; set; }


    }
}
