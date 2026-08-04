// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using WinGetXAvalonia.Core.Runtime;

namespace WinGetXAvalonia.Tests;

public class RuntimeFeatureManagerTests
{
    private static RuntimeFeature Feature(string id, bool enabled = true, bool experimental = false)
        => new() { Id = id, Name = id, Enabled = enabled, Experimental = experimental };

    [Fact]
    public void IsEnabled_RegisteredAndEnabled_ReturnsTrue()
    {
        var manager = new RuntimeFeatureManager();
        manager.Register(Feature("Founder"));

        Assert.True(manager.IsEnabled("Founder"));
    }

    [Fact]
    public void IsEnabled_IsCaseInsensitive()
    {
        var manager = new RuntimeFeatureManager();
        manager.Register(Feature("founder"));

        Assert.True(manager.IsEnabled("FOUNDER"));
    }

    [Fact]
    public void IsEnabled_NotRegistered_ReturnsFalse()
    {
        var manager = new RuntimeFeatureManager();

        Assert.False(manager.IsEnabled("Missing"));
    }

    [Fact]
    public void IsEnabled_DisabledFeature_ReturnsFalse()
    {
        var manager = new RuntimeFeatureManager();
        manager.Register(Feature("Founder", enabled: false));

        Assert.False(manager.IsEnabled("Founder"));
    }

    [Fact]
    public void Enable_TurnsOnRegisteredFeature()
    {
        var manager = new RuntimeFeatureManager();
        manager.Register(Feature("Founder", enabled: false));

        manager.Enable("Founder");

        Assert.True(manager.IsEnabled("Founder"));
    }

    [Fact]
    public void Enable_NotRegistered_DoesNotThrow()
    {
        var manager = new RuntimeFeatureManager();

        manager.Enable("Missing");
    }

    [Fact]
    public void IsExperimental_RegisteredExperimental_ReturnsTrue()
    {
        var manager = new RuntimeFeatureManager();
        manager.Register(Feature("Plugins", experimental: true));

        Assert.True(manager.IsExperimental("Plugins"));
        Assert.False(manager.IsExperimental("Founder"));
    }

    [Fact]
    public void Register_ReturnsManagerForChaining()
    {
        var manager = new RuntimeFeatureManager();

        var result = manager.Register(Feature("Studio"));

        Assert.Same(manager, result);
        Assert.True(manager.IsRegistered("Studio"));
    }

    [Fact]
    public void Register_Null_Throws()
    {
        var manager = new RuntimeFeatureManager();

        Assert.Throws<ArgumentNullException>(() => manager.Register(null!));
    }

    [Fact]
    public void Register_EmptyId_Throws()
    {
        var manager = new RuntimeFeatureManager();

        Assert.Throws<ArgumentException>(() => manager.Register(Feature("")));
    }

    [Fact]
    public void Register_DuplicateId_ReplacesExisting()
    {
        var manager = new RuntimeFeatureManager();
        manager.Register(Feature("Studio", enabled: true));

        manager.Register(Feature("Studio", enabled: false));

        Assert.False(manager.IsEnabled("Studio"));
    }

    [Fact]
    public void All_ReturnsRegisteredFeatures()
    {
        var manager = new RuntimeFeatureManager();
        manager.Register(Feature("Founder"));
        manager.Register(Feature("Analytics"));

        Assert.Equal(2, manager.All.Count);
    }
}
