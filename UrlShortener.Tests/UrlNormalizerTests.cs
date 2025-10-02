using System;
using FluentAssertions;
using UrlShortener.Application.Abstractions;
using UrlShortener.Infrastructure.Services;

namespace UrlShortener.Tests;

/// <summary>
/// Tests for URL normalization logic.
/// </summary>
public class UrlNormalizerTests
{
    private readonly IUrlNormalizer _normalizer = new UrlNormalizer();

    [Theory]
    [InlineData("HTTP://Example.com", "http://example.com/")]
    [InlineData("https://Example.com", "https://example.com/")]
    [InlineData("https://example.com", "https://example.com/")]
    [InlineData("https://example.com/", "https://example.com/")]
    public void Lowercases_host_and_keeps_scheme(string input, string expectedPrefix)
        => _normalizer.Normalize(input).Should().StartWith(expectedPrefix);

    
    [Theory]
    [InlineData("https://example.com////path//to///file", "https://example.com/path/to/file")]
    [InlineData("https://example.com/path/./to/../here", "https://example.com/path/here")]
    public void Collapses_redundant_path_segments(string input, string expected)
        => _normalizer.Normalize(input).Should().Be(expected);

    [Theory]
    [InlineData("https://example.com:443", "https://example.com/")]
    [InlineData("http://example.com:80", "http://example.com/")]
    [InlineData("https://example.com:8443", "https://example.com:8443/")]
    public void Removes_default_ports(string input, string expected)
        => _normalizer.Normalize(input).Should().Be(expected);

    [Theory]
    [InlineData("https://example.com?b=2&a=1", "https://example.com/?a=1&b=2")]
    [InlineData("https://example.com?z=9&z=1&a=2", "https://example.com/?a=2&z=1&z=9")]
    public void Sorts_query_params(string input, string expected)
        => _normalizer.Normalize(input).Should().Be(expected);

    
    [Fact]
    public void Drops_trailing_question_mark_without_params()
        => _normalizer.Normalize("https://example.com?").Should().Be("https://example.com/");

    [Fact]
    public void Throws_on_invalid_uri()
        => ((Action)(() => _normalizer.Normalize("not a url")))
           .Should().Throw<UriFormatException>();

    [Theory]
    [InlineData("  https://example.com  ", "https://example.com/")]
    [InlineData("\t\nhttps://EXAMPLE.com\t", "https://example.com/")]
    public void Trims_and_normalizes(string input, string expected)
        => _normalizer.Normalize(input).Should().Be(expected);
}
