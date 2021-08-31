using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// random from list
// play a sound with random pitch from randomized list
// maybe use loudspeaker object

public class Toolbox {
    static public IEnumerator RunAfterTime(float delay, Action function) {
        float timer = 0;
        while (timer < delay) {
            timer += Time.deltaTime;
            yield return null;
        }
        function();
        yield return null;
    }
    static public T RandomFromList<T>(IReadOnlyList<T> list) {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }
    static public void RandomizeOneShot(AudioSource audioSource, AudioClip audioClip, float randomPitchWidth = 0.2f) {
        if (!audioSource.isActiveAndEnabled)
            return;
        if (randomPitchWidth > 0) {
            audioSource.pitch = UnityEngine.Random.Range(1 - (randomPitchWidth / 2f), 1 + (randomPitchWidth / 2f));
        }
        audioSource.PlayOneShot(audioClip);
    }
    static public void RandomizeOneShot(AudioSource audioSource, AudioClip[] audioClips, float randomPitchWidth = 0.2f) {
        if (audioSource == null) {
            Debug.LogWarning("Randomize oneshot called with null audiosource");
            return;
        }
        if (audioClips.Length == 0) {
            // Debug.LogWarning("Randomize oneshot called with empty clips");
            return;
        }
        RandomizeOneShot(audioSource, RandomFromList(audioClips), randomPitchWidth: randomPitchWidth);
    }
    public static void AudioSpeaker(Vector3 position, AudioClip[] clips) {
        GameObject audioSpeaker = GameObject.Instantiate(Resources.Load("prefabs/audioSpeaker")) as GameObject;
        DestroyOnSoundStop ds = audioSpeaker.GetComponent<DestroyOnSoundStop>();
        if (ds != null) {
            ds.clip = clips;
        }
    }
    static public void DestroyIfExists<T>(GameObject gameObject) where T : Component {
        T t = gameObject.GetComponent<T>();
        if (t != null) {
            GameObject.Destroy(t);
        }
    }

    static public Direction DirectionFromAngle(float angle) {
        if (angle < 22.5 && angle >= -22.5) {
            return Direction.up;
        } else if (angle >= 22.5 && angle < 67.5) {
            return Direction.leftUp;
        } else if (angle >= 67.5 && angle < 112.5) {
            return Direction.left;
        } else if (angle >= 112.5 && angle < 157.5) {
            return Direction.leftDown;
        } else if (angle > 157.5 || angle < -157.5) {
            return Direction.down;
        } else if (angle >= -157.5 && angle < -112.5) {
            return Direction.rightDown;
        } else if (angle >= -112.5 && angle < -67.5) {
            return Direction.right;
        } else if (angle >= -67.5 && angle < -22.5) {
            return Direction.rightUp;
        } else return Direction.right;
    }
    static public T GetOrReturnComponent<T>(GameObject target) where T : Component {
        T component = target.GetComponent<T>();
        if (component != null) {
            return component;
        } else {
            component = target.AddComponent<T>();
            return component;
        }
    }
    static public TagSystemData GetTagData(GameObject target) {
        TagSystem system = target.GetComponent<TagSystem>();
        if (system != null) {
            return system.data;
        } else return new TagSystemData();
    }
    static public Vector3 SuperJumpVelocity(Vector3 position, float superJumpSpeed, float gravity) {
        Vector3 phat = new Vector3(position.x, 0, position.z).normalized;

        float angle = SuperJumpAngle(position, superJumpSpeed, gravity);

        return (phat * Mathf.Cos(angle) + new Vector3(0, Mathf.Sin(angle), 0)) * superJumpSpeed;
    }
    static public float SuperJumpAngle(Vector3 position, float superJumpSpeed, float gravity) {
        float m = superJumpSpeed;
        float m2 = Mathf.Pow(superJumpSpeed, 2);
        float m4 = Mathf.Pow(superJumpSpeed, 4);
        float g2 = Mathf.Pow(gravity, 2);
        float p = Mathf.Pow(position.x, 2) + Mathf.Pow(position.z, 2);

        return (float)Mathf.Acos(Mathf.Sqrt(m2 - Mathf.Sqrt(m4 - g2 * p)) / (m * Mathf.Sqrt(2)));
    }
    static public float SuperJumpTime(Vector3 position, float superJumpSpeed, float gravity) {

        float angle = SuperJumpAngle(position, superJumpSpeed, gravity);
        return -1f * (2 * superJumpSpeed * Mathf.Sin(angle)) / gravity;
    }
    static public float SuperJumpRange(float superJumpSpeed, float gravity) {
        // TODO: data-driven range, potentially
        return Mathf.Abs((Mathf.Pow(superJumpSpeed, 2) / gravity)) / 2f;
    }
}
