using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Audio;
// random from list
// play a sound with random pitch from randomized list
// maybe use loudspeaker object

public class Toolbox {
    public static GameObject explosiveRadiusPrefab;
    public static readonly string ExplosiveRadiusPath = "prefabs/explosiveRadius";
    public static AudioMixer sfxMixer;
    static Regex cloneFinder = new Regex(@"(.+)\(Clone\)$", RegexOptions.Multiline);
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
    static public void RandomizeOneShot(AudioSource audioSource, AudioClip audioClip, float randomPitchWidth = 0.2f, float volume = 1f) {
        if (!audioSource.isActiveAndEnabled)
            return;
        if (randomPitchWidth > 0) {
            audioSource.pitch = UnityEngine.Random.Range(1 - (randomPitchWidth / 2f), 1 + (randomPitchWidth / 2f));
        }
        audioSource.PlayOneShot(audioClip, volume);
    }
    static public string NameWithoutClone(GameObject gameObject) {
        if (gameObject == null)
            return "";
        return CloneRemover(gameObject.name);
    }
    public static string CloneRemover(string input) {
        string output = input;
        if (input != null) {
            MatchCollection matches = cloneFinder.Matches(input);
            if (matches.Count > 0) {                                    // the object is a clone, capture just the normal name
                foreach (Match match in matches) {
                    output = match.Groups[1].Value;
                }
            }
            // TODO: numbermatcher
        }
        return output;
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
        GameObject audioSpeaker = PoolManager.I.GetPool("prefabs/audioSpeaker").GetObject(position);
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
    static public T GetOrCreateComponent<T>(GameObject target, bool inChildren = false) where T : Component {
        T component = inChildren ? target.GetComponentInChildren<T>() : target.GetComponent<T>();

        if (component != null) {
            return component;
        } else {
            component = target.AddComponent<T>();
            return component;
        }
    }
    static public TagSystemData GetTagData(GameObject target) {
        TagSystem system = target.transform.root.GetComponentInChildren<TagSystem>();
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
    public static Explosion Explosion(Vector3 position) {
        if (explosiveRadiusPrefab == null) {
            explosiveRadiusPrefab = Resources.Load(ExplosiveRadiusPath) as GameObject;
        }
        return GameObject.Instantiate(explosiveRadiusPrefab, position, Quaternion.identity).GetComponent<Explosion>();
    }
    public static NoiseComponent Noise(Vector3 position, NoiseData data, GameObject source) {
        GameObject noiseObject = PoolManager.I.GetPool("prefabs/noise").GetObject(position);
        NoiseComponent component = noiseObject.GetComponent<NoiseComponent>();
        component.data = data with {
            source = source
        };
        component.sphereCollider.radius = data.volume;
        return component;
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
        // TODO: support sound mixers
        AudioSource source = GetOrCreateComponent<AudioSource>(g);
        if (source.outputAudioMixerGroup == null) {
            sfxMixer = Resources.Load("mixers/SoundEffectMixer") as AudioMixer;
            source.outputAudioMixerGroup = sfxMixer.FindMatchingGroups("General")[0];
        }
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = 3f;
        source.maxDistance = 23f;
        source.spatialBlend = 1;
        source.spread = 0.2f;
        return source;
    }
    public static int Moddo(int x, int m) {
        if (x < 0) {
            return m + x + 1;
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
            if (dirInt < lower && dirInt > upper) {
                if (Math.Abs(dirInt - lower) < Math.Abs(dirInt - upper)) {
                    dirInt = lower;
                } else {
                    dirInt = upper;
                }
            }
        } else if (upper < clampInt) { // |-upper----------------lower---clamp---|
            if (dirInt < lower && dirInt > upper) {
                if (Math.Abs(dirInt - lower) < Math.Abs(dirInt - upper)) {
                    dirInt = lower;
                } else {
                    dirInt = upper;
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
    public static Quaternion QuaternionDelta(Quaternion origin, Quaternion target) {
        Quaternion identityOrigin = Quaternion.identity * Quaternion.Inverse(origin);
        Quaternion identityTarget = Quaternion.identity * Quaternion.Inverse(target);

        return identityOrigin * Quaternion.Inverse(identityTarget);
    }

    public static float Triangle(float minLevel, float maxLevel, float period, float phase, float t) {
        float pos = Mathf.Repeat(t - phase, period) / period;
        if (pos < .5f) {
            return Mathf.Lerp(minLevel, maxLevel, pos * 2f);
        } else {
            return Mathf.Lerp(maxLevel, minLevel, (pos - .5f) * 2f);
        }
    }

    public static int DiscreteLightLevel(float lightLevel, bool isCrouching, bool isMoving) {
        // B < 10: -
        // 10 < B < 15: +
        // 15 < B < 30: ++
        // 30 < B < 60: +++
        // 60 < B < 80: ++++
        // 80 < B : +++++

        int level = 0;
        if (lightLevel <= 10) {
            level = 0;
        } else if (10 < lightLevel && lightLevel <= 15) {
            level = 1;
        } else if (15 < lightLevel && lightLevel <= 30) {
            level = 2;
        } else if (30 < lightLevel && lightLevel <= 60) {
            level = 3;
        } else if (60 < lightLevel && lightLevel <= 80) {
            level = 4;
        } else if (80 < lightLevel) {
            level = 5;
        }
        if (isCrouching) {
            level -= 1;
        }
        if (isMoving) {
            level += 1;
        }
        level = Math.Max(0, level);
        level = Math.Min(level, 5);

        return level;
    }
    public static T Max<T>(T a, T b) where T : IComparable {
        return a.CompareTo(b) >= 0 ? a : b;
    }

    static public float SquareWave(float currentphase, float dutycycle = 0f) {
        // dutycyle: [-1, 1] float
        // -1: constant 0
        // +1: constant 1
        return (1 + Mathf.Sign(Mathf.Sin(currentphase * 2f * (float)Math.PI) + dutycycle)) / 2f;
    }

    static public Quaternion SnapToClosestRotation(Quaternion input, List<Quaternion> lattice) =>
        lattice.Aggregate((curMin, x) => (curMin == null || (Quaternion.Angle(input, x)) < Quaternion.Angle(input, curMin) ? x : curMin));


    static public Rect GetTotalRenderBoundingBox(Transform root, Camera UICamera, bool adjustYScale = true) {
        float total_min_x = float.MaxValue;
        float total_max_x = float.MinValue;
        float total_min_y = float.MaxValue;
        float total_max_y = float.MinValue;
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>()) {
            if (renderer is LineRenderer) continue;
            if (renderer.name.ToLower().Contains("jumppoint")) continue;
            if (renderer.name.ToLower().Contains("alerticon")) continue;
            if (renderer.name.ToLower().Contains("shadowcaster")) continue;
            if (renderer.name.ToLower().Contains("blood_spray")) continue;
            if (renderer.name.ToLower().Contains("cube")) continue;
            if (renderer.name.ToLower().Contains("lightsprite")) continue;
            if (renderer.name.ToLower().Contains("damagedecal")) continue;
            if (renderer.name.ToLower().Contains("bullethole")) continue;
            if (renderer.name.ToLower().Contains("callout")) continue;
            if (renderer.name.ToLower().Contains("particle")) continue;
            if (renderer.name.ToLower().Contains("sharp_explosion")) continue;
            if (renderer.name.ToLower().Contains("target")) continue;
            if (renderer.name.ToLower().Contains("shadow")) continue;
            Debug.Log(renderer.name);

            Bounds bounds = renderer.bounds;
            if (renderer is SpriteRenderer) {
                SpriteRenderer spriteRenderer = (SpriteRenderer)renderer;
                if (spriteRenderer.sprite != null) {
                    bounds = spriteRenderer.sprite.bounds;

                    // weird hack. maybe because billboarding? i.e. should be camera angle dependent?
                    Vector3 extents = bounds.extents;
                    if (adjustYScale)
                        extents.y *= 1.3f;
                    bounds.extents = extents;
                }
            }

            // add offset
            bounds.center = renderer.transform.position + (bounds.extents / 2f);

            Vector3[] screenSpaceCorners = new Vector3[8];
            screenSpaceCorners[0] = UICamera.WorldToScreenPoint(new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z));
            screenSpaceCorners[1] = UICamera.WorldToScreenPoint(new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z));
            screenSpaceCorners[2] = UICamera.WorldToScreenPoint(new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z));
            screenSpaceCorners[3] = UICamera.WorldToScreenPoint(new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z));

            screenSpaceCorners[4] = UICamera.WorldToScreenPoint(new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z));
            screenSpaceCorners[5] = UICamera.WorldToScreenPoint(new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z));
            screenSpaceCorners[6] = UICamera.WorldToScreenPoint(new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z));
            screenSpaceCorners[7] = UICamera.WorldToScreenPoint(new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z));

            float min_x = screenSpaceCorners.Aggregate((curMin, x) => (curMin == null || x.x < curMin.x ? x : curMin)).x;
            float max_x = screenSpaceCorners.Aggregate((curMin, x) => (curMin == null || x.x > curMin.x ? x : curMin)).x;
            float min_y = screenSpaceCorners.Aggregate((curMin, x) => (curMin == null || x.y < curMin.y ? x : curMin)).y;
            float max_y = screenSpaceCorners.Aggregate((curMin, x) => (curMin == null || x.y > curMin.y ? x : curMin)).y;

            total_max_x = Mathf.Max(total_max_x, max_x);
            total_min_x = Mathf.Min(total_min_x, min_x);
            total_max_y = Mathf.Max(total_max_y, max_y);
            total_min_y = Mathf.Min(total_min_y, min_y);
        }
        Rect totalRect = new Rect(total_min_x, total_min_y, total_max_x - total_min_x, total_max_y - total_min_y);
        return totalRect;
    }
    public static string AssetRelativePath(UnityEngine.Object asset) {
        // String rootPath = AssetDatabase.GetAssetPath(asset);
        return ResourceReference.I.GetPath(asset);
        // String relativePath = rootPath.Replace("Assets/Resources/", "");
        // int fileExtPos = relativePath.LastIndexOf(".");
        // if (fileExtPos >= 0)
        //     relativePath = relativePath.Substring(0, fileExtPos);
        // return relativePath;
    }
    public static float Sigmoid(float value) {
        return 1.0f / (1.0f + (float)Math.Exp(-value));
    }

    public static Texture2D RenderToTexture2D(RenderTexture rTex) {
        Texture2D tex = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }

    // public static double Factorial(int number) {
    //     // please do not calculate facorials of large numbers
    //     if (number == 1)
    //         return 1;
    //     else
    //         return number * Factorial(number - 1);
    // }
    // public static float PossionCDF(int k, float lambda) {
    //     double result = 0;
    //     for (int j = 0; j < k; j++) {
    //         result += Mathf.Pow(lambda, j) / Factorial(j);
    //     }
    //     result *= Mathf.Exp(-1f * lambda);
    //     return (float)result;
    // }
}

