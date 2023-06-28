using Ravenfield;
using UnityEngine;
using BepInEx;
using System.Collections.Generic;
using System.Collections;
using System;
using System.IO;
using System.Reflection;

namespace ravenfootsteps
{
    [BepInPlugin("org.personperhaps.ravenfootsteps", "RavenSteps", "1.0.0.0")]
    public class RavenSteps : BaseUnityPlugin
        {
            void Start()
            {
                StartCoroutine(LoadAssetBundle(Assembly.GetExecutingAssembly().GetManifestResourceStream("ravenfootsteps.assets.footsteps")));
            }
            IEnumerator LoadAssetBundle(Stream path)
            {
                if(path == null)
                {
                Debug.LogWarning("uh oh no path");
                    yield break;
                }
                var bundleLoadRequest = AssetBundle.LoadFromStreamAsync(path);
                yield return bundleLoadRequest;

                var assetBundle = bundleLoadRequest.assetBundle;
                if (assetBundle == null)
                {
                Debug.LogWarning("uh oh no bundle");
                    yield break;
                }
                List<AudioClip> audio = new List<AudioClip>();
                var assetLoadRequest = assetBundle.LoadAllAssetsAsync<AudioClip>();
                yield return assetLoadRequest;
                if (assetLoadRequest.allAssets == null)
                {
                    Debug.LogWarning("uh oh loading sounds failed");
                    yield break;
                }
                List<UnityEngine.Object> objects = new List<UnityEngine.Object>(assetLoadRequest.allAssets);
                List<AudioClip> concreteSounds = new List<AudioClip>();
                List<AudioClip> dirtSounds = new List<AudioClip>();
                List<AudioClip> wetSounds = new List<AudioClip>();
                foreach (UnityEngine.Object asset in objects)
                {
                    if (asset.name.StartsWith("c"))
                    {
                        Debug.Log("Added " + asset.name + " to concrete list");
                        concreteSounds.Add(asset as AudioClip);
                    }
                    else if (asset.name.StartsWith("d"))
                    {
                        Debug.Log("Added " + asset.name + " to dirt list");
                        dirtSounds.Add(asset as AudioClip);
                    }
                    else if (asset.name.StartsWith("s"))
                    {
                        Debug.Log("Added " + asset.name + " to wet list");
                        wetSounds.Add(asset as AudioClip);
                    }
                }

                while (FootstepAudio.instance == null || GameManager.instance == null)
                {
                 yield return null;
                }
                FootstepAudio.instance.indoorClips = concreteSounds.ToArray();
                FootstepAudio.instance.outdoorClips = dirtSounds.ToArray();
                FootstepAudio.instance.waterClips = wetSounds.ToArray();
                FootstepAudio.instance.sightOutputGroup = GameManager.instance.worldMixerGroup;
                FootstepAudio.instance.noSightOutputGroup = GameManager.instance.worldMixerGroup;
                List<AudioSource> sources = new List<AudioSource>(FootstepAudio.instance.GetComponentsInChildren<AudioSource>());
                foreach(AudioSource source in sources)
                {
                    source.maxDistance = 10;
                    source.minDistance = 2;
                }
                assetBundle.Unload(false);
            }
    }
}
