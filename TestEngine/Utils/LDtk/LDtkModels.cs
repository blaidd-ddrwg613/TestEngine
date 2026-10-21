using System.Numerics;
using System.Text.Json;

namespace TestEngine.Utils.LDtk;

public class LDtkModels
{
    /// <summary>
    /// A world within a multi-world LDtk project, containing a subset of the project's levels.
    /// </summary>
    public sealed class LdtkWorld
    {
        /// <summary>World identifier.</summary>
        public string Identifier { get; internal set; } = string.Empty;

        /// <summary>Unique instance ID of the world.</summary>
        public string Iid { get; internal set; } = string.Empty;

        /// <summary>World layout (Free, GridVania, LinearHorizontal or LinearVertical).</summary>
        public string Layout { get; internal set; } = string.Empty;

        /// <summary>Width of the world grid in pixels (GridVania layout).</summary>
        public int GridWidth { get; internal set; }

        /// <summary>Height of the world grid in pixels (GridVania layout).</summary>
        public int GridHeight { get; internal set; }

        /// <summary>Levels belonging to this world.</summary>
        public IReadOnlyList<LdtkLevel> Levels { get; internal set; } = Array.Empty<LdtkLevel>();
    }

    /// <summary>
    /// A single LDtk level, holding its layers, entities and custom fields.
    /// Entity and layer lookups are built lazily on first use and cached.
    /// </summary>
    public sealed class LdtkLevel
    {
        private IReadOnlyList<LdtkLayer> _layers = Array.Empty<LdtkLayer>();
        private Dictionary<string, LdtkLayer>? _layersByIdentifier;
        private Dictionary<string, List<LdtkEntity>>? _entitiesByIdentifier;
        private Dictionary<string, LdtkEntity>? _entitiesByIid;

        /// <summary>Level name as set in LDtk.</summary>
        public string Identifier { get; internal set; } = string.Empty;

        /// <summary>Unique instance ID of the level.</summary>
        public string Iid { get; internal set; } = string.Empty;

        /// <summary>Unique numeric ID of the level within the project.</summary>
        public int Uid { get; internal set; }

        /// <summary>Top-left pixel position of the level in world space.</summary>
        public Vector2 WorldPosition { get; internal set; }

        /// <summary>Depth of the level in the world, used to order overlapping levels.</summary>
        public int WorldDepth { get; internal set; }

        /// <summary>Level width in pixels.</summary>
        public int PixelWidth { get; internal set; }

        /// <summary>Level height in pixels.</summary>
        public int PixelHeight { get; internal set; }

        /// <summary>Background color as a hex string (for example "#40465B"), or null if unset.</summary>
        public string? BackgroundColor { get; internal set; }

        /// <summary>
        /// Layers in draw order (the first entry is drawn first). Setting this resets the cached lookups.
        /// </summary>
        public IReadOnlyList<LdtkLayer> Layers
        {
            get => _layers;
            internal set
            {
                _layers = value;
                _layersByIdentifier = null;
                _entitiesByIdentifier = null;
                _entitiesByIid = null;
            }
        }

        /// <summary>Custom level field values keyed by field identifier, as raw JSON.</summary>
        public IReadOnlyDictionary<string, JsonElement> Fields { get; internal set; } =
            new Dictionary<string, JsonElement>(StringComparer.Ordinal);

        /// <summary>Gets a layer by identifier, or null if the level has none.</summary>
        public LdtkLayer? GetLayer(string identifier)
        {
            if (_layersByIdentifier == null)
            {
                _layersByIdentifier = new Dictionary<string, LdtkLayer>(StringComparer.Ordinal);
                foreach (LdtkLayer layer in _layers)
                {
                    _layersByIdentifier.TryAdd(layer.Identifier, layer);
                }
            }

            return _layersByIdentifier.GetValueOrDefault(identifier);
        }

        /// <summary>Gets every entity in the level with the given identifier, across all layers.</summary>
        public IReadOnlyList<LdtkEntity> GetEntities(string identifier)
        {
            BuildEntityLookups();
            return _entitiesByIdentifier!.TryGetValue(identifier, out List<LdtkEntity>? entities)
                ? entities
                : Array.Empty<LdtkEntity>();
        }

        /// <summary>Gets the first entity with the given identifier, or null if none exists.</summary>
        public LdtkEntity? FindEntity(string identifier)
        {
            IReadOnlyList<LdtkEntity> entities = GetEntities(identifier);
            return entities.Count > 0 ? entities[0] : null;
        }

        /// <summary>Gets an entity by IID, or null if none exists.</summary>
        public LdtkEntity? FindEntityByIid(string iid)
        {
            BuildEntityLookups();
            return _entitiesByIid!.GetValueOrDefault(iid);
        }

        private void BuildEntityLookups()
        {
            if (_entitiesByIdentifier != null)
            {
                return;
            }

            Dictionary<string, List<LdtkEntity>> byIdentifier = new(StringComparer.Ordinal);
            Dictionary<string, LdtkEntity> byIid = new(StringComparer.Ordinal);
            foreach (LdtkLayer layer in _layers)
            {
                foreach (LdtkEntity entity in layer.Entities)
                {
                    if (!byIdentifier.TryGetValue(entity.Identifier, out List<LdtkEntity>? list))
                    {
                        list = new List<LdtkEntity>();
                        byIdentifier.Add(entity.Identifier, list);
                    }

                    list.Add(entity);
                    byIid.TryAdd(entity.Iid, entity);
                }
            }

            _entitiesByIid = byIid;
            _entitiesByIdentifier = byIdentifier;
        }
    }

    /// <summary>
    /// A layer of a level: tiles, auto-layer tiles, IntGrid values or entities, depending on <see cref="Type"/>.
    /// </summary>
    public sealed class LdtkLayer
    {
        /// <summary>Layer name as set in LDtk.</summary>
        public string Identifier { get; internal set; } = string.Empty;

        /// <summary>Unique instance ID of the layer.</summary>
        public string Iid { get; internal set; } = string.Empty;

        /// <summary>UID of the layer definition this layer was created from.</summary>
        public int LayerDefinitionUid { get; internal set; }

        /// <summary>Layer type: IntGrid, Entities, Tiles or AutoLayer.</summary>
        public string Type { get; internal set; } = string.Empty;

        /// <summary>Identifier of the level that owns this layer.</summary>
        public string LevelIdentifier { get; internal set; } = string.Empty;

        /// <summary>Layer width in grid cells.</summary>
        public int GridWidth { get; internal set; }

        /// <summary>Layer height in grid cells.</summary>
        public int GridHeight { get; internal set; }

        /// <summary>Size of one grid cell in pixels.</summary>
        public int GridSize { get; internal set; }

        /// <summary>Total pixel offset of the layer relative to the level origin.</summary>
        public Vector2 PixelOffset { get; internal set; }

        /// <summary>Layer opacity from 0 to 1.</summary>
        public float Opacity { get; internal set; } = 1f;

        /// <summary>Whether the layer is visible in LDtk.</summary>
        public bool Visible { get; internal set; } = true;

        /// <summary>UID of the tileset used by the layer, or null if it has none.</summary>
        public int? TilesetDefinitionUid { get; internal set; }

        /// <summary>IntGrid values in row-major order (index = row * GridWidth + column). 0 means empty.</summary>
        public IReadOnlyList<int> IntGridValues { get; internal set; } = Array.Empty<int>();

        /// <summary>Tiles to draw for tile and auto-layers.</summary>
        public IReadOnlyList<LdtkTile> Tiles { get; internal set; } = Array.Empty<LdtkTile>();

        /// <summary>Entities placed on this layer.</summary>
        public IReadOnlyList<LdtkEntity> Entities { get; internal set; } = Array.Empty<LdtkEntity>();

        /// <summary>Gets the tile drawn at a grid cell, or null if the cell has none.</summary>
        public LdtkTile? GetTileAt(int column, int row)
        {
            if (_tilesByCell is null)
            {
                _tilesByCell = new Dictionary<(int, int), LdtkTile>();
                foreach (LdtkTile tile in Tiles)
                {
                    _tilesByCell[((int)(tile.Position.X / GridSize), (int)(tile.Position.Y / GridSize))] = tile;
                }
            }

            return _tilesByCell.TryGetValue((column, row), out LdtkTile? found) ? found : null;
        }

        /// <summary>Converts a world position to a grid cell of this layer.</summary>
        /// <param name="worldPosition">Position in world space.</param>
        /// <param name="levelWorldPosition">The owning level's world position.</param>
        public (int Column, int Row) WorldToCell(Vector2 worldPosition, Vector2 levelWorldPosition)
        {
            Vector2 local = worldPosition - levelWorldPosition - PixelOffset;
            return ((int)MathF.Floor(local.X / GridSize), (int)MathF.Floor(local.Y / GridSize));
        }

        private Dictionary<(int, int), LdtkTile>? _tilesByCell;

        /// <summary>Gets the IntGrid value at a grid cell.</summary>
        /// <param name="column">Zero-based column.</param>
        /// <param name="row">Zero-based row.</param>
        /// <exception cref="ArgumentOutOfRangeException">The cell is outside the layer.</exception>
        /// <exception cref="InvalidOperationException">The layer holds no IntGrid data for the cell.</exception>
        public int GetIntGridValue(int column, int row)
        {
            if ((uint)column >= (uint)GridWidth)
            {
                throw new ArgumentOutOfRangeException(nameof(column));
            }

            if ((uint)row >= (uint)GridHeight)
            {
                throw new ArgumentOutOfRangeException(nameof(row));
            }

            int index = row * GridWidth + column;
            if (index >= IntGridValues.Count)
            {
                throw new InvalidOperationException(
                    $"Layer '{Identifier}' does not contain IntGrid data for ({column}, {row}).");
            }

            return IntGridValues[index];
        }
    }

    /// <summary>A single tile instance drawn from a tileset.</summary>
    public sealed class LdtkTile
    {
        /// <summary>Pixel position of the tile within its layer.</summary>
        public Vector2 Position { get; internal set; }

        /// <summary>Top-left pixel of the tile in the tileset image.</summary>
        public Vector2 SourcePosition { get; internal set; }

        /// <summary>Width of the source rectangle in pixels.</summary>
        public int SourceWidth { get; internal set; }

        /// <summary>Height of the source rectangle in pixels.</summary>
        public int SourceHeight { get; internal set; }

        /// <summary>Tile index within the tileset.</summary>
        public int TileId { get; internal set; }

        /// <summary>Switches this tile to another tile of the same tileset.</summary>
        public void SetTileId(int tileId, LdtkTilesetDefinition tileset)
        {
            int step = tileset.TileGridSize + tileset.Spacing;
            int columns = (tileset.PixelWidth - tileset.Padding * 2 + tileset.Spacing) / step;
            TileId = tileId;
            SourcePosition = new Vector2(
                tileset.Padding + tileId % columns * step,
                tileset.Padding + tileId / columns * step);
        }

        /// <summary>UID of the tileset the tile comes from.</summary>
        public int TilesetDefinitionUid { get; internal set; }

        /// <summary>Whether the tile is flipped horizontally.</summary>
        public bool FlipX { get; internal set; }

        /// <summary>Whether the tile is flipped vertically.</summary>
        public bool FlipY { get; internal set; }

        /// <summary>Tile opacity from 0 to 1.</summary>
        public float Alpha { get; internal set; } = 1f;
    }

    /// <summary>
    /// An entity instance placed in an LDtk entity layer, such as a spawn point or an enemy.
    /// </summary>
    public sealed class LdtkEntity
    {
        /// <summary>Entity type name as set in LDtk. Not unique within a level.</summary>
        public string Identifier { get; internal set; } = string.Empty;

        /// <summary>Unique instance ID of this entity.</summary>
        public string Iid { get; internal set; } = string.Empty;

        /// <summary>UID of the entity definition.</summary>
        public int DefinitionUid { get; internal set; }

        /// <summary>
        /// Pixel position of the entity's pivot within its layer, relative to the level origin.
        /// Add <see cref="LdtkLevel.WorldPosition"/> to get world space.
        /// </summary>
        public Vector2 Position { get; internal set; }

        /// <summary>Pivot as a 0-1 fraction of the entity size, for example (0.5, 0.5) is the center.</summary>
        public Vector2 Pivot { get; internal set; }

        /// <summary>Entity width in pixels.</summary>
        public int Width { get; internal set; }

        /// <summary>Entity height in pixels.</summary>
        public int Height { get; internal set; }

        /// <summary>Tags assigned to the entity definition in LDtk.</summary>
        public IReadOnlyList<string> Tags { get; internal set; } = Array.Empty<string>();

        /// <summary>Custom field values keyed by field identifier, as raw JSON.</summary>
        public IReadOnlyDictionary<string, JsonElement> Fields { get; internal set; } =
            new Dictionary<string, JsonElement>(StringComparer.Ordinal);

        /// <summary>Tile used to render the entity, or null if it has none.</summary>
        public LdtkEntityTile? Tile { get; internal set; }

        /// <summary>Tries to get the raw JSON value of a custom field.</summary>
        public bool TryGetField(string identifier, out JsonElement value)
        {
            return Fields.TryGetValue(identifier, out value);
        }

        /// <summary>Deserializes a custom field to <typeparamref name="T"/>.</summary>
        /// <exception cref="KeyNotFoundException">The entity has no field with that identifier.</exception>
        public T? GetField<T>(string identifier)
        {
            if (!Fields.TryGetValue(identifier, out JsonElement value))
            {
                throw new KeyNotFoundException(
                    $"Entity '{Identifier}' does not have a field named '{identifier}'.");
            }

            return value.Deserialize<T>();
        }
    }

    /// <summary>Tileset region used to render an entity.</summary>
    public sealed class LdtkEntityTile
    {
        /// <summary>UID of the tileset.</summary>
        public int TilesetDefinitionUid { get; internal set; }

        /// <summary>Top-left pixel of the region in the tileset image.</summary>
        public Vector2 SourcePosition { get; internal set; }

        /// <summary>Texture width in pixels.</summary>
        public int SourceWidth { get; internal set; }

        /// <summary>Texture height in pixels.</summary>
        public int SourceHeight { get; internal set; }
    }

    /// <summary>Definition of a tileset and its source image.</summary>
    public sealed class LdtkTilesetDefinition
    {
        /// <summary>Unique numeric ID of the tileset.</summary>
        public int Uid { get; internal set; }

        /// <summary>Tileset name as set in LDtk.</summary>
        public string Identifier { get; internal set; } = string.Empty;

        /// <summary>Image path relative to the .ldtk file, or null if the tileset has no image.</summary>
        public string? RelativePath { get; internal set; }

        /// <summary>Size of one tile in pixels.</summary>
        public int TileGridSize { get; internal set; }

        /// <summary>Image width in pixels.</summary>
        public int PixelWidth { get; internal set; }

        /// <summary>Image height in pixels.</summary>
        public int PixelHeight { get; internal set; }

        /// <summary>Pixels between tiles in the image.</summary>
        public int Spacing { get; internal set; }

        /// <summary>Pixels of padding around the image edge.</summary>
        public int Padding { get; internal set; }
    }

    /// <summary>Definition of a layer type shared by all levels.</summary>
    public sealed class LdtkLayerDefinition
    {
        /// <summary>Unique numeric ID of the layer definition.</summary>
        public int Uid { get; internal set; }

        /// <summary>Layer name as set in LDtk.</summary>
        public string Identifier { get; internal set; } = string.Empty;

        /// <summary>Layer type: IntGrid, Entities, Tiles or AutoLayer.</summary>
        public string Type { get; internal set; } = string.Empty;

        /// <summary>Size of one grid cell in pixels.</summary>
        public int GridSize { get; internal set; }

        /// <summary>IntGrid value definitions for this layer.</summary>
        public IReadOnlyList<LdtkIntGridValueDefinition> IntGridValues { get; internal set; } =
            Array.Empty<LdtkIntGridValueDefinition>();
    }

    /// <summary>Definition of one value an IntGrid layer can hold.</summary>
    public sealed class LdtkIntGridValueDefinition
    {
        /// <summary>Numeric value stored in the grid.</summary>
        public int Value { get; internal set; }

        /// <summary>Optional name given to the value in LDtk.</summary>
        public string? Identifier { get; internal set; }

        /// <summary>Editor color as a hex string.</summary>
        public string Color { get; internal set; } = string.Empty;
    }
}