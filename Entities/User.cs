﻿namespace MyBoards;

public class User
{
    public Guid Id { get; set; }
    // public string FirstName { get; set; }
    // public string LastName { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public virtual Adress Adress { get; set; }
    public virtual List<WorkItem> WorkItems { get; set; } = new List<WorkItem>();
    public virtual List<Comment> Comments { get; set; } = new List<Comment>();
}
