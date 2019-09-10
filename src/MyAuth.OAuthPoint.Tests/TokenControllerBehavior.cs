using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MyAuth.OAuthPoint.Models;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace MyAuth.OAuthPoint.Tests
{
    using static TestLoginRegistry;
    public class TokenControllerBehavior: IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;
        private readonly ITestOutputHelper _output;
        
        public TokenControllerBehavior(
            TestWebApplicationFactory factory,
            ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }
        
        [Theory]
        [InlineData("Wrong authCode", "foo", "bar", "baz", TokenResponseErrorCode.InvalidClient)]
        [InlineData("Wrong clientId", TestAuthCode, "bar", "baz", TokenResponseErrorCode.InvalidRequest)]
        [InlineData("Wrong code verifier", TestAuthCode, TestClientId, "baz", TokenResponseErrorCode.InvalidRequest)]
        [InlineData("Empty code verifier", TestAuthCode, TestClientId, "", TokenResponseErrorCode.InvalidRequest)]
        public async Task ShouldFailInvalidRequests(
            string desc, 
            string authCode, 
            string clientId, 
            string codeVerifier,
            TokenResponseErrorCode expectedCode)
        {
            //Arrange
            var client = _factory.CreateClient();
            var request = new TokenRequest
            {
                AuthCode = authCode,
                ClientId = clientId,
                CodeVerifier = codeVerifier,
                GrantType = "authorization_code"
            };
            var reqContent = request.ToUrlEncodedContent();

            //Act
            var resp = await client.PostAsync("/token", reqContent);
            var respContent = await resp.Content.ReadAsStringAsync();

            var respError = JsonConvert.DeserializeObject<ErrorTokenResponse>(respContent);
            _output.WriteLine(respError.ErrorDescription);
            
            //Assert
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
            Assert.Equal(expectedCode, respError.ErrorCode);
        }
        
        [Fact]
        public async Task ShouldPassValidRequests()
        {
            //Arrange
            var client = _factory.CreateClient();
            var request = new TokenRequest
            {
                AuthCode = TestAuthCode,
                ClientId = TestClientId,
                CodeVerifier = TestCodeVerifier,
                GrantType = "authorization_code"
            };
            var reqContent = request.ToUrlEncodedContent();
            
            //Act
            var resp = await client.PostAsync("/token", reqContent);
            var respContent = await resp.Content.ReadAsStringAsync();
            _output.WriteLine(respContent);
            
            //Assert
            Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        }
    }
}