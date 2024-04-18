using Microsoft.EntityFrameworkCore;
using MyBoards;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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
app.Run();
