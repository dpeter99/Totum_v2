using System.Collections.Concurrent;
using System.Diagnostics;

namespace Cellarium.Tests.Infrastructure;

/// <summary>
/// Thread-safe collector for OpenTelemetry spans during testing.
/// Provides utilities for querying and asserting on collected telemetry data.
/// </summary>
public class TelemetryCollector : ICollection<Activity>
{
    private readonly ConcurrentBag<Activity> _collectedSpans = [];
    
    public static readonly TelemetryCollector CollectedSpans = new();

    public int Count => _collectedSpans.Count;
    public bool IsReadOnly => false;

    public void Add(Activity item)
    {
        if (item != null)
        {
            _collectedSpans.Add(item);
        }
    }

    public void Clear()
    {
        while (_collectedSpans.TryTake(out _)) { }
    }

    public bool Contains(Activity item) => _collectedSpans.Contains(item);

    public void CopyTo(Activity[] array, int arrayIndex)
    {
        _collectedSpans.CopyTo(array, arrayIndex);
    }

    public IEnumerator<Activity> GetEnumerator() => _collectedSpans.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Remove(Activity item) => false; // Not supported for ConcurrentBag

    // Test utility methods
    public Activity? FindSpanByName(string displayName)
    {
        return _collectedSpans.FirstOrDefault(span => span.DisplayName == displayName);
    }

    public IEnumerable<Activity> FindSpansContaining(string nameFragment)
    {
        return _collectedSpans.Where(span => span.DisplayName.Contains(nameFragment));
    }

    public Activity? FindSpanWithTag(string tagKey, string tagValue)
    {
        return _collectedSpans.FirstOrDefault(span => 
            span.Tags.Any(tag => tag.Key == tagKey && tag.Value == tagValue));
    }

    public bool HasSpanWithName(string displayName)
    {
        return _collectedSpans.Any(span => span.DisplayName == displayName);
    }

    public void AssertSpanExists(string displayName)
    {
        if (!HasSpanWithName(displayName))
        {
            var availableSpans = string.Join(", ", _collectedSpans.Select(s => s.DisplayName));
            throw new InvalidOperationException(
                $"Expected span '{displayName}' not found. Available spans: {availableSpans}");
        }
    }

    public void AssertSpanHasTag(string displayName, string tagKey, string expectedValue)
    {
        var span = FindSpanByName(displayName);
        if (span == null)
        {
            throw new InvalidOperationException($"Span '{displayName}' not found");
        }

        var tag = span.Tags.FirstOrDefault(t => t.Key == tagKey);
        if (tag.Key == null)
        {
            var availableTags = string.Join(", ", span.Tags.Select(t => t.Key));
            throw new InvalidOperationException(
                $"Tag '{tagKey}' not found on span '{displayName}'. Available tags: {availableTags}");
        }

        if (tag.Value != expectedValue)
        {
            throw new InvalidOperationException(
                $"Tag '{tagKey}' on span '{displayName}' has value '{tag.Value}', expected '{expectedValue}'");
        }
    }
}