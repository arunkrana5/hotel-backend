using NetArchTest.Rules;
using Xunit;

namespace HotelOS.ArchitectureTests;

public class ArchitectureBoundaryTests
{
    private const string DomainNamespace = "HotelOS.Domain";
    private const string ApplicationNamespace = "HotelOS.Application";
    private const string InfrastructureNamespace = "HotelOS.Infrastructure";
    private const string ApiNamespace = "HotelOS.Api";

    [Fact]
    public void Domain_Should_Not_Have_Dependency_On_Other_Projects()
    {
        var result = Types.InAssembly(typeof(HotelOS.Domain.Common.BaseEntity).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain layer must have no dependencies on outer layers.");
    }

    [Fact]
    public void Application_Should_Not_Have_Dependency_On_Infrastructure_Or_Api()
    {
        var result = Types.InAssembly(typeof(HotelOS.Application.Common.Interfaces.IAuthService).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "Application layer must not depend on Infrastructure or Api.");
    }
}
