﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PlatformTM.Services.Services.UserManagement;

namespace PlatformTM.API.Auth
{
    public class TokenProviderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenAuthOptions _options;
        private readonly UserAccountService _accountService;

        public TokenProviderMiddleware(RequestDelegate next, UserAccountService userService, IOptions<TokenAuthOptions> options)
        {
            _next = next;
            _options = options.Value;
            _accountService = userService;
        }

        public Task Invoke(HttpContext context)
        {
            // If the request path doesn't match, skip
            if (!context.Request.Path.Equals(_options.Endpoint, StringComparison.Ordinal))
            {
                return _next(context);
            }

            //if (context.Request.Form["granttype"] != "passwrod")
            //{
            //    return BadRequestResult(_next);
            //}

            // Request must be POST with Content-Type: application/x-www-form-urlencoded
            if (!context.Request.Method.Equals("POST")
               || !context.Request.HasFormContentType)
            {
                context.Response.StatusCode = 400;
                return context.Response.WriteAsync("Bad request.");
            }

            return GenerateToken(context);
        }

        private async Task GenerateToken(HttpContext context)
        {

            var appUser = await _accountService.FindUserAsync(context.Request.Form["userName"], context.Request.Form["password"]);

            if (appUser == null)
            {
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                var result = new
                {   token_result= "fail",
                    msg = "Invalid username or password"
                };
                await context.Response.WriteAsync(JsonConvert.SerializeObject(result, new JsonSerializerSettings { Formatting = Formatting.Indented, ContractResolver = new CamelCasePropertyNamesContractResolver() }));    
            }
            var identity = await _accountService.GetClaimsPrincipleForUser(appUser);
            var userData = _accountService.GetUserInfo(identity.FindFirstValue(ClaimTypes.NameIdentifier)).Result;
           
            var now = DateTime.UtcNow;
            
            //Generate Token
            var jwtHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                SigningCredentials = _options.SigningCredentials,
                Subject = (ClaimsIdentity)identity.Identity,
                NotBefore = now,   
                Expires = now.Add(_options.ExpiresSpan)
            });
            var encodedToken = jwtHandler.WriteToken(securityToken);

            var response = new
            {
                access_token = encodedToken,
                token_result = "success",
                user = userData,
                expires_in = (int)_options.ExpiresSpan.TotalSeconds
            };
            // Serialize and return the response
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response, new JsonSerializerSettings { Formatting = Formatting.Indented, ContractResolver = new CamelCasePropertyNamesContractResolver()}));
            //return new OkObjectResult(response);
        }
    }
}