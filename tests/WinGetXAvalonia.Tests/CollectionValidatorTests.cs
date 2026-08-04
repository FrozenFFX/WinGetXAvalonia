// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Domain.Collections;

namespace WinGetXAvalonia.Tests;

public class CollectionValidatorTests
{
    [Theory]
    [InlineData("Git Tools", true)]
    [InlineData("Visual Studio Code", true)]
    [InlineData("  Git Tools", false)]
    [InlineData("Git Tools ", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    [InlineData("a:b", false)]
    [InlineData("a/b", false)]
    [InlineData("a\\b", false)]
    [InlineData("a|b", false)]
    [InlineData("a?b", false)]
    public void IsValidName_ValidatesNames(string? name, bool expected)
        => Assert.Equal(expected, CollectionValidator.IsValidName(name));

    [Fact]
    public void IsValidName_EnforcesMaxLength()
    {
        Assert.False(CollectionValidator.IsValidName(new string('a', CollectionValidator.MaxNameLength + 1)));
        Assert.True(CollectionValidator.IsValidName(new string('a', CollectionValidator.MaxNameLength)));
    }

    [Fact]
    public void NormalizeName_TrimsWhitespace()
    {
        Assert.Equal("Git", CollectionValidator.NormalizeName("  Git  "));
        Assert.Equal("", CollectionValidator.NormalizeName("   "));
        Assert.Equal("", CollectionValidator.NormalizeName(null));
    }
}
