global using Ardalis.Result;
global using FastEndpoints;
global using FluentValidation;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using System.Diagnostics.CodeAnalysis;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using TC.CloudGames.Contracts.Events;
global using TC.CloudGames.Contracts.Events.Games;
global using TC.CloudGames.Games.Application.Abstractions.Mappers;
global using TC.CloudGames.Games.Application.Abstractions.Ports;
global using TC.CloudGames.Games.Application.UseCases.GetGameById;
global using TC.CloudGames.Games.Domain.Aggregates;
global using TC.CloudGames.Games.Domain.ValueObjects;
global using TC.CloudGames.SharedKernel.Application.Commands;
global using TC.CloudGames.SharedKernel.Application.Handlers;
global using TC.CloudGames.SharedKernel.Application.Ports;
global using TC.CloudGames.SharedKernel.Domain.Aggregate;
global using TC.CloudGames.SharedKernel.Domain.Events;
global using TC.CloudGames.SharedKernel.Extensions;
global using TC.CloudGames.SharedKernel.Infrastructure.Messaging;
global using TC.CloudGames.SharedKernel.Infrastructure.UserClaims;
global using Wolverine.Marten;
global using static TC.CloudGames.Games.Domain.Aggregates.GameAggregate;
//**//
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("TC.CloudGames.Games.Unit.Tests")]