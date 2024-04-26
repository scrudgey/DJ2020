using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class CullingGrid {
    public CullingGridPoint[][] points;
    public int xRows;
    public int zRows;

    public Vector3 origin;
    public float gridSpacing;

    public CullingGrid() { }//required for serialization


    public CullingGrid(Bounds boundingBox, float gridSpacing, float floorHeight, int floor) {
        this.origin = boundingBox.center - boundingBox.extents;
        this.gridSpacing = gridSpacing;
        this.xRows = (int)(boundingBox.extents.x / gridSpacing) * 2;
        this.zRows = (int)(boundingBox.extents.z / gridSpacing) * 2;

        this.points = new CullingGridPoint[xRows][];

        for (int x = 0; x < xRows; x++) {
            CullingGridPoint[] row = new CullingGridPoint[zRows];
            for (int z = 0; z < zRows; z++) {
                Vector3 position = new Vector3(origin.x + (x * gridSpacing), floorHeight, origin.z + (z * gridSpacing));
                CullingGridPoint point = new CullingGridPoint(position, floor);
                row[z] = point;
            }
            points[x] = row;
        }

        Debug.Log($"floor:{floor}\tx rows: {xRows}\tz rows: {zRows}\tpoints: {xRows * zRows}");
    }
    public int GetBufferSize(float radius) {
        int radiusPoints = (int)(radius / gridSpacing);
        return (2 * radiusPoints) * (2 * radiusPoints);
    }
    public List<CullingGridPoint> SubgridAroundWorldPoint(Vector3 position, float radius) {
        Vector3 localPosition = position - origin;
        int localPositionX = (int)(localPosition.x / gridSpacing);
        int localPositionZ = (int)(localPosition.z / gridSpacing);
        int radiusPoints = (int)(radius / gridSpacing);

        int minX = Mathf.Max(0, localPositionX - radiusPoints);
        int maxX = Mathf.Min(xRows, localPositionX + radiusPoints);

        // localPositionX + radiusPoints - ( localPositionX - radiusPoints)
        // = 2 * radiuspoints

        int minZ = Mathf.Max(0, localPositionZ - radiusPoints);
        int maxZ = Mathf.Min(zRows, localPositionZ + radiusPoints);
        List<CullingGridPoint> output = new List<CullingGridPoint>();
        for (int i = minX; i < maxX; i++) {
            CullingGridPoint[] row = points[i];
            for (int j = minZ; j < maxZ; j++) {
                if (!row[j].isEmpty)
                    output.Add(row[j]);
            }
        }
        return output;
    }

    public void Write(string levelName, string sceneName) {
        XmlSerializer serializer = new XmlSerializer(typeof(CullingGrid));
        string path = CullingFilePath(levelName, sceneName);
        Debug.Log($"writing culling data to {path}...");
        using (FileStream sceneStream = File.Create(path)) {
            serializer.Serialize(sceneStream, this);
        }
    }

    public static string CullingFilePath(string levelName, string sceneName) {
        string scenePath = LevelState.LevelDataPath(levelName);
        return Path.Combine(scenePath, $"culling_{sceneName}.xml");
    }

    public static CullingGrid Load(string levelName, string sceneName) {
        string path = $"data/missions/{levelName}/culling_{sceneName}";
        TextAsset textAsset = Resources.Load<TextAsset>(path) as TextAsset;
        XmlSerializer serializer = new XmlSerializer(typeof(CullingGrid));
        Debug.Log($"loading culling grid at {path}...");
        using (var reader = new System.IO.StringReader(textAsset.text)) {
            CullingGrid cullingGrid = serializer.Deserialize(reader) as CullingGrid;
            Debug.Log($"loaded culling grid with {cullingGrid.xRows}x{cullingGrid.zRows}...");
            return cullingGrid;
        }
    }
}