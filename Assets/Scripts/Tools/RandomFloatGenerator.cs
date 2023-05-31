using UnityEngine;
using UnityEngine.Events;

public class RandomFloatGenerator : MonoBehaviour {    
    [SerializeField] bool randomOnStart = false;
    [SerializeField] bool randomOnEnable = false;
    [SerializeField] Vector2 minMaxFloat = Vector2.zero;
    [SerializeField] UnityEvent<float> onFloatGenerated;
    [SerializeField, ReadOnly] float lastFloatGenerated;   

    void Start() {
        if (randomOnStart) { GenerateFloat(); }
    }

    void OnEnable() {
        if (randomOnEnable) { GenerateFloat(); }
    }

    public void GenerateFloat() {
        lastFloatGenerated = Random.Range(minMaxFloat.x, minMaxFloat.y);
        onFloatGenerated?.Invoke(lastFloatGenerated);
    }
}
