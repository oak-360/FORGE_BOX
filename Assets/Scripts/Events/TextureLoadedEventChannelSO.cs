using UnityEngine;

[CreateAssetMenu(menuName = "Events/Texture Loaded")]
public class TextureLoadedEventChannelSO : ScriptableObject
{
    public event System.Action<(ExperienceData exp, Texture2D texture)> OnRaised;
    
    public void Raise((ExperienceData exp, Texture2D texture) data)
    {
        Debug.Log($"[TextureLoadedEvent] Texture loaded for experience: {data.exp?.experienceName ?? "Unknown"}");
        OnRaised?.Invoke(data);
    }
}
