namespace WinGetXAvalonia.Core.Runtime;

public sealed class RuntimeFeatureManager
{
    private readonly Dictionary<string, RuntimeFeature> _features = new(StringComparer.OrdinalIgnoreCase);

    public RuntimeFeatureManager Register(RuntimeFeature feature)
    {
        ArgumentNullException.ThrowIfNull(feature);

        if (string.IsNullOrWhiteSpace(feature.Id))
            throw new ArgumentException("Feature Id is required.", nameof(feature));

        _features[feature.Id] = feature;
        return this;
    }

    public bool IsEnabled(string id)
        => _features.TryGetValue(id, out var feature) && feature.Enabled;

    public bool IsRegistered(string id)
        => _features.ContainsKey(id);

    public bool IsExperimental(string id)
        => _features.TryGetValue(id, out var feature) && feature.Experimental;

    public RuntimeFeature? Get(string id)
        => _features.GetValueOrDefault(id);

    public void Enable(string id, bool enabled = true)
    {
        if (_features.TryGetValue(id, out var feature))
            feature.Enabled = enabled;
    }

    public IReadOnlyList<RuntimeFeature> All
        => _features.Values.OrderBy(f => f.Name).ToList();
}
