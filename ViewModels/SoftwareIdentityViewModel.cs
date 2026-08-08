// Copyright (c) 2026 FrozenFFX. All rights reserved.
// https://github.com/FrozenFFX/WinGetXAvalonia

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinGetXAvalonia.Core.State;
using WinGetXAvalonia.Models;
using WinGetXAvalonia.Services.Backup;
using WinGetXAvalonia.Services.Core;
using WinGetXAvalonia.Services.Dialogs;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Identity;
using WinGetXAvalonia.Services.Interfaces;
using WinGetXAvalonia.Services.Notifications;

namespace WinGetXAvalonia.ViewModels;

public partial class SoftwareIdentityViewModel : AsyncViewModel
{
    private readonly IIdentityService _identity;
    private readonly IFilePickerService _filePicker;
    private readonly IDialogService _dialogs;
    private readonly INotificationService _notifications;
    private readonly IExportService _export;
    private readonly IHistoryService _history;
    private readonly LoggerService _logger;

    private readonly List<SoftwareIdentity> _all = new();

    public ObservableCollection<SoftwareIdentity> FilteredIdentities { get; } = new();

    [ObservableProperty]
    private SoftwareIdentity? selectedIdentity;

    [ObservableProperty]
    private string searchText = "";

    public bool IsNothingSelected => SelectedIdentity is null;

    public IReadOnlyList<PropertyItem> IdentityProperties { get; private set; } =
        Array.Empty<PropertyItem>();

    public SoftwareIdentityViewModel(
        IIdentityService identity,
        IFilePickerService filePicker,
        IDialogService dialogs,
        INotificationService notifications,
        IExportService export,
        IHistoryService history,
        LoggerService logger)
    {
        _identity = identity;
        _filePicker = filePicker;
        _dialogs = dialogs;
        _notifications = notifications;
        _export = export;
        _history = history;
        _logger = logger;
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedIdentityChanged(SoftwareIdentity? value)
    {
        OnPropertyChanged(nameof(IsNothingSelected));
        IdentityProperties = BuildProperties(value);
        OnPropertyChanged(nameof(IdentityProperties));
    }

    private static IReadOnlyList<PropertyItem> BuildProperties(SoftwareIdentity? identity)
    {
        if (identity is null)
            return Array.Empty<PropertyItem>();

        return new List<PropertyItem>
        {
            new("Publisher", identity.Publisher),
            new("Version", identity.Version),
            new("Installed", identity.InstalledDateDisplay),
            new("Last Updated", identity.UpdatedDateDisplay),
            new("Updates", identity.UpdateCount.ToString()),
            new("Source", identity.Source),
            new("Workspace", identity.Workspace ?? "—"),
            new("Blueprint", identity.Blueprint ?? "—"),
            new("Automation", identity.Automation ?? "—"),
            new("Trust Score", identity.TrustText),
            new("Health", identity.HealthText),
            new("Community", identity.CommunityDisplay),
            new("Rollbacks", identity.RollbackCount.ToString()),
            new("Repair", identity.RepairSupported ? "Available" : "Not supported"),
            new("Dependencies", identity.DependenciesSummary)
        };
    }

    public override Task InitializeAsync()
        => LoadAsync();

    [RelayCommand]
    public async Task LoadAsync()
    {
        State = LoadState.Loading;
        ErrorMessage = null;

        try
        {
            var identities = await _identity.GetAllAsync();

            _all.Clear();
            _all.AddRange(identities);

            ApplyFilter();

            if (SelectedIdentity is null && FilteredIdentities.Count > 0)
                SelectedIdentity = FilteredIdentities[0];

            State = identities.Count == 0 ? LoadState.Empty : LoadState.Loaded;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Software identity load failed");
            State = LoadState.Error;
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task RecordUpdateAsync()
    {
        if (SelectedIdentity is not { } identity)
            return;

        try
        {
            await _identity.RecordUpdateAsync(identity.Id);

            var updated = await _identity.GetAsync(identity.Id);

            if (updated is null)
                return;

            UpdateItem(identity, updated);

            _notifications.ShowSuccess(
                "Identity Updated",
                $"{identity.Name} profile updated.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Identity update failed for {identity.Name}");
            _notifications.ShowError("Update Failed", ex.Message);
        }
    }

    [RelayCommand]
    private async Task RecordRollbackAsync()
    {
        if (SelectedIdentity is not { } identity)
            return;

        bool confirmed = await _dialogs.ConfirmAsync(
            "Rollback",
            $"Record a rollback for '{identity.Name}'?");

        if (!confirmed)
            return;

        try
        {
            await _identity.RecordRollbackAsync(identity.Id);

            var updated = await _identity.GetAsync(identity.Id);

            if (updated is null)
                return;

            UpdateItem(identity, updated);

            _notifications.ShowWarning(
                "Rollback Recorded",
                $"{identity.Name} health decreased.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Identity rollback failed for {identity.Name}");
            _notifications.ShowError("Rollback Failed", ex.Message);
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedIdentity is not { } identity)
            return;

        bool confirmed = await _dialogs.ConfirmAsync(
            "Delete Identity",
            $"Remove the software profile for '{identity.Name}'?");

        if (!confirmed)
            return;

        try
        {
            await _identity.DeleteAsync(identity.Id);

            FilteredIdentities.Remove(identity);
            _all.Remove(identity);

            SelectedIdentity = FilteredIdentities.FirstOrDefault();

            if (FilteredIdentities.Count == 0)
                State = LoadState.Empty;

            _notifications.ShowSuccess(
                "Identity Removed",
                $"{identity.Name} profile deleted.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Identity delete failed for {identity.Name}");
            _notifications.ShowError("Delete Failed", ex.Message);
        }
    }

    [RelayCommand]
    private async Task ExportJsonAsync()
    {
        var file = await _filePicker.PickSaveJsonAsync(
            "Export Identities",
            $"software-identities-{DateTime.Now:yyyyMMdd-HHmmss}.json");

        if (file is null)
            return;

        try
        {
            await _export.ExportJsonAsync(_all, file.Path.LocalPath);

            _notifications.ShowSuccess(
                "Identities Exported",
                "Software profiles saved as JSON.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Identity export failed");
            _notifications.ShowError("Export Failed", ex.Message);
        }
    }

    private void UpdateItem(SoftwareIdentity old, SoftwareIdentity updated)
    {
        var filteredIndex = FilteredIdentities.IndexOf(old);

        if (filteredIndex >= 0)
            FilteredIdentities[filteredIndex] = updated;

        _all[_all.IndexOf(old)] = updated;

        SelectedIdentity = updated;
    }

    private void ApplyFilter()
    {
        FilteredIdentities.Clear();

        foreach (var identity in _all)
        {
            if (string.IsNullOrWhiteSpace(SearchText)
                || identity.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || identity.Publisher.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            {
                FilteredIdentities.Add(identity);
            }
        }
    }
}

