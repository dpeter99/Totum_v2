using System.Collections.Concurrent;
using System.Diagnostics;

namespace cellarium_backend.Tests;

public class InMemoryTestSpans : ICollection<Activity>
{
    private readonly ConcurrentDictionary<string, List<Activity>> _spansByTraceId = new();
    private readonly List<Activity> _allSpans = [];

    public int Count => _allSpans.Count;
    public bool IsReadOnly => false;

    public void Add(Activity item)
    {
        if (item == null) return;
        
        lock (_allSpans)
        {
            _allSpans.Add(item);
        }
        
        var traceId = item.TraceId.ToString();
        _spansByTraceId.AddOrUpdate(traceId, 
            [item], 
            (key, existing) => 
            {
                existing.Add(item);
                return existing;
            });
    }

    public void Clear()
    {
        lock (_allSpans)
        {
            _allSpans.Clear();
        }
        _spansByTraceId.Clear();
    }

    public bool Contains(Activity item) => _allSpans.Contains(item);

    public void CopyTo(Activity[] array, int arrayIndex) => _allSpans.CopyTo(array, arrayIndex);

    public IEnumerator<Activity> GetEnumerator() => _allSpans.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Remove(Activity item)
    {
        lock (_allSpans)
        {
            return _allSpans.Remove(item);
        }
    }

    // Utility methods for tests
    public bool SpanExistsWithName(string name)
    {
        return _allSpans.Any(span => span.DisplayName.Contains(name));
    }

    public IEnumerable<Activity> GetSpanByName(string name)
    {
        return _allSpans.Where(span => span.DisplayName.Contains(name));
    }

    public void RemoveAllSpansForTest()
    {
        Clear();
    }

    public Activity? FirstOrDefault(Func<Activity, bool> predicate)
    {
        return _allSpans.FirstOrDefault(predicate);
    }
}