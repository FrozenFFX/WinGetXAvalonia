// ==========================================================
// WinGetX - https://github.com/FrozenFFX
// ==========================================================
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Interfaces;

namespace WinGetXAvalonia.Tests;

// kaneki
public class AutoSaveServiceTests
{
    // kaneki
    private sealed class FakeSettingsService : ISettingsService
    {
        public AppSettings Settings { get; } = new();

        public int UpdateCalls { get; private set; }

#pragma warning disable CS0067
        public event EventHandler<SettingsChangedEventArgs>? SettingsChanged;
#pragma warning restore CS0067

        public Task LoadAsync() => Task.CompletedTask;

        public Task SaveAsync() => Task.CompletedTask;

        public Task UpdateAsync(Action<AppSettings> update)
        {
            UpdateCalls++;
            update(Settings);
            return Task.CompletedTask;
        }

        public Task ExportAsync(string filePath)
            => throw new NotImplementedException();

        public Task<bool> ImportAsync(string filePath)
            => throw new NotImplementedException();

        public Task ResetAsync()
            => throw new NotImplementedException();
    }

    [Fact]
    public async Task TenSchedules_CoalesceIntoSingleSave()
    {
        var fake = new FakeSettingsService();
        var autoSave = new AutoSaveService(fake, TimeSpan.FromMilliseconds(20));

        for (var i = 0; i < 10; i++)
            autoSave.Schedule(s => s.ParallelDownloads = 42);

        await Task.Delay(200);

        Assert.Equal(1, fake.UpdateCalls);
        Assert.Equal(42, fake.Settings.ParallelDownloads);
    }

    [Fact]
    public async Task ConsecutiveSchedules_AfterDebounce_EachSave()
    {
        var fake = new FakeSettingsService();
        var autoSave = new AutoSaveService(fake, TimeSpan.FromMilliseconds(20));

        autoSave.Schedule(s => s.Theme = "Light");
        await Task.Delay(100);

        autoSave.Schedule(s => s.Theme = "Dark");
        await Task.Delay(100);

        Assert.Equal(2, fake.UpdateCalls);
        Assert.Equal("Dark", fake.Settings.Theme);
    }
}
// kaneki
