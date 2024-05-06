using Microsoft.EntityFrameworkCore;
using Sieve.Services;

namespace MyBoards;

public class ApplicationSieveFilterMethods : ISieveCustomFilterMethods
{
    public IQueryable<T> HasInArea<T>(IQueryable<T> source, string op, string[] values)
        where T : WorkItem // Generic functions are allowed too
    {
        var result = source.Where(e => e.Area.Contains(values[0]));
        return result;
    }

    public IQueryable<T> WorkItemsOfTop5Authors<T>(IQueryable<T> source, string op, string[] values)
        where T : WorkItem
    {
        var top5Authors = source
            .GroupBy(wi => wi.AuthorId)
            .Select(g => new { authorId = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .Take(5)
            .AsQueryable();

        var result = source.Join(
            top5Authors,
            wi => wi.AuthorId,
            a => a.authorId,
            (wi, authors) => wi
        );

        return result;
    }
}
