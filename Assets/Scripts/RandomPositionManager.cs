using System.Collections.Generic;
using UnityEngine;

public class RandomPositionManager : MonoBehaviour {
    [System.Serializable]
    class RandomObjectToPlace {
        [HideInInspector] public string objectName;
        [ReadOnly] public int currentRandomIndex = -1;
        public Transform objectTransform;
        public Vector3 objectPositionOffset = Vector3.up;
        public Vector3 objectRotationOffset = Vector3.zero;
    }

    [SerializeField] Transform[] randomTransforms;
    [SerializeField] RandomObjectToPlace[] randomObjects;
    [SerializeField] bool deactivateActivateObjects = false;

    List<int> randomIndexes = new List<int>();
    Transform transformToPlace, randomTransform;
    int currentRandomIndex;

    void OnValidate() {
        foreach (var obj in randomObjects) {
            if (obj.objectTransform != null) { obj.objectName = obj.objectTransform.name; }
        }
    }

    void OnEnable() {
        if (randomObjects == null || randomObjects.Length <= 0 || randomTransforms == null || randomTransforms.Length <= 0) return;
        if (deactivateActivateObjects) {
            foreach (var obj in randomObjects) {
                obj.objectTransform.gameObject.SetActive(false);
            }
        }
        randomIndexes.Clear();
        for (int i = 0; i < randomTransforms.Length; i++) {
            randomIndexes.Add(i);
        }
        for (int i = 0; i < randomObjects.Length; i++) {
            currentRandomIndex = randomIndexes[Random.Range(0, randomIndexes.Count)];
            randomObjects[i].currentRandomIndex = currentRandomIndex;
            randomIndexes.Remove(currentRandomIndex);
            transformToPlace = randomObjects[i].objectTransform;
            if (transformToPlace == null) { continue; }
            randomTransform = randomTransforms[currentRandomIndex];
            if (randomTransform == null) { continue; }
            Debug.Log(transformToPlace.name + " going to " + randomTransform.name);
            transformToPlace.position = randomTransform.position + randomObjects[i].objectPositionOffset;
            transformToPlace.rotation = Quaternion.Euler(randomTransform.rotation.eulerAngles + randomObjects[i].objectRotationOffset);            
        }
        if (deactivateActivateObjects) {
            foreach (var obj in randomObjects) {
                obj.objectTransform.gameObject.SetActive(true);
            }
        }
    }
}
