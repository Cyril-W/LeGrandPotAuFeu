using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AudioManager : MonoBehaviour {
    [System.Serializable]
    class Audio {
        [HideInInspector] public string AudioName;
        [ReadOnly] public AudioData AudioData;
        [ReadOnly] public AudioSource AudioSource;
    }

    public static AudioManager Instance { get; private set; }

    [SerializeField] Audio[] audios;
    [SerializeField] AudioSource soundPrefab;
    [SerializeField, ReadOnly] string lastSoundEffectPlayed;
    [SerializeField, ReadOnly] string lastMusicPlayed;

    static readonly string AUDIODATAS_PATH = "Assets/Datas/AudioDatas.asset";

    void OnValidate() {
        if (audios == null) { return; }
        foreach (var audio in audios) {
            if (audio.AudioData != null) { audio.AudioName = audio.AudioData.name; }
        }
#if UNITY_EDITOR
        PopulateAudios();
#endif
    }

    void Awake() {
        if (Instance == null) { 
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        } else {
            //Destroy(gameObject);
            return;
        }        
        if (audios == null) { return; }
        foreach (var audio in audios) {
            var sP = Instantiate(soundPrefab, transform);
            sP.enabled = false;
            sP.name = audio.AudioName;
            var sound = audio.AudioData;
            sP.clip = sound.AudioClip;
            sP.volume = sound.Volume;
            sP.pitch = sound.Pitch;
            sP.loop = sound.IsLooping;
            audio.AudioSource = sP;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Populate Audios")]
    void PopulateAudios() {
        var audioDatas = AssetDatabase.LoadAssetAtPath<AudioDatas>(AUDIODATAS_PATH);
        if (audioDatas == null) { Debug.LogError("No Assets Datas found at " + AUDIODATAS_PATH); return; }
        var newAudios = audios.ToList();
        var audioDataList = new List<AudioData>();
        audioDataList.AddRange(audioDatas.soundEffectDatas);
        audioDataList.AddRange(audioDatas.musicDatas);
        foreach (var sE in audioDataList) {
            if (newAudios.Any(a => a.AudioName == sE.name)) { continue; }
            var newAudio = new Audio();
            newAudio.AudioName = sE.name;
            newAudio.AudioData = sE;
            newAudio.AudioSource = null;
            newAudios.Add(newAudio);
            Debug.Log("[AudioManager] - Loading new audio: " + newAudio.AudioName);
        }
        audios = newAudios.ToArray();
    }
#endif

    public void PlayMusic(MusicData music) {
        var audio = FindAudio(music);
        if (audio != null) {
            audio.AudioSource.enabled = true;
            audio.AudioSource.volume = music.Volume;
            audio.AudioSource.pitch = music.Pitch;
            audio.AudioSource.timeSamples = 0;
            audio.AudioSource.Play();
            lastMusicPlayed = music.name;
        }
    }

    public void PauseMusic(MusicData music) {
        var audio = FindAudio(music);
        if (audio != null) {
            audio.AudioSource.Pause();
        }
    }

    public void UnPauseMusic(MusicData music) {
        var audio = FindAudio(music);
        if (audio != null) {
            audio.AudioSource.UnPause();
        }
    }

    public void StopMusic(MusicData music) {
        var audio = FindAudio(music);
        if (audio != null) {
            audio.AudioSource.Stop();
            audio.AudioSource.enabled = false;
            if (lastMusicPlayed == music.name) { lastMusicPlayed = ""; }
        }
    }

    float GetRandomPitch(SoundEffectData soundEffect) {
        if (Mathf.Abs(soundEffect.MinMaxRandomPitch.magnitude) <= 0.1f) { return soundEffect.Pitch; }
        return Random.Range(soundEffect.Pitch + soundEffect.MinMaxRandomPitch.x, soundEffect.Pitch + soundEffect.MinMaxRandomPitch.y);
    }

    public void PlaySoundEffect(SoundEffectData soundEffect) {
        var audio = FindAudio(soundEffect);
        if (audio != null) {
            audio.AudioSource.enabled = true;
            audio.AudioSource.volume = soundEffect.Volume;
            audio.AudioSource.pitch = GetRandomPitch(soundEffect);
            audio.AudioSource.timeSamples = 0;
            audio.AudioSource.Play();
            lastSoundEffectPlayed = soundEffect.name;
        }
    }

    public void PlayReverseSoundEffect(SoundEffectData soundEffect) {
        var audio = FindAudio(soundEffect);
        if (audio != null) {
            audio.AudioSource.enabled = true;
            audio.AudioSource.volume = soundEffect.Volume;
            audio.AudioSource.pitch = GetRandomPitch(soundEffect) * -1f;
            audio.AudioSource.timeSamples = soundEffect.AudioClip.samples - 1; ;
            audio.AudioSource.Play();
            lastSoundEffectPlayed = soundEffect.name;
        }
    }

    public void PlayOneShotSoundEffect(SoundEffectData soundEffect) {
        var audio = FindAudio(soundEffect);
        if (audio != null) {
            audio.AudioSource.enabled = true;
            audio.AudioSource.volume = soundEffect.Volume;
            audio.AudioSource.pitch = GetRandomPitch(soundEffect);
            audio.AudioSource.timeSamples = 0;
            audio.AudioSource.PlayOneShot(audio.AudioSource.clip);
            lastSoundEffectPlayed = soundEffect.name;
        }
    }

    public void StopSoundEffect(SoundEffectData soundEffect) {
        var audio = FindAudio(soundEffect);
        if (audio != null) {
            audio.AudioSource.Stop();
            audio.AudioSource.enabled = false;
            if (lastSoundEffectPlayed == soundEffect.name) { lastSoundEffectPlayed = ""; }
        }
    }

    Audio FindAudio (AudioData audioData) {
        if (audios == null || audios.Length <= 0) { return null; }
        var a = System.Array.Find(audios, a => a.AudioData == audioData);
        if (a == null) { Debug.LogError("No sound found with name: " + audioData.name); return null; }
        if (a.AudioSource == null) { Debug.LogError("The sound with name: " + audioData.name + " does not have an audiosource instantiated"); return null; }
        return a;
    }
}
