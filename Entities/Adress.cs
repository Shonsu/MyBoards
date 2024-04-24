using Microsoft.EntityFrameworkCore;

namespace MyBoards;

public class Adress
{
    public Guid Id { get; set; }
    public string Country { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string PostaCode { get; set; }
    public virtual User User { get; set; }
    public Guid UserId { get; set; }
    // name of FK must be the same as coresponding relation
    // public User Person { get; set; }
    // public Guid PersonId { get; set; }
    public Coordinate Coordinate { get; set; }
}
// [Owned] by owner
public class Coordinate
{
    public decimal? Longitude { get; set; }
    public decimal? Latitude { get; set; }
}
