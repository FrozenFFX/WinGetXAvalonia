// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

namespace WinGetXAvalonia.Core.PackageEngine;

/// <summary>
/// Orchestrates Validator, Analyzer, Queue and Executor stages.
/// For now it runs a request directly through the engine.
/// </summary>
public class PackagePipeline
{
    private readonly IPackageEngine _engine;

    public PackagePipeline(IPackageEngine engine)
    {
        _engine = engine;
    }

    public Task<PackageResult> RunAsync(PackageRequest request)
    {
        var context = new PackageContext(request);

        return ExecuteAsync(context);
    }

    private async Task<PackageResult> ExecuteAsync(PackageContext context)
    {
        context.Result = await _engine.ExecuteAsync(context.Request);

        return context.Result;
    }
}
