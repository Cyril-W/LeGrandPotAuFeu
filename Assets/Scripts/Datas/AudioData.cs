using UnityEngine;

public class AudioData : ScriptableObject {
    public AudioClip AudioClip;
    [Range(0f, 1f)] public float Volume = 1f;
    [Range(-3f, 3f)] public float Pitch = 1f;
    public bool IsLooping = false;
}
