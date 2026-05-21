global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Security.Claims;
global using System.Threading;
global using System.Threading.Tasks;
global using Company.Template.Composition.Abstractions.Contexts;
global using Company.Template.Composition.Abstractions.Contracts;
global using Company.Template.Api.FeatureCatalog;
global using Company.Template.Application.Products;
global using Company.Template.Composition.AspNetCore.Contexts;
global using Company.Template.Composition.AspNetCore.Contracts;
//#if (auth == "Keycloak")
global using Microsoft.AspNetCore.Authentication.JwtBearer;
//#endif
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Diagnostics;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Routing;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
//#if (auth == "Keycloak")
global using Microsoft.IdentityModel.Tokens;
//#endif
global using Microsoft.OpenApi;
