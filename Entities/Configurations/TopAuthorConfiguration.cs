using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyBoards;

public class TopAuthorConfiguration : IEntityTypeConfiguration<TopAuthor>
{
    public void Configure(EntityTypeBuilder<TopAuthor> builder)
    {
        builder.ToView("View_TopAuthors");
        builder.HasNoKey();
    }
}
