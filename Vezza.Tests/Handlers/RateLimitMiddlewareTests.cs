using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Vezza.Api.Security;

namespace Vezza.Tests.Handlers;

[TestClass]
public class RateLimitMiddlewareTests
{
    [TestMethod]
    public async Task EndpointPublico_AposLimite_DeveRetornarTooManyRequests()
    {
        var nextCalls = 0;
        var configuration = new ConfigurationBuilder().Build();
        var middleware = new PublicEndpointRateLimitMiddleware(
            _ =>
            {
                nextCalls++;
                return Task.CompletedTask;
            },
            configuration);

        for (var request = 1; request <= 6; request++)
        {
            var context = CreateContext();

            await middleware.InvokeAsync(context);

            if (request <= 5)
                Assert.AreNotEqual(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
            else
                Assert.AreEqual(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        }

        Assert.AreEqual(5, nextCalls);
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/v1/usuarios/autenticar";
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.10");
        context.Response.Body = new MemoryStream();
        return context;
    }
}
