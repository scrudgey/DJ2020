using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Easings;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
// random from list
// play a sound with random pitch from randomized list
// maybe use loudspeaker object

public class Toolbox {
    static RaycastHit[] raycastHits = new RaycastHit[1];
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
        if (list.Count == 0) return default(T);
        return list[UnityEngine.Random.Range(0, list.Count)];
    }
    static public T RandomFromListExcept<T>(IReadOnlyList<T> list, IReadOnlyList<T> exceptions) {
        if (list.Count == 0) return default(T);
        List<T> copyList = new List<T>(list);
        foreach (T except in exceptions) {
            if (copyList.Contains(except)) {
                copyList.Remove(except);
            }
        }
        if (copyList.Count == 0) {
            Debug.LogError($"random from list exceptions eliminated all possibilities!");
            return default(T);
        }
        return RandomFromList(copyList);
    }
    public static T RandomFromListByWeight<T>(IEnumerable<T> sequence, Func<T, float> weightSelector) {
        float totalWeight = sequence.Sum(weightSelector);
        float itemWeightIndex = (float)new System.Random().NextDouble() * totalWeight;
        float currentWeightIndex = 0;
        foreach (var item in from weightedItem in sequence select new { Value = weightedItem, Weight = weightSelector(weightedItem) }) {
            currentWeightIndex += item.Weight;
            if (currentWeightIndex >= itemWeightIndex)
                return item.Value;
        }
        return default(T);
    }
    static public void RandomizeOneShot(AudioSource audioSource, AudioClip audioClip, float randomPitchWidth = 0.1f, float volume = 1f) {
        if (audioSource == null || !audioSource.isActiveAndEnabled)
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
        if (audioClips == null || audioClips.Length == 0) {
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
    public static void DrawPlane(Vector3 position, Plane plane, float time = 0.75f) {
        // Debug.Log($"draw plane: {position} {plane}");
        // Debug.DrawLine(position, position + plane.normal * 10f, Color.cyan, 10f);
        Vector3 normal = plane.normal;

        Vector3 v3;

        if (normal.normalized != Vector3.forward)
            v3 = Vector3.Cross(normal, Vector3.forward).normalized * normal.magnitude;
        else
            v3 = Vector3.Cross(normal, Vector3.up).normalized * normal.magnitude; ;

        var corner0 = position + v3;
        var corner2 = position - v3;
        var q = Quaternion.AngleAxis(90.0f, normal);
        v3 = q * v3;
        var corner1 = position + v3;
        var corner3 = position - v3;

        Debug.DrawLine(corner0, corner2, Color.cyan, time);
        Debug.DrawLine(corner1, corner3, Color.cyan, time);
        Debug.DrawLine(corner0, corner1, Color.cyan, time);
        Debug.DrawLine(corner1, corner2, Color.cyan, time);
        Debug.DrawLine(corner2, corner3, Color.cyan, time);
        Debug.DrawLine(corner3, corner0, Color.cyan, time);
        Debug.DrawRay(position, normal, Color.red, time);
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
    public static NoiseComponent Noise(Vector3 position, NoiseData data, GameObject source, bool spherical = true) {
        GameObject noiseObject = PoolManager.I.GetPool("prefabs/noise").GetObject(position);
        NoiseComponent component = noiseObject.GetComponent<NoiseComponent>();
        component.data = data with {
            source = source
        };
        if (spherical) {
            component.meshCollider.enabled = false;
            component.sphereCollider.enabled = true;
            component.sphereCollider.radius = data.volume;
            component.transform.localScale = Vector3.one;
        } else {
            component.meshCollider.enabled = true;
            component.sphereCollider.enabled = false;
            component.transform.localScale = new Vector3(data.volume, 2f, data.volume);
        }
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
        source.minDistance = 0.75f;
        source.maxDistance = 23f;
        source.spatialBlend = 1f;
        source.spread = 0.2f;
        source.volume = 1f;
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

    static public Vector3 GetBoundsCenter(Transform root) {
        Vector3 center = root.position;
        Renderer renderer = root.GetComponentInChildren<Renderer>();
        if (renderer != null) {
            center = renderer.bounds.center;
        }
        return center;
    }
    static public Rect GetTotalRenderBoundingBox(Transform root, Camera UICamera, bool adjustYScale = true, bool useColliders = false) {
        float total_min_x = float.MaxValue;
        float total_max_x = float.MinValue;
        float total_min_y = float.MaxValue;
        float total_max_y = float.MinValue;

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();

        if (useColliders) {
            foreach (Collider collider in root.GetComponents<Collider>()) {
                // collider.bounds.size
                Bounds bounds = collider.bounds;
                Vector3 rescale = collider.transform.lossyScale;
                bounds.size = new Vector3(bounds.size.x / rescale.x, bounds.size.y / rescale.y, bounds.size.z / rescale.z);

                // add offset
                bounds.center = collider.transform.position + (bounds.extents / 2f);

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
        } else if (renderers.Length == 0) {
            RectTransform childRectTransform = root.GetComponent<RectTransform>();
            if (childRectTransform != null) {
                Vector3[] v = new Vector3[4];
                childRectTransform.GetWorldCorners(v);

                Vector3[] screenSpaceCorners = v.Select(point => UICamera.WorldToScreenPoint(point)).ToArray();

                float min_x = screenSpaceCorners.Aggregate((curMin, x) => (curMin == null || x.x < curMin.x ? x : curMin)).x;
                float max_x = screenSpaceCorners.Aggregate((curMin, x) => (curMin == null || x.x > curMin.x ? x : curMin)).x;
                float min_y = screenSpaceCorners.Aggregate((curMin, x) => (curMin == null || x.y < curMin.y ? x : curMin)).y;
                float max_y = screenSpaceCorners.Aggregate((curMin, x) => (curMin == null || x.y > curMin.y ? x : curMin)).y;

                total_max_x = Mathf.Max(total_max_x, max_x);
                total_min_x = Mathf.Min(total_min_x, min_x);
                total_max_y = Mathf.Max(total_max_y, max_y);
                total_min_y = Mathf.Min(total_min_y, min_y);

                return new Rect(total_min_x, total_min_y, (total_max_x - total_min_x) * root.lossyScale.x * 100, (total_max_y - total_min_y) * root.lossyScale.y * 100);
            }
        } else {
            foreach (Renderer renderer in renderers) {
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
                // Debug.Log(renderer.name);

                Bounds bounds = renderer.bounds;
                if (renderer is SpriteRenderer) {
                    SpriteRenderer spriteRenderer = (SpriteRenderer)renderer;
                    if (spriteRenderer.sprite != null) {
                        bounds = spriteRenderer.sprite.bounds;
                        // bounds = spriteRenderer.bounds;

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
        }


        Rect totalRect = new Rect(total_min_x, total_min_y, (total_max_x - total_min_x) * root.lossyScale.x, (total_max_y - total_min_y) * root.lossyScale.y);
        return totalRect;
    }
    public static string AssetRelativePath(UnityEngine.Object asset) {
        return ResourceReference.I.GetPath(asset);
    }
    public static float Sigmoid(float value) {
        return 1.0f / (1.0f + (float)Math.Exp(-value));
    }

    public static Texture2D RenderToTexture2D(RenderTexture rTex) {
        Texture2D tex = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        for (int i = 0, j = tex.width; i < j; i += 1) {
            for (int k = 0, l = tex.height; k < l; k += 1) {
                if (tex.GetPixel(i, k) == Color.black) {
                    tex.SetPixel(i, k, Color.clear);
                }
            }
        }
        tex.Apply();
        return tex;
    }

    public static int ListHashCode<T>(List<T> inlist) {
        unchecked {
            int hash = 19;
            foreach (var foo in inlist) {
                hash = hash * 31 + foo.GetHashCode();
            }
            return hash;
        }
    }

    public static Gradient Gradient2Color(Color color1, Color color2) {
        Gradient gradient = new Gradient();

        // TODO: get length of ray from line renderer points

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        GradientColorKey[] colorKey = new GradientColorKey[2];
        colorKey[0].color = color1;
        colorKey[0].time = 0.0f;

        colorKey[1].color = color2;
        colorKey[1].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = color1.a;
        alphaKey[0].time = 0.0f;

        alphaKey[1].alpha = color2.a;
        alphaKey[1].time = 1f;

        gradient.SetKeys(colorKey, alphaKey);

        return gradient;
    }

    public static IEnumerator WaitForSceneLoadingToFinish(Action callback) {
        WaitForEndOfFrame waiter = new WaitForEndOfFrame();
        while (GameManager.I.isLoadingLevel) {
            yield return waiter;
        }
        callback();
    }

    public static IEnumerator RunJobRepeatedly(Func<IEnumerator> coroutine) {
        while (true) {
            yield return coroutine();
        }
    }

    public static IEnumerator ChainCoroutines(params IEnumerator[] coroutines) {
        foreach (IEnumerator coroutine in coroutines) {
            yield return coroutine;
        }
    }
    public static IEnumerator Parallelize(params IEnumerator[] ienumerators) {
        List<CheckableCutscene> cutscenes = ienumerators.Select(enumerator => new CheckableCutscene(enumerator)).ToList();
        cutscenes.ForEach(cutscene => CutsceneManager.I.StartCoroutine(cutscene.Do()));
        while (cutscenes.Any(cutscene => cutscene.isRunning)) {
            yield return null;
        }
    }

    public static IEnumerator OpenStore(RectTransform bottomRect, AudioSource audioSource, AudioClip[] discloseBottomSound) {
        bottomRect.sizeDelta = new Vector2(1f, 0f);
        yield return new WaitForSecondsRealtime(0.5f);
        RandomizeOneShot(audioSource, discloseBottomSound, randomPitchWidth: 0.02f);
        float timer = 0f;
        float duration = 0.1f;
        while (timer < duration) {
            timer += Time.unscaledDeltaTime;
            float height = (float)PennerDoubleAnimation.Linear(timer, 0f, 610f, duration);
            bottomRect.sizeDelta = new Vector2(1f, height);
            yield return null;
        }
        bottomRect.sizeDelta = new Vector2(1f, 610f);
    }

    public static IEnumerator CloseMenu(RectTransform rect) {
        float initialHeight = rect.rect.height;
        float width = rect.rect.width;
        return ChainCoroutines(
            Ease(null, 0.1f, initialHeight, 10f, PennerDoubleAnimation.Linear, (amount) => {
                rect.sizeDelta = new Vector2(width, amount);
            }, unscaledTime: true),
            CoroutineFunc(() => GameManager.I.CloseMenu())
        );
    }

    public static int ClampWrap(int value, int min, int max) {
        if (value > max) {
            return 0;
        } else if (value < min) {
            return max;
        } else return value;
    }

    public static IEnumerator BlitText(TextMeshProUGUI textMesh, string content, float interval = 0.05f) {
        float timer = 0f;
        int index = 0;
        bool foundTag = false;
        while (index < content.Length) {
            timer += Time.unscaledDeltaTime;
            if (timer > interval) {
                timer -= interval;
                index += 1;
            }
            if (index < content.Length && content[index] == '<') {
                foundTag = !foundTag;
                while (index < content.Length && content[index] != '>') {
                    index++;
                }
                index++;
            }

            string substring = content.Substring(0, index);
            if (foundTag) {
                substring += "</color>";
            }
            textMesh.text = substring;
            yield return null;
        }
    }
    public static IEnumerator TypeText(TextMeshProUGUI text, string prefix, string totalText, bool typedInput = false) {

        // TODO: audio
        // TODO: looping cursor
        // TODO: with delay

        // float characterDuration = 0.04f;
        // float duration = characterDuration * totalText.Length;
        int index = 0;
        // "ab" + "cde"
        //  01     234 = 5-1
        int finalIndex = totalText.Length;
        float duration = 0.04f;
        float cursorDuration = 0.2f;

        float cursortimer = 0;
        float timer = 0f;
        bool cursorvisible = true;
        text.text = "";
        while (index < finalIndex) {
            timer += Time.deltaTime;
            cursortimer += Time.deltaTime;
            if (timer > duration) {
                timer -= duration;
                if (typedInput) {
                    duration = UnityEngine.Random.Range(0.05f, 0.1f);
                    index++;
                } else {
                    index += finalIndex / 2;
                }
            }
            if (cursortimer > cursorDuration) {
                cursorvisible = !cursorvisible;
                cursortimer -= cursorDuration;
            }
            // int numberCharacters = (int)(totalText.Length * (timer / duration));
            try {
                string currentText = prefix + totalText.Substring(0, Math.Min(finalIndex, index));
                if (cursorvisible) {
                    currentText += "█";
                }

                text.text = currentText;
            }
            catch (Exception e) {
                Debug.Log($"{totalText} {index} {finalIndex}");
                //ping 127.0.5.10 17 18
                //012345678901234
            }


            yield return null;
        }
        text.text = prefix + totalText;
    }

    public static IEnumerator Ease(Coroutine routine,
                                    float duration,
                                    float start,
                                    float end,
                                    Func<double, double, double, double, double> easing,
                                    Action<float> update,
                                    bool unscaledTime = false,
                                    bool looping = false) {
        float timer = 0;
        update(start);
        while (timer < duration || looping) {
            timer += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (looping && timer > duration) {
                timer -= duration;
            }
            float value = (float)easing(timer, start, end - start, duration);
            update(value);
            yield return null;
        }
        update(end);
        routine = null;
    }
    public static IEnumerator CoroutineFunc(Action action) {
        action();
        yield return null;
    }

    [Obsolete("use async clearlineofsight.")]
    public static bool ClearLineOfSight(Vector3 position, Collider other, float MAXIMUM_SIGHT_RANGE = 25f) {
        Vector3[] directions = new Vector3[0];
        float distance = 0;
        if (other is BoxCollider || other is SphereCollider || other is CapsuleCollider) {
            directions = new Vector3[]{
                other.ClosestPoint(position) - position
            };
        } else {
            directions = new Vector3[]{
                other.bounds.center - position
            };
        }
        bool clearLineOfSight = false;
        foreach (Vector3 direction in directions) {
            // distance = Math.Min(direction.magnitude, MAXIMUM_SIGHT_RANGE);
            distance = direction.magnitude;
            if (distance > MAXIMUM_SIGHT_RANGE)
                return false;
            Ray ray = new Ray(position, direction);
            int numberHits = Physics.RaycastNonAlloc(ray, raycastHits, distance * 0.99f, LayerUtil.GetLayerMask(Layer.def, Layer.obj, Layer.interactive), QueryTriggerInteraction.Ignore);
            clearLineOfSight |= numberHits == 0;
        }
        return clearLineOfSight;
    }


    public static void AsyncClearLineOfSight(Vector3 position, Collider other, Action<RaycastHit> callback) {
        Vector3 direction = other.bounds.center - position;
        float distance = direction.magnitude;
        AsyncRaycastService.I.RequestRaycast(position, direction, distance, LayerUtil.GetLayerMask(Layer.def, Layer.obj, Layer.interactive), callback);
    }

    public static Bounds TransformBounds(Transform _transform, Bounds _localBounds) {
        var center = _transform.TransformPoint(_localBounds.center);

        // transform the local extents' axes
        var extents = _localBounds.extents;
        var axisX = _transform.TransformVector(extents.x, 0, 0);
        var axisY = _transform.TransformVector(0, extents.y, 0);
        var axisZ = _transform.TransformVector(0, 0, extents.z);

        // sum their absolute value to get the world extents
        extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);

        return new Bounds { center = center, extents = extents };
    }

    public static IEnumerator ShakeTree(Transform target, Quaternion initialRotation) {
        Quaternion initial = target.rotation;

        float timer = 0f;
        float length = UnityEngine.Random.Range(0.5f, 1.2f);
        float intensity = UnityEngine.Random.Range(5f, 15f);

        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized;
        Vector3 axis = new Vector3(randomCircle.x, 0f, randomCircle.y);

        while (timer < length) {
            timer += Time.deltaTime;

            float angle = (float)PennerDoubleAnimation.ElasticEaseOut(timer, intensity, -intensity, length);
            target.rotation = Quaternion.AngleAxis(angle, axis) * initialRotation;
            yield return null;
        }

        target.rotation = initial;
    }

    public static float RandomGaussian(float minValue = 0.0f, float maxValue = 1.0f) {
        float u, v, S;
        do {
            u = 2.0f * UnityEngine.Random.value - 1.0f;
            v = 2.0f * UnityEngine.Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0f);

        // Standard Normal Distribution
        float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);

        // Normal Distribution centered between the min and max value
        // and clamped following the "three-sigma rule"
        float mean = (minValue + maxValue) / 2.0f;
        float sigma = (maxValue - mean) / 3.0f;
        return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
    }

    public static IEnumerator BlinkEmphasis(MonoBehaviour component, int pulses = 7, bool unlimited = false, float duration = 0.1f) {
        float timer = 0f;
        int cycles = 0;
        while (cycles < pulses && !unlimited) {
            timer += Time.unscaledDeltaTime;
            if (timer > duration) {
                timer -= duration;
                cycles += 1;
                component.enabled = !component.enabled;
            }
            yield return null;
        }
        component.enabled = true;
    }
    public static IEnumerator BlinkColor(TextMeshProUGUI textmesh, Color color, int pulses = 7, bool unlimited = false) {
        float timer = 0f;
        int cycles = 0;
        bool enable = false;
        while (cycles < pulses && !unlimited) {
            timer += Time.unscaledDeltaTime;
            if (timer > 0.2f) {
                enable = !enable;
                timer -= 0.2f;
                cycles += 1;
                textmesh.color = enable ? color : Color.white;
            }
            yield return null;
        }
        textmesh.color = color;
    }
    public static IEnumerator BlinkColor(Image image, Color normalColor, Color offColor, int pulses = 7, bool unlimited = false) {
        float timer = 0f;
        int cycles = 0;
        bool enable = false;
        float duration = 0.025f;
        while (cycles < pulses && !unlimited) {
            timer += Time.unscaledDeltaTime;
            if (timer > duration) {
                enable = !enable;
                timer -= duration;
                cycles += 1;
                image.color = enable ? normalColor : offColor;
            }
            yield return null;
        }
        image.color = normalColor;
    }
    public static IEnumerator BlinkVis(Image image, Action inbetweener, float blinkInterval = 0.05f) {
        WaitForSecondsRealtime waiter = new WaitForSecondsRealtime(blinkInterval / 2f);
        image.enabled = false;
        yield return waiter;
        inbetweener();
        yield return waiter;
        image.enabled = true;
    }


    public static Vector3 Round(Vector3 vector3, int decimalPlaces = 2) {
        float multiplier = 1;
        for (int i = 0; i < decimalPlaces; i++) {
            multiplier *= 10f;
        }
        return new Vector3(
            Mathf.Round(vector3.x * multiplier) / multiplier,
            Mathf.Round(vector3.y * multiplier) / multiplier,
            Mathf.Round(vector3.z * multiplier) / multiplier);
    }


    public static int GetPoisson(double lambda) {
        return (lambda < 30.0) ? PoissonSmall(lambda) : PoissonLarge(lambda);
    }

    private static int PoissonSmall(double lambda) {
        // Algorithm due to Donald Knuth, 1969.
        double p = 1.0, L = Math.Exp(-lambda);
        int k = 0;
        do {
            k++;
            p *= UnityEngine.Random.Range(0, 1f);
        }
        while (p > L);
        return k - 1;
    }

    private static int PoissonLarge(double lambda) {
        // "Rejection method PA" from "The Computer Generation of 
        // Poisson Random Variables" by A. C. Atkinson,
        // Journal of the Royal Statistical Society Series C 
        // (Applied Statistics) Vol. 28, No. 1. (1979)
        // The article is on pages 29-35. 
        // The algorithm given here is on page 32.

        double c = 0.767 - 3.36 / lambda;
        double beta = Math.PI / Math.Sqrt(3.0 * lambda);
        double alpha = beta * lambda;
        double k = Math.Log(c) - lambda - Math.Log(beta);

        for (; ; )
        {
            double u = UnityEngine.Random.Range(0, 1f);
            double x = (alpha - Math.Log((1.0 - u) / u)) / beta;
            int n = (int)Math.Floor(x + 0.5);
            if (n < 0)
                continue;
            double v = UnityEngine.Random.Range(0, 1f);
            double y = alpha - beta * x;
            double temp = 1.0 + Math.Exp(y);
            double lhs = y + Math.Log(v / (temp * temp));
            double rhs = k + n * Math.Log(lambda) - LogFactorial(n);
            if (lhs <= rhs)
                return n;
        }
    }

    static double LogFactorial(int n) {
        if (n < 0) {
            throw new ArgumentOutOfRangeException();
        } else if (n > 254) {
            double x = n + 1;
            return (x - 0.5) * Math.Log(x) - x + 0.5 * Math.Log(2 * Math.PI) + 1.0 / (12.0 * x);
        } else {
            double[] lf =
            {
            0.000000000000000,
            0.000000000000000,
            0.693147180559945,
            1.791759469228055,
            3.178053830347946,
            4.787491742782046,
            6.579251212010101,
            8.525161361065415,
            10.604602902745251,
            12.801827480081469,
            15.104412573075516,
            17.502307845873887,
            19.987214495661885,
            22.552163853123421,
            25.191221182738683,
            27.899271383840894,
            30.671860106080675,
            33.505073450136891,
            36.395445208033053,
            39.339884187199495,
            42.335616460753485,
            45.380138898476908,
            48.471181351835227,
            51.606675567764377,
            54.784729398112319,
            58.003605222980518,
            61.261701761002001,
            64.557538627006323,
            67.889743137181526,
            71.257038967168000,
            74.658236348830158,
            78.092223553315307,
            81.557959456115029,
            85.054467017581516,
            88.580827542197682,
            92.136175603687079,
            95.719694542143202,
            99.330612454787428,
            102.968198614513810,
            106.631760260643450,
            110.320639714757390,
            114.034211781461690,
            117.771881399745060,
            121.533081515438640,
            125.317271149356880,
            129.123933639127240,
            132.952575035616290,
            136.802722637326350,
            140.673923648234250,
            144.565743946344900,
            148.477766951773020,
            152.409592584497350,
            156.360836303078800,
            160.331128216630930,
            164.320112263195170,
            168.327445448427650,
            172.352797139162820,
            176.395848406997370,
            180.456291417543780,
            184.533828861449510,
            188.628173423671600,
            192.739047287844900,
            196.866181672889980,
            201.009316399281570,
            205.168199482641200,
            209.342586752536820,
            213.532241494563270,
            217.736934113954250,
            221.956441819130360,
            226.190548323727570,
            230.439043565776930,
            234.701723442818260,
            238.978389561834350,
            243.268849002982730,
            247.572914096186910,
            251.890402209723190,
            256.221135550009480,
            260.564940971863220,
            264.921649798552780,
            269.291097651019810,
            273.673124285693690,
            278.067573440366120,
            282.474292687630400,
            286.893133295426990,
            291.323950094270290,
            295.766601350760600,
            300.220948647014100,
            304.686856765668720,
            309.164193580146900,
            313.652829949878990,
            318.152639620209300,
            322.663499126726210,
            327.185287703775200,
            331.717887196928470,
            336.261181979198450,
            340.815058870798960,
            345.379407062266860,
            349.954118040770250,
            354.539085519440790,
            359.134205369575340,
            363.739375555563470,
            368.354496072404690,
            372.979468885689020,
            377.614197873918670,
            382.258588773060010,
            386.912549123217560,
            391.575988217329610,
            396.248817051791490,
            400.930948278915760,
            405.622296161144900,
            410.322776526937280,
            415.032306728249580,
            419.750805599544780,
            424.478193418257090,
            429.214391866651570,
            433.959323995014870,
            438.712914186121170,
            443.475088120918940,
            448.245772745384610,
            453.024896238496130,
            457.812387981278110,
            462.608178526874890,
            467.412199571608080,
            472.224383926980520,
            477.044665492585580,
            481.872979229887900,
            486.709261136839360,
            491.553448223298010,
            496.405478487217580,
            501.265290891579240,
            506.132825342034830,
            511.008022665236070,
            515.890824587822520,
            520.781173716044240,
            525.679013515995050,
            530.584288294433580,
            535.496943180169520,
            540.416924105997740,
            545.344177791154950,
            550.278651724285620,
            555.220294146894960,
            560.169054037273100,
            565.124881094874350,
            570.087725725134190,
            575.057539024710200,
            580.034272767130800,
            585.017879388839220,
            590.008311975617860,
            595.005524249382010,
            600.009470555327430,
            605.020105849423770,
            610.037385686238740,
            615.061266207084940,
            620.091704128477430,
            625.128656730891070,
            630.172081847810200,
            635.221937855059760,
            640.278183660408100,
            645.340778693435030,
            650.409682895655240,
            655.484856710889060,
            660.566261075873510,
            665.653857411105950,
            670.747607611912710,
            675.847474039736880,
            680.953419513637530,
            686.065407301994010,
            691.183401114410800,
            696.307365093814040,
            701.437263808737160,
            706.573062245787470,
            711.714725802289990,
            716.862220279103440,
            722.015511873601330,
            727.174567172815840,
            732.339353146739310,
            737.509837141777440,
            742.685986874351220,
            747.867770424643370,
            753.055156230484160,
            758.248113081374300,
            763.446610112640200,
            768.650616799717000,
            773.860102952558460,
            779.075038710167410,
            784.295394535245690,
            789.521141208958970,
            794.752249825813460,
            799.988691788643450,
            805.230438803703120,
            810.477462875863580,
            815.729736303910160,
            820.987231675937890,
            826.249921864842800,
            831.517780023906310,
            836.790779582469900,
            842.068894241700490,
            847.352097970438420,
            852.640365001133090,
            857.933669825857460,
            863.231987192405430,
            868.535292100464630,
            873.843559797865740,
            879.156765776907600,
            884.474885770751830,
            889.797895749890240,
            895.125771918679900,
            900.458490711945270,
            905.796028791646340,
            911.138363043611210,
            916.485470574328820,
            921.837328707804890,
            927.193914982476710,
            932.555207148186240,
            937.921183163208070,
            943.291821191335660,
            948.667099599019820,
            954.046996952560450,
            959.431492015349480,
            964.820563745165940,
            970.214191291518320,
            975.612353993036210,
            981.015031374908400,
            986.422203146368590,
            991.833849198223450,
            997.249949600427840,
            1002.670484599700300,
            1008.095434617181700,
            1013.524780246136200,
            1018.958502249690200,
            1024.396581558613400,
            1029.838999269135500,
            1035.285736640801600,
            1040.736775094367400,
            1046.192096209724900,
            1051.651681723869200,
            1057.115513528895000,
            1062.583573670030100,
            1068.055844343701400,
            1073.532307895632800,
            1079.012946818975000,
            1084.497743752465600,
            1089.986681478622400,
            1095.479742921962700,
            1100.976911147256000,
            1106.478169357800900,
            1111.983500893733000,
            1117.492889230361000,
            1123.006317976526100,
            1128.523770872990800,
            1134.045231790853000,
            1139.570684729984800,
            1145.100113817496100,
            1150.633503306223700,
            1156.170837573242400,
        };
            return lf[n];
        }
    }

    public static Vector3 BoundsOnBoxFromInside(Bounds bounds, Vector3 point) {
        Vector3 localPoint = point - bounds.center;
        Dictionary<string, float> faceDistances = new Dictionary<string, float>{
            {"x", bounds.extents.x - Math.Abs(localPoint.x)},
            {"y", bounds.extents.y - Math.Abs(localPoint.y)},
            {"z", bounds.extents.z - Math.Abs(localPoint.z)},
        };
        string nearestFace = faceDistances.Keys.OrderBy(key => faceDistances[key]).ToList()[0];
        return bounds.center + nearestFace switch {
            "x" => localPoint.x > 0 ? new Vector3(bounds.extents.x, localPoint.y, localPoint.z) : new Vector3(-bounds.extents.x, localPoint.y, localPoint.z),
            "y" => localPoint.y > 0 ? new Vector3(localPoint.x, bounds.extents.y, localPoint.z) : new Vector3(localPoint.x, -bounds.extents.y, localPoint.z),
            "z" => localPoint.z > 0 ? new Vector3(localPoint.x, localPoint.y, bounds.extents.z) : new Vector3(localPoint.x, localPoint.y, -bounds.extents.z)
        };
    }

    public static float DecayingSin(float t, float tau, float freq) {
        return Mathf.Sin(t * freq) * Mathf.Exp((-1f * t) / tau);
    }
    public static float DecayingCos(float t, float tau, float freq) {
        return Mathf.Cos(t * freq) * Mathf.Exp((-1f * t) / tau);
    }
}

