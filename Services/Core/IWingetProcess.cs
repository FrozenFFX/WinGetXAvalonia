// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Threading.Tasks;

namespace WinGetXAvalonia.Services.Core;

public interface IWingetProcess
{
    Task<string> ExecuteAsync(string arguments);
}
