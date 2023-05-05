using UnityEngine;

public class AudioManager : MonoBehaviour {
    [System.Serializable]
    class Sound {
        [HideInInspector] public string SoundName;
        public AudioClip AudioClip;
        [Range(0f, 1f)] public float Volume = 1f;
        [Range(-3f, 3f)] public float Pitch = 1f;
        public bool IsLooping = false;
        [ReadOnly] public AudioSource AudioSource;
    }

    public static AudioManager Instance { get; private set; }

    [SerializeField] Sound[] sounds;
    [SerializeField] AudioSource soundPrefab;
    [SerializeField, ReadOnly] string lastSoundPlayed;

    void OnValidate() {
        if (sounds == null) { return; }
        foreach (var sound in sounds) {
            if (sound.AudioClip != null) { sound.SoundName = sound.AudioClip.name; }
        }
    }

    void Awake() {
        if (Instance == null) { 
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        } else {
            //Destroy(gameObject);
            return;
        }        
        if (sounds == null) { return; }
        foreach (var sound in sounds) {
            var sP = Instantiate(soundPrefab, transform);
            sP.name = sound.SoundName;
            sP.clip = sound.AudioClip;
            sP.volume = sound.Volume;
            sP.pitch = sound.Pitch;
            sP.loop = sound.IsLooping;
            sound.AudioSource = sP;
        }
    }

    public void Play(AudioClip clip) {
        var s = FindSound(clip);
        if (s != null && s.AudioSource != null) {
            s.AudioSource.pitch = s.Pitch;
            s.AudioSource.timeSamples = 0;
            s.AudioSource.Play();
            lastSoundPlayed = s.SoundName;
        }
    }

    public void PlayReverse(AudioClip clip) {
        var s = FindSound(clip);
        if (s != null && s.AudioSource != null) {
            s.AudioSource.pitch = s.Pitch * -1f;
            s.AudioSource.timeSamples = s.AudioClip.samples - 1;
            s.AudioSource.Play();
            lastSoundPlayed = s.SoundName;
        }
    }

    public void Stop(AudioClip clip) {
        var s = FindSound(clip);
        if (s != null && s.AudioSource != null) { 
            s.AudioSource.Stop();
            if (lastSoundPlayed == s.SoundName) { lastSoundPlayed = ""; }
        }
    }

    Sound FindSound (AudioClip clip) {
        if (sounds == null || sounds.Length <= 0) { return null; }
        var s = System.Array.Find(sounds, s => s.SoundName == clip.name);
        if (s == null) { Debug.LogError("No sound found with name: " + clip.name); }
        return s;
    }
}
