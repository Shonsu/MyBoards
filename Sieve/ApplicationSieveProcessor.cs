﻿using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;

namespace MyBoards;

public class ApplicationSieveProcessor : SieveProcessor
{
    public ApplicationSieveProcessor(
        IOptions<SieveOptions> options,
        ISieveCustomFilterMethods customFilterMethods 
    )
        : base(options, customFilterMethods) { }

    protected override SievePropertyMapper MapProperties(SievePropertyMapper mapper)
    {
        mapper.Property<Epic>(e => e.Priority).CanSort().CanFilter();
        mapper.Property<Epic>(e => e.Area).CanSort().CanFilter();
        mapper.Property<Epic>(e => e.StartDate).CanSort().CanFilter();
        mapper.Property<Epic>(e => e.StateId).CanSort().CanFilter();
        mapper
            .Property<Epic>(e => e.Author.FullName)
            .CanSort()
            .CanFilter()
            .HasName("authorFullName");

        return mapper;
    }
}
