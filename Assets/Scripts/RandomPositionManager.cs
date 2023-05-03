using UnityEngine;

public class RandomPositionManager : MonoBehaviour {
    [SerializeField] Transform[] randomPositions;
    [SerializeField] Transform objectToPosition;
    [SerializeField] Vector3 objectPositionOffset = Vector3.up;
    [SerializeField] Vector3 objectRotationOffset = Vector3.zero;
    [SerializeField, ReadOnly] int currentRandom = 0; 
    void OnEnable() {
        if (objectToPosition == null || randomPositions == null || randomPositions.Length <= 0) return;
        currentRandom = Random.Range(0, randomPositions.Length);
        var randomPosition = randomPositions[currentRandom];
        objectToPosition.position = randomPosition.position + objectPositionOffset;
        objectToPosition.rotation = Quaternion.Euler(randomPosition.rotation.eulerAngles + objectRotationOffset);
    }
}
