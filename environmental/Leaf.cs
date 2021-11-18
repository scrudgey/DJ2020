using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf : MonoBehaviour {

    public SpriteRenderer spriteRenderer;
    public Sprite[] sprites;
    public Transform graphic;
    public Rigidbody body;
    private float timer;
    private float spriteInterval = 0.1f;
    private int index;
    void Start() {
        graphic.localPosition = new Vector3(Random.Range(0.1f, 0.35f), 0f, 0f);
        transform.rotation *= Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);
        index = Random.Range(0, sprites.Length);
        spriteRenderer.sprite = sprites[index];
        spriteRenderer.transform.localScale = new Vector3(Random.Range(0.8f, 1.1f), Random.Range(0.8f, 1.1f), Random.Range(0.8f, 1.1f));
        spriteInterval = Random.Range(0.1f, 0.3f);
        body.drag = Random.Range(3f, 5f);
    }
    void Update() {
        timer += Time.deltaTime;
        transform.rotation *= Quaternion.AngleAxis(1f, Vector3.up);
        if (timer > spriteInterval) {
            timer = 0f;
            index += 1;
            if (index >= sprites.Length) {
                index = 0;
            }
            spriteRenderer.sprite = sprites[index];
        }
        // if (body.velocity.magnitude <= 0.001f) {
        //     Destroy(this);
        // }
    }
    void OnCollisionEnter(Collision collision) {
        Destroy(this);
    }
}
