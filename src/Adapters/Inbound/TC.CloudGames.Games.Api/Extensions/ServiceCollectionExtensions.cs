namespace TC.CloudGames.Games.Api.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGameServices(this IServiceCollection services, WebApplicationBuilder builder)
        {
            // Configure FluentValidation globally
            ConfigureFluentValidationGlobals();

            // Add Marten configuration only if not testing
            if (!builder.Environment.IsEnvironment("Testing"))
            {
                services.AddMartenEventSourcing();
                builder.AddWolverineMessaging();
            }

            services.AddHttpClient()
                .AddCorrelationIdGenerator()
                .AddCaching()
                .AddCustomAuthentication(builder.Configuration)
                .AddCustomFastEndpoints()
                .ConfigureAppSettings(builder.Configuration)
                .AddCustomHealthCheck();

            //services// Add custom telemetry services
            //    .AddSingleton<UserMetrics>()
            //services.AddCustomOpenTelemetry()

            return services;
        }

        // FluentValidation Global Setup
        private static void ConfigureFluentValidationGlobals()
        {
            ValidatorOptions.Global.PropertyNameResolver = (type, memberInfo, expression) => memberInfo?.Name;
            ValidatorOptions.Global.DisplayNameResolver = (type, memberInfo, expression) => memberInfo?.Name;
            ValidatorOptions.Global.ErrorCodeResolver = validator => validator.Name;
            ValidatorOptions.Global.LanguageManager = new LanguageManager
            {
                Enabled = true,
                Culture = new System.Globalization.CultureInfo("en")
            };
        }

        // Health Checks with Enhanced Telemetry
        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddNpgSql(sp =>
                {
                    var connectionProvider = sp.GetRequiredService<IConnectionStringProvider>();
                    return connectionProvider.ConnectionString;
                },
                    name: "PostgreSQL",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: ["db", "sql", "postgres", "live", "ready"])
                .AddTypeActivatedCheck<RedisHealthCheck>("Redis",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: ["cache", "redis", "live", "ready"])
                .AddCheck("Memory", () =>
                {
                    var allocated = GC.GetTotalMemory(false);
                    var mb = allocated / 1024 / 1024;

                    return mb < 1024
                    ? HealthCheckResult.Healthy($"Memory usage: {mb} MB")
                    : HealthCheckResult.Degraded($"High memory usage: {mb} MB");
                },
                    tags: ["memory", "system", "live"])
                .AddCheck("Custom-Metrics", () =>
                {
                    // Add any custom health logic for your metrics system
                    return HealthCheckResult.Healthy("Custom metrics are functioning");
                },
                    tags: ["metrics", "telemetry", "live"]);

            return services;
        }

        // FastEndpoints Configuration
        public static IServiceCollection AddCustomFastEndpoints(this IServiceCollection services)
        {
            services.AddFastEndpoints(dicoveryOptions =>
            {
                dicoveryOptions.Assemblies = [typeof(Application.DependencyInjection).Assembly];
            })
            .SwaggerDocument(o =>
            {
                o.DocumentSettings = s =>
                {
                    s.Title = "TC.CloudGames API";
                    s.Version = "v1";
                    s.Description = "API for TC.CloudGames";
                    s.MarkNonNullablePropsAsRequired();
                };

                o.RemoveEmptyRequestSchema = true;
                o.NewtonsoftSettings = s => { s.Converters.Add(new StringEnumConverter()); };
            });

            return services;
        }

        private static IServiceCollection AddCaching(this IServiceCollection services)
        {
            // Add FusionCache for caching
            services.AddFusionCache()
                .WithDefaultEntryOptions(options =>
                {
                    options.Duration = TimeSpan.FromSeconds(20);
                    options.DistributedCacheDuration = TimeSpan.FromSeconds(30);
                })
                .WithDistributedCache(sp =>
                {
                    var cacheProvider = sp.GetRequiredService<ICacheProvider>();

                    var options = new RedisCacheOptions { Configuration = cacheProvider.ConnectionString, InstanceName = cacheProvider.InstanceName };

                    return new RedisCache(options);
                })
                .WithSerializer(new FusionCacheSystemTextJsonSerializer())
                .AsHybridCache();

            return services;
        }

        // 2) Configure Wolverine messaging with RabbitMQ transport and durable outbox
        private static WebApplicationBuilder AddWolverineMessaging(this WebApplicationBuilder builder)
        {
            builder.Host.UseWolverine(opts =>
            {
                // -------------------------------
                // Define schema for Wolverine durability and Postgres persistence
                // -------------------------------
                const string wolverineSchema = "wolverine";
                opts.Durability.MessageStorageSchemaName = wolverineSchema;

                // -------------------------------
                // Envelope customizer and routing convention
                // -------------------------------
                opts.Services.AddSingleton<IEnvelopeCustomizer, GenericEventContextEnvelopeCustomizer>();
                opts.Services.AddSingleton<IMessageRoutingConvention, EventContextRoutingConvention>();

                // -------------------------------
                // Enable durable local queues and auto transaction application
                // -------------------------------
                opts.Policies.UseDurableLocalQueues();
                opts.Policies.AutoApplyTransactions();

                // -------------------------------
                // Load and configure message broker
                // -------------------------------
                var broker = MessageBrokerHelper.Build(builder.Configuration);

                switch (broker.Type)
                {
                    case BrokerType.RabbitMQ when broker.RabbitMqSettings is { } mq:
                        var rabbitOpts = opts.UseRabbitMq(factory =>
                        {
                            factory.Uri = new Uri(mq.ConnectionString);
                            factory.VirtualHost = mq.VirtualHost;

                            //Metadata for monitoring and tracing
                            factory.ClientProperties["application"] = $"TC.CloudGames.Games.Api";
                            factory.ClientProperties["environment"] = builder.Environment.EnvironmentName;
                        });

                        if (mq.AutoProvision) rabbitOpts.AutoProvision();
                        if (mq.UseQuorumQueues) rabbitOpts.UseQuorumQueues();
                        if (mq.AutoPurgeOnStartup) rabbitOpts.AutoPurgeOnStartup();

                        // Publish all messages to the configured exchange with durable outbox
                        ////opts.PublishAllMessages().ToRabbitExchange(mq.Exchange);

                        // Durable outbox
                        opts.Policies.UseDurableOutboxOnAllSendingEndpoints();

                        // Register messages
                        opts.PublishMessage<EventContext<GameBasicInfoUpdatedIntegrationEvent, GameAggregate>>()
                            .ToRabbitExchange(mq.Exchange);
                        opts.PublishMessage<EventContext<GamePriceUpdatedIntegrationEvent, GameAggregate>>()
                            .ToRabbitExchange(mq.Exchange);
                        opts.PublishMessage<EventContext<GameStatusUpdatedIntegrationEvent, GameAggregate>>()
                            .ToRabbitExchange(mq.Exchange);
                        opts.PublishMessage<EventContext<GameRatingUpdatedIntegrationEvent, GameAggregate>>()
                            .ToRabbitExchange(mq.Exchange);
                        opts.PublishMessage<EventContext<GameDetailsUpdatedIntegrationEvent, GameAggregate>>()
                            .ToRabbitExchange(mq.Exchange);
                        opts.PublishMessage<EventContext<GameActivatedIntegrationEvent, GameAggregate>>()
                            .ToRabbitExchange(mq.Exchange);
                        opts.PublishMessage<EventContext<GameDeactivatedIntegrationEvent, GameAggregate>>()
                            .ToRabbitExchange(mq.Exchange);
                        break;

                    case BrokerType.AzureServiceBus when broker.ServiceBusSettings is { } sb:
                        var azureOpts = opts.UseAzureServiceBus(sb.ConnectionString);

                        if (sb.AutoProvision) azureOpts.AutoProvision();
                        if (sb.AutoPurgeOnStartup) azureOpts.AutoPurgeOnStartup();
                        if (sb.UseControlQueues) azureOpts.EnableWolverineControlQueues();

                        // Durable outbox for all sending endpoints
                        opts.Policies.UseDurableOutboxOnAllSendingEndpoints();

                        // Publish all messages to a Topic with buffered in-memory delivery and durable outbox
                        ////opts.PublishAllMessages()
                        ////    .ToAzureServiceBusTopic(sb.TopicName)
                        ////    .BufferedInMemory();

                        // Register messages for Azure Service Bus Topic with buffered in-memory delivery
                        opts.PublishMessage<EventContext<GameBasicInfoUpdatedIntegrationEvent, GameAggregate>>()
                            .ToAzureServiceBusTopic(sb.TopicName)
                            .BufferedInMemory();
                        opts.PublishMessage<EventContext<GamePriceUpdatedIntegrationEvent, GameAggregate>>()
                            .ToAzureServiceBusTopic(sb.TopicName)
                            .BufferedInMemory();
                        opts.PublishMessage<EventContext<GameStatusUpdatedIntegrationEvent, GameAggregate>>()
                            .ToAzureServiceBusTopic(sb.TopicName)
                            .BufferedInMemory();
                        opts.PublishMessage<EventContext<GameRatingUpdatedIntegrationEvent, GameAggregate>>()
                            .ToAzureServiceBusTopic(sb.TopicName)
                            .BufferedInMemory();
                        opts.PublishMessage<EventContext<GameDetailsUpdatedIntegrationEvent, GameAggregate>>()
                            .ToAzureServiceBusTopic(sb.TopicName)
                            .BufferedInMemory();
                        opts.PublishMessage<EventContext<GameActivatedIntegrationEvent, GameAggregate>>()
                            .ToAzureServiceBusTopic(sb.TopicName)
                            .BufferedInMemory();
                        opts.PublishMessage<EventContext<GameDeactivatedIntegrationEvent, GameAggregate>>()
                            .ToAzureServiceBusTopic(sb.TopicName)
                            .BufferedInMemory();
                        break;
                }

                // -------------------------------
                // Persist Wolverine messages in Postgres using the same schema
                // -------------------------------
                opts.PersistMessagesWithPostgresql(
                    PostgresHelper.Build(builder.Configuration).ConnectionString,
                    wolverineSchema
                );
            });

            // -------------------------------
            // Ensure all messaging resources and schema are created at startup
            // -------------------------------
            builder.Services.AddResourceSetupOnStartup();

            return builder;
        }

        // 1) Configure Marten with event sourcing, projections, and Wolverine integration
        private static IServiceCollection AddMartenEventSourcing(this IServiceCollection services)
        {
            services.AddMarten(serviceProvider =>
            {
                var connProvider = serviceProvider.GetRequiredService<IConnectionStringProvider>();

                var options = new StoreOptions();
                options.Connection(connProvider.ConnectionString);
                options.Logger(new ConsoleMartenLogger());

                options.Events.DatabaseSchemaName = "events";
                options.DatabaseSchemaName = "documents";

                options.Projections.Add<GameProjectionHandler>(ProjectionLifecycle.Inline);

                options.Schema.For<GameProjection>()
                    .DatabaseSchemaName("documents")
                    // Campos duplicados para filtros e ordenação
                    ////.Duplicate(x => x.Name, pgType: "varchar(200)")
                    ////.Duplicate(x => x.Developer, pgType: "varchar(200)")
                    ////.Duplicate(x => x.Publisher, pgType: "varchar(200)")
                    ////.Duplicate(x => x.Genre, pgType: "varchar(100)")
                    ////.Duplicate(x => x.GameMode, pgType: "varchar(50)")
                    .Duplicate(x => x.DistributionFormat, pgType: "varchar(50)")
                    .Duplicate(x => x.GameStatus, pgType: "varchar(50)")
                    .Duplicate(x => x.PriceAmount, pgType: "numeric")
                    .Duplicate(x => x.RatingAverage, pgType: "numeric")
                    .Duplicate(x => x.CreatedAt, pgType: "timestamptz")
                    .Duplicate(x => x.UpdatedAt, pgType: "timestamptz")
                    .Duplicate(x => x.IsActive, pgType: "boolean");

                // Computed indexes (case-insensitive)
                options.Schema.For<GameProjection>()
                    .Index(x => x.Name, x => { x.Casing = ComputedIndex.Casings.Lower; x.Method = IndexMethod.btree; })
                    .Index(x => x.Developer, x => { x.Casing = ComputedIndex.Casings.Lower; x.Method = IndexMethod.btree; })
                    .Index(x => x.Publisher, x => { x.Casing = ComputedIndex.Casings.Lower; x.Method = IndexMethod.btree; })
                    .Index(x => x.Genre, x => { x.Casing = ComputedIndex.Casings.Lower; x.Method = IndexMethod.btree; })
                    .Index(x => x.GameMode, x => { x.Casing = ComputedIndex.Casings.Lower; x.Method = IndexMethod.btree; });

                // GIN index on JSONB
                options.Schema.For<GameProjection>().GinIndexJsonData();

                options.CreateDatabasesForTenants(c =>
                {
                    c.MaintenanceDatabase(connProvider.MaintenanceConnectionString);
                    c.ForTenant()
                        .CheckAgainstPgDatabase()
                        .WithOwner("postgres")
                        .WithEncoding("UTF-8")
                        .ConnectionLimit(-1);
                });

                return options;
            })
            .UseLightweightSessions()
            .IntegrateWithWolverine(cfg =>
            {
                cfg.UseWolverineManagedEventSubscriptionDistribution = true;
            })
            .ApplyAllDatabaseChangesOnStartup();

            return services;
        }

        // Authentication and Authorization
        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();

                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings!.Issuer, // Ensure this matches the issuer in your token
                    ValidateAudience = true,
                    ValidAudiences = jwtSettings!.Audience, // Ensure this matches the audience in your token
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings!.SecretKey ?? string.Empty)), // Use the same secret key
                    ValidateIssuerSigningKey = true,
                    RoleClaimType = "role",
                    NameClaimType = JwtRegisteredClaimNames.Name
                };

                opt.MapInboundClaims = false; // Keep original claim types
            });

            services.AddAuthorization()
                .AddHttpContextAccessor();

            return services;
        }

        public static IServiceCollection ConfigureAppSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));
            services.Configure<AzureServiceBusOptions>(configuration.GetSection("AzureServiceBus"));
            services.Configure<PostgresOptions>(configuration.GetSection("Database"));
            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
            services.Configure<CacheProviderSettings>(configuration.GetSection("Cache"));

            return services;
        }
    }
}
