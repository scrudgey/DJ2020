using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// random from list
// play a sound with random pitch from randomized list
// maybe use loudspeaker object

public class Toolbox {
    public static GameObject explosiveRadiusPrefab;
    public static readonly string ExplosiveRadiusPath = "prefabs/explosiveRadius";
    public static AudioMixer sfxMixer;
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
    public static AudioSource AudioSpeaker(Vector3 position, AudioClip[] clips, float volume = 1f) {
        GameObject audioSpeaker = GameObject.Instantiate(Resources.Load("prefabs/audioSpeaker"), position, Quaternion.identity) as GameObject;
        DestroyOnSoundStop ds = audioSpeaker.GetComponent<DestroyOnSoundStop>();
        if (ds != null) {
            ds.clip = clips;
            ds.audioSource.volume = volume;
        }
        return ds.audioSource;
    }
    static public void DestroyIfExists<T>(GameObject gameObject) where T : Component {
        T t = gameObject.GetComponent<T>();
        if (t != null) {
            GameObject.Destroy(t);
        }
    }
    static public void DisableIfExists<T>(GameObject gameObject) where T : MonoBehaviour {
        T t = gameObject.GetComponent<T>();
        if (t != null) {
            t.enabled = false;
        }
    }
    static public void EnableIfExists<T>(GameObject gameObject) where T : MonoBehaviour {
        T t = gameObject.GetComponent<T>();
        if (t != null) {
            t.enabled = true;
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
    static public T GetOrCreateComponent<T>(GameObject target) where T : Component {
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
    static public Vector3 RandomInsideBounds(Collider collider, float padding = 1f) {
        float x = UnityEngine.Random.Range(collider.bounds.center.x - (collider.bounds.extents.x * padding) / 2f, collider.bounds.center.x + (collider.bounds.extents.x * padding) / 2f);
        float y = UnityEngine.Random.Range(collider.bounds.center.y - (collider.bounds.extents.y * padding) / 2f, collider.bounds.center.y + (collider.bounds.extents.y * padding) / 2f);
        float z = UnityEngine.Random.Range(collider.bounds.center.z - (collider.bounds.extents.z * padding) / 2f, collider.bounds.center.z + (collider.bounds.extents.z * padding) / 2f);
        return new Vector3(x, y, z);
    }
    public static Vector3 GetVertexWorldPosition(Vector3 vertex, Transform owner) {
        return owner.localToWorldMatrix.MultiplyPoint3x4(vertex);
    }
    public static float RandomFromLoHi(LoHi input) {
        return UnityEngine.Random.Range(input.low, input.high);
    }
    public static Explosion Explosion(Vector3 position) {
        if (explosiveRadiusPrefab == null) {
            explosiveRadiusPrefab = Resources.Load(ExplosiveRadiusPath) as GameObject;
        }
        return GameObject.Instantiate(explosiveRadiusPrefab, position, Quaternion.identity).GetComponent<Explosion>();
    }
    public static float CalculateExplosionValue(Vector3 source, Vector3 target, float range, float power) {
        float dist = (target - source).magnitude;
        if (dist > range)
            return 0.0f;
        return (1.0f - dist / range) * power;
    }
    public static Vector3 CalculateExplosionVector(Vector3 source, Vector3 target, float range, float power) {
        Vector3 direction = (target - source).normalized;
        return CalculateExplosionValue(source, target, range, power) * direction;
    }

    public static Vector3 RandomPointOnPlane(Vector3 position, Vector3 normal, float radius) {
        Vector3 randomPoint;
        do {
            randomPoint = Vector3.Cross(UnityEngine.Random.insideUnitSphere, normal);
        } while (randomPoint == Vector3.zero);

        randomPoint.Normalize();
        randomPoint *= radius;
        randomPoint += position;

        return randomPoint;
    }

    public static Vector3 ColorToVector(Color color) {
        return new Vector3(color.r, color.g, color.b);
    }
    public static Color VectorToColor(Vector3 vector) {
        float x = Math.Abs(vector.x);
        float y = Math.Abs(vector.y);
        float z = Math.Abs(vector.z);
        return new Color(x, y, z);
    }

    public static AudioSource SetUpAudioSource(GameObject g) {
        AudioSource source = GetOrCreateComponent<AudioSource>(g);

        if (sfxMixer == null) {
            sfxMixer = Resources.Load("mixers/SoundEffectMixer") as AudioMixer;
        }
        source.outputAudioMixerGroup = sfxMixer.FindMatchingGroups("Master")[0];

        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = 1f;
        source.maxDistance = 5.42f;
        source.spatialBlend = 1;
        source.spread = 0.2f;
        return source;
    }
    public static int Moddo(int x, int m) {
        // // if (x < 0) {
        // //     return (x + m + 1) % m;
        // // } else return (x + m) % m;
        // return ((x % m) + m) % m;
        if (x < 0) {
            return m + x + 1; // -1 -> 7 = 7 - 1 + 1
        } else if (x <= m) {
            return x;
        } else {
            return x % m - 1;
        }
    }


    public static Direction ClampDirection(Direction direction, Direction clamp, int width = 2, bool suppressOutput = true) {
        int clampInt = (int)clamp;
        int dirInt = (int)direction;
        int lower = Moddo((clampInt - width), 7);
        int upper = Moddo((clampInt + width), 7);

        if (lower < clampInt && upper > clampInt) { // |---------lower----clamp------upper------|
            if (!suppressOutput)
                Debug.Log($"I");
            if (dirInt <= lower) {
                if (lower - dirInt > width) {
                    dirInt = upper;
                } else {
                    dirInt = lower;
                }
            } else if (dirInt >= upper) {
                if (dirInt - upper > width) {
                    dirInt = lower;
                } else {
                    dirInt = upper;

                }
            }
        } else if (lower > clampInt) { // |-----clamp--upper----------lower---|
            if (!suppressOutput)
                Debug.Log($"II");
            if (dirInt < lower && dirInt > upper) {
                if (Math.Abs(dirInt - lower) < Math.Abs(dirInt - upper)) {
                    dirInt = lower;
                } else {
                    dirInt = upper;
                }
            }
        } else if (upper < clampInt) { // |-upper----------------lower---clamp---|
            if (!suppressOutput)
                Debug.Log($"III");

            if (dirInt < lower && dirInt > upper) {
                if (Math.Abs(dirInt - lower) < Math.Abs(dirInt - upper)) {
                    dirInt = lower;
                } else {
                    dirInt = upper;
                    if (!suppressOutput)
                        Debug.Log($"({direction}, {clamp}) -> {(Direction)dirInt} III");
                }
            }
        }
        if (!suppressOutput) {
            Debug.Log($"direction: {direction}:{dirInt}");
            Debug.Log($"clamp: {clamp}:{clampInt}");
            Debug.Log($"bounds: [{(Direction)lower}:{lower}, {(Direction)upper}:{upper}])");
            Debug.Log($"output: {(Direction)dirInt}");
        }
        return (Direction)dirInt;
    }
}


