using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class LaserBeam : MonoBehaviour {
    public Transform beam;
    public CapsuleCollider capsule;
    public float maxLaserLength = 6f;
    public LaserTripwire tripWire;
    public MeshRenderer laserMesh;
    float tempVisibleTimer;
    void Start() {
        DetermineMaxLength(beam);
        SetLaserLength(beam);
        SetColliderLength(capsule);
    }
    private void OnTriggerEnter(Collider other) {
        if (other.isTrigger)
            return;
        SetLaserLength(beam);
    }
    void OnTriggerStay(Collider other) {
        if (other.isTrigger)
            return;
        SetLaserLength(beam);
    }
    void DetermineMaxLength(Transform laser) {
        Vector3 direction = laser.up;
        Ray ray = new Ray(transform.position, direction);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxLaserLength, LayerUtil.GetLayerMask(Layer.def, Layer.obj));

        foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) {
            if (hit.transform.IsChildOf(transform.root))
                continue;
            if (hit.collider.isTrigger)
                continue;
            maxLaserLength = hit.distance;
            break;
        }
    }
    void SetLaserLength(Transform laser) {
        Vector3 direction = laser.up;
        Ray ray = new Ray(transform.position, direction);
        // TODO: nonalloc
        RaycastHit[] hits = Physics.RaycastAll(ray, maxLaserLength, LayerUtil.GetLayerMask(Layer.def, Layer.obj, Layer.interactive), QueryTriggerInteraction.Ignore);
        float length = maxLaserLength / 2f;
        foreach (RaycastHit hit in hits.OrderBy(h => h.distance)) {
            if (hit.transform.IsChildOf(transform.root))
                continue;
            length = hit.distance / 2f;
            if (GameManager.I.playerObject != null && hit.collider.transform.IsChildOf(GameManager.I.playerObject.transform)) {
                // TODO: possibly, trigger on anything, not just player
                AlertTripWire();
            }
            break;
        }
        Vector3 newScale = new Vector3(0.02f, length, 0.02f);
        Vector3 newPosition = new Vector3(-1f * length, 0f, 0f);
        laser.localScale = newScale;
        laser.localPosition = newPosition;
    }
    void SetColliderLength(CapsuleCollider collider) {
        collider.radius = 0.15f;
        collider.center = new Vector3(-1f * maxLaserLength / 2f, 0f, 0f);
        collider.height = maxLaserLength;
        collider.transform.localPosition = Vector3.zero;
    }

    void AlertTripWire() {
        if (tripWire != null) {
            tripWire.LaserTripCallback();
        }
    }

    void Update() {
        if (tempVisibleTimer > 0) {
            tempVisibleTimer -= Time.unscaledDeltaTime;
            if (tempVisibleTimer <= 0) {
                laserMesh.gameObject.layer = LayerUtil.GetLayer(Layer.laser);
            }
        }
    }
    public void ShowLaserTemporarily(float timeout = 1f) {
        tempVisibleTimer = 1f;
        laserMesh.gameObject.layer = LayerUtil.GetLayer(Layer.def);
    }
}
