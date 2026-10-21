using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.Json;
using Raylib_cs;
using TestEngine.Utils.LDtk;

namespace ColaEngine.Graphics;

/// <summary>
/// A loaded LDtk project and its level data. Dispose the project to release textures loaded for drawing.
/// </summary>
public sealed class LdtkProject : IDisposable
{
    private readonly Dictionary<int, Texture2D> _textures = new();
    private readonly Dictionary<int, LDtkModels.LdtkTilesetDefinition> _tilesetsByUid = new();
    private readonly Dictionary<string, LDtkModels.LdtkLevel> _levelsByIdentifier = new(StringComparer.Ordinal);
    private bool _disposed;

    /// <summary>Absolute path of the loaded .ldtk file.</summary>
    public string ProjectPath { get; }

    /// <summary>Project identifier (the project's IID).</summary>
    public string Identifier { get; }

    /// <summary>LDtk JSON format version the project was saved with.</summary>
    public string JsonVersion { get; }

    /// <summary>World layout (Free, GridVania, LinearHorizontal, LinearVertical or MultiWorlds).</summary>
    public string WorldLayout { get; }

    /// <summary>Width of the world grid in pixels (GridVania layout).</summary>
    public int WorldGridWidth { get; }

    /// <summary>Height of the world grid in pixels (GridVania layout).</summary>
    public int WorldGridHeight { get; }

    /// <summary>Default grid size in pixels for new layers.</summary>
    public int DefaultGridSize { get; }

    /// <summary>All levels in the project, across every world.</summary>
    public IReadOnlyList<LDtkModels.LdtkLevel> Levels { get; }

    /// <summary>Worlds in the project. Used by multi-world projects.</summary>
    public IReadOnlyList<LDtkModels.LdtkWorld> Worlds { get; }

    /// <summary>Tileset definitions in the project.</summary>
    public IReadOnlyList<LDtkModels.LdtkTilesetDefinition> Tilesets { get; }

    /// <summary>Layer definitions in the project.</summary>
    public IReadOnlyList<LDtkModels.LdtkLayerDefinition> LayerDefinitions { get; }

    private LdtkProject(
        string projectPath,
        string identifier,
        string jsonVersion,
        string worldLayout,
        int worldGridWidth,
        int worldGridHeight,
        int defaultGridSize,
        List<LDtkModels.LdtkLevel> levels,
        List<LDtkModels.LdtkWorld> worlds,
        List<LDtkModels.LdtkTilesetDefinition> tilesets,
        List<LDtkModels.LdtkLayerDefinition> layerDefinitions)
    {
        ProjectPath = projectPath;
        Identifier = identifier;
        JsonVersion = jsonVersion;
        WorldLayout = worldLayout;
        WorldGridWidth = worldGridWidth;
        WorldGridHeight = worldGridHeight;
        DefaultGridSize = defaultGridSize;
        Levels = levels.AsReadOnly();
        Worlds = worlds.AsReadOnly();
        Tilesets = tilesets.AsReadOnly();
        LayerDefinitions = layerDefinitions.AsReadOnly();

        foreach (LDtkModels.LdtkTilesetDefinition tileset in tilesets)
        {
            _tilesetsByUid.Add(tileset.Uid, tileset);
        }

        foreach (LDtkModels.LdtkLevel level in levels)
        {
            _levelsByIdentifier.TryAdd(level.Identifier, level);
        }
    }

    /// <summary>
    /// Loads an LDtk project (.ldtk) and any external level files it references.
    /// </summary>
    public static LdtkProject Load(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);
        string fullPath = Path.GetFullPath(projectPath);

        using FileStream projectStream = File.OpenRead(fullPath);
        using JsonDocument document = JsonDocument.Parse(projectStream);
        JsonElement root = document.RootElement;
        string projectDirectory = Path.GetDirectoryName(fullPath)!;
        JsonElement definitions = GetProperty(root, "defs");

        List<LDtkModels.LdtkTilesetDefinition> tilesets = ParseTilesets(definitions);
        List<LDtkModels.LdtkLayerDefinition> layerDefinitions = ParseLayerDefinitions(definitions);
        Dictionary<int, LDtkModels.LdtkTilesetDefinition> tilesetsByUid = new();
        foreach (LDtkModels.LdtkTilesetDefinition tileset in tilesets)
        {
            tilesetsByUid.Add(tileset.Uid, tileset);
        }

        List<LDtkModels.LdtkLevel> levels = new();
        Dictionary<string, LDtkModels.LdtkLevel> levelsByIid = new(StringComparer.Ordinal);
        JsonElement levelArray = GetArray(root, "levels");
        foreach (JsonElement levelData in levelArray.EnumerateArray())
        {
            LDtkModels.LdtkLevel level = ParseLevel(levelData, projectDirectory, tilesetsByUid);
            levels.Add(level);
            levelsByIid.Add(level.Iid, level);
        }

        string projectLayout = GetNullableString(root, "worldLayout") ?? "MultiWorlds";
        ApplyLinearLayout(levels, projectLayout);
        List<LDtkModels.LdtkWorld> worlds = ParseWorlds(
            root,
            projectDirectory,
            tilesetsByUid,
            levels,
            levelsByIid);

        return new LdtkProject(
            fullPath,
            GetString(root, "iid"),
            GetString(root, "jsonVersion"),
            GetNullableString(root, "worldLayout") ?? "MultiWorlds",
            GetNullableInt(root, "worldGridWidth") ?? 0,
            GetNullableInt(root, "worldGridHeight") ?? 0,
            GetInt(root, "defaultGridSize"),
            levels,
            worlds,
            tilesets,
            layerDefinitions);
    }

    /// <summary>Gets a tileset definition by UID.</summary>
    public LDtkModels.LdtkTilesetDefinition GetTileset(int uid) => _tilesetsByUid[uid];

    /// <summary>
    /// Draws every level at its LDtk world position, with an optional world-space offset.
    /// </summary>
    public void Draw(Vector2 offset = default)
    {
        ThrowIfDisposed();
        foreach (LDtkModels.LdtkLevel level in Levels)
        {
            DrawLevel(level, offset);
        }
    }

    /// <summary>
    /// Draws one level at its LDtk world position, with an optional world-space offset.
    /// </summary>
    public void DrawLevel(string identifier, Vector2 offset = default)
    {
        ThrowIfDisposed();
        DrawLevel(GetLevel(identifier), offset);
    }

    /// <summary>
    /// Gets a level by identifier. Throws <see cref="KeyNotFoundException"/> if it does not exist.
    /// </summary>
    public LDtkModels.LdtkLevel GetLevel(string identifier)
    {
        return TryGetLevel(identifier, out LDtkModels.LdtkLevel? level)
            ? level
            : throw new KeyNotFoundException($"LDtk project '{Identifier}' has no level named '{identifier}'.");
    }

    /// <summary>
    /// Tries to get a level by identifier. Returns false if it does not exist.
    /// </summary>
    public bool TryGetLevel(string identifier, [NotNullWhen(true)] out LDtkModels.LdtkLevel? level)
    {
        return _levelsByIdentifier.TryGetValue(identifier, out level);
    }

    /// <summary>
    /// Gets every entity with the given identifier across all levels.
    /// </summary>
    public IEnumerable<LDtkModels.LdtkEntity> GetEntities(string identifier)
    {
        foreach (LDtkModels.LdtkLevel level in Levels)
        {
            foreach (LDtkModels.LdtkEntity entity in level.GetEntities(identifier))
            {
                yield return entity;
            }
        }
    }

    /// <summary>
    /// Gets the first entity with the given identifier across all levels, or null if none exists.
    /// </summary>
    public LDtkModels.LdtkEntity? FindEntity(string identifier)
    {
        foreach (LDtkModels.LdtkLevel level in Levels)
        {
            LDtkModels.LdtkEntity? entity = level.FindEntity(identifier);
            if (entity != null)
            {
                return entity;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets an entity by IID across all levels, or null if none exists.
    /// </summary>
    public LDtkModels.LdtkEntity? FindEntityByIid(string iid)
    {
        foreach (LDtkModels.LdtkLevel level in Levels)
        {
            LDtkModels.LdtkEntity? entity = level.FindEntityByIid(iid);
            if (entity != null)
            {
                return entity;
            }
        }

        return null;
    }

    /// <summary>
    /// Draws every level belonging to a multi-world project world.
    /// </summary>
    public void DrawWorld(string worldIid, Vector2 offset = default)
    {
        ThrowIfDisposed();
        foreach (LDtkModels.LdtkWorld world in Worlds)
        {
            if (string.Equals(world.Iid, worldIid, StringComparison.Ordinal))
            {
                foreach (LDtkModels.LdtkLevel level in world.Levels)
                {
                    DrawLevel(level, offset);
                }

                return;
            }
        }

        throw new KeyNotFoundException($"LDtk project '{Identifier}' has no world with IID '{worldIid}'.");
    }

    private void DrawLevel(LDtkModels.LdtkLevel level, Vector2 offset)
    {
        Vector2 levelOrigin = level.WorldPosition + offset;
        for (int layerIndex = level.Layers.Count - 1; layerIndex >= 0; layerIndex--)
        {
            LDtkModels.LdtkLayer layer = level.Layers[layerIndex];
            if (!layer.Visible)
            {
                continue;
            }

            foreach (LDtkModels.LdtkTile tile in layer.Tiles)
            {
                DrawTile(
                    tile.TilesetDefinitionUid,
                    tile.SourcePosition,
                    tile.SourceWidth,
                    tile.SourceHeight,
                    layer.GridSize,
                    layer.GridSize,
                    levelOrigin + layer.PixelOffset + tile.Position,
                    layer.Opacity * tile.Alpha,
                    tile.FlipX,
                    tile.FlipY);
            }

            foreach (LDtkModels.LdtkEntity entity in layer.Entities)
            {
                if (entity.Tile is not null)
                {
                    DrawTile(
                        entity.Tile.TilesetDefinitionUid,
                        entity.Tile.SourcePosition,
                        entity.Tile.SourceWidth,
                        entity.Tile.SourceHeight,
                        entity.Width,
                        entity.Height,
                        levelOrigin + layer.PixelOffset +
                        entity.Position - entity.Pivot * new Vector2(entity.Width, entity.Height),
                        layer.Opacity,
                        false,
                        false);
                }
            }
        }
    }

    private void DrawTile(
        int tilesetUid,
        Vector2 sourcePosition,
        int sourceWidth,
        int sourceHeight,
        int destinationWidth,
        int destinationHeight,
        Vector2 position,
        float opacity,
        bool flipX,
        bool flipY)
    {
        Texture2D texture = GetTexture(tilesetUid);
        float alpha = Math.Clamp(opacity, 0f, 1f);
        byte alphaByte = (byte)Math.Round(alpha * 255f);
        Rectangle source = new(
            sourcePosition.X,
            sourcePosition.Y,
            flipX ? -sourceWidth : sourceWidth,
            flipY ? -sourceHeight : sourceHeight);
        Rectangle destination = new(position.X, position.Y, destinationWidth, destinationHeight);
        Raylib.DrawTexturePro(
            texture,
            source,
            destination,
            Vector2.Zero,
            0f,
            new Color((byte)255, (byte)255, (byte)255, alphaByte));
    }

    private Texture2D GetTexture(int tilesetUid)
    {
        if (_textures.TryGetValue(tilesetUid, out Texture2D texture))
        {
            return texture;
        }

        if (!_tilesetsByUid.TryGetValue(tilesetUid, out LDtkModels.LdtkTilesetDefinition? tileset))
        {
            throw new InvalidDataException($"LDtk references missing tileset UID {tilesetUid}.");
        }

        if (string.IsNullOrWhiteSpace(tileset.RelativePath))
        {
            throw new InvalidDataException(
                $"LDtk tileset '{tileset.Identifier}' (UID {tilesetUid}) has no image path.");
        }

        string texturePath = Path.IsPathRooted(tileset.RelativePath)
            ? tileset.RelativePath
            : Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(ProjectPath)!,
                tileset.RelativePath
                    .Replace('/', Path.DirectorySeparatorChar)
                    .Replace('\\', Path.DirectorySeparatorChar)));
        texture = Raylib.LoadTexture(texturePath);
        if (texture.Id == 0)
        {
            throw new IOException(
                $"Raylib could not load tileset image '{texturePath}' (UID {tilesetUid}).");
        }

        _textures.Add(tilesetUid, texture);
        return texture;
    }

    private static List<LDtkModels.LdtkTilesetDefinition> ParseTilesets(JsonElement definitions)
    {
        List<LDtkModels.LdtkTilesetDefinition> tilesets = new();
        JsonElement tilesetArray = GetArray(definitions, "tilesets");
        foreach (JsonElement item in tilesetArray.EnumerateArray())
        {
            tilesets.Add(new LDtkModels.LdtkTilesetDefinition
            {
                Uid = GetInt(item, "uid"),
                Identifier = GetString(item, "identifier"),
                RelativePath = GetNullableString(item, "relPath"),
                TileGridSize = GetInt(item, "tileGridSize"),
                PixelWidth = GetInt(item, "pxWid"),
                PixelHeight = GetInt(item, "pxHei"),
                Spacing = GetInt(item, "spacing"),
                Padding = GetInt(item, "padding")
            });
        }

        return tilesets;
    }

    private static List<LDtkModels.LdtkLayerDefinition> ParseLayerDefinitions(JsonElement definitions)
    {
        List<LDtkModels.LdtkLayerDefinition> layerDefinitions = new();
        JsonElement layerArray = GetArray(definitions, "layers");
        foreach (JsonElement item in layerArray.EnumerateArray())
        {
            List<LDtkModels.LdtkIntGridValueDefinition> values = new();
            JsonElement valueArray = GetArray(item, "intGridValues");
            foreach (JsonElement value in valueArray.EnumerateArray())
            {
                values.Add(new LDtkModels.LdtkIntGridValueDefinition
                {
                    Value = GetInt(value, "value"),
                    Identifier = GetNullableString(value, "identifier"),
                    Color = GetString(value, "color")
                });
            }

            layerDefinitions.Add(new LDtkModels.LdtkLayerDefinition
            {
                Uid = GetInt(item, "uid"),
                Identifier = GetString(item, "identifier"),
                Type = GetString(item, "type"),
                GridSize = GetInt(item, "gridSize"),
                IntGridValues = values.AsReadOnly()
            });
        }

        return layerDefinitions;
    }

    private static LDtkModels.LdtkLevel ParseLevel(
        JsonElement data,
        string projectDirectory,
        IReadOnlyDictionary<int, LDtkModels.LdtkTilesetDefinition> tilesetsByUid)
    {
        JsonElement layerData = GetProperty(data, "layerInstances");
        string? externalPath = GetNullableString(data, "externalRelPath");
        if (externalPath is not null)
        {
            string resolvedPath = Path.GetFullPath(Path.Combine(
                projectDirectory,
                externalPath
                    .Replace('/', Path.DirectorySeparatorChar)
                    .Replace('\\', Path.DirectorySeparatorChar)));
            using FileStream externalStream = File.OpenRead(resolvedPath);
            using JsonDocument externalDocument = JsonDocument.Parse(externalStream);
            layerData = GetProperty(externalDocument.RootElement, "layerInstances");
            if (layerData.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidDataException(
                    $"External LDtk level '{resolvedPath}' does not contain layerInstances.");
            }

            return ParseLevelWithLayers(data, layerData, tilesetsByUid);
        }

        if (layerData.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException(
                $"LDtk level '{GetString(data, "identifier")}' has no layerInstances or external level file.");
        }

        return ParseLevelWithLayers(data, layerData, tilesetsByUid);
    }

    private static LDtkModels.LdtkLevel ParseLevelWithLayers(
        JsonElement data,
        JsonElement layerData,
        IReadOnlyDictionary<int, LDtkModels.LdtkTilesetDefinition> tilesetsByUid)
    {
        List<LDtkModels.LdtkLayer> layers = new();
        if (layerData.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement layer in layerData.EnumerateArray())
            {
                layers.Add(ParseLayer(layer, GetString(data, "identifier"), tilesetsByUid));
            }
        }

        return new LDtkModels.LdtkLevel
        {
            Identifier = GetString(data, "identifier"),
            Iid = GetString(data, "iid"),
            Uid = GetInt(data, "uid"),
            WorldPosition = new Vector2(GetInt(data, "worldX"), GetInt(data, "worldY")),
            WorldDepth = GetInt(data, "worldDepth"),
            PixelWidth = GetInt(data, "pxWid"),
            PixelHeight = GetInt(data, "pxHei"),
            BackgroundColor = GetNullableString(data, "bgColor") ?? GetString(data, "__bgColor"),
            Layers = layers.AsReadOnly(),
            Fields = ParseFields(GetProperty(data, "fieldInstances"))
        };
    }

    private static LDtkModels.LdtkLayer ParseLayer(
        JsonElement data,
        string levelIdentifier,
        IReadOnlyDictionary<int, LDtkModels.LdtkTilesetDefinition> tilesetsByUid)
    {
        int? layerTilesetUid = GetNullableInt(data, "overrideTilesetUid")
                               ?? GetNullableInt(data, "__tilesetDefUid");
        int gridSize = GetInt(data, "__gridSize");
        List<int> intGridValues = new();
        JsonElement intGrid = GetArray(data, "intGridCsv");
        foreach (JsonElement value in intGrid.EnumerateArray())
        {
            intGridValues.Add(value.GetInt32());
        }

        if (GetString(data, "__type") == "IntGrid" &&
            intGridValues.Count != GetInt(data, "__cWid") * GetInt(data, "__cHei"))
        {
            throw new InvalidDataException(
                $"LDtk layer '{GetString(data, "__identifier")}' has an IntGrid size that does not match its dimensions.");
        }

        List<LDtkModels.LdtkTile> tiles = new();
        AddTiles(GetArray(data, "autoLayerTiles"), layerTilesetUid, tilesetsByUid, gridSize, tiles);
        AddTiles(GetArray(data, "gridTiles"), layerTilesetUid, tilesetsByUid, gridSize, tiles);

        List<LDtkModels.LdtkEntity> entities = new();
        JsonElement entityData = GetArray(data, "entityInstances");
        foreach (JsonElement entity in entityData.EnumerateArray())
        {
            entities.Add(ParseEntity(entity));
        }

        return new LDtkModels.LdtkLayer
        {
            Identifier = GetString(data, "__identifier"),
            Iid = GetString(data, "iid"),
            LayerDefinitionUid = GetInt(data, "layerDefUid"),
            Type = GetString(data, "__type"),
            LevelIdentifier = levelIdentifier,
            GridWidth = GetInt(data, "__cWid"),
            GridHeight = GetInt(data, "__cHei"),
            GridSize = gridSize,
            PixelOffset = new Vector2(
                GetInt(data, "__pxTotalOffsetX"),
                GetInt(data, "__pxTotalOffsetY")),
            Opacity = GetFloat(data, "__opacity", 1f),
            Visible = GetProperty(data, "visible").ValueKind != JsonValueKind.False,
            TilesetDefinitionUid = layerTilesetUid,
            IntGridValues = intGridValues.AsReadOnly(),
            Tiles = tiles.AsReadOnly(),
            Entities = entities.AsReadOnly()
        };
    }

    private static void AddTiles(
        JsonElement tileArray,
        int? layerTilesetUid,
        IReadOnlyDictionary<int, LDtkModels.LdtkTilesetDefinition> tilesetsByUid,
        int gridSize,
        List<LDtkModels.LdtkTile> tiles)
    {
        if (tileArray.GetArrayLength() > 0 && gridSize <= 0)
        {
            throw new InvalidDataException("LDtk tile layers must have a positive grid size.");
        }

        foreach (JsonElement data in tileArray.EnumerateArray())
        {
            int tilesetUid = GetNullableInt(data, "tilesetUid") ?? layerTilesetUid
                ?? throw new InvalidDataException("LDtk tile has no tileset definition UID.");
            if (layerTilesetUid.HasValue && tilesetUid != layerTilesetUid.Value)
            {
                throw new InvalidDataException(
                    $"LDtk tile references tileset UID {tilesetUid}, but its layer uses UID {layerTilesetUid}.");
            }

            int[] position = GetIntArray(data, "px", 2);
            int[] source = GetIntArray(data, "src", 2);
            int flip = GetInt(data, "f");
            if (!tilesetsByUid.TryGetValue(tilesetUid, out LDtkModels.LdtkTilesetDefinition? tileset))
            {
                throw new InvalidDataException($"LDtk tile references missing tileset UID {tilesetUid}.");
            }

            tiles.Add(new LDtkModels.LdtkTile
            {
                Position = new Vector2(position[0], position[1]),
                SourcePosition = new Vector2(source[0], source[1]),
                SourceWidth = tileset.TileGridSize,
                SourceHeight = tileset.TileGridSize,
                TileId = GetInt(data, "t"),
                TilesetDefinitionUid = tilesetUid,
                FlipX = (flip & 1) != 0,
                FlipY = (flip & 2) != 0,
                Alpha = GetFloat(data, "a", 1f)
            });
        }
    }

    private static LDtkModels.LdtkEntity ParseEntity(JsonElement data)
    {
        List<string> tags = new();
        JsonElement tagsData = GetProperty(data, "__tags");
        if (tagsData.ValueKind != JsonValueKind.Array)
        {
            tagsData = GetArray(data, "tags");
        }

        foreach (JsonElement tag in tagsData.EnumerateArray())
        {
            tags.Add(tag.GetString() ?? string.Empty);
        }

        LDtkModels.LdtkEntityTile? tile = ParseEntityTile(GetProperty(data, "__tile"));
        int[] position = GetIntArray(data, "px", 2);
        string pivotProperty = GetProperty(data, "__pivot").ValueKind == JsonValueKind.Array
            ? "__pivot"
            : "pivot";
        Vector2 pivot = GetVector2(data, pivotProperty);

        return new LDtkModels.LdtkEntity
        {
            Identifier = GetString(data, "__identifier"),
            Iid = GetString(data, "iid"),
            DefinitionUid = GetInt(data, "defUid"),
            Position = new Vector2(position[0], position[1]),
            Pivot = pivot,
            Width = GetInt(data, "width"),
            Height = GetInt(data, "height"),
            Tags = tags.AsReadOnly(),
            Fields = ParseFields(GetProperty(data, "fieldInstances")),
            Tile = tile
        };
    }

    private static LDtkModels.LdtkEntityTile? ParseEntityTile(JsonElement data)
    {
        if (data.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        int x = GetInt(data, "x");
        int y = GetInt(data, "y");
        int width = GetInt(data, "w");
        int height = GetInt(data, "h");
        return new LDtkModels.LdtkEntityTile
        {
            TilesetDefinitionUid = GetInt(data, "tilesetUid"),
            SourcePosition = new Vector2(x, y),
            SourceWidth = width,
            SourceHeight = height
        };
    }

    private static Dictionary<string, JsonElement> ParseFields(JsonElement fieldArray)
    {
        if (fieldArray.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException("Expected LDtk JSON array property 'fieldInstances'.");
        }

        Dictionary<string, JsonElement> fields = new(StringComparer.Ordinal);
        foreach (JsonElement field in fieldArray.EnumerateArray())
        {
            fields[GetString(field, "__identifier")] = GetProperty(field, "__value").Clone();
        }

        return fields;
    }

    private static List<LDtkModels.LdtkWorld> ParseWorlds(
        JsonElement root,
        string projectDirectory,
        IReadOnlyDictionary<int, LDtkModels.LdtkTilesetDefinition> tilesetsByUid,
        List<LDtkModels.LdtkLevel> allLevels,
        Dictionary<string, LDtkModels.LdtkLevel> levelsByIid)
    {
        List<LDtkModels.LdtkWorld> worlds = new();
        JsonElement worldArray = GetArray(root, "worlds");
        foreach (JsonElement data in worldArray.EnumerateArray())
        {
            List<LDtkModels.LdtkLevel> levels = new();
            JsonElement worldLevels = GetArray(data, "levels");
            foreach (JsonElement levelReference in worldLevels.EnumerateArray())
            {
                string iid = GetString(levelReference, "iid");
                if (!levelsByIid.TryGetValue(iid, out LDtkModels.LdtkLevel? level))
                {
                    if (GetProperty(levelReference, "identifier").ValueKind != JsonValueKind.String)
                    {
                        throw new InvalidDataException(
                            $"LDtk world references missing level IID '{iid}'.");
                    }

                    level = ParseLevel(levelReference, projectDirectory, tilesetsByUid);
                    levelsByIid.Add(level.Iid, level);
                    allLevels.Add(level);
                }

                levels.Add(level);
            }

            string layout = GetNullableString(data, "worldLayout") ?? "Free";
            ApplyLinearLayout(levels, layout);
            worlds.Add(new LDtkModels.LdtkWorld
            {
                Identifier = GetString(data, "identifier"),
                Iid = GetString(data, "iid"),
                Layout = layout,
                GridWidth = GetInt(data, "worldGridWidth"),
                GridHeight = GetInt(data, "worldGridHeight"),
                Levels = levels.AsReadOnly()
            });
        }

        return worlds;
    }

    private static void ApplyLinearLayout(List<LDtkModels.LdtkLevel> levels, string layout)
    {
        float x = 0f;
        float y = 0f;
        foreach (LDtkModels.LdtkLevel level in levels)
        {
            if (layout == "LinearHorizontal")
            {
                level.WorldPosition = new Vector2(x, 0f);
                x += level.PixelWidth;
            }
            else if (layout == "LinearVertical")
            {
                level.WorldPosition = new Vector2(0f, y);
                y += level.PixelHeight;
            }
        }
    }

    private static JsonElement GetProperty(JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object &&
               element.TryGetProperty(name, out JsonElement property)
            ? property
            : default;
    }

    private static JsonElement GetArray(JsonElement element, string name)
    {
        JsonElement value = GetProperty(element, name);
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException($"Expected LDtk JSON array property '{name}'.");
        }

        return value;
    }

    private static string GetString(JsonElement element, string name)
    {
        JsonElement value = GetProperty(element, name);
        if (value.ValueKind != JsonValueKind.String)
        {
            throw new InvalidDataException($"Expected LDtk JSON string property '{name}'.");
        }

        return value.GetString()!;
    }

    private static string? GetNullableString(JsonElement element, string name)
    {
        JsonElement value = GetProperty(element, name);
        return value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
            ? null
            : value.GetString();
    }

    private static int GetInt(JsonElement element, string name)
    {
        JsonElement value = GetProperty(element, name);
        if (value.ValueKind != JsonValueKind.Number)
        {
            throw new InvalidDataException($"Expected LDtk JSON integer property '{name}'.");
        }

        return value.GetInt32();
    }

    private static int? GetNullableInt(JsonElement element, string name)
    {
        JsonElement value = GetProperty(element, name);
        return value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
            ? null
            : value.GetInt32();
    }

    private static float GetFloat(JsonElement element, string name, float defaultValue)
    {
        JsonElement value = GetProperty(element, name);
        return value.ValueKind == JsonValueKind.Number
            ? value.GetSingle()
            : defaultValue;
    }

    private static int[] GetIntArray(JsonElement element, string name, int expectedLength)
    {
        JsonElement value = GetProperty(element, name);
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException($"Expected LDtk JSON array property '{name}'.");
        }

        int[] result = new int[expectedLength];
        int index = 0;
        foreach (JsonElement item in value.EnumerateArray())
        {
            if (index == expectedLength)
            {
                throw new InvalidDataException(
                    $"LDtk JSON array property '{name}' has more than {expectedLength} values.");
            }

            result[index++] = item.GetInt32();
        }

        if (index != expectedLength)
        {
            throw new InvalidDataException(
                $"LDtk JSON array property '{name}' must have {expectedLength} values.");
        }

        return result;
    }

    private static Vector2 GetVector2(JsonElement element, string name)
    {
        JsonElement value = GetProperty(element, name);
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException($"Expected LDtk JSON array property '{name}'.");
        }

        System.Text.Json.JsonElement.ArrayEnumerator values = value.EnumerateArray();
        if (!values.MoveNext())
        {
            throw new InvalidDataException($"LDtk JSON array property '{name}' must have two values.");
        }

        float x = values.Current.GetSingle();
        if (!values.MoveNext())
        {
            throw new InvalidDataException($"LDtk JSON array property '{name}' must have two values.");
        }

        float y = values.Current.GetSingle();
        if (values.MoveNext())
        {
            throw new InvalidDataException($"LDtk JSON array property '{name}' has more than two values.");
        }

        return new Vector2(x, y);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    /// <summary>
    /// Unloads all textures loaded for drawing. The project can no longer be drawn afterwards,
    /// but level data stays readable.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (Texture2D texture in _textures.Values)
        {
            Raylib.UnloadTexture(texture);
        }

        _textures.Clear();
        _disposed = true;
    }
}