// 核心框架
global using FluentAssertions;
global using Testcontainers.PostgreSql;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.AspNetCore.TestHost;
global using Microsoft.EntityFrameworkCore;
// 项目内命名空间
global using Legend2Toolbox.Api.Endpoints.Identity;
global using Legend2Toolbox.Application.Feature.Identity;
global using Legend2Toolbox.Domain.Enums;
global using Legend2Toolbox.Infrastructure.Identity;
global using Legend2Toolbox.Infrastructure.Persistence;
// 系统与常用基础库
global using Microsoft.Extensions.DependencyInjection;
global using System.Data.Common;
global using System.Net;
global using System.Net.Http.Headers;
global using System.Net.Http.Json;

