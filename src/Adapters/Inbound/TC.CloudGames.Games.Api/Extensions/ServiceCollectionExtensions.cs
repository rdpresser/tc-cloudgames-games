using Azure.Identity;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry;
using TC.CloudGames.SharedKernel.Infrastructure.Telemetry;

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
                .AddValidatorsFromAssemblyContaining<CreateGameCommandValidator>()
                .AddCaching()
                .AddCustomAuthentication(builder.Configuration)
                .AddCustomFastEndpoints(builder.Configuration)
                .ConfigureAppSettings(builder.Configuration)
                .AddCustomHealthCheck()
                .AddCustomOpenTelemetry(builder, builder.Configuration);

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

        private static void AddOpenTelemetryExporters(OpenTelemetryBuilder otelBuilder, IHostApplicationBuilder builder)
        {
            var appInsightsConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
            var useAzureMonitor = !string.IsNullOrWhiteSpace(appInsightsConnectionString);
            var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

            // Priority 1: Azure Monitor (Production - buffered/async, no timeout issues)
            if (useAzureMonitor)
            {
                // Get sampling ratio from configuration with validation (default: 1.0 = 100%)
                var samplingRatioConfig = builder.Configuration["AzureMonitor:SamplingRatio"];
                var samplingRatio = 1.0f;

                if (!string.IsNullOrWhiteSpace(samplingRatioConfig))
                {
                    if (float.TryParse(samplingRatioConfig, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var ratio))
                    {
                        if (ratio >= 0.0f && ratio <= 1.0f)
                        {
                            samplingRatio = ratio;
                        }
                        else
                        {
                            Console.WriteLine($"[WARN] Invalid AzureMonitor:SamplingRatio '{samplingRatioConfig}'. Value must be between 0.0 and 1.0. Falling back to default 1.0 (100%).");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[WARN] Could not parse AzureMonitor:SamplingRatio value '{samplingRatioConfig}' as a floating-point number. Falling back to default 1.0 (100%).");
                    }
                }

                // Store sampling ratio in service collection for later logging in Program.cs
                builder.Services.AddSingleton(new TelemetryExporterInfo
                {
                    ExporterType = "AzureMonitor",
                    SamplingRatio = samplingRatio
                });

                // Configure Azure Monitor exporter
                otelBuilder.UseAzureMonitor(options =>
                {
                    options.ConnectionString = appInsightsConnectionString;

                    // Use DefaultAzureCredential for RBAC/Workload Identity authentication
                    // This enables AAD-based auth when running in AKS with Workload Identity
                    options.Credential = new DefaultAzureCredential();

                    // Sampling ratio from configuration
                    options.SamplingRatio = samplingRatio;

                    // Enable Live Metrics for real-time monitoring
                    options.EnableLiveMetrics = true;
                });

                return;
            }

            // Priority 2: Grafana Agent OTLP (Local development)
            var grafanaSettings = GrafanaHelper.Build(builder.Configuration);

            if (grafanaSettings.Agent.Enabled && useOtlpExporter)
            {
                otelBuilder.WithTracing(tracerBuilder =>
                {
                    tracerBuilder.AddOtlpExporter(otlp =>
                    {
                        otlp.Endpoint = new Uri(grafanaSettings.Otlp.Endpoint);
                        otlp.Protocol = grafanaSettings.Otlp.Protocol.ToLowerInvariant() == "grpc"
                            ? OpenTelemetry.Exporter.OtlpExportProtocol.Grpc
                            : OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;

                        if (!string.IsNullOrWhiteSpace(grafanaSettings.Otlp.Headers))
                        {
                            otlp.Headers = grafanaSettings.Otlp.Headers;
                        }

                        otlp.TimeoutMilliseconds = grafanaSettings.Otlp.TimeoutSeconds * 1000;
                    });
                });

                // Store OTLP info in service collection for later logging in Program.cs
                builder.Services.AddSingleton(new TelemetryExporterInfo
                {
                    ExporterType = "OTLP",
                    Endpoint = grafanaSettings.Otlp.Endpoint,
                    Protocol = grafanaSettings.Otlp.Protocol
                });

                return;
            }

            // Fallback: No external exporter configured
            builder.Services.AddSingleton(new TelemetryExporterInfo
            {
                ExporterType = "None"
            });
        }

        public static IServiceCollection AddCustomOpenTelemetry(this IServiceCollection services, IHostApplicationBuilder builder, IConfiguration configuration)
        {
            var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? TelemetryConstants.Version;
            var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
            var instanceId = Environment.MachineName;

            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });

            var otelBuilder = services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(TelemetryConstants.ServiceName, serviceVersion: serviceVersion, serviceInstanceId: instanceId)
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["deployment.environment"] = environment.ToLowerInvariant(),
                        ["service.namespace"] = TelemetryConstants.ServiceNamespace.ToLowerInvariant(),
                        ["service.instance.id"] = instanceId,
                        ["container.name"] = Environment.GetEnvironmentVariable("HOSTNAME") ?? instanceId,
                        ["cloud.provider"] = "azure",
                        ["cloud.platform"] = "azure_kubernetes_service",
                        ["service.team"] = "engineering",
                        ["service.owner"] = "devops"
                    }))
                .WithMetrics(metricsBuilder =>
                    metricsBuilder
                        // ASP.NET Core and system instrumentation
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation() // CPU, Memory, GC metrics
                        .AddFusionCacheInstrumentation()
                        .AddNpgsqlInstrumentation()
                        // Custom meters (app + Wolverine + Marten)
                        .AddMeter("Microsoft.AspNetCore.Hosting")
                        .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                        .AddMeter("System.Net.Http")
                        .AddMeter("System.Runtime")
                        // Custom application meters
                        .AddMeter("Wolverine")
                        .AddMeter("Marten")
                        .AddMeter(TelemetryConstants.GamesMeterName)
                        // Exporters
                        .AddPrometheusExporter()
                    )
                .WithTracing(tracingBuilder =>
                    tracingBuilder
                        .AddHttpClientInstrumentation(options =>
                        {
                            options.FilterHttpRequestMessage = request =>
                            {
                                // Filter out health check and metrics requests
                                var path = request.RequestUri?.AbsolutePath ?? "";
                                return !path.Contains("/health") && !path.Contains("/metrics") && !path.Contains("/prometheus");
                            };
                            options.EnrichWithHttpRequestMessage = (activity, request) =>
                            {
                                activity.SetTag("http.request.method", request.Method.ToString());
                                activity.SetTag("http.request.body.size", request.Content?.Headers?.ContentLength);
                                activity.SetTag("user_agent", request.Headers.UserAgent?.ToString());
                            };
                            options.EnrichWithHttpResponseMessage = (activity, response) =>
                            {
                                activity.SetTag("http.response.status_code", (int)response.StatusCode);
                                activity.SetTag("http.response.body.size", response.Content?.Headers?.ContentLength);
                            };
                        })
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            options.Filter = httpContext =>
                            {
                                // Filter out health check, metrics, and prometheus requests
                                var path = httpContext.Request.Path.Value ?? "";
                                return !path.Contains("/health") && !path.Contains("/metrics") && !path.Contains("/prometheus");
                            };
                            options.EnrichWithHttpRequest = (activity, request) =>
                            {
                                activity.SetTag("http.method", request.Method);
                                activity.SetTag("http.scheme", request.Scheme);
                                activity.SetTag("http.host", request.Host.Value);
                                activity.SetTag("http.target", request.Path);
                                if (request.ContentLength.HasValue)
                                    activity.SetTag("http.request_content_length", request.ContentLength.Value);

                                activity.SetTag("http.request.size", request.ContentLength);
                                activity.SetTag("user.id", request.HttpContext.User?.Identity?.Name);
                                activity.SetTag("user.authenticated", request.HttpContext.User?.Identity?.IsAuthenticated);
                                activity.SetTag("http.route", request.HttpContext.GetRouteValue("action")?.ToString());
                                activity.SetTag("http.client_ip", request.HttpContext.Connection.RemoteIpAddress?.ToString());

                                if (request.Headers.TryGetValue(TelemetryConstants.CorrelationIdHeader, out var correlationId))
                                {
                                    activity.SetTag("correlation.id", correlationId.FirstOrDefault());
                                }
                            };
                            options.EnrichWithHttpResponse = (activity, response) =>
                            {
                                activity.SetTag("http.status_code", response.StatusCode);
                                if (response.ContentLength.HasValue)
                                    activity.SetTag("http.response_content_length", response.ContentLength.Value);

                                activity.SetTag("http.response.size", response.ContentLength);
                            };

                            options.EnrichWithException = (activity, exception) =>
                            {
                                activity.SetTag("exception.type", exception.GetType().Name);
                                activity.SetTag("exception.message", exception.Message);
                                activity.SetTag("exception.stacktrace", exception.StackTrace);
                            };
                        })
                        .AddFusionCacheInstrumentation()
                        .AddNpgsql()
                        //.AddRedisInstrumentation()
                        // Custom sources (Application, Wolverine, Marten)
                        .AddSource(TelemetryConstants.GameActivitySource)
                        .AddSource(TelemetryConstants.DatabaseActivitySource)
                        .AddSource(TelemetryConstants.CacheActivitySource)
                        .AddSource("Wolverine")
                        .AddSource("Marten"));

            AddOpenTelemetryExporters(otelBuilder, builder);

            // Register custom metrics classes
            services.AddSingleton<GameMetrics>();
            services.AddSingleton<SystemMetrics>();

            return services;
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
        public static IServiceCollection AddCustomFastEndpoints(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddFastEndpoints(dicoveryOptions =>
            {
                dicoveryOptions.Assemblies = [typeof(Application.DependencyInjection).Assembly];
            })
            .SwaggerDocument(o =>
            {
                o.DocumentSettings = s =>
                {
                    s.Title = "TC.CloudGames.Games API";
                    s.Version = "v1";
                    s.Description = "Game API for TC.CloudGames";
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
                opts.UseSystemTextJsonForSerialization();
                opts.ApplicationAssembly = typeof(Program).Assembly;
                opts.Discovery.IncludeAssembly(typeof(UserSnapshotProjectionHandler).Assembly);
                // -------------------------------
                // Define schema for Wolverine durability and Postgres persistence
                // -------------------------------
                const string wolverineSchema = "wolverine";
                opts.Durability.MessageStorageSchemaName = wolverineSchema;
                opts.ServiceName = "tccloudgames";

                // -------------------------------
                // Persist Wolverine messages in Postgres using the same schema
                // -------------------------------
                opts.PersistMessagesWithPostgresql(
                        PostgresHelper.Build(builder.Configuration).ConnectionString,
                        wolverineSchema
                    );

                ////opts.Policies.OnException<Exception>().RetryTimes(5);
                opts.Policies.OnAnyException()
                    .RetryWithCooldown(
                        TimeSpan.FromMilliseconds(200),
                        TimeSpan.FromMilliseconds(400),
                        TimeSpan.FromMilliseconds(600),
                        TimeSpan.FromMilliseconds(800),
                        TimeSpan.FromMilliseconds(1000)
                    );

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

                        // Durable outbox
                        opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
                        opts.Policies.UseDurableInboxOnAllListeners();

                        var exchangeName = $"{mq.Exchange}-exchange";
                        // Register messages
                        opts.PublishMessage<EventContext<GameCreatedIntegrationEvent>>()
                            .ToRabbitExchange(exchangeName)
                            .BufferedInMemory()
                            .UseDurableOutbox();

                        #region Games API EVENTS

                        ////opts.PublishMessage<EventContext<GameBasicInfoUpdatedIntegrationEvent>>()
                        ////    .ToRabbitExchange(exchangeName)
                        ////    .BufferedInMemory()
                        ////    .UseDurableOutbox();

                        ////opts.PublishMessage<EventContext<GamePriceUpdatedIntegrationEvent>>()
                        ////    .ToRabbitExchange(exchangeName)
                        ////    .BufferedInMemory()
                        ////    .UseDurableOutbox();

                        ////opts.PublishMessage<EventContext<GameStatusUpdatedIntegrationEvent>>()
                        ////    .ToRabbitExchange(exchangeName)
                        ////    .BufferedInMemory()
                        ////    .UseDurableOutbox();

                        ////opts.PublishMessage<EventContext<GameRatingUpdatedIntegrationEvent>>()
                        ////    .ToRabbitExchange(exchangeName)
                        ////    .BufferedInMemory()
                        ////    .UseDurableOutbox();

                        ////opts.PublishMessage<EventContext<GameDetailsUpdatedIntegrationEvent>>()
                        ////    .ToRabbitExchange(exchangeName)
                        ////    .BufferedInMemory()
                        ////    .UseDurableOutbox();

                        ////opts.PublishMessage<EventContext<GameActivatedIntegrationEvent>>()
                        ////    .ToRabbitExchange(exchangeName)
                        ////    .BufferedInMemory()
                        ////    .UseDurableOutbox();

                        ////opts.PublishMessage<EventContext<GameDeactivatedIntegrationEvent>>()
                        ////    .ToRabbitExchange(exchangeName)
                        ////    .BufferedInMemory()
                        ////    .UseDurableOutbox();
                        #endregion

                        opts.PublishMessage<EventContext<GamePurchasedIntegrationEvent>>()
                            .ToRabbitExchange(exchangeName)
                            .BufferedInMemory()
                            .UseDurableOutbox();

                        // Declara fila para eventos de Users
                        opts.ListenToRabbitQueue($"games.{mq.ListenUserExchange}-queue", configure =>
                            {
                                configure.IsDurable = mq.Durable;
                                configure.BindExchange(exchangeName: $"{mq.ListenUserExchange}-exchange");
                            })
                        .UseDurableInbox();

                        // Declara fila para eventos de PAYMENTS
                        opts.ListenToRabbitQueue($"games.{mq.ListenPaymentExchange}-queue", configure =>
                        {
                            configure.IsDurable = mq.Durable;
                            configure.BindExchange(exchangeName: $"{mq.ListenPaymentExchange}-exchange");
                        })
                        .UseDurableInbox();

                        break;

                    case BrokerType.AzureServiceBus when broker.ServiceBusSettings is { } sb:
                        var azureOpts = opts.ConfigureAzureServiceBus(sb, builder.Environment);

                        if (sb.AutoProvision) azureOpts.AutoProvision();
                        if (sb.AutoPurgeOnStartup) azureOpts.AutoPurgeOnStartup();
                        if (sb.UseControlQueues)
                        {
                            azureOpts.EnableWolverineControlQueues();
                            azureOpts.SystemQueuesAreEnabled(true);
                        }

                        // Durable outbox for all sending endpoints
                        opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
                        opts.Policies.UseDurableInboxOnAllListeners();
                        opts.Durability.DurabilityAgentEnabled = true;

                        // PAYMENTS API EVENTS -------------------------------
                        opts.RegisterPaymentEvents();

                        // USERS API EVENTS -------------------------------
                        opts.RegisterUserEvents();

                        // GAMES API EVENTS -------------------------------
                        opts.RegisterGameEvents();

                        var topicName = $"{sb.TopicName}-topic";
                        opts.PublishMessage<EventContext<GameCreatedIntegrationEvent>>()
                            .ToAzureServiceBusTopic(topicName)
                            .CustomizeOutgoing(e =>
                            {
                                e.Headers["DomainAggregate"] = "GameAggregate";
                            })
                            .BufferedInMemory()
                            .UseDurableOutbox()
                            .CircuitBreaking(configure =>
                            {
                                configure.FailuresBeforeCircuitBreaks = 5;
                                configure.MaximumEnvelopeRetryStorage = 10;
                            });

                        opts.PublishMessage<GamePurchasePaymentApprovedFunctionEvent>()
                            .ToAzureServiceBusTopic(topicName)
                            .CustomizeOutgoing(e =>
                            {
                                e.Headers["DomainAggregate"] = "GameAggregate";
                            })
                            .BufferedInMemory()
                            .UseDurableOutbox()
                            .CircuitBreaking(configure =>
                            {
                                configure.FailuresBeforeCircuitBreaks = 5;
                                configure.MaximumEnvelopeRetryStorage = 10;
                            });

                        opts.PublishMessage<EventContext<GamePurchasedIntegrationEvent>>()
                            .ToAzureServiceBusTopic(topicName)
                            .CustomizeOutgoing(e =>
                            {
                                e.Headers["DomainAggregate"] = "GameAggregate";
                            })
                            .BufferedInMemory()
                            .UseDurableOutbox()
                            .CircuitBreaking(configure =>
                            {
                                configure.FailuresBeforeCircuitBreaks = 5;
                                configure.MaximumEnvelopeRetryStorage = 10;
                            });

                        #region Games API EVENTS
                        ////opts.PublishMessage<EventContext<GameBasicInfoUpdatedIntegrationEvent>>()
                        ////    .ToAzureServiceBusTopic(topicName)
                        ////    .CustomizeOutgoing(e => e.Headers["DomainAggregate"] = "GameAggregate")
                        ////    .BufferedInMemory()
                        ////    .UseDurableOutbox();

                        ////opts.PublishMessage<EventContext<GamePriceUpdatedIntegrationEvent>>()
                        ////    .ToAzureServiceBusTopic(topicName)
                        ////    .CustomizeOutgoing(e => e.Headers["DomainAggregate"] = "GameAggregate")
                        ////    .BufferedInMemory()
                        ////    .UseDurableOutbox();

                        ////opts.PublishMessage<EventContext<GameStatusUpdatedIntegrationEvent>>()
                        ////    .ToAzureServiceBusTopic(topicName)
                        ////    .CustomizeOutgoing(e => e.Headers["DomainAggregate"] = "GameAggregate")
                        ////    .BufferedInMemory()
                        ////    .UseDurableOutbox();

                        ////opts.PublishMessage<EventContext<GameRatingUpdatedIntegrationEvent>>()
                        ////    .ToAzureServiceBusTopic(topicName)
                        ////    .CustomizeOutgoing(e => e.Headers["DomainAggregate"] = "GameAggregate")
                        ////    .BufferedInMemory()
                        ////    .UseDurableOutbox();

                        ////opts.PublishMessage<EventContext<GameDetailsUpdatedIntegrationEvent>>()
                        ////    .ToAzureServiceBusTopic(topicName)
                        ////    .CustomizeOutgoing(e => e.Headers["DomainAggregate"] = "GameAggregate")
                        ////    .BufferedInMemory()
                        ////    .UseDurableOutbox();

                        ////opts.PublishMessage<EventContext<GameActivatedIntegrationEvent>>()
                        ////    .ToAzureServiceBusTopic(topicName)
                        ////    .CustomizeOutgoing(e => e.Headers["DomainAggregate"] = "GameAggregate")
                        ////    .BufferedInMemory()
                        ////    .UseDurableOutbox();

                        ////opts.PublishMessage<EventContext<GameDeactivatedIntegrationEvent>>()
                        ////    .ToAzureServiceBusTopic(topicName)
                        ////    .CustomizeOutgoing(e => e.Headers["DomainAggregate"] = "GameAggregate")
                        ////    .BufferedInMemory()
                        ////    .UseDurableOutbox();

                        #endregion

                        // Declare subscription for USER events
                        opts.ListenToAzureServiceBusSubscription(
                            subscriptionName: $"games.{sb.UsersTopicName}-subscription",
                            configureSubscriptions: configure =>
                            {
                                configure.TopicName = $"{sb.UsersTopicName}-topic";
                                configure.MaxDeliveryCount = sb.MaxDeliveryCount;
                                configure.DeadLetteringOnMessageExpiration = sb.EnableDeadLettering;
                            },
                            configureSubscriptionRule: configure =>
                            {
                                ////configure.Name = "$Default";
                                ////configure.Filter = new TrueRuleFilter();

                                configure.Name = "UsersDomainAggregateFilter";
                                configure.Filter = new SqlRuleFilter("DomainAggregate = 'UserAggregate'");
                            })
                        .FromTopic($"{sb.UsersTopicName}-topic")
                        .UseDurableInbox();

                        // Declare subscription for PAYMENT events
                        opts.ListenToAzureServiceBusSubscription(
                            subscriptionName: $"games.{sb.PaymentsTopicName}-subscription",
                            configureSubscriptions: configure =>
                            {
                                configure.TopicName = $"{sb.PaymentsTopicName}-topic";
                                configure.MaxDeliveryCount = sb.MaxDeliveryCount;
                                configure.DeadLetteringOnMessageExpiration = sb.EnableDeadLettering;
                            },
                            configureSubscriptionRule: configure =>
                            {
                                configure.Name = "PaymentsDomainAggregateFilter";
                                configure.Filter = new SqlRuleFilter("DomainAggregate = 'PaymentAggregate'");
                            })
                        .FromTopic($"{sb.PaymentsTopicName}-topic")
                        .UseDurableInbox();

                        break;
                }
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
                options.Projections.Add<UserGameLibraryProjectionHandler>(ProjectionLifecycle.Inline);

                // Snapshot automático do aggregate (para acelerar LoadAsync)
                options.Projections.Snapshot<UserGameLibraryAggregate>(SnapshotLifecycle.Inline);

                ////options.Schema.For<GameAggregate>();
                options.Schema.For<GameProjection>()
                    .DatabaseSchemaName("documents")
                    // Campos duplicados para filtros e ordenação
                    .Duplicate(x => x.Publisher, pgType: "varchar(200)")
                    .Duplicate(x => x.DistributionFormat, pgType: "varchar(50)")
                    .Duplicate(x => x.GameStatus, pgType: "varchar(50)")
                    .Duplicate(x => x.PriceAmount, pgType: "numeric")
                    .Duplicate(x => x.RatingAverage, pgType: "numeric")
                    .Duplicate(x => x.CreatedAt, pgType: "timestamptz")
                    .Duplicate(x => x.UpdatedAt, pgType: "timestamptz")
                    .Duplicate(x => x.IsActive, pgType: "boolean");

                // Computed indexes (case-insensitive)
                options.Schema.For<GameProjection>()
                    .DatabaseSchemaName("documents")
                    .Index(x => x.Name, x => { x.Casing = ComputedIndex.Casings.Lower; x.Method = IndexMethod.btree; x.Name = "idx_gameprojection_name_lower"; })
                    .Index(x => x.Developer, x => { x.Casing = ComputedIndex.Casings.Lower; x.Method = IndexMethod.btree; x.Name = "idx_gameprojection_developer_lower"; })
                    .Index(x => x.Genre, x => { x.Casing = ComputedIndex.Casings.Lower; x.Method = IndexMethod.btree; x.Name = "idx_gameprojection_genre_lower"; })
                    .Index(x => x.GameMode, x => { x.Casing = ComputedIndex.Casings.Lower; x.Method = IndexMethod.btree; x.Name = "idx_gameprojection_gamemode_lower"; });

                // GIN index on JSONB
                options.Schema.For<GameProjection>()
                    .DatabaseSchemaName("documents")
                    .GinIndexJsonData();

                options.Schema.For<UserGameLibraryProjection>()
                    .DatabaseSchemaName("documents")
                    .Duplicate(x => x.UserId, pgType: "uuid")
                    .Duplicate(x => x.GameId, pgType: "uuid")
                    .Duplicate(x => x.PaymentId, pgType: "uuid")
                    .Duplicate(x => x.PurchaseDate, pgType: "timestamptz")
                    .Duplicate(x => x.CreatedAt, pgType: "timestamptz")
                    .Duplicate(x => x.UpdatedAt, pgType: "timestamptz")
                    .Duplicate(x => x.IsActive, pgType: "boolean");

                options.Schema.For<UserGameLibraryProjection>()
                    .DatabaseSchemaName("documents")
                    .Index(x => x.GameName, x => { x.Casing = ComputedIndex.Casings.Lower; x.Method = IndexMethod.btree; x.Name = "idx_usergamelibrary_game_name_lower"; });

                // GIN index on JSONB
                options.Schema.For<UserGameLibraryProjection>()
                    .DatabaseSchemaName("documents")
                    .GinIndexJsonData();

                options.CreateDatabasesForTenants(c =>
                {
                    c.MaintenanceDatabase(connProvider.MaintenanceConnectionString);
                    c.ForTenant()
                        .CheckAgainstPgDatabase()
                        .WithOwner("postgres")
                        .WithEncoding("UTF-8")
                        .ConnectionLimit(-1);
                });

                // -------------------------------
                // Configuração do UserSnapshot
                // -------------------------------
                options.Schema.For<UserSnapshot>()
                    .DatabaseSchemaName("documents")
                    // Duplicates simples (Marten já cria índices básicos)
                    .Duplicate(x => x.IsActive, pgType: "boolean")
                    .Duplicate(x => x.Role, pgType: "varchar(50)")
                    .Duplicate(x => x.CreatedAt, pgType: "timestamptz")
                    .Duplicate(x => x.UpdatedAt, pgType: "timestamptz");

                // Computed indexes customizados (sem Duplicate para evitar conflito)
                options.Schema.For<UserSnapshot>()
                    .Index(x => x.Id, x =>
                    {
                        x.IsUnique = true;
                        x.Method = IndexMethod.btree;
                        x.Name = "idx_usersnapshot_id";
                    })
                    .Index(x => x.Email, x =>
                    {
                        x.Casing = ComputedIndex.Casings.Lower;
                        x.IsUnique = true;
                        x.Method = IndexMethod.btree;
                        x.Name = "idx_usersnapshot_email_lower";
                    })
                    .Index(x => x.Username, x =>
                    {
                        x.Casing = ComputedIndex.Casings.Lower;
                        x.Method = IndexMethod.btree;
                        x.Name = "idx_usersnapshot_username_lower";
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