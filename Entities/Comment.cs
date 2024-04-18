namespace MyBoards;

public class Comment
{
    public int Id { get; set; }
    public string Message { get; set; }
    public User Author { get; set; }
    public Guid AuthorId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdateDate { get; set; }
    public WorkItem WorkItem { get; set; }
    public int WorkItemId { get; set; } // not necessary, because EF would take care of creating it. But it's conviniet way to add references, and filtering it
}
