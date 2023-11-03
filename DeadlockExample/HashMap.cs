#if false

using System.Collections.Generic;

public class HashMap
{
    private record struct ValuePair(string Index, int Value);

    private readonly List<ValuePair>[] _buckets;

    public HashMap(int bucketCount)
    {
        _buckets = new List<ValuePair>[bucketCount];
    }

    public int Get(string index) 
    {
        var bucketIndex = GetBucketIndex(index);
        var pairs = _buckets[bucketIndex];

        if (pairs != null)
        {
            for (int i = 0 ; i < pairs.Count ; i++)
            {
                if (pairs[i].Index == index)
                {
                    return pairs[i].Value;
                }
            }
        }

        throw new KeyNotFoundException(index);
    }

    public void Set(string index, int value)
    {
        var bucketIndex = GetBucketIndex(index);
        var pairs = _buckets[bucketIndex];

        if (pairs != null)
        {
            for (int i = 0 ; i < pairs.Count ; i++)
            {
                pairs.
            }
        }

    }

    public bool Contains(string index)
    {

    }

    public void Remove(string index)
    {

    }

    public void SetAll(int value)
    {

    }

    private int GetBucketIndex(string index)
    {
        return index.GetHashCode() % _buckets.Length;
    }
}

#endif
