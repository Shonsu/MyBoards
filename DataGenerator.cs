using Bogus;

namespace MyBoards;

public class DataGenerator
{
    public static async void Seed(MyBoardsContext db)
    {
        string locale = "pl";
        List<User> users = new List<User>();
        for (int i = 0; i < 100; i++)
        {
            users.Add(UserWithAddressGenerator(locale).Generate());
        }
        await db.AddRangeAsync(users);
        await db.SaveChangesAsync();
    }

    private static Faker<Adress> AddressGenerator(string locale)
    {
        return new Faker<Adress>(locale: locale)
            // .StrictMode(true)
            .RuleFor(a => a.City, f => f.Address.City())
            .RuleFor(a => a.Country, f => f.Address.Country())
            .RuleFor(a => a.PostaCode, f => f.Address.ZipCode())
            .RuleFor(a => a.Street, f => f.Address.StreetName());
    }

    private static Faker<User> UserWithAddressGenerator(string locale)
    {
        return new Faker<User>(locale: locale)
            .RuleFor(u => u.FullName, f => f.Person.FullName)
            .RuleFor(u => u.Email, f => f.Person.Email)
            .RuleFor(u => u.Adress, AddressGenerator(locale: locale).Generate());
    }
}
