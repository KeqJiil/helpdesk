using System.Reflection;
using Helpdesk.SharedKernel.Abstractions;
using NetArchTest.Rules;

namespace Helpdesk.ArchitectureTests;

/// <summary>One test per rule in docs/architecture-rules.md.</summary>
public sealed class ArchitectureRulesTests
{
    private static readonly Assembly SharedKernel = typeof(Helpdesk.SharedKernel.ModuleInfo).Assembly;
    private static readonly Assembly Domain = typeof(Helpdesk.Tickets.Domain.ModuleInfo).Assembly;
    private static readonly Assembly Application = typeof(Helpdesk.Tickets.Application.ModuleInfo).Assembly;
    private static readonly Assembly Infrastructure = typeof(Helpdesk.Tickets.Infrastructure.ModuleInfo).Assembly;
    private static readonly Assembly Presentation = typeof(Helpdesk.Tickets.Presentation.ModuleInfo).Assembly;

    // Rule 1
    [Fact]
    public void Tickets_Domain_may_depend_only_on_SharedKernel_and_System()
    {
        var result = Types.InAssembly(Domain)
            .Should()
            .OnlyHaveDependenciesOn("Helpdesk.Tickets.Domain", "Helpdesk.SharedKernel", "System")
            .GetResult();

        AssertRule(result, "Tickets.Domain may depend only on Helpdesk.SharedKernel and System");
    }

    // Rule 2
    [Fact]
    public void Tickets_Domain_must_not_reference_EntityFrameworkCore_anywhere()
    {
        var result = Types.InAssembly(Domain)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        AssertRule(result, "Tickets.Domain must not reference Microsoft.EntityFrameworkCore");

        // "Anywhere": also no assembly reference, even if no type uses it yet.
        var efReferences = Domain.GetReferencedAssemblies()
            .Where(a => a.Name?.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) == true)
            .Select(a => a.Name)
            .ToList();
        Assert.True(efReferences.Count == 0,
            $"Tickets.Domain must not reference Microsoft.EntityFrameworkCore. Offending assembly references: {string.Join(", ", efReferences)}");
    }

    // Rule 3
    [Fact]
    public void Tickets_Application_must_not_depend_on_Infrastructure_or_Presentation()
    {
        var result = Types.InAssembly(Application)
            .ShouldNot()
            .HaveDependencyOnAny("Helpdesk.Tickets.Infrastructure", "Helpdesk.Tickets.Presentation")
            .GetResult();

        AssertRule(result, "Tickets.Application must not depend on Tickets.Infrastructure or Tickets.Presentation");
    }

    // Rule 4
    [Fact]
    public void Command_and_query_handlers_must_be_internal()
    {
        var handlers = Types.InAssemblies([Domain, Application, Infrastructure, Presentation])
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or().ImplementInterface(typeof(ICommandHandler<,>))
            .Or().ImplementInterface(typeof(IQueryHandler<,>));

        // Guard against a vacuous pass if the selection ever stops matching anything.
        Assert.NotEmpty(handlers.GetTypes());

        var result = handlers.Should().NotBePublic().GetResult();

        AssertRule(result, "Command and query handlers must be internal, not public");
    }

    // Rule 5
    [Fact]
    public void No_type_outside_Tickets_may_depend_on_Tickets_Infrastructure()
    {
        // Every assembly outside Helpdesk.Tickets.*, except the Host composition root (allowed by exception).
        var result = Types.InAssemblies([SharedKernel])
            .ShouldNot()
            .HaveDependencyOn("Helpdesk.Tickets.Infrastructure")
            .GetResult();

        AssertRule(result, "No type outside Helpdesk.Tickets.* may depend on Helpdesk.Tickets.Infrastructure");
    }

    private static void AssertRule(TestResult result, string rule)
    {
        var offenders = result.FailingTypeNames ?? [];
        Assert.True(result.IsSuccessful, $"{rule}. Offending types: {string.Join(", ", offenders)}");
    }
}
