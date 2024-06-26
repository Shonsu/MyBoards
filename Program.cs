using System.Linq.Expressions;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBoards;
using Sieve.Models;
using Sieve.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ISieveProcessor, ApplicationSieveProcessor>();
builder.Services.AddScoped<ISieveCustomFilterMethods, ApplicationSieveFilterMethods>();
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
    option
    // .UseLazyLoadingProxies()
    .UseMySql(
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
DataGenerator.Seed(dbContext);

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
app.MapGet(
    "rawsql",
    (MyBoardsContext db) =>
    {
        int wiNumber = 82;
        // var states = db
        //     .WorkItemStates.FromSqlRaw(
        //         @"SELECT wis.State, wis.Id, count(*) as number
        //             FROM myboards.WorkItemStates as wis
        //             JOIN myboards.WorkItems as wi on wis.Id = wi.StateId
        //             GROUP BY wis.Id, wis.State
        //             HAVING count(*) > @number",
        //         new MySqlParameter("number", wiNumber)
        //     )
        //     .ToList();

        var states = db
            .WorkItemStates.FromSqlInterpolated(
                $@"SELECT wis.State, wis.Id, count(*) as number
                    FROM myboards.WorkItemStates as wis
                    JOIN myboards.WorkItems as wi on wis.Id = wi.StateId 
                    GROUP BY wis.Id, wis.State 
                    HAVING count(*) > {wiNumber}"
            )
            .ToList();

        db.Database.ExecuteSqlRaw(
            @"
            UPDATE Comments
            SET UpdateDate = NOW()
            WHERE AuthorId = '1930065b-4517-407f-cbee-08da10ab0e61'"
        );
        return states;
    }
);
app.MapGet(
    "topAuthor",
    async (MyBoardsContext db) =>
    {
        List<TopAuthor> topAuthors = await db.ViewTopAuthors.ToListAsync();
        return topAuthors;
    }
);
app.MapGet(
    "address/coordinate/{addressId}",
    async ([FromRoute] Guid addressId, MyBoardsContext db) =>
    {
        List<Adress> adresses = await db
            .Adresses.Where(a => a.Id == addressId && a.Coordinate.Latitude > 10) // without include Coordinate
            .ToListAsync();
        return adresses;
    }
);

app.MapGet(
    "user/{userId}/{withAddress}",
    async ([FromRoute] Guid userId, [FromRoute] bool withAddress, MyBoardsContext db) =>
    {
        //var withAddress = true;
        User user = await db.Users.FirstAsync(u => u.Id == userId);
        if (withAddress)
        {
            var result = new
            {
                FullName = user.FullName,
                Adress = $"{user.Adress.Street} {user.Adress.City}"
            };
            return result;
        }
        return new { FullName = user.FullName, Adress = "-" };
    }
);

app.MapGet(
    "userwithpagination",
    async ([AsParameters] PageQueryProperties pageQueryProperties, MyBoardsContext db) =>
    {
        var filter = pageQueryProperties.Filter;
        string sortBy = pageQueryProperties.SortBy; // FullName Email null
        bool sortByDescending = pageQueryProperties.SortByDescending;
        int pageNumber = pageQueryProperties.PageNumber;
        int pageSize = pageQueryProperties.PageSize;

        IQueryable<User> query = db.Users.Where(u =>
            string.IsNullOrEmpty(filter)
            || u.FullName.ToLower().Contains(filter.ToLower())
            || u.Email.ToLower().Contains(filter.ToLower())
        );
        int totalCount = query.Count();
        if (!string.IsNullOrEmpty(sortBy))
        {
            // Expression<Func<User, object>> sortByExpression = user => user.Email;
            var columnSelector = new Dictionary<string, Expression<Func<User, object>>>
            {
                { nameof(User.Email), user => user.Email },
                { nameof(User.FullName), user => user.FullName }
            };
            var sortByExpression = columnSelector[sortBy];
            query = sortByDescending
                ? query.OrderByDescending(sortByExpression)
                : query.OrderBy(sortByExpression);
        }

        var result = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        var pageResult = new PageResult<User>(result, totalCount, pageSize, pageNumber);
        return pageResult;
    }
);

app.MapGet(
    "usersfrom",
    async ([FromQuery] string country, MyBoardsContext db) =>
    {
        // var users = db
        //     .Users.Include(u => u.Adress)
        //     .Where(u => u.Adress.Country == country)
        //     .Select(u=> u.FullName)
        //     .ToListAsync();
        var usersWithComments = await db
            .Users.Include(u => u.Adress)
            .Include(u => u.Comments)
            .Where(u => u.Adress.Country == country)
            .Select(u => new
            {
                u.FullName,
                commentMessages = u.Comments.Select(c => c.Message).ToList()
            })
            //.Select(c=>c.Message)
            .ToListAsync();
        //var comments = users.SelectMany(u => u.Comments).Select(c=>c.Message);
        return usersWithComments;
    }
);
app.MapGet(
    "nplusone",
    async ([FromQuery] string country, MyBoardsContext db) =>
    {
        var users = await db
            .Users.Include(u => u.Adress)
            .Include(u => u.Comments)
            .Where(u => u.Adress.Country.ToLower() == country.ToLower())
            .ToListAsync();
        foreach (var user in users)
        {
            //var comments = db.Comments.Where(c => c.AuthorId == user.Id).ToList();
            foreach (var comment in user.Comments)
            {
                // process (comment)
            }
        }
        //return users;
    }
);
app.MapPost(
    "sieve",
    async ([FromBody] SieveModel query, ISieveProcessor sieveProcessor, MyBoardsContext db) =>
    {
        var epics = db.Epics.Include(e => e.Author).AsQueryable();
        var epicDtos = await sieveProcessor
            .Apply(query, epics)   
            .Select(e => new EpicDTOSieve
            {
                Id = e.Id, 
                Area = e.Area,
                Priority = e.Priority,
                StartDate = e.StartDate,
                AuthorFullName = e.Author.FullName,
                StateId = e.StateId
            })
            .ToListAsync();

        int totalCount = await sieveProcessor
            .Apply(query, epics, applyPagination: false, applySorting: false)
            .CountAsync();

        var result = new PageResult<EpicDTOSieve>(
            epicDtos,
            totalCount,
            query.PageSize.Value,
            query.Page.Value
        );
        return result;
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

// SELECT u.Id as id,  u.FullName as fullname, count(*) as WorkItemsCreated FROM myboards.Users as u
// JOIN WorkItems as wi on wi.AuthorId = u.Id group by u.Id, u.FullName order by WorkItemsCreated desc LIMIT 5;
app.Run();
