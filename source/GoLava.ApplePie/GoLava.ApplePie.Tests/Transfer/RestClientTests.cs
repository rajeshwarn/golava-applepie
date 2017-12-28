﻿using System.Net;
using System.Threading.Tasks;
using GoLava.ApplePie.Transfer;
using RichardSzalay.MockHttp;
using Xunit;

namespace GoLava.ApplePie.Tests.Transfer
{
    public class RestClientTests
    {
        [Fact]
        public async Task ContentIsHandeledCorrectly()
        {
            var json = "{'foo' : 'Bar'}";

            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect("http://domain.ext/")
                .Respond("application/json", json); // Respond with JSON

            var restClient = new RestClient(mockHttp);
            var context = new RestClientContext();
            var request = RestRequest.Get(new RestUri("http://domain.ext/"));
            var response = await restClient.SendAsync<Contract>(context, request);

            Assert.True(response.IsSuccess, "IsSuccess is false");
            Assert.Equal(RestContentType.Json, response.ContentType);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(json, response.RawContent);
            Assert.Equal(response.Content.Foo, "Bar");

            mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task CookiesAreHandeledCorrectly()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp
                .Expect("http://domain.ext/")
                .WithHeaders("set-cookie", "hello=world")
                .Respond("application/json", "{'foo' : 'Bar'}"); // Respond with JSON

            var restClient = new RestClient(mockHttp);
            var context = new RestClientContext();
            var request = RestRequest.Get(new RestUri("http://domain.ext/"));
            var response = await restClient.SendAsync<Contract>(context, request);

            var cookies = context.CookieJar.GetCookies(new RestUri("http://domain.ext/"));
            Assert.Equal(1, cookies.Count);
            Assert.Equal("hello", cookies[0].Name);
            Assert.Equal("world", cookies[0].Value);

            mockHttp.VerifyNoOutstandingExpectation();
        }

        public class Contract
        {
            public string Foo { get; set; }
        }
    }
}