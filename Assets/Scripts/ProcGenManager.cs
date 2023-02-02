using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.SceneManagement;
#endif // UNITY_EDITOR

public enum EGenerationStage
{
    Beginning = 1,

    BuildTextureMap,
    BuildDetailMap,
    BuildBiomeMap,
    HeightMapGeneration,
    TerrainPainting,
    ObjectPlacement,
    DetailPainting,

    Complete,
    NumStages = Complete
}

public class ProcGenManager : MonoBehaviour
{
    public class GenerationData
    {
        public ProcGenManager Manager;
        public ProcGenConfigSO Config;
        public Transform ObjectRoot;

        public int MapResolution;
        public float[,] HeightMap;
        public Vector3 HeightmapScale;
        public float[,,] AlphaMaps;
        public int AlphaMapResolution;

        public byte[,] BiomeMap;
        public float[,] BiomeStrengths;

        public float[,] SlopeMap;

        public Dictionary<TextureConfig, int> BiomeTextureToTerrainLayerIndex = new Dictionary<TextureConfig, int>();
        public Dictionary<TerrainDetailConfig, int> BiomeTerrainDetailToDetailLayerIndex = new Dictionary<TerrainDetailConfig, int>();

        public List<int[,]> DetailLayerMaps;
        public int DetailMapResolution;
        public int MaxDetailsPerPatch;
    }

    [SerializeField] ProcGenConfigSO Config;
    [SerializeField] Terrain TargetTerrain;

    [Header("Debugging")]
    [SerializeField] bool DEBUG_TurnOffObjectPlacers = false;

    GenerationData Data;

    public IEnumerator AsyncRegenerateWorld(System.Action<EGenerationStage, string> reportStatusFn = null)
    {
        Data = new GenerationData();

        // cache the core info
        Data.Manager = this;
        Data.Config = Config;
        Data.ObjectRoot = TargetTerrain.transform;

        // cache the map resolution
        Data.MapResolution = TargetTerrain.terrainData.heightmapResolution;
        Data.AlphaMapResolution = TargetTerrain.terrainData.alphamapResolution;
        Data.DetailMapResolution = TargetTerrain.terrainData.detailResolution;
        Data.MaxDetailsPerPatch = TargetTerrain.terrainData.detailResolutionPerPatch;

        if (reportStatusFn != null) reportStatusFn.Invoke(EGenerationStage.Beginning, "Beginning Generation");
        yield return new WaitForSeconds(1f);

        // clear out any previously spawned objects
        for (int childIndex = Data.ObjectRoot.childCount - 1; childIndex >= 0; --childIndex)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                Destroy(Data.ObjectRoot.GetChild(childIndex).gameObject);
            else
                Undo.DestroyObjectImmediate(Data.ObjectRoot.GetChild(childIndex).gameObject);
#else
            Destroy(Data.ObjectRoot.GetChild(childIndex).gameObject);
#endif // UNITY_EDITOR
        }

        if (reportStatusFn != null) reportStatusFn.Invoke(EGenerationStage.BuildTextureMap, "Building texture map");
        yield return new WaitForSeconds(1f);

        // Generate the texture mapping
        Perform_GenerateTextureMapping();

        if (reportStatusFn != null) reportStatusFn.Invoke(EGenerationStage.BuildDetailMap, "Building detail map");
        yield return new WaitForSeconds(1f);

        // Generate the detail mapping
        Perform_GenerateTerrainDetailMapping();

        if (reportStatusFn != null) reportStatusFn.Invoke(EGenerationStage.BuildBiomeMap, "Build biome map");
        yield return new WaitForSeconds(1f);

        // generate the biome map
        Perform_BiomeGeneration();

        if (reportStatusFn != null) reportStatusFn.Invoke(EGenerationStage.HeightMapGeneration, "Modifying heights");
        yield return new WaitForSeconds(1f);

        // update the terrain heights
        Perform_HeightMapModification();

        if (reportStatusFn != null) reportStatusFn.Invoke(EGenerationStage.TerrainPainting, "Painting the terrain");
        yield return new WaitForSeconds(1f);

        // paint the terrain
        Perform_TerrainPainting();

        if (reportStatusFn != null) reportStatusFn.Invoke(EGenerationStage.ObjectPlacement, "Placing objects");
        yield return new WaitForSeconds(1f);

        // place the objects
        Perform_ObjectPlacement();

        if (reportStatusFn != null) reportStatusFn.Invoke(EGenerationStage.DetailPainting, "Placing objects");
        yield return new WaitForSeconds(1f);

        // paint the details
        Perform_DetailPainting();

        if (reportStatusFn != null) reportStatusFn.Invoke(EGenerationStage.Complete, "Generation complete");
    }

    void Perform_GenerateTextureMapping()
    {
        Data.BiomeTextureToTerrainLayerIndex.Clear();

        // build up list of all textures
        List<TextureConfig> allTextures = new List<TextureConfig>();
        foreach(var biomeMetadata in Config.Biomes)
        {
            List<TextureConfig> biomeTextures = biomeMetadata.Biome.RetrieveTextures();

            if (biomeTextures == null || biomeTextures.Count == 0)
                continue;

            allTextures.AddRange(biomeTextures);
        }

        if (Config.PaintingPostProcessingModifier != null)
        {
            // extract all textures from every painter
            BaseTexturePainter[] allPainters = Config.PaintingPostProcessingModifier.GetComponents<BaseTexturePainter>();
            foreach(var painter in allPainters)
            {
                var painterTextures = painter.RetrieveTextures();

                if (painterTextures == null || painterTextures.Count == 0)
                    continue;

                allTextures.AddRange(painterTextures);
            }            
        }

        // filter out any duplicate entries
        allTextures = allTextures.Distinct().ToList();

        // iterate over the texture configs
        int layerIndex = 0;
        foreach(var textureConfig in allTextures)
        {
            Data.BiomeTextureToTerrainLayerIndex[textureConfig] = layerIndex;
            ++layerIndex;
        }
    }

    void Perform_GenerateTerrainDetailMapping()
    {
        Data.BiomeTerrainDetailToDetailLayerIndex.Clear();

        // build up list of all terrain details
        List<TerrainDetailConfig> allTerrainDetails = new List<TerrainDetailConfig>();
        foreach (var biomeMetadata in Config.Biomes)
        {
            List<TerrainDetailConfig> biomeTerrainDetails = biomeMetadata.Biome.RetrieveTerrainDetails();

            if (biomeTerrainDetails == null || biomeTerrainDetails.Count == 0)
                continue;

            allTerrainDetails.AddRange(biomeTerrainDetails);
        }

        if (Config.DetailPaintingPostProcessingModifier != null)
        {
            // extract all terrain details from every painter
            BaseDetailPainter[] allPainters = Config.DetailPaintingPostProcessingModifier.GetComponents<BaseDetailPainter>();
            foreach (var painter in allPainters)
            {
                var terrainDetails = painter.RetrieveTerrainDetails();

                if (terrainDetails == null || terrainDetails.Count == 0)
                    continue;

                allTerrainDetails.AddRange(terrainDetails);
            }
        }

        // filter out any duplicate entries
        allTerrainDetails = allTerrainDetails.Distinct().ToList();

        // iterate over the terrain detail configs
        int layerIndex = 0;
        foreach (var terrainDetail in allTerrainDetails)
        {
            Data.BiomeTerrainDetailToDetailLayerIndex[terrainDetail] = layerIndex;
            ++layerIndex;
        }
    }

#if UNITY_EDITOR
    public void RegenerateTextures()
    {
        Perform_LayerSetup();
    }

    public void RegenerateDetailPrototypes()
    {
        Perform_DetailPrototypeSetup();
    }

    void Perform_LayerSetup()
    {
        // delete any existing layers
        if (TargetTerrain.terrainData.terrainLayers != null || TargetTerrain.terrainData.terrainLayers.Length > 0)
        {
            Undo.RecordObject(TargetTerrain, "Clearing previous layers");

            // build up list of asset paths for each layer
            List<string> layersToDelete = new List<string>();
            foreach(var layer in TargetTerrain.terrainData.terrainLayers)
            {
                if (layer == null)
                    continue;

                layersToDelete.Add(AssetDatabase.GetAssetPath(layer.GetInstanceID()));
            }

            // remove all links to layers
            TargetTerrain.terrainData.terrainLayers = null;

            // delete each layer
            foreach(var layerFile in layersToDelete)
            {
                if (string.IsNullOrEmpty(layerFile))
                    continue;

                AssetDatabase.DeleteAsset(layerFile);
            }

            Undo.FlushUndoRecordObjects();
        }

        string scenePath = System.IO.Path.GetDirectoryName(SceneManager.GetActiveScene().path);

        Perform_GenerateTextureMapping();

        // generate all of the layers
        int numLayers = Data.BiomeTextureToTerrainLayerIndex.Count;
        List<TerrainLayer> newLayers = new List<TerrainLayer>(numLayers);
        
        // preallocate the layers
        for (int layerIndex = 0; layerIndex < numLayers; ++layerIndex)
        {
            newLayers.Add(new TerrainLayer());
        }

        // iterate over the texture map
        foreach(var textureMappingEntry in Data.BiomeTextureToTerrainLayerIndex)
        {
            var textureConfig = textureMappingEntry.Key;
            var textureLayerIndex = textureMappingEntry.Value;
            var textureLayer = newLayers[textureLayerIndex];

            // configure the terrain layer textures
            textureLayer.diffuseTexture = textureConfig.Diffuse;
            textureLayer.normalMapTexture = textureConfig.NormalMap;

            // save as asset
            string layerPath = System.IO.Path.Combine(scenePath, "Layer_" + textureLayerIndex);
            AssetDatabase.CreateAsset(textureLayer, $"{layerPath}.asset");
        }

        Undo.RecordObject(TargetTerrain.terrainData, "Updating terrain layers");
        TargetTerrain.terrainData.terrainLayers = newLayers.ToArray();
    }

    void Perform_DetailPrototypeSetup()
    {
        Perform_GenerateTerrainDetailMapping();

        // build the list of detail prototypes
        var detailPrototypes = new DetailPrototype[Data.BiomeTerrainDetailToDetailLayerIndex.Count];
        foreach(var kvp in Data.BiomeTerrainDetailToDetailLayerIndex)
        {
            TerrainDetailConfig detailData = kvp.Key;
            int layerIndex = kvp.Value;

            DetailPrototype newDetail = new DetailPrototype();

            // is this a mesh?
            if (detailData.DetailPrefab)
            {
                newDetail.prototype = detailData.DetailPrefab;
                newDetail.renderMode = DetailRenderMode.VertexLit;
                newDetail.usePrototypeMesh = true;
                newDetail.useInstancing = true;
            }
            else
            {
                newDetail.prototypeTexture = detailData.BillboardTexture;
                newDetail.renderMode = DetailRenderMode.GrassBillboard;
                newDetail.usePrototypeMesh = false;
                newDetail.useInstancing = false;
                newDetail.healthyColor = detailData.HealthyColour;
                newDetail.dryColor = detailData.DryColour;
            }

            // transfer the common data
            newDetail.minWidth = detailData.MinWidth;
            newDetail.maxWidth = detailData.MaxWidth;
            newDetail.minHeight = detailData.MinHeight;
            newDetail.maxHeight = detailData.MaxHeight;
            newDetail.noiseSeed = detailData.NoiseSeed;
            newDetail.noiseSpread = detailData.NoiseSpread;
            newDetail.holeEdgePadding = detailData.HoleEdgePadding;

            // check the prototype
            string errorMessage;
            if (!newDetail.Validate(out errorMessage))
            {
                throw new System.InvalidOperationException(errorMessage);
            }

            detailPrototypes[layerIndex] = newDetail;
        }

        // update the detail prototypes
        Undo.RecordObject(TargetTerrain.terrainData, "Updating Detail Prototypes");
        TargetTerrain.terrainData.detailPrototypes = detailPrototypes;
        TargetTerrain.terrainData.RefreshPrototypes();
    }
#endif // UNITY_EDITOR

    void Perform_BiomeGeneration()
    {
        // allocate the biome map and strength map
        Data.BiomeMap = new byte[Data.MapResolution, Data.MapResolution];
        Data.BiomeStrengths = new float[Data.MapResolution, Data.MapResolution];

        // execute any initial height modifiers
        if (Config.BiomeGenerators != null)
        {
            BaseBiomeMapGenerator[] generators = Config.BiomeGenerators.GetComponents<BaseBiomeMapGenerator>();

            foreach (var generator in generators)
            {
                generator.Execute(Data);
            }
        }
    }

    void Perform_HeightMapModification()
    {
        Data.HeightMap = TargetTerrain.terrainData.GetHeights(0, 0, Data.MapResolution, Data.MapResolution);

        // execute any initial height modifiers
        if (Config.InitialHeightModifier != null)
        {
            BaseHeightMapModifier[] modifiers = Config.InitialHeightModifier.GetComponents<BaseHeightMapModifier>();

            foreach(var modifier in modifiers)
            {
                modifier.Execute(Data);
            }
        }

        // run heightmap generation for each biome
        for (int biomeIndex = 0; biomeIndex < Config.NumBiomes; ++biomeIndex)
        {
            var biome = Config.Biomes[biomeIndex].Biome;
            if (biome.HeightModifier == null)
                continue;

            BaseHeightMapModifier[] modifiers = biome.HeightModifier.GetComponents<BaseHeightMapModifier>();

            foreach(var modifier in modifiers)
            {
                modifier.Execute(Data, biomeIndex, biome);
            }
        }

        // execute any post processing height modifiers
        if (Config.HeightPostProcessingModifier != null)
        {
            BaseHeightMapModifier[] modifiers = Config.HeightPostProcessingModifier.GetComponents<BaseHeightMapModifier>();
        
            foreach(var modifier in modifiers)
            {
                modifier.Execute(Data);
            }
        }     

        TargetTerrain.terrainData.SetHeights(0, 0, Data.HeightMap);

        // generate the slope map
        Data.SlopeMap = new float[Data.AlphaMapResolution, Data.AlphaMapResolution];
        for (int y = 0; y < Data.AlphaMapResolution; ++y)
        {
            for (int x = 0; x < Data.AlphaMapResolution; ++x)
            {
                Data.SlopeMap[x, y] = TargetTerrain.terrainData.GetInterpolatedNormal((float) x / Data.AlphaMapResolution, (float) y / Data.AlphaMapResolution).y;
            }
        }          
    }

    public int GetLayerForTexture(TextureConfig textureConfig)
    {
        return Data.BiomeTextureToTerrainLayerIndex[textureConfig];
    }

    public int GetDetailLayerForTerrainDetail(TerrainDetailConfig detailConfig)
    {
        return Data.BiomeTerrainDetailToDetailLayerIndex[detailConfig];
    }

    void Perform_TerrainPainting()
    {
        Data.AlphaMaps = TargetTerrain.terrainData.GetAlphamaps(0, 0, Data.AlphaMapResolution, Data.AlphaMapResolution);

        // zero out all layers
        for (int y = 0; y < Data.AlphaMapResolution; ++y)
        {
            for (int x = 0; x < Data.AlphaMapResolution; ++x)
            {
                for (int layerIndex = 0; layerIndex < TargetTerrain.terrainData.alphamapLayers; ++layerIndex)
                {
                    Data.AlphaMaps[x, y, layerIndex] = 0;
                }
            }
        }   

        // run terrain painting for each biome
        for (int biomeIndex = 0; biomeIndex < Config.NumBiomes; ++biomeIndex)
        {
            var biome = Config.Biomes[biomeIndex].Biome;
            if (biome.TerrainPainter == null)
                continue;

            BaseTexturePainter[] modifiers = biome.TerrainPainter.GetComponents<BaseTexturePainter>();

            foreach(var modifier in modifiers)
            {
                modifier.Execute(Data, biomeIndex, biome);
            }
        }        

        // run texture post processing
        if (Config.PaintingPostProcessingModifier != null)
        {
            BaseTexturePainter[] modifiers = Config.PaintingPostProcessingModifier.GetComponents<BaseTexturePainter>();

            foreach(var modifier in modifiers)
            {
                modifier.Execute(Data);
            }    
        }

        TargetTerrain.terrainData.SetAlphamaps(0, 0, Data.AlphaMaps);
    }

    void Perform_ObjectPlacement()
    {
        if (DEBUG_TurnOffObjectPlacers)
            return;

        // run object placement for each biome
        for (int biomeIndex = 0; biomeIndex < Config.NumBiomes; ++biomeIndex)
        {
            var biome = Config.Biomes[biomeIndex].Biome;
            if (biome.ObjectPlacer == null)
                continue;

            BaseObjectPlacer[] modifiers = biome.ObjectPlacer.GetComponents<BaseObjectPlacer>();

            foreach(var modifier in modifiers)
            {
                modifier.Execute(Data, biomeIndex, biome);
            }
        }        
    }

    void Perform_DetailPainting()
    {
        // create a new empty set of layers
        int numDetailLayers = TargetTerrain.terrainData.detailPrototypes.Length;
        Data.DetailLayerMaps = new List<int[,]>(numDetailLayers);
        for (int layerIndex = 0; layerIndex < numDetailLayers; ++layerIndex)
        {
            Data.DetailLayerMaps.Add(new int[Data.DetailMapResolution, Data.DetailMapResolution]);
        }

        // run terrain detail painting for each biome
        for (int biomeIndex = 0; biomeIndex < Config.NumBiomes; ++biomeIndex)
        {
            var biome = Config.Biomes[biomeIndex].Biome;
            if (biome.DetailPainter == null)
                continue;

            BaseDetailPainter[] modifiers = biome.DetailPainter.GetComponents<BaseDetailPainter>();

            foreach (var modifier in modifiers)
            {
                modifier.Execute(Data, biomeIndex, biome);
            }
        }

        // run detail painting post processing
        if (Config.DetailPaintingPostProcessingModifier != null)
        {
            BaseDetailPainter[] modifiers = Config.DetailPaintingPostProcessingModifier.GetComponents<BaseDetailPainter>();

            foreach (var modifier in modifiers)
            {
                modifier.Execute(Data);
            }
        }

        // apply the detail layers
        for (int layerIndex = 0; layerIndex < numDetailLayers; ++layerIndex)
        {
            TargetTerrain.terrainData.SetDetailLayer(0, 0, layerIndex, Data.DetailLayerMaps[layerIndex]);
        }
    }
}
