using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a simple script for getting random choices from multisets based on probability weight of each particular choice
/// </summary>
/// <typeparam name="T"></typeparam>
public class ProbabilityDispenser<T>
{
    int weights_sum;
    public List<Pair<IntRange, T>> probability_mapping;
    public ProbabilityDispenser(Dictionary<T, int> weights_of_choices) // the 'int' is weight of the choice - the greater the weight relative to other choices, the more often it will be picked
    {
        probability_mapping = new List<Pair<IntRange, T>>();

        weights_sum = 0;
        int last_max = 0;
        foreach (KeyValuePair<T, int> kv in weights_of_choices)
        {
            weights_sum += kv.Value;
            IntRange range = new IntRange(last_max, last_max + kv.Value);
            last_max += kv.Value;
            probability_mapping.Add(new Pair<IntRange, T>(range, kv.Key));

        }
    }



    public (int, T) GetRandomElement()
    {
        int roll = Random.Range(0, weights_sum - 1);

        Pair<IntRange, T> ret = probability_mapping.Find(pm => pm.first.min <= roll && pm.first.max > roll);
        return (roll, ret.second);

    }
}
