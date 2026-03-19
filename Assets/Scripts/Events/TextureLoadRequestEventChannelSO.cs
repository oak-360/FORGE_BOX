using UnityEngine;

[CreateAssetMenu(menuName = "Events/Texture Load Request")]
public class TextureLoadRequestEventChannelSO : ScriptableObject
{
    public event System.Action<ExperienceData> OnRaised;
    
    public void Raise(ExperienceData experience)
    {
        if (experience != null)
        {
            Debug.Log($"[TextureLoadRequestEvent] Requesting load for experience: {experience.experienceName}");
            OnRaised?.Invoke(experience);
        }
        else
        {
            Debug.LogWarning("[TextureLoadRequestEvent] Attempted to raise event with null experience data");
        }
    }
}
