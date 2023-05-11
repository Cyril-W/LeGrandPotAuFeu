using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RandomIntGenerator : MonoBehaviour {
    [System.Serializable]
    class IntEvent {
        [HideInInspector] public string IntEventName;
        public int IntParameter;
        public UnityEvent OnIntEvent;
    }

    [SerializeField] bool checkIntEvents = false;
    [SerializeField] Vector2 minMaxInt = Vector2.zero;
    [SerializeField] UnityEvent<int> onIntGenerated;
    [SerializeField] IntEvent[] onIntGeneratedEvents;
    [SerializeField, ReadOnly] int lastIntGenerated;

    List<int> indexNoEvent = new List<int>();

    void OnValidate() {
        checkIntEvents = false;
        var min = Mathf.RoundToInt(minMaxInt.x);
        minMaxInt.x = min;
        var max = Mathf.RoundToInt(minMaxInt.y);
        minMaxInt.y = max;
        if (onIntGeneratedEvents == null) { Debug.LogError("onIntGeneratedEvents is null"); return; }
        indexNoEvent.Clear();
        for (int i = min; i <= max; i++) {
            indexNoEvent.Add(i);
        }
        foreach (var e in onIntGeneratedEvents) {
            e.IntEventName = "Event[" + e.IntParameter + "]";
            if (indexNoEvent.Contains(e.IntParameter)) {
                indexNoEvent.Remove(e.IntParameter);
            }
        }
        if (indexNoEvent.Count > 0) { Debug.LogError("Not enough UnityEvent to match possible randoms. Still " + indexNoEvent.Count + " conflicts"); }
    }

    public void GenerateInt() {
        lastIntGenerated = Random.Range(Mathf.RoundToInt(minMaxInt.x), Mathf.RoundToInt(minMaxInt.y) + 1);
        onIntGenerated?.Invoke(lastIntGenerated);
        if (onIntGeneratedEvents == null) { Debug.LogError("UnityEvent[] is null"); return; }
        var e = System.Array.Find(onIntGeneratedEvents, e => e.IntParameter == lastIntGenerated);
        if (e == null) {
            Debug.LogError("Not enough UnityEvent to match current random number " + lastIntGenerated + ". Length is: " + onIntGeneratedEvents.Length);
        } else {
            e.OnIntEvent?.Invoke();
        }
    }
}
