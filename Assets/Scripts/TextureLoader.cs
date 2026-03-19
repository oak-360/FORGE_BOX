using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TextureLoader : MonoBehaviour
{
    [SerializeField] private BatchReadyEventChannelSO onBatchReady;
    [SerializeField] private TextureLoadRequestEventChannelSO onTextureLoadRequest;
    [SerializeField] private TextureLoadedEventChannelSO onTextureLoaded;
    
    private Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();

    private void OnEnable()
    {
        onBatchReady.OnRaised += HandleBatchReady;
        onTextureLoadRequest.OnRaised += HandleTextureLoadRequest;
    }

    private void OnDisable()
    {
        onBatchReady.OnRaised -= HandleBatchReady;
        onTextureLoadRequest.OnRaised -= HandleTextureLoadRequest;
    }

    private void HandleBatchReady(List<ExperienceData> batch, int batchIndex, int totalBatches)
    {
        foreach (var exp in batch)
        {
            if (textureCache.TryGetValue(exp.imageReferenceGuid, out Texture2D cachedTexture))
            {
                onTextureLoaded.Raise((exp, cachedTexture));
            }
            else
            {
                onTextureLoadRequest.Raise(exp);
            }
        }
    }

    private void HandleTextureLoadRequest(ExperienceData exp)
    {
        StartCoroutine(LoadTextureCoroutine(exp));
    }

    private IEnumerator LoadTextureCoroutine(ExperienceData exp)
    {
        Texture2D texture = null;
        AsyncOperationHandle<Texture2D> handle = default;

        if (exp.imageTriggerTexture.OperationHandle.IsValid())
        {
            if (exp.imageTriggerTexture.OperationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                texture = exp.imageTriggerTexture.Asset as Texture2D;
            }
            else
            {
                yield return exp.imageTriggerTexture.OperationHandle;
                if (exp.imageTriggerTexture.OperationHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    texture = exp.imageTriggerTexture.Asset as Texture2D;
                }
            }
        }
        else
        {
            handle = exp.imageTriggerTexture.LoadAssetAsync<Texture2D>();
            yield return handle;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                texture = handle.Result;
            }
        }

        if (texture != null && texture.isReadable)
        {
            textureCache[exp.imageReferenceGuid] = texture;
            onTextureLoaded.Raise((exp, texture));
        }
    }
}