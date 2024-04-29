using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyBoards;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasOne(u => u.Adress).WithOne(a => a.User).HasForeignKey<Adress>(a => a.UserId);
        builder
            .HasMany(u => u.Comments)
            .WithOne(c => c.Author)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.ClientCascade);
        // builder.HasIndex(u => u.Email);
        // in case of composite index from with two columns/properties
        builder.HasIndex(u => new { u.Email, u.FullName });
    }
}
