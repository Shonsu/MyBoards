using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyBoards;

public class WorkItemStateConfiguration : IEntityTypeConfiguration<WorkItemState>
{
    public void Configure(EntityTypeBuilder<WorkItemState> builder)
    {
        builder.Property(wi => wi.State).IsRequired().HasMaxLength(60);
        builder.HasData(
            new WorkItemState { Id = 1, State = "To Do" },
            new WorkItemState { Id = 2, State = "In progress" },
            new WorkItemState { Id = 3, State = "Done" }
        );
    }
}
