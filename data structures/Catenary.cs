using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class Catenary {
    public Vector3 start;
    public Vector3 end;
    public float slack;
    public int steps;
    static readonly Vector3[] emptyCurve = new Vector3[] { new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 0.0f) };
    Vector3[] points;

    public Vector3 midPoint {
        get {
            Vector3 mid = Vector3.zero;
            if (steps == 2) {
                return (points[0] + points[1]) * 0.5f;
            } else if (steps > 2) {
                int m = steps / 2;
                if ((steps % 2) == 0) {
                    // if (points.InRange(m + 1))
                    mid = (points[m] + points[m + 1]) * 0.5f;
                } else {
                    // if (points.InRange(m))
                    mid = points[m];
                }
            }
            return mid;
        }
    }

    public Catenary() {
        points = emptyCurve;
        start = Vector3.up;
        end = Vector3.up + Vector3.forward;
        slack = 0.5f;
        steps = 20;
        // regen = true;
        // m_color = Color.white;
        // handles = true;
    }

    public Catenary(Catenary v) {
        points = v.Points();
        start = v.start;
        end = v.end;
        slack = v.slack;
        steps = v.steps;
        // regen = v.regenPoints;
        // m_color = v.color;
        // handles = v.drawHandles;
    }

    // public Color[] Colors() {
    //     Color[] cols = new Color[m_steps];
    //     for (int c = 0; c < m_steps; c++) {
    //         cols[c] = m_color;
    //     }
    //     return cols;
    // }

    public Vector3[] Points() {
        if (steps < 2)
            return emptyCurve;

        float lineDist = Vector3.Distance(end, start);
        float lineDistH = Vector3.Distance(new Vector3(end.x, start.y, end.z), start);
        float l = lineDist + Mathf.Max(0.0001f, slack);
        float r = 0.0f;
        float s = start.y;
        float u = lineDistH;
        float v = end.y;

        if ((u - r) == 0.0f)
            return emptyCurve;

        float ztarget = Mathf.Sqrt(Mathf.Pow(l, 2.0f) - Mathf.Pow(v - s, 2.0f)) / (u - r);

        int loops = 30;
        int iterationCount = 0;
        int maxIterations = loops * 10; // For safety.
        bool found = false;

        float z = 0.0f;
        float ztest = 0.0f;
        float zstep = 100.0f;
        float ztesttarget = 0.0f;
        for (int i = 0; i < loops; i++) {
            for (int j = 0; j < 10; j++) {
                iterationCount++;
                ztest = z + zstep;
                ztesttarget = (float)Math.Sinh(ztest) / ztest;

                if (float.IsInfinity(ztesttarget))
                    continue;

                if (ztesttarget == ztarget) {
                    found = true;
                    z = ztest;
                    break;
                } else if (ztesttarget > ztarget) {
                    break;
                } else {
                    z = ztest;
                }

                if (iterationCount > maxIterations) {
                    found = true;
                    break;
                }
            }

            if (found)
                break;

            zstep *= 0.1f;
        }

        float a = (u - r) / 2.0f / z;
        float p = (r + u - a * Mathf.Log((l + v - s) / (l - v + s))) / 2.0f;
        float q = (v + s - l * (float)Math.Cosh(z) / (float)Math.Sinh(z)) / 2.0f;

        points = new Vector3[steps];
        float stepsf = steps - 1;
        float stepf;
        for (int i = 0; i < steps; i++) {
            stepf = i / stepsf;
            Vector3 pos = Vector3.zero;
            pos.x = Mathf.Lerp(start.x, end.x, stepf);
            pos.z = Mathf.Lerp(start.z, end.z, stepf);
            pos.y = a * (float)Math.Cosh(((stepf * lineDistH) - p) / a) + q;
            points[i] = pos;
        }

        // regen = false;
        return points;
    }
}