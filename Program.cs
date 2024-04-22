using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBoards;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// builder.Services.AddControllers().AddJsonOptions(options =>
// {
//     options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
// options.JsonSerializerOptions.MaxDepth = 6;
// options.JsonSerializerOptions.WriteIndented = true;
// });
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    //options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddDbContext<MyBoardsContext>(option =>
    option.UseMySql(
        builder.Configuration.GetConnectionString("MyBoardsConnectionString"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("MyBoardsConnectionString")
        )
    )
);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetService<MyBoardsContext>();
IEnumerable<string> pendingMigrations = dbContext.Database.GetPendingMigrations();
if (pendingMigrations.Any())
{
    dbContext.Database.Migrate();
}

List<User> users = dbContext.Users.ToList();
if (users.Count == 0)
{
    var user1 = new User()
    {
        Email = "gamer@test.com",
        FullName = "Gamer One",
        Adress = new Adress() { City = "Warszawa", Street = "Szeroka" }
    };
    var user2 = new User()
    {
        Email = "rocky@test.com",
        FullName = "Rocky Balboa",
        Adress = new Adress() { City = "Pułtusk", Street = "Leśna" }
    };
    dbContext.Users.AddRange([user1, user2]);
    await dbContext.SaveChangesAsync();
}

app.MapGet("tag", (MyBoardsContext db) => db.Tags.ToList());
app.MapGet(
    "newestcomment",
    async ([FromQuery] int newest, MyBoardsContext db) =>
        await db
            .Comments.AsNoTracking()
            .OrderByDescending(c => c.CreatedDate)
            .Take(newest)
            .ToListAsync()
);
app.MapGet(
    "comment",
    async ([FromQuery(Name = "afterdate")] DateTime afterDate, MyBoardsContext db) =>
    {
        return await db
            .Comments.Where(c => c.CreatedDate > afterDate)
            .OrderBy(c => c.CreatedDate)
            .ToListAsync();
    }
);
app.MapGet(
    "workitem",
    ([FromQuery(Name = "stateid")] int stateId, MyBoardsContext db) =>
    {
        return db.WorkItems.Where(wi => wi.StateId == stateId).ToList();
    }
);
app.MapGet(
    "workitem/groupbystate",
    (MyBoardsContext db) =>
        db
            .WorkItems.GroupBy(wi => wi.State.State)
            .Select(g => new { state = g.Key, count = g.Count() })
            .ToListAsync()
);
app.MapGet(
    "epic",
    (MyBoardsContext db) =>
    {
        User user = db.Users.First(u => u.FullName == "User One");
        var entries1 = db.ChangeTracker.Entries();
        user.Email = "nowy@email.pl";
        var entries2 = db.ChangeTracker.Entries();
        db.SaveChanges();
        //Epic epic = db.Epics.First();
        return new { user };
    }
);
app.MapGet(
    "epiconhold",
    async (MyBoardsContext db) =>
    {
        return await db
            .Epics.Where(e => e.State.State == "On Hold")
            .OrderBy(e => e.Priority)
            .ToListAsync();
    }
);
app.MapGet(
    "userwithmostcomment",
    async (MyBoardsContext db) =>
    {
        // first way
        // var user = await db
        //     .Comments.GroupBy(c => c.AuthorId)
        //     .Select(g => new { authorId = g.Key, count = g.Count() })
        //     .OrderByDescending(x => x.count)
        //     .Take(1)
        //     .ToListAsync();
        // return await db.Users.FindAsync(user[0].authorId);

        // second way
        var userCommentCounts = await db
            .Comments.GroupBy(c => c.AuthorId)
            .Select(g => new { authorId = g.Key, Count = g.Count() })
            .ToListAsync();
        var topAuthor = userCommentCounts.First(x =>
            x.Count == userCommentCounts.Max(acc => acc.Count)
        );
        var userDetails = await db.Users.FindAsync(topAuthor.authorId);
        return new { userDetails, commentCount = topAuthor.Count };
    }
);

app.MapPut(
    "epic/{epicId}",
    async ([FromRoute] int epicId, [FromBody] EpicDto epicDto, MyBoardsContext db) =>
    {
        Epic epic = await db.Epics.FirstAsync(epic => epic.Id == epicId);
        epic.Area = epicDto.Area;
        epic.Priority = epicDto.Priority;
        epic.StartDate = epicDto.StartDate;
        await db.SaveChangesAsync();
        return epic;
    }
);
app.MapPatch(
    "epic/{epicId}/state",
    async ([FromRoute] int epicId, [FromBody] ChangeStateDto parameters, MyBoardsContext db) =>
    {
        Epic epic = await db.Epics.FirstAsync(epic => epic.Id == epicId);
        if (parameters.StateId != null)
        {
            epic.StateId = (int)parameters.StateId;
        }
        else if (!string.IsNullOrEmpty(parameters.State))
        {
            var onHoldState = await db.WorkItemStates.FirstAsync(s => s.State == parameters.State);
            epic.StateId = onHoldState.Id;
            //epic.State = onHoldState;
        }
        await db.SaveChangesAsync();
        return epic;
    }
);
app.MapPost(
    "tag",
    async ([FromBody] TagDto tagDto, MyBoardsContext db) =>
    {
        Tag newTag = new Tag { Value = tagDto.Value };
        await db.Tags.AddAsync(newTag);
        //await db.AddAsync(newTag);
        await db.SaveChangesAsync();
        return newTag;
    }
);
app.MapPost(
    "tagrange",
    async ([FromBody] List<TagDto> tagDtos, MyBoardsContext db) =>
    {
        List<Tag> tags = new List<Tag>();
        foreach (TagDto tagDto in tagDtos)
        {
            tags.Add(new Tag { Value = tagDto.Value });
        }
        await db.Tags.AddRangeAsync(tags);
        // await db.AddRangeAsync(tags);
        await db.SaveChangesAsync();
        return tags;
    }
);

app.MapPost(
    "user",
    async ([FromBody] UserAddressDto userAddressDto, MyBoardsContext db) =>
    {
        Adress adress = new Adress
        {
            City = userAddressDto.City,
            PostaCode = userAddressDto.PostaCode,
            Street = userAddressDto.Street,
            Country = userAddressDto.Country
        };
        //User user = await db.Users.FindAsync(userId);
        User user = new User
        {
            FullName = userAddressDto.FullName,
            Email = userAddressDto.Email,
            Adress = adress
        };
        await db.Users.AddAsync(user);
        //await db.AddAsync(newTag);
        await db.SaveChangesAsync();
        return new UserAddressDto
        {
            FullName = user.FullName,
            Email = user.Email,
            Country = user.Adress.Country,
            City = user.Adress.City,
            Street = user.Adress.Street,
            PostaCode = user.Adress.PostaCode
        };
    }
);

app.MapGet(
    "user/{userId}",
    async ([FromRoute] Guid userId, MyBoardsContext db) =>
    {
        User user = await db
            .Users.Include(u => u.Comments)
            .ThenInclude(c => c.WorkItem)
            .Include(u => u.Adress)
            .FirstAsync(u => u.Id == userId);
        // List<Comment> comments = await db.Comments.Where(c => c.AuthorId == userId).ToListAsync();
        // relation between user and comments is automatically created
        // var userComments = user.Comments;
        return user;
    }
);
app.MapDelete(
    "workitemtag/{workItemId}/{workItemId2}",
    async ([FromRoute] int workItemId, [FromRoute] int workItemId2, MyBoardsContext db) =>
    {
        List<WorkItemTag> workItemTags = await db
            .WorkItemTag.Where(wit => wit.WorkItemId == workItemId)
            .ToListAsync();
        db.WorkItemTag.RemoveRange(workItemTags);

        var wortItem = await db.WorkItems.FirstAsync(wi => wi.Id == workItemId2);
        db.Remove(wortItem);

        await db.SaveChangesAsync();
    }
);
app.MapDelete(
    "user/{userId}",
    async ([FromRoute] Guid userId, MyBoardsContext db) =>
    {
        User user = await db.Users.FirstAsync(u => u.Id == userId);
        List<Comment> comments = await db.Comments.Where(c => c.AuthorId == userId).ToListAsync();
        db.Comments.RemoveRange(comments);
        await db.SaveChangesAsync();

        db.Users.Remove(user);
        await db.SaveChangesAsync();
    }
);
app.MapDelete(
    "userwithcomments/{userId}",
    async ([FromRoute] Guid userId, MyBoardsContext db) =>
    {
        // with OnDelete(DeleteBehavior.ClientCascade) EF will take care about deleting user's comments too
        User user = await db.Users.Include(u => u.Comments).FirstAsync(u => u.Id == userId);

        db.Users.Remove(user);
        await db.SaveChangesAsync();
    }
);
app.MapDelete(
    "workitem/{workitemId}",
    ([FromRoute] int workitemId, MyBoardsContext db) =>
    {
        var workItem = new Epic() { Id = workitemId };
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Epic> epic = db.Attach(workItem);
        epic.State = EntityState.Deleted;
        db.SaveChanges();
    }
);

// cascade delete
// SELECT * FROM myboards.WorkItems WHERE Id = 21;
// SELECT * FROM myboards.Comments WHERE WorkItemId = 21;
// SELECT * FROM myboards.WorkItemTag WHERE WorkItemId = 21;
// DELETE FROM myboards.WorkItems WHERE Id = 21;

// SELECT * FROM myboards.Comments WHERE AuthorId = "6afc3a1d-cf04-4d8a-cbcf-08da10ab0e61";
// SELECT * FROM WorkItems WHERE AuthorId = "6afc3a1d-cf04-4d8a-cbcf-08da10ab0e61";
// SELECT * FROM Users WHERE Id = "6afc3a1d-cf04-4d8a-cbcf-08da10ab0e61";
// DELETE FROM Comments WHERE AuthorId = "6afc3a1d-cf04-4d8a-cbcf-08da10ab0e61";
// DELETE FROM Users WHERE Id = "6afc3a1d-cf04-4d8a-cbcf-08da10ab0e61";

app.Run();
