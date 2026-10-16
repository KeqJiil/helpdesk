namespace Helpdesk.Tickets.IntegrationTests;

/// <summary>
/// Test classes marked <c>[Collection(TicketsApiCollection.Name)]</c> share one app and one SQL container.
/// </summary>
[CollectionDefinition(Name)]
public sealed class TicketsApiCollection : ICollectionFixture<TicketsApiFactory>
{
    public const string Name = "Tickets API";
}
