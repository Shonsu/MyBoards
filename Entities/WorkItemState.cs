using System.ComponentModel.DataAnnotations;

namespace MyBoards;

public class WorkItemState
{
    public int Id { get; set; }
    // [Required]
    // [MaxLength(50)]
    public string State { get; set; }
    public List<WorkItem> WorkItems { get; set; }
}
