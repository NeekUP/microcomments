using Authentication.Controllers;
using Authentication.Controllers.DTO;
using Authentication.Usecases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Net.Http.Headers;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Authentication.Tests
{
    public class UserControllerTests
    {
        [Fact]
        public async Task Register_Ok()
        {
            UserController controller = CreateUserController();
            var request = new CreateUserRequest()
            {
                Email = "valid@email.email",
                Name = "Valid Name",
                Password = "password",
                Fingerprint = Guid.NewGuid().ToString("N")
            };

            var result = await controller.CreateUser( request );

            Assert.NotNull( result );
            Assert.IsType<CreatedResult>( result );
            var jsonResult = result as CreatedResult;
            Assert.NotNull( jsonResult );
            Assert.NotNull( ((CreateTokenResponse)jsonResult.Value).AuthToken );
            Assert.NotNull( ((CreateTokenResponse)jsonResult.Value).RefreshToken );
        }

        private UserController CreateUserController()
        {
            var httpRequest = new Mock<HttpRequest>();
            httpRequest.Setup( a => a.Path ).Returns( "/api/v1/user" );
            httpRequest.Setup( a => a.Headers ).Returns( new HeaderDictionary() { [HeaderNames.UserAgent] = "sadad" } );

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup( a => a.Request ).Returns( httpRequest.Object );

            var serviceProvider = MockServiceProvider();
            var logger = new NullLogger<UserController>();
            var controller = new UserController( serviceProvider, logger )
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext.Object
                }
            };
            return controller;
        }

        private IRegisterHandler MockUserRegister()
        {
            var mock = new Mock<IRegisterHandler>();
            mock.Setup( a => a.Handle( It.IsAny<RegisterRequest>() ) )
                .Returns( Task.FromResult( Result<RegisterResponse>.Ok( new RegisterResponse( Guid.NewGuid(), "name" ) ) ) );

            return mock.Object;
        }

        private ILoginHandler MockUserLogin()
        {
            var mock = new Mock<ILoginHandler>();
            mock.Setup( a => a.Handle( It.IsAny<LoginRequest>() ) )
                .Returns( Task.FromResult( Result<LoginResponse>.Ok( new LoginResponse( Guid.NewGuid().ToString(), Guid.NewGuid().ToString() ) ) ) );

            return mock.Object;
        }

        private IServiceProvider MockServiceProvider()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup( x => x.GetService( typeof( IRegisterHandler ) ) )
                .Returns( MockUserRegister() );

            serviceProvider
                .Setup( x => x.GetService( typeof( ILoginHandler ) ) )
                .Returns( MockUserLogin() );


            return serviceProvider.Object;
        }
    }


}
