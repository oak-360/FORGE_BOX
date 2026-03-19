using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Experiences Processed")]
public class ExperiencesProcessedEventChannelSO : ScriptableObject
{
    public event System.Action<List<ExperienceData>> OnRaised;
    
    public void Raise(List<ExperienceData> filteredExperiences)
    {
        Debug.Log($"[ExperiencesProcessedEvent] Raising event with {filteredExperiences?.Count ?? 0} filtered experiences");
        OnRaised?.Invoke(filteredExperiences);
    }
}
