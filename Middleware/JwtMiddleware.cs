using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;


namespace Middleware
{
    public class JwtMiddleware : IMiddleware
    {

        private readonly IJwtBuilder _jwtBuilder;

        public JwtMiddleware(IJwtBuilder jwtBuilder)
        {
            _jwtBuilder = jwtBuilder;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Get the token from the Authorization header
            var bearer = context.Request.Headers["Authorization"].ToString();
            var token = bearer.Replace("Bearer ", string.Empty);

            if (!string.IsNullOrEmpty(token))
            {
                // Verify the token using the IJwtBuilder
                var userId = _jwtBuilder.ValidateToken(token);

                if (ObjectId.TryParse(userId, out _))
                {
                    // Store the userId in the HttpContext items for later use
                    context.Items["userId"] = userId;
                }
                else
                {
                    // If token or userId are invalid, send 401 Unauthorized status
                    context.Response.StatusCode = 401;
                }
            }

            // Continue processing the request
            await next(context);
        }

    }
}
