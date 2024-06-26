﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyBoards;

public class TaskConfiguration : IEntityTypeConfiguration<Task>
{
    public void Configure(EntityTypeBuilder<Task> builder)
    {
        builder.Property(wi => wi.Activity).HasMaxLength(200);
        builder.Property(wi => wi.RemaningWork).HasPrecision(14, 2);
    }
}
