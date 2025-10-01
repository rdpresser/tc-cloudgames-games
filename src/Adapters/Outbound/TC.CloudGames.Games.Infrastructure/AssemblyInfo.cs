global using Elastic.Clients.Elasticsearch;
global using Elastic.Clients.Elasticsearch.Aggregations;
global using Elastic.Clients.Elasticsearch.Core.Bulk;
global using Elastic.Clients.Elasticsearch.QueryDsl;
global using Marten;
global using Marten.Events.Projections;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using System.Diagnostics.CodeAnalysis;
global using TC.CloudGames.Games.Application.Abstractions.Ports;
global using TC.CloudGames.Games.Application.Abstractions.Projections;
global using TC.CloudGames.Games.Application.UseCases.GetGameById;
global using TC.CloudGames.Games.Application.UseCases.GetGameList;
global using TC.CloudGames.Games.Domain.Aggregates.Game;
global using TC.CloudGames.Games.Domain.Aggregates.UserGameLibrary;
global using TC.CloudGames.Games.Infrastructure.Projections;
global using TC.CloudGames.Games.Infrastructure.Repositories;
global using TC.CloudGames.SharedKernel.Infrastructure.Authentication;
global using TC.CloudGames.SharedKernel.Infrastructure.Caching.Provider;
global using TC.CloudGames.SharedKernel.Infrastructure.Caching.Service;
global using TC.CloudGames.SharedKernel.Infrastructure.Clock;
global using TC.CloudGames.SharedKernel.Infrastructure.Database;
global using TC.CloudGames.SharedKernel.Infrastructure.Elasticsearch;
global using TC.CloudGames.SharedKernel.Infrastructure.Repositories;
global using TC.CloudGames.SharedKernel.Infrastructure.UserClaims;
//**//
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("TC.CloudGames.Games.Unit.Tests")]