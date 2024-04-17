using Microsoft.EntityFrameworkCore;

namespace MyBoards;

public class MyBoardsContext : DbContext
{
    public MyBoardsContext(DbContextOptions<MyBoardsContext> options) : base(options)
    {
    }
    public DbSet<WorkItem> WorkItems { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Adress> Adresses { get; set; }
    public DbSet<WorkItemState> WorkItemStates { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //base.OnModelCreating(modelBuilder);
        // creating composite key example
        // modelBuilder.Entity<User>()
        // .HasKey(u => new { u.LastName, u.Email });

        modelBuilder.Entity<WorkItemState>()
        .Property(wi => wi.State)
        .IsRequired()
        .HasMaxLength(50);

        // modelBuilder.Entity<WorkItemState>()
        // .HasMany(wis=>wis.WorkItems)
        // .WithOne(wi=>wi.State)
        // .HasForeignKey(wi=>wi.StateId);

        modelBuilder.Entity<WorkItem>()
        .Property(wi => wi.Area)
        .HasColumnType("varchar(200)");

        modelBuilder.Entity<WorkItem>(eb =>
            {
                eb.HasOne(wi=>wi.State).WithMany().HasForeignKey(wi=>wi.StateId);
                eb.Property(wi => wi.IterationPath).HasColumnName("Iteration_Path");
                eb.Property(wi => wi.EndDate).HasPrecision(3);
                eb.Property(wi => wi.Efford).HasColumnType("decimal(5,2)");
                eb.Property(wi => wi.Activity).HasMaxLength(200);
                eb.Property(wi => wi.RemaningWork).HasPrecision(14, 2);
                eb.Property(wi => wi.Priority).HasDefaultValue(1);
                eb.HasMany(wi => wi.Comments).WithOne(c => c.WorkItem).HasForeignKey(c => c.WorkItemId);
                eb.HasOne(wi => wi.Author).WithMany(a => a.WorkItems).HasForeignKey(wi => wi.AuthorId);
                eb.HasMany(wi => wi.Tags).WithMany(t => t.WorkItems).UsingEntity<WorkItemTag>(
                        l => l.HasOne(wit => wit.Tag).WithMany().HasForeignKey(wit => wit.TagId),
                        r => r.HasOne(wit => wit.WorkItem).WithMany().HasForeignKey(wit => wit.WorkItemId),
                        wit =>
                        {
                            wit.HasKey(x => new { x.TagId, x.WorkItemId });
                            wit.Property(x => x.PublicationDate).HasDefaultValueSql("getutcdate()");
                        });
            }
        );
        modelBuilder.Entity<Comment>(eb =>
            {
                eb.Property(c => c.CreatedDate).HasDefaultValueSql("getutcdate()");
                eb.Property(c => c.UpdateDate).ValueGeneratedOnUpdate();
            }
        );
        modelBuilder.Entity<User>()
        .HasOne(u => u.Adress)
        .WithOne(a => a.User)
        .HasForeignKey<Adress>(a => a.UserId);

        // modelBuilder.Entity<WorkItemTag>()
        // .HasKey(wit => new { wit.TagId, wit.WorkItemId });
    }
}
