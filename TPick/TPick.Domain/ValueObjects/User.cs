﻿#nullable enable
using CsMicro.Core;

namespace TPick.Domain.ValueObjects;

public class User : ValueObject
{
    public Guid Id { get; }
    public string Name { get; set; } = null!;
    public string? Momo { get; set; } = null!;

    private User()
    {
    }

    public User(Guid id, string name, string? momo)
    {
        Id = id;
        Name = name;
        Momo = momo;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return Name;
        if (Momo is not null) yield return Momo;
    }
}