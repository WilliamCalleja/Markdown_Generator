using System;
using UnityEngine;

[Serializable]
public class MapGenerationParameters
{
    public Vector2 cellSize;
    public float noiseScale;
    public Vector2Int resolution;

    public int CellCount()
    {
        var dim = CellDimensions();
        return dim.x * dim.y;
    }
    public Vector2Int CellDimensions()
    {
        return new Vector2Int(
            Mathf.FloorToInt(resolution.x / cellSize.x),
            Mathf.FloorToInt(resolution.y / cellSize.y));
    }
}

public class MapManager : MonoBehaviour
{
    [Serializable]
    public struct Zone
    {
        public Vector2 cell;
        public Vector2 origin;
        public Vector2 connection0;
        public Vector2 connection1;
        public Vector2 connection2;
        public Vector2 connection3;
        /*public Vector2 connection4;*/
        /*public Vector2 connection5;*/
    }
    
    public RenderTexture result;
    public ComputeShader voronoiGenerator;
    public Material visualizer;
    public MapGenerationParameters parameters;
    public Zone[] zones;
    private static readonly int CellSize = Shader.PropertyToID("pmg_cell_size");
    private static readonly int NoiseScale = Shader.PropertyToID("pmg_noise_scale");
    private static readonly int Result = Shader.PropertyToID("_Result");

    private void FixedUpdate()
    {
        foreach (var zone in zones)
        {
            DebugPlus.DrawSphere(VisualizerOffset(zone.origin), 0.1f)
                .Duration(0.02f).Color(Color.yellow);
            
            DebugPlus.DrawLine(VisualizerOffset(zone.connection0), VisualizerOffset(zone.origin))
                .Duration(0.02f).Color(Color.cyan);
            
            DebugPlus.DrawLine(VisualizerOffset(zone.connection1), VisualizerOffset(zone.origin))
                .Duration(0.02f).Color(Color.magenta);
            
            DebugPlus.DrawLine(VisualizerOffset(zone.connection2), VisualizerOffset(zone.origin))
                .Duration(0.02f).Color(Color.red);
            
            DebugPlus.DrawLine(VisualizerOffset(zone.connection3), VisualizerOffset(zone.origin))
                .Duration(0.02f).Color(Color.green);
            
            /*DebugPlus.DrawLine(VisualizerOffset(zone.connection4), VisualizerOffset(zone.origin))
                .Duration(0.02f).Color(Color.yellow);*/
            
            /*DebugPlus.DrawLine(VisualizerOffset(zone.connection5), VisualizerOffset(zone.origin))
                .Duration(0.02f).Color(Color.yellow);*/
        }
    }

    private Vector2 VisualizerOffset(Vector2 origin)
    {
        return new Vector2(-5, -5) + ((origin / parameters.resolution) * 10.0f);
    }

    private void OnValidate()
    {
        const int size = sizeof(float) * 12;
        var zoneBuffer = new ComputeBuffer(parameters.CellCount(), size);
        zones = new Zone[parameters.CellCount()];
        zoneBuffer.SetData(zones);
        
        result = new RenderTexture(
            parameters.resolution.x, 
            parameters.resolution.y, 
            24)
        {
            enableRandomWrite = true
        };
        voronoiGenerator.SetVector("cell_dimensions", (Vector2)parameters.CellDimensions());
        voronoiGenerator.SetBuffer(0, "zone_buffer", zoneBuffer);
        voronoiGenerator.SetTexture(0, "result", result);
        voronoiGenerator.Dispatch(0, result.width/8, result.height/8, 1);
        
        zoneBuffer.GetData(zones);
        zoneBuffer.Release();
        
        visualizer.SetTexture(Result, result);
        
        Shader.SetGlobalVector(CellSize, parameters.cellSize);
        Shader.SetGlobalFloat(NoiseScale, parameters.noiseScale);
    }
}
