using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SFXEntry
{
    public string name;
    public AudioClip clip;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music Tracks")]
    public List<AudioClip> battleMusicTracks;
    public List<AudioClip> buildingMusicTracks;
    private bool isBattleMode = false;

    [Header("SFX Clips")]
    public List<SFXEntry> sfxEntries;
    private Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SwitchToBattleMode();   
        InitializeSFXDictionary();
    }

    private void InitializeSFXDictionary()
    {
        foreach (var entry in sfxEntries)
        {
            if (!sfxClips.ContainsKey(entry.name))
            {
                sfxClips[entry.name] = entry.clip;
            }
        }
    }

    private void PlayMusicTrack(List<AudioClip> musicList)
    {
        if (musicList.Count == 0) return;
        
        int randomIndex = Random.Range(0, musicList.Count);
        musicSource.clip = musicList[randomIndex];
        
        StartCoroutine(FadeInMusic());
    }

    public void SwitchToBattleMode()
    {
        if (!isBattleMode && battleMusicTracks.Count > 0)
        {
            isBattleMode = true;
            StartCoroutine(FadeOutAndSwitch(battleMusicTracks));
        }
    }

    public void SwitchToBuildingMode()
    {
        if (isBattleMode && buildingMusicTracks.Count > 0)
        {
            isBattleMode = false;
            StartCoroutine(FadeOutAndSwitch(buildingMusicTracks));
        }
    }

    private IEnumerator FadeOutAndSwitch(List<AudioClip> newMusicList)
    {
        float startVolume = musicSource.volume;
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime;
            yield return null;
        }
        
        PlayMusicTrack(newMusicList);
    }

    private IEnumerator FadeInMusic()
    {
        musicSource.Play();
        float targetVolume = 0.5f;
        musicSource.volume = 0;
        while (musicSource.volume < targetVolume)
        {
            musicSource.volume += targetVolume * Time.deltaTime;
            yield return null;
        }
    }

    public void PlaySFX(string sfxName, float volume)
    {
        if (sfxClips.TryGetValue(sfxName, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.LogWarning("SFX not found: " + sfxName);
        }
    }

    public void PlaySFXWithPitch(string sfxName, float volume, float pitch)
    {
        if (sfxClips.TryGetValue(sfxName, out AudioClip clip))
        {
            sfxSource.pitch = Mathf.Clamp(pitch, 0.5f, 2f);
            sfxSource.PlayOneShot(clip, volume);
        }
        else
        {
            Debug.LogWarning("SFX not found: " + sfxName);
        }
    }
}