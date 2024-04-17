using Microsoft.EntityFrameworkCore;
using MyBoards;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<MyBoardsContext>(option =>
    option.UseMySql(builder.Configuration.GetConnectionString("MyBoardsConnectionString"),
                    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MyBoardsConnectionString")))
);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
