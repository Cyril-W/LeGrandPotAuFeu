using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;

[Serializable]
public class HeroModel {
    [HideInInspector] public string HeroName;
    public Hero Hero;
    public GameObject Model;
    public SpellBehavior SpellBehavior;
    public HeroBehavior HeroBehavior;
    public bool Saved = false;
}

public class GroupBehavior : MonoBehaviour {
    [SerializeField] HeroModel[] heroes;
    [Header("Witch")]
    [SerializeField, Layer] int layer = 3;
    [SerializeField] BoxCollider teleportArea;
    [SerializeField, Range(0, 100)] int numberXTeleportPoints = 20;
    [SerializeField, Range(0, 100)] int numberZTeleportPoints = 20;
    [SerializeField] float distanceToObstacles = 0.5f;
    [SerializeField] float distanceToPlayer = 1f;
    [SerializeField] float sphereSize = 0.1f;
    [SerializeField] Color sphereColor = Color.magenta;

    List<Collider> collidersToAvoid = new List<Collider>();
    List<Vector3> teleportPoints = new List<Vector3>();

    [ContextMenu("Fill Hero Models")]
    void FillHeroeModels () {
        var spellBehaviors = FindObjectsOfType<SpellBehavior>();
        var heroBehaviors = FindObjectsOfType<HeroBehavior>();
        foreach (var h in heroes) {
            h.HeroName = h.Hero.ToString();
            if (h.Model == null) { 
                var child = transform.GetChild(1).Find("Witch");
                if (child != null) h.Model = child.gameObject;
            }
            if (h.SpellBehavior == null) { h.SpellBehavior = spellBehaviors.FirstOrDefault(sB => sB.GetHero() == h.Hero); }
            if (h.HeroBehavior == null) { h.HeroBehavior = heroBehaviors.FirstOrDefault(hB => hB.GetHero() == h.Hero); }
        }
    }

    void OnEnable() {
        foreach (var h in heroes) {
            UpdateHero(h, h.Saved);
        }
        collidersToAvoid.Clear();
        var boxColliders = FindObjectsOfType<BoxCollider>();
        foreach (var coll in boxColliders) {
            if (coll.enabled && coll.gameObject.layer == layer) collidersToAvoid.Add(coll);
        }
        var sphereColliders = FindObjectsOfType<SphereCollider>();
        foreach (var coll in sphereColliders) {
            if (coll.enabled && coll.gameObject.layer == layer) collidersToAvoid.Add(coll);
        }
        GetTeleportPoints();
    }

    [ContextMenu("Teleport")]
    public void Teleport() {
        GetTeleportPoints();

        if (teleportPoints.Count <= 0) return;
        transform.position = teleportPoints[UnityEngine.Random.Range(0, teleportPoints.Count)] + Vector3.up;
    }

    void GetTeleportPoints() {
        teleportPoints.Clear();
        var boxBounds = teleportArea.bounds;
        var topLeft = new Vector3(boxBounds.center.x - boxBounds.extents.x, 0f, boxBounds.center.z + boxBounds.extents.z);
        var topRight = new Vector3(boxBounds.center.x + boxBounds.extents.x, 0f, boxBounds.center.z - boxBounds.extents.z);
        var bottomLeft = new Vector3(boxBounds.center.x - boxBounds.extents.x, 0f, boxBounds.center.z - boxBounds.extents.z);
        var x = topLeft.x;
        for (var i = 0; i <= numberXTeleportPoints; i++) {
            var z = topLeft.z;
            for (var j = 0; j <= numberZTeleportPoints; j++) {
                var point = new Vector3(x, 0f, z);
                if (Vector3.Distance(transform.position, point) > distanceToPlayer) {
                    var isToAvoid = false;
                    foreach (var coll in collidersToAvoid) {
                        if (coll.bounds.Contains(point) || Vector3.Distance(coll.ClosestPointOnBounds(point), point) <= distanceToObstacles) {
                            isToAvoid = true;
                            break;
                        }
                    }
                    if (!isToAvoid) {
                        teleportPoints.Add(point);
                    }
                } 
                z -= Mathf.Abs(topLeft.z - bottomLeft.z) / numberZTeleportPoints;
            }
            x += Mathf.Abs(topLeft.x - topRight.x) / numberXTeleportPoints;
        }
    }

    public void SaveHero(Hero hero) {
        if (heroes.Any(h => h.Hero == hero)) {
            var savedHero = heroes.Where(h => h.Hero == hero).FirstOrDefault();
            if (savedHero != null) {
                UpdateHero(savedHero, true);
            }
        }
    }

    void UpdateHero(HeroModel hero, bool saved) {
        hero.Saved = saved;
        if (hero.Model != null) hero.Model.SetActive(hero.Saved);
        if (hero.SpellBehavior != null) hero.SpellBehavior.gameObject.SetActive(hero.Saved);
        if (hero.HeroBehavior != null) hero.HeroBehavior.gameObject.SetActive(!hero.Saved);
    }

    void OnDrawGizmos() {
        Gizmos.color = sphereColor;
        foreach (var teleportPoint in teleportPoints) {
            Gizmos.DrawSphere(teleportPoint, sphereSize);
        }
    }
}
