namespace DifyApiClient.Utilities;

/// <summary>
/// Helper class for building query strings
/// </summary>
internal class QueryStringBuilder
{
    private readonly List<string> _parameters = [];

    public QueryStringBuilder Add(string key, string value)
    {
        _parameters.Add($"{key}={Uri.EscapeDataString(value)}");
        return this;
    }

    public QueryStringBuilder Add(string key, int value)
    {
        _parameters.Add($"{key}={value}");
        return this;
    }

    public QueryStringBuilder Add(string key, bool value)
    {
        _parameters.Add($"{key}={value.ToString().ToLower()}");
        return this;
    }

    public QueryStringBuilder AddIfNotNull(string key, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            Add(key, value);
        }
        return this;
    }

    public QueryStringBuilder AddIfNotNull(string key, int? value)
    {
        if (value.HasValue)
        {
            Add(key, value.Value);
        }
        return this;
    }

    public QueryStringBuilder AddIfNotNull(string key, bool? value)
    {
        if (value.HasValue)
        {
            Add(key, value.Value);
        }
        return this;
    }

    public string Build()
    {
        return _parameters.Count > 0 ? string.Join("&", _parameters) : string.Empty;
    }

    public override string ToString() => Build();
}
