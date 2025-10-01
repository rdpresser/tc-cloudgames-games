global using Ardalis.Result;
global using Azure.Messaging.ServiceBus.Administration;
global using Bogus;
global using FastEndpoints;
global using FastEndpoints.Swagger;
global using FluentValidation;
global using FluentValidation.Resources;
global using HealthChecks.UI.Client;
global using JasperFx.Events.Projections;
global using JasperFx.Resources;
global using Marten;
global using Marten.Schema;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Diagnostics.HealthChecks;
global using Microsoft.Extensions.Caching.StackExchangeRedis;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using Microsoft.IdentityModel.JsonWebTokens;
global using Microsoft.IdentityModel.Tokens;
global using Newtonsoft.Json.Converters;
global using Serilog;
global using Serilog.Core;
global using Serilog.Enrichers.Span;
global using Serilog.Events;
global using Serilog.Sinks.Grafana.Loki;
global using System.Diagnostics.CodeAnalysis;
global using System.Net;
global using System.Text;
global using TC.CloudGames.Contracts.Events.Games;
global using TC.CloudGames.Games.Api.Extensions;
global using TC.CloudGames.Games.Api.Middleware;
global using TC.CloudGames.Games.Api.Telemetry;
global using TC.CloudGames.Games.Application;
global using TC.CloudGames.Games.Application.Abstractions;
global using TC.CloudGames.Games.Application.Abstractions.Ports;
global using TC.CloudGames.Games.Application.MessageBrokerHandlers;
global using TC.CloudGames.Games.Application.UseCases.CreateGame;
global using TC.CloudGames.Games.Application.UseCases.GetGameById;
global using TC.CloudGames.Games.Application.UseCases.GetGameList;
global using TC.CloudGames.Games.Domain.Aggregates.Game;
global using TC.CloudGames.Games.Domain.ValueObjects;
global using TC.CloudGames.Games.Infrastructure;
global using TC.CloudGames.Games.Infrastructure.Projections;
global using TC.CloudGames.Messaging.Extensions;
global using TC.CloudGames.SharedKernel.Api.EndPoints;
global using TC.CloudGames.SharedKernel.Application.Behaviors;
global using TC.CloudGames.SharedKernel.Extensions;
global using TC.CloudGames.SharedKernel.Infrastructure.Authentication;
global using TC.CloudGames.SharedKernel.Infrastructure.Caching.HealthCheck;
global using TC.CloudGames.SharedKernel.Infrastructure.Caching.Provider;
global using TC.CloudGames.SharedKernel.Infrastructure.Database;
global using TC.CloudGames.SharedKernel.Infrastructure.Database.Initializer;
global using TC.CloudGames.SharedKernel.Infrastructure.MessageBroker;
global using TC.CloudGames.SharedKernel.Infrastructure.Messaging;
global using TC.CloudGames.SharedKernel.Infrastructure.Middleware;
global using TC.CloudGames.SharedKernel.Infrastructure.Snapshots.Users;
global using Weasel.Postgresql.Tables;
global using Wolverine;
global using Wolverine.AzureServiceBus;
global using Wolverine.Marten;
global using Wolverine.Postgresql;
global using Wolverine.RabbitMQ;
global using ZiggyCreatures.Caching.Fusion;
global using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;
global using CreateGame = TC.CloudGames.Games.Application.UseCases.CreateGame;
global using GameDetails = TC.CloudGames.Games.Domain.ValueObjects.GameDetails;
//**//
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("TC.CloudGames.Games.Unit.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
//**// REMARK: Required for functional and integration tests to work.
namespace TC.CloudGames.Games.Api
{
    public partial class Program;
}