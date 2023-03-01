using AutoMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaStateMachineWorkerService;
using SagaStateMachineWorkerService.Mappings;
using SagaStateMachineWorkerService.Models;
using Shared;
using System.Reflection;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostcontext,services) =>
    {
        services.AddAutoMapper(typeof(MappingProfiles));
;        services.AddMassTransit(cfg => {

            cfg.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>().EntityFrameworkRepository(opt =>
            {
                opt.AddDbContext<DbContext, OrderStateDbContext>((provider, builder) =>
                {
                    builder.UseNpgsql(hostcontext.Configuration.GetConnectionString("PostgresqlConnection"),m=>m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name));
                });
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            });

            cfg.UsingRabbitMq((context,config)=>
            {
                config.Host(hostcontext.Configuration.GetConnectionString("RabbitMQ"));
                config.ReceiveEndpoint(RabbitMQSettingsConst.OrderSaga, e =>
                {
                    e.ConfigureSaga<OrderStateInstance>(context);
                });
            });

        
        });
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
