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
    public DbSet<WorkItemTag> WorkItemTag { get; set; }
    public DbSet<TopAuthor> ViewTopAuthors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // new AddressConfiguration().Configure(modelBuilder.Entity<Adress>()); and soo on
        modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
    }
}
