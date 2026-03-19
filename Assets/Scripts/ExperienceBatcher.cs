// ExperienceBatcher.cs
using System.Collections.Generic;
using UnityEngine;

public static class ExperienceBatcher
{
  public static List<List<ExperienceData>> Chunk(
    List<ExperienceData> items, int batchSize)
  {
    var batches = new List<List<ExperienceData>>();
    for (int i = 0; i < items.Count; i += batchSize)
      batches.Add(items.GetRange(i, 
        Mathf.Min(batchSize, items.Count - i)));
    return batches;
  }
}