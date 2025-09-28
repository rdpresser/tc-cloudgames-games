// Global usings for test project
global using Ardalis.Result;
global using AutoFixture;
global using AutoFixture.AutoFakeItEasy;
global using AutoFixture.Xunit3;
global using FakeItEasy;
global using FastEndpoints;
global using FastEndpoints.Testing;
global using FluentValidation;
global using FluentValidation.Resources;
global using Marten;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Logging;
global using Microsoft.IdentityModel.JsonWebTokens;
global using Shouldly;
global using System.Diagnostics.CodeAnalysis;
global using System.Security.Claims;
global using TC.CloudGames.SharedKernel.Application.Commands;
global using TC.CloudGames.SharedKernel.Application.Queries;
global using TC.CloudGames.SharedKernel.Infrastructure.Authentication;
global using TC.CloudGames.SharedKernel.Infrastructure.Caching.Service;
global using TC.CloudGames.SharedKernel.Infrastructure.UserClaims;
global using TC.CloudGames.Games.Api.Middleware;
global using TC.CloudGames.Games.Application.Abstractions;
global using TC.CloudGames.Games.Application.Abstractions.Ports;
global using TC.CloudGames.Games.Application.UseCases.CreateGame;
global using TC.CloudGames.Games.Application.UseCases.GetGameById;
global using TC.CloudGames.Games.Application.UseCases.GetGameList;


global using TC.CloudGames.Games.Unit.Tests.Api.Abstractions;
global using TC.CloudGames.Games.Unit.Tests.Common;
global using TC.CloudGames.Games.Unit.Tests.Shared;
global using Xunit;
global using ZiggyCreatures.Caching.Fusion;


