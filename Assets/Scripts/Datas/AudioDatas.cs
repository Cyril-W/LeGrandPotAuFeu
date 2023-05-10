using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "AudioDatas", menuName = "ScriptableObjects/AudioDatas", order = 1)]
public class AudioDatas : ScriptableObject {
    public List<SoundEffectData> soundEffectDatas = new List<SoundEffectData>();
    public List<MusicData> musicDatas = new List<MusicData>();
    [SerializeField] bool showLog = false;

    static readonly string SOUNDEFFECTS_DATAS_PATH = "Assets/Datas/SoundEffects";
    static readonly string SOUNDEFFECTS_AUDIOCLIP_PATH = "Assets/Audio/SoundEffects";
    static readonly string MUSIC_DATAS_PATH = "Assets/Datas/Music";
    static readonly string MUSIC_AUDIOCLIP_PATH = "Assets/Audio/Music";

#if UNITY_EDITOR
    [ContextMenu("Load Audio Datas")]
    void LoadAudioDatas() {
        LoadSoundEffectDatas();
        LoadMusicData();
    }

    void LoadSoundEffectDatas() {
        var soundEffectsGUIDs = AssetDatabase.FindAssets("t:" + typeof(SoundEffectData).ToString(), new[] { SOUNDEFFECTS_DATAS_PATH });
        soundEffectDatas.Clear();
        foreach (var guid in soundEffectsGUIDs) {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            Log("[SoundEffect] Loading SoundEffectData at: " + path, false);
            var soundEffect = AssetDatabase.LoadAssetAtPath<SoundEffectData>(path);
            if (soundEffect != null) { soundEffectDatas.Add(soundEffect); }
            else { Log("[SoundEffect] SoundEffectData not found at: " + path, true); }
        }
    }

    [ContextMenu("Create Sound Effect Datas")]
    void CreateSoundEffectDatas() {
        Log("========== [SoundEffect] Loading started ==========", false);
        LoadSoundEffectDatas();
        Log("========== [SoundEffect] Loading finished ==========", false);
        var soundEffectGUIDs = AssetDatabase.FindAssets(GetTypeOfSearchInstruction(typeof(AudioClip)), new[] { SOUNDEFFECTS_AUDIOCLIP_PATH });
        foreach (var guid in soundEffectGUIDs) {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            Log("[SoundEffect] Loading AudioClip at: " + path, false);
            var audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (audioClip != null) {
                if (!soundEffectDatas.Any(s => s.name == audioClip.name)) {
                    var soundEffect = CreateInstance<SoundEffectData>();
                    soundEffect.name = audioClip.name;
                    soundEffect.AudioClip = audioClip;
                    var assetPath = SOUNDEFFECTS_DATAS_PATH + "/" + soundEffect.name + ".asset";
                    AssetDatabase.CreateAsset(soundEffect, assetPath);
                    var newAsset = AssetDatabase.LoadAssetAtPath<SoundEffectData>(assetPath);
                    if (newAsset != null) { soundEffectDatas.Add(newAsset); }
                    else { Log("[SoundEffect] Created SoundEffectData not found at: " + newAsset, true); }
                } else { Log("[SoundEffect] AudioClip already has a SoundEffectData: " + audioClip.name, false); }
            } else { Log("[SoundEffect] AudioClip not found at: " + path, true); }
        }
        AssetDatabase.SaveAssets();
    }

    void LoadMusicData() {
        var musicGUIDs = AssetDatabase.FindAssets("t:" + typeof(MusicData).ToString(), new[] { MUSIC_DATAS_PATH });
        musicDatas.Clear();
        foreach (var guid in musicGUIDs) {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            Log("[Music] Loading MusicData at: " + path, false);
            var music = AssetDatabase.LoadAssetAtPath<MusicData>(path);
            if (music != null) { musicDatas.Add(music); } 
            else { Log("[Music] MusicData not found at: " + path, true); }
        }
    }

    [ContextMenu("Create Music Datas")]
    void CreateMusicDatas() {
        Log("========== [Music] Loading started ==========", false);
        LoadMusicData();
        Log("========== [Music] Loading finished ==========", false);
        var musicGUIDs = AssetDatabase.FindAssets(GetTypeOfSearchInstruction(typeof(AudioClip)), new[] { MUSIC_AUDIOCLIP_PATH });
        foreach (var guid in musicGUIDs) {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            Log("[Music] Loading AudioClip at: " + path, false);
            var audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (audioClip != null) {
                if (!musicDatas.Any(m => m.name == audioClip.name)) {
                    var music = CreateInstance<MusicData>();
                    music.name = audioClip.name;
                    music.AudioClip = audioClip;
                    var assetPath = MUSIC_DATAS_PATH + "/" + music.name + ".asset";
                    AssetDatabase.CreateAsset(music, assetPath);
                    var newAsset = AssetDatabase.LoadAssetAtPath<MusicData>(assetPath);
                    if (newAsset != null) { musicDatas.Add(newAsset); } 
                    else { Log("[Music] Created MusicData not found at: " + newAsset, true); }
                } else { Log("[Music] AudioClip already has a MusicData: " + audioClip.name, false); }
            } else { Log("[Music] AudioClip not found at: " + path, true); }
        }
        AssetDatabase.SaveAssets();
    }

    string GetTypeOfSearchInstruction(System.Type t) {
        return "t:" + t.ToString().Replace("UnityEngine.", "");
    }

    void Log(string message, bool isError) {
        if (!showLog) { return; }
        if (isError) { Debug.LogError(message); }
        else { Debug.Log(message); }
    }
#endif
}
