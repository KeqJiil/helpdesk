using Helpdesk.Tickets.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Helpdesk.Tickets.Infrastructure.Persistence.Configurations;

internal sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets", "tickets");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Ignore(t => t.DomainEvents);

        // Covering index over open tickets only; closed tickets stay out of it.
        builder.HasIndex(t => t.Id)
            .HasFilter("[Status] = N'Open'")
            .IncludeProperties(t => t.Status)
            .HasDatabaseName("IX_Tickets_Open");

        // Child tasks are part of the aggregate: owned, loaded with the ticket, no DbSet of their own.
        builder.OwnsMany(t => t.ChildTasks, child =>
        {
            child.ToTable("ChildTasks", "tickets");
            child.WithOwner().HasForeignKey("TicketId");

            child.HasKey(c => c.Id);
            child.Property(c => c.Id).ValueGeneratedNever();

            child.Property(c => c.Description).HasMaxLength(500).IsRequired();
            child.Property(c => c.IsDone).IsRequired();

            // Open (not done) child tasks per ticket; used e.g. by the "can close" check.
            child.HasIndex("TicketId")
                .HasFilter("[IsDone] = 0")
                .HasDatabaseName("IX_ChildTasks_TicketId_Open");
        });

        builder.Navigation(t => t.ChildTasks).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
