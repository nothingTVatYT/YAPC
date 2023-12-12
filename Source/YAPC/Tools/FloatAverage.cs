using System.Collections.Generic;
using System.Linq;

namespace YAPC.Tools;

public class FloatAverage
{
    private Queue<float> _values;
    private int _windowSize;
    public float Average { get; }
    
    public FloatAverage(int windowSize)
    {
        _windowSize = windowSize;
        _values = new Queue<float>(windowSize);
        Average = _values.Count > 0 ? _values.Sum() / _values.Count : 0;
    }

    /// <summary>
    /// Add a value to the sliding window of values to average over.
    /// </summary>
    /// <param name="value"></param>
    public void Add(float value)
    {
        if (_values.Count == _windowSize)
            _values.Dequeue();
        _values.Enqueue(value);
    }
    
}