using Microsoft.EntityFrameworkCore;

namespace MyBoards;

public class MyBoardsContext : DbContext
{
    public MyBoardsContext(DbContextOptions<MyBoardsContext> options)
        : base(options) { }

    public DbSet<WorkItem> WorkItems { get; set; }
    public DbSet<Issue> Issues { get; set; }
    public DbSet<Epic> Epics { get; set; }
    public DbSet<Task> Tasks { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Adress> Adresses { get; set; }
    public DbSet<WorkItemState> WorkItemStates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkItemState>().Property(wi => wi.State).IsRequired().HasMaxLength(60);
        modelBuilder
            .Entity<WorkItemState>()
            .HasData(
                new WorkItemState { Id = 1, State = "To Do" },
                new WorkItemState { Id = 2, State = "In progress" },
                new WorkItemState { Id = 3, State = "Done" }
            );

        modelBuilder.Entity<Epic>().Property(e => e.EndDate).HasPrecision(3);

        modelBuilder.Entity<Issue>().Property(wi => wi.Efford).HasColumnType("decimal(5,2)");

        modelBuilder.Entity<Task>(t =>
        {
            t.Property(wi => wi.Activity).HasMaxLength(200);
            t.Property(wi => wi.RemaningWork).HasPrecision(14, 2);
        });

        modelBuilder.Entity<WorkItem>().Property(wi => wi.Area).HasColumnType("varchar(200)");

        modelBuilder.Entity<WorkItem>(eb =>
        {
            eb.HasOne(wi => wi.State).WithMany().HasForeignKey(wi => wi.StateId);
            eb.Property(wi => wi.IterationPath).HasColumnName("Iteration_Path");
            eb.Property(wi => wi.Priority).HasDefaultValue(1);
            eb.HasMany(wi => wi.Comments).WithOne(c => c.WorkItem).HasForeignKey(c => c.WorkItemId);
            eb.HasOne(wi => wi.Author).WithMany(a => a.WorkItems).HasForeignKey(wi => wi.AuthorId);
            eb.HasMany(wi => wi.Tags)
                .WithMany(t => t.WorkItems)
                .UsingEntity<WorkItemTag>(
                    l => l.HasOne(wit => wit.Tag).WithMany().HasForeignKey(wit => wit.TagId),
                    r =>
                        r.HasOne(wit => wit.WorkItem)
                            .WithMany()
                            .HasForeignKey(wit => wit.WorkItemId),
                    wit =>
                    {
                        wit.HasKey(x => new { x.TagId, x.WorkItemId });
                        wit.Property(x => x.PublicationDate)
                            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
                    }
                );
        });

        modelBuilder.Entity<Comment>(eb =>
        {
            eb.Property(c => c.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            eb.Property(c => c.UpdateDate).ValueGeneratedOnUpdate();
        });

        modelBuilder.Entity<User>(eb =>
        {
            eb.HasOne(u => u.Adress).WithOne(a => a.User).HasForeignKey<Adress>(a => a.UserId);
            eb.HasMany(u => u.Comments)
                .WithOne(c => c.Author)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
