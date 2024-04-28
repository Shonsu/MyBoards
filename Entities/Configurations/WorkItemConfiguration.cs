using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyBoards;

public class WorkItemConfiguration : IEntityTypeConfiguration<WorkItem>
{
    public void Configure(EntityTypeBuilder<WorkItem> builder)
    {
        builder.Property(wi => wi.Area).HasColumnType("varchar(200)");

        builder.HasOne(wi => wi.State).WithMany().HasForeignKey(wi => wi.StateId);
        builder.Property(wi => wi.IterationPath).HasColumnName("Iteration_Path");
        builder.Property(wi => wi.Priority).HasDefaultValue(1);
        builder
            .HasMany(wi => wi.Comments)
            .WithOne(c => c.WorkItem)
            .HasForeignKey(c => c.WorkItemId);
        builder.HasOne(wi => wi.Author).WithMany(a => a.WorkItems).HasForeignKey(wi => wi.AuthorId);
        builder
            .HasMany(wi => wi.Tags)
            .WithMany(t => t.WorkItems)
            .UsingEntity<WorkItemTag>(
                l => l.HasOne(wit => wit.Tag).WithMany().HasForeignKey(wit => wit.TagId),
                r => r.HasOne(wit => wit.WorkItem).WithMany().HasForeignKey(wit => wit.WorkItemId),
                wit =>
                {
                    wit.HasKey(x => new { x.TagId, x.WorkItemId });
                    wit.Property(x => x.PublicationDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
                }
            );
    }
}
