using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace CulinaryBlog.ArchitectureTests;

/// <summary>NFR-MAINT-004: Dependency Rule của Clean Architecture – dependency chỉ đi vào trong (hướng Domain).</summary>
public class LayerDependencyTests
{
    private const string DomainNs = "CulinaryBlog.Domain";
    private const string ApplicationNs = "CulinaryBlog.Application";
    private const string InfrastructureNs = "CulinaryBlog.Infrastructure";
    private const string ApiNs = "CulinaryBlog.API";

    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            System.Reflection.Assembly.Load(DomainNs),
            System.Reflection.Assembly.Load(ApplicationNs),
            System.Reflection.Assembly.Load(InfrastructureNs),
            typeof(Program).Assembly)
        .Build();

    [Fact]
    public void Domain_ShouldNotDependOn_OuterLayers() =>
        Types().That().ResideInNamespace(DomainNs)
            .Should().NotDependOnAny(Types().That().ResideInNamespace(ApplicationNs)
                .Or().ResideInNamespace(InfrastructureNs)
                .Or().ResideInNamespace(ApiNs))
            .WithoutRequiringPositiveResults()
            .Check(Architecture);

    [Fact]
    public void Application_ShouldNotDependOn_InfrastructureOrApi() =>
        Types().That().ResideInNamespace(ApplicationNs)
            .Should().NotDependOnAny(Types().That().ResideInNamespace(InfrastructureNs)
                .Or().ResideInNamespace(ApiNs))
            .WithoutRequiringPositiveResults()
            .Check(Architecture);

    [Fact]
    public void Infrastructure_ShouldNotDependOn_Api() =>
        Types().That().ResideInNamespace(InfrastructureNs)
            .Should().NotDependOnAny(Types().That().ResideInNamespace(ApiNs))
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
}
