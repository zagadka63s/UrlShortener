using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using UrlShortener.Application.Abstractions;
using UrlShortener.Infrastructure.Services;

namespace UrlShortener.Tests;

/// <summary>
/// Tests for the ShortCodeGenerator.
/// </summary>
public class ShortCodeGeneratorTests
{
    private readonly IShortCodeGenerator _gen = new ShortCodeGenerator();

    [Fact]
    public void Generates_non_empty()
        => _gen.Next().Should().NotBeNullOrWhiteSpace();

    [Fact]
    public void Has_reasonable_length()
        => _gen.Next().Length.Should().BeInRange(6, 12); 

    [Fact]
    public void Allowed_characters_only()
        => _gen.Next().Should().MatchRegex("^[A-Za-z0-9_-]+$");

    [Fact]
    public void Mostly_unique_in_bulk()
    {
        var set = new HashSet<string>();
        for (int i = 0; i < 10_000; i++) set.Add(_gen.Next());
        set.Count.Should().BeGreaterThan(9900);
    }
}
