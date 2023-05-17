#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using System.Linq;

[InitializeOnLoad]
#endif
public class SwitchShortcutsProfileOnPlay {
#if UNITY_EDITOR
    private const string PlayingProfileId = "Playing";
    private const string DefaultProfileId = "Default";
    private static string _activeProfileId;
    //private static bool _switched;

    static SwitchShortcutsProfileOnPlay() {
        EditorApplication.playModeStateChanged += DetectPlayModeState;
    }

    private static void SetActiveProfile(string profileId) {
        _activeProfileId = ShortcutManager.instance.activeProfileId;
        if (_activeProfileId.Equals(profileId)) {
            Debug.Log("Same as active");
            return; 
        } 
        var allProfiles = ShortcutManager.instance.GetAvailableProfileIds().ToList();
        if (!allProfiles.Contains(PlayingProfileId)) {
            Debug.LogError("Couldn't find profile named: " + profileId);
            return; 
        }
        Debug.Log($"Activating Shortcut profile \"{profileId}\"");
        ShortcutManager.instance.activeProfileId = profileId;
    }

    private static void DetectPlayModeState(PlayModeStateChange state) {
        switch (state) {
            case PlayModeStateChange.EnteredPlayMode:
                OnEnteredPlayMode();
                break;
            case PlayModeStateChange.ExitingPlayMode:
                OnExitingPlayMode();
                break;
        }
    }

    private static void OnExitingPlayMode() {
        //if (!_switched) { return; }
        //_switched = false;
        SetActiveProfile(DefaultProfileId);
    }

    private static void OnEnteredPlayMode() {  
        //_switched = true;
        SetActiveProfile(PlayingProfileId);
    }
#endif
}
