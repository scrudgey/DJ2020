using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Easings;
using UnityEngine;
public class AmbienceManager : Singleton<AmbienceManager> {
    CharacterCamera target;
    HashSet<AmbientZone> zones;
    Vector3 targetPosition;
    public AudioSource[] audioSources;
    // Dictionary<AudioSource, AmbientZone> audioSourceToZone;
    Stack<AudioSource> freeAudioSources;
    Dictionary<AmbientZone, AudioSource> playingAmbientZones;
    Coroutine zoneRoutine;
    public void Bind(CharacterCamera newTargetObject) {
        Stop();
        if (newTargetObject == null)
            return;
        // Debug.Log($"{this} binding to target {newTargetObject}");
        if (target != null && target.OnValueChanged != null)
            target.OnValueChanged -= HandleValueChanged;

        target = newTargetObject;
        if (target != null) {
            target.OnValueChanged += HandleValueChanged;
            HandleValueChanged(target);
        }
    }

    public void InitializeScene() {
        zones = new HashSet<AmbientZone>();
        // audioSourceToZone = new Dictionary<AudioSource, AmbientZone>();
        freeAudioSources = new Stack<AudioSource>();
        playingAmbientZones = new Dictionary<AmbientZone, AudioSource>();
        foreach (AudioSource audioSource in audioSources) {
            // audioSourceToZone[audioSource] = null;
            freeAudioSources.Push(audioSource);
        }
        foreach (AmbientZone zone in GameObject.FindObjectsOfType<AmbientZone>()) {
            zones.Add(zone);
            playingAmbientZones[zone] = null;
        }
        zoneRoutine = StartCoroutine(Toolbox.RunJobRepeatedly(() => ProcessZones()));
    }
    override public void OnDestroy() {
        base.OnDestroy();
        if (target != null && target.OnValueChanged != null)
            target.OnValueChanged -= HandleValueChanged;
    }

    public void HandleValueChanged(CharacterCamera cam) {
        targetPosition = cam.cullingTargetPosition;
    }
    public void Stop() {
        if (zoneRoutine != null) StopCoroutine(zoneRoutine);
    }
    IEnumerator ProcessZones() {
        // find all zones that contain the target point
        HashSet<AmbientZone> containedZones = new HashSet<AmbientZone>();
        foreach (AmbientZone zone in zones) {
            if (zone.activeZone.bounds.Contains(targetPosition)) {
                containedZones.Add(zone);
            } else {
                Stop(zone);
            }
        }
        yield return null;

        // calculate distances to source zones
        Dictionary<AmbientZone, float> distances = containedZones.ToDictionary(zone => zone, zone => Vector3.Distance(targetPosition, zone.sourceZone.ClosestPoint(targetPosition)));

        yield return null;

        // order by distances
        List<AmbientZone> sortedZones = distances.OrderByDescending(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
        foreach (AmbientZone zone in sortedZones) {
            if (playingAmbientZones[zone] != null) {
                AudioSource audioSource = playingAmbientZones[zone];
                SetVolume(zone, audioSource);
            } else if (freeAudioSources.Count > 0) {
                Play(zone);
            }
        }
    }

    void Play(AmbientZone zone) {
        // attempt to play new
        AudioSource audioSource = freeAudioSources.Pop();
        zone.Play(audioSource);
        SetVolume(zone, audioSource);
        playingAmbientZones[zone] = audioSource;
    }

    void SetVolume(AmbientZone zone, AudioSource audioSource) {
        if (zone.sourceZone.bounds.Contains(targetPosition)) {
            audioSource.volume = zone.volume;
        } else {
            Vector3 sourcePoint = zone.sourceZone.ClosestPointOnBounds(targetPosition);
            Vector3 edgePoint = Toolbox.BoundsOnBoxFromInside(zone.activeZone.bounds, targetPosition);

            float distanceFromSource = Vector3.Distance(sourcePoint, targetPosition);
            float totalDistance = Vector3.Distance(sourcePoint, edgePoint);

            // Debug.DrawLine(targetPosition, sourcePoint, Color.magenta);
            // Debug.DrawLine(targetPosition, edgePoint, Color.cyan);

            audioSource.volume = zone.volume * (float)PennerDoubleAnimation.Linear(distanceFromSource, 1f, -1f, totalDistance);
        }
    }

    void Stop(AmbientZone zone) {
        if (playingAmbientZones[zone] != null) {
            AudioSource audioSource = playingAmbientZones[zone];
            zone.Stop();
            freeAudioSources.Push(audioSource);
            playingAmbientZones[zone] = null;
        }
    }

}