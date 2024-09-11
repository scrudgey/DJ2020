using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public class CullingComponent : MonoBehaviour
{
    public bool debug;
    public bool dontCull;
    public SceneData sceneData;
    public enum CullingState { normal, interloper, above }
    public enum RendererState { opaque, transparent, invisible }
    public string idn;
    public int floor;
    public Bounds bounds;
    public string rooftopZoneIdn;
    public CullingState state;
    Dictionary<Renderer, SubRenderHandler> handlers;
    public List<SubRenderHandler> handlerList;
    public Vector3 adjustedPosition;
    public bool isDynamic;
    public bool hasCutaway;
    public TagSystemData data;
    bool initialized;
    WaitForSeconds waiter = new WaitForSeconds(0.1f);

    RooftopZone[] rooftopZones;

    public void Initialize(SceneData sceneData, bool isDynamic = false)
    {
        if (initialized) return;
        this.sceneData = sceneData;
        this.isDynamic = isDynamic;

        TagSystem system = GetComponent<TagSystem>();
        if (system != null)
        {
            data = system.data;
        }
        else
        {
            data = new TagSystemData();
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds tmpBounds = renderers[0].bounds;

            this.handlers = new Dictionary<Renderer, SubRenderHandler>();
            this.handlerList = new List<SubRenderHandler>();
            foreach (Renderer renderer in renderers)
            {
                // if (renderer is ParticleSystemRenderer) continue;    
                if (renderer is TrailRenderer) continue;
                if (renderer is LineRenderer) continue;
                if (renderer.name == "cutaway") continue;
                AddSubRenderHandler(renderer);
                tmpBounds.Encapsulate(renderer.bounds);
            }
            this.bounds = tmpBounds;
            Transform findAnchor = transform.Find("anchor");
            if (findAnchor != null)
            {
                this.adjustedPosition = findAnchor.position;
            }
            else
            {
                this.adjustedPosition = tmpBounds.center;
                // this.adjustedPosition.y -= tmpBounds.extents.y;
            }
        }
        else
        {
            this.handlers = new Dictionary<Renderer, SubRenderHandler>();
            this.handlerList = new List<SubRenderHandler>();
        }
        if (isDynamic)
        {
            rooftopZoneIdn = "-1";
            rooftopZones = GameObject.FindObjectsOfType<RooftopZone>();
            StartCoroutine(Toolbox.RunJobRepeatedly(() => UpdateRoofZone()));
        }
        initialized = true;
    }

    IEnumerator UpdateRoofZone()
    {
        Vector3 position = transform.position;
        floor = sceneData.GetCullingFloorForPosition(position);
        foreach (RooftopZone zone in rooftopZones)
        {
            if (zone.ContainsGeometry(position))
            {
                rooftopZoneIdn = zone.idn;
                break;
            }
        }
        yield return waiter;
    }
    public void AddSubRenderHandler(Renderer renderer)
    {
        SubRenderHandler handler = new SubRenderHandler(renderer, this);
        handlers[renderer] = handler;
        hasCutaway |= handler.isCutaway;
        handlerList.Add(handler);
    }
    // public void RemoveSubRenderHandler(Renderer renderer) {
    //     handlers.Remove(renderer);
    // }
    public void ApplyInterloper(Vector3 playerPosition)
    {
        ChangeState(CullingComponent.CullingState.interloper);
    }
    public void StopCulling()
    {
        ChangeState(CullingComponent.CullingState.normal);
    }
    public void ChangeState(CullingState toState)
    {
        switch (toState)
        {
            case CullingState.normal:
                Visible();
                break;
            case CullingState.interloper:
                Cutaway();
                break;
            case CullingState.above:
                Invisible();
                break;
        }
        state = toState;
    }
    public bool IsAbove(int playerFloor)
    {
        return floor > playerFloor;
    }
    void Invisible()
    {
        if (dontCull) return;
        foreach (SubRenderHandler handler in handlers.Values)
        {
            handler.TotallyInvisible(debug);
        }
    }
    void Cutaway()
    {
        if (dontCull) return;
        foreach (SubRenderHandler handler in handlers.Values)
        {
            if (handler.data.partialTransparentIsInvisible)
            {
                handler.TotallyInvisible(debug);
            }
            else
            {
                handler.CutawayInvisible(hasCutaway, debug);
            }
        }
    }
    void Visible()
    {
        foreach (SubRenderHandler handler in handlers.Values)
        {
            handler.Visible(debug);
        }
    }

}