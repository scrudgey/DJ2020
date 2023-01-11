using System.Collections;
using System.Collections.Generic;
using Items;
using UnityEngine;
public class RocketLauncher : MonoBehaviour {
    public Octet<Sprite[]> spritesheet;
    public SpriteRenderer spriteRenderer;
    public void ShootRocket(RocketLauncherItem item, PlayerInput input, ItemHandler handler) {
        Vector3 direction = gunDirection(input.Fire.cursorData).normalized;
        GameObject obj = GameObject.Instantiate(item.rocketData.rocketPrefab, transform.position + direction, Quaternion.identity);
        Rigidbody rocketBody = obj.GetComponent<Rigidbody>();
        foreach (Collider myCollider in handler.transform.root.GetComponentsInChildren<Collider>()) {
            foreach (Collider rocketCollider in obj.GetComponentsInChildren<Collider>()) {
                Physics.IgnoreCollision(myCollider, rocketCollider, true);
            }
        }
        rocketBody.velocity = item.rocketData.rocketVelocity * direction;
    }
    public void SetSprite(Direction direction) {
        if (spritesheet == null || spritesheet[direction].Length == 0) return;
        spriteRenderer.sprite = spritesheet[direction][0];
        spriteRenderer.flipX = direction == Direction.left || direction == Direction.leftUp || direction == Direction.leftDown;
    }
    public Vector3 gunPosition() {
        return new Vector3(transform.position.x, transform.position.y + 0.45f, transform.position.z);
    }
    public Vector3 gunDirection(CursorData data) {
        if (data == null) return Vector3.zero;
        return data.worldPosition - this.gunPosition();
    }
}
