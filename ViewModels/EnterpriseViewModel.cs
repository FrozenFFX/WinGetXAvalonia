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
using WinGetXAvalonia.Services.Enterprise;
using WinGetXAvalonia.Services.History;
using WinGetXAvalonia.Services.Interfaces;
using WinGetXAvalonia.Services.Notifications;

namespace WinGetXAvalonia.ViewModels;

public partial class EnterpriseViewModel : AsyncViewModel
{
    private readonly IEnterpriseService _enterprise;
    private readonly FleetReportWriter _reportWriter;
    private readonly IFilePickerService _filePicker;
    private readonly IDialogService _dialogs;
    private readonly INotificationService _notifications;
    private readonly IHistoryService _history;
    private readonly LoggerService _logger;

    private readonly List<CompanyComputer> _allComputers = new();

    public ObservableCollection<CompanyComputer> Computers { get; } = new();

    public ObservableCollection<RemoteCommand> Commands { get; } = new();

    public ObservableCollection<PolicyRule> Policies { get; } = new();

    public ObservableCollection<PolicyViolation> Violations { get; } = new();

    public ObservableCollection<CompanyComputer> FilteredComputers { get; } = new();

    public IReadOnlyList<string> PolicyActionOptions { get; } =
    [
        "Block uninstall",
        "Block install",
        "Allow only"
    ];

    [ObservableProperty]
    private CompanyComputer? selectedComputer;

    [ObservableProperty]
    private PolicyRule? selectedPolicy;

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    private string policyName = "";

    [ObservableProperty]
    private string policyPackage = "";

    [ObservableProperty]
    private string policyDescription = "";

    [ObservableProperty]
    private bool policyEnabled = true;

    [ObservableProperty]
    private int policyActionIndex;

    [ObservableProperty]
    private string powerShellScript = "Get-ComputerInfo | Select-Object OsName, OsVersion";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FleetSummary))]
    private int total;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FleetSummary))]
    private int online;

    [ObservableProperty]
    private int offline;

    [ObservableProperty]
    private int pendingUpdates;

    [ObservableProperty]
    private int criticalUpdates;

    [ObservableProperty]
    private int failedInstallations;

    public string FleetSummary => $"{Online} of {Total} online";

    public bool HasPendingActions => Commands.Any(c => c.Status == RemoteCommandStatus.Pending);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasViolations))]
    private int violationCount;

    public bool HasViolations => ViolationCount > 0;

    public bool IsEditingPolicy => SelectedPolicy is not null;

    public bool IsNothingSelected => SelectedComputer is null;

    public string EditorPolicyTitle => IsEditingPolicy ? "Edit Policy" : "New Policy";

    public EnterpriseViewModel(
        IEnterpriseService enterprise,
        FleetReportWriter reportWriter,
        IFilePickerService filePicker,
        IDialogService dialogs,
        INotificationService notifications,
        IHistoryService history,
        LoggerService logger)
    {
        _enterprise = enterprise;
        _reportWriter = reportWriter;
        _filePicker = filePicker;
        _dialogs = dialogs;
        _notifications = notifications;
        _history = history;
        _logger = logger;

        Commands.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasPendingActions));
    }

    public override Task InitializeAsync()
        => LoadAsync();

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedComputerChanged(CompanyComputer? value)
        => OnPropertyChanged(nameof(IsNothingSelected));

    partial void OnSelectedPolicyChanged(PolicyRule? value)
    {
        if (value is null)
        {
            PolicyName = "";
            PolicyPackage = "";
            PolicyDescription = "";
            PolicyEnabled = true;
            PolicyActionIndex = 0;
        }
        else
        {
            PolicyName = value.Name;
            PolicyPackage = value.Package;
            PolicyDescription = value.Description;
            PolicyEnabled = value.Enabled;
            PolicyActionIndex = (int)value.Action;
        }

        OnPropertyChanged(nameof(IsEditingPolicy));
        OnPropertyChanged(nameof(EditorPolicyTitle));
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        State = LoadState.Loading;
        ErrorMessage = null;

        try
        {
            var computers = await _enterprise.GetComputersAsync();
            var stats = await _enterprise.GetStatsAsync();
            var commands = await _enterprise.GetCommandsAsync();
            var policies = await _enterprise.GetPoliciesAsync();

            _allComputers.Clear();
            _allComputers.AddRange(computers);

            Total = stats.Total;
            Online = stats.Online;
            Offline = stats.Offline;
            PendingUpdates = stats.PendingUpdates;
            CriticalUpdates = stats.CriticalUpdates;
            FailedInstallations = stats.FailedInstallations;

            Commands.Clear();
            foreach (var command in commands)
                Commands.Add(command);

            Policies.Clear();
            foreach (var policy in policies)
                Policies.Add(policy);

            ApplyFilter();

            if (SelectedComputer is null && FilteredComputers.Count > 0)
                SelectedComputer = FilteredComputers[0];

            State = computers.Count == 0 ? LoadState.Empty : LoadState.Loaded;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Enterprise fleet load failed");
            State = LoadState.Error;
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task InstallPackageAsync(string? package)
    {
        if (SelectedComputer is not { } computer || string.IsNullOrWhiteSpace(package))
            return;

        await DispatchAsync(computer, RemoteCommandKind.InstallPackage, package);
    }

    [RelayCommand]
    private async Task SendPowerShellAsync()
    {
        if (SelectedComputer is not { } computer)
            return;

        var script = string.IsNullOrWhiteSpace(PowerShellScript)
            ? "Write-Host 'Hello WinGetX'"
            : PowerShellScript.Trim();

        await DispatchAsync(computer, RemoteCommandKind.PowerShell, script);
    }

    [RelayCommand]
    private async Task RestartAsync()
    {
        if (SelectedComputer is not { } computer)
            return;

        bool confirmed = await _dialogs.ConfirmAsync(
            "Restart Computer",
            $"Restart '{computer.Name}' remotely?");

        if (!confirmed)
            return;

        await DispatchAsync(computer, RemoteCommandKind.Restart, "restart");
    }

    private async Task DispatchAsync(CompanyComputer computer, RemoteCommandKind kind, string payload)
    {
        try
        {
            var command = await _enterprise.SendCommandAsync(computer.Id, kind, payload);

            Commands.Insert(0, command);

            RefreshStatsFromComputers();

            if (command.Status == RemoteCommandStatus.Success)
            {
                _notifications.ShowSuccess(
                    "Remote Command",
                    $"'{payload}' completed on {computer.Name}.");
            }
            else
            {
                _notifications.ShowError(
                    "Remote Command Failed",
                    $"{computer.Name} is offline; command queued.");
            }

            await _history.AddAsync(new HistoryEntry
            {
                Timestamp = DateTime.Now,
                Operation = $"Remote {kind}",
                PackageName = payload,
                PackageId = computer.Name,
                Success = command.Status == RemoteCommandStatus.Success
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Remote command failed on {computer.Name}");
            _notifications.ShowError("Remote Command Failed", ex.Message);
        }
    }

    private void RefreshStatsFromComputers()
    {
        Total = _allComputers.Count;
        Online = _allComputers.Count(c => c.Online);
        Offline = _allComputers.Count(c => !c.Online);
        PendingUpdates = _allComputers.Sum(c => c.PendingUpdates);
        CriticalUpdates = _allComputers.Sum(c => c.CriticalUpdates);
        FailedInstallations = _allComputers.Sum(c => c.FailedInstallations);
    }

    [RelayCommand]
    private void NewPolicy()
    {
        SelectedPolicy = null;
        PolicyName = "";
        PolicyPackage = "";
        PolicyDescription = "";
        PolicyEnabled = true;
        PolicyActionIndex = 0;
    }

    [RelayCommand]
    private async Task SavePolicyAsync()
    {
        if (string.IsNullOrWhiteSpace(PolicyName))
            return;

        var rule = SelectedPolicy ?? new PolicyRule();

        rule.Name = PolicyName.Trim();
        rule.Package = PolicyPackage.Trim();
        rule.Description = PolicyDescription.Trim();
        rule.Enabled = PolicyEnabled;
        rule.Action = (PolicyAction)PolicyActionIndex;

        try
        {
            await _enterprise.SavePolicyAsync(rule);

            if (SelectedPolicy is null)
                Policies.Add(rule);

            SelectedPolicy = null;
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync("Save Failed", ex.Message);
        }
    }

    [RelayCommand]
    private async Task DeletePolicyAsync()
    {
        if (SelectedPolicy is not { } rule)
            return;

        bool confirmed = await _dialogs.ConfirmAsync(
            "Delete Policy",
            $"Delete policy '{rule.Name}'?");

        if (!confirmed)
            return;

        try
        {
            await _enterprise.DeletePolicyAsync(rule.Id);
            Policies.Remove(rule);
            SelectedPolicy = null;
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync("Delete Failed", ex.Message);
        }
    }

    [RelayCommand]
    private async Task EvaluatePoliciesAsync()
    {
        try
        {
            var violations = await _enterprise.EvaluatePoliciesAsync();

            Violations.Clear();

            foreach (var violation in violations)
                Violations.Add(violation);

            ViolationCount = violations.Count;

            _notifications.ShowInfo(
                "Policy Check",
                violations.Count == 0
                    ? "No policy violations found."
                    : $"{violations.Count} policy violation(s) found.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Policy evaluation failed");
            await _dialogs.ShowErrorAsync("Policy Check Failed", ex.Message);
        }
    }

    [RelayCommand]
    private async Task ExportJsonAsync()
    {
        var file = await _filePicker.PickSaveJsonAsync(
            "Export Fleet Report",
            $"fleet-report-{DateTime.Now:yyyyMMdd-HHmmss}.json");

        if (file is null)
            return;

        try
        {
            await _reportWriter.WriteJsonAsync(file.Path.LocalPath, _allComputers);

            _notifications.ShowSuccess("Report Exported", "Fleet report saved as JSON.");
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync("Export Failed", ex.Message);
        }
    }

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        var file = await _filePicker.PickSaveJsonAsync(
            "Export Fleet Report",
            $"fleet-report-{DateTime.Now:yyyyMMdd-HHmmss}.csv");

        if (file is null)
            return;

        try
        {
            await _reportWriter.WriteCsvAsync(file.Path.LocalPath, _allComputers);

            _notifications.ShowSuccess("Report Exported", "Fleet report saved as CSV.");
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync("Export Failed", ex.Message);
        }
    }

    private void ApplyFilter()
    {
        IEnumerable<CompanyComputer> filtered = _allComputers;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(c =>
                c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || c.Os.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                || c.Cpu.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        var matches = filtered.ToList();

        if (SelectedComputer is not null && !matches.Contains(SelectedComputer))
            SelectedComputer = null;

        FilteredComputers.Clear();

        foreach (var item in matches)
            FilteredComputers.Add(item);
    }
}

