using UnityEngine;

[CreateAssetMenu(menuName = "Events/Experience Interaction")]
public class ExperienceInteractionEventChannelSO : ScriptableObject
{
    public event System.Action<ExperienceData> OnRaised;
    
    public void Raise(ExperienceData experience)
    {
        if (experience != null)
        {
            Debug.Log($"[ExperienceInteractionEvent] Interaction with experience: {experience.experienceName}");
            OnRaised?.Invoke(experience);
        }
        else
        {
            Debug.LogWarning("[ExperienceInteractionEvent] Attempted to raise event with null experience data");
        }
    }
}
