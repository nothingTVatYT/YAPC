using System.Collections.Generic;
using System.Linq;

namespace YAPC.Tools;

public class FloatAverage
{
    private Queue<float> _values;
    private int _windowSize;
    /// <summary>
    /// returns the average over the latest (windowSize) values or 0 if there are no values
    /// </summary>
    public float Average { get; }
    
    /// <summary>
    /// Create an accumulator for a floating window average of float values
    /// </summary>
    /// <param name="windowSize"></param>
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