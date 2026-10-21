using Raylib_cs;

namespace TestEngine.Graphics;

public class Tileset
{
    private readonly List<TextureRegion> _tiles;

    /// <summary>
    /// Gets the width, in pixels, of each tile in this tileset.
    /// </summary>
    public int TileWidth { get; }

    /// <summary>
    /// Gets the height, in pixels, of each tile in this tileset.
    /// </summary>
    public int TileHeight { get; }

    /// <summary>
    /// Gets the total number of columns in this tileset.
    /// </summary>
    public int Columns { get; }

    /// <summary>
    /// Gets the total number of rows in this tileset.
    /// </summary>
    public int Rows { get; }

    /// <summary>
    /// Gets the total number of tiles in this tileset.
    /// </summary>
    public int Count { get; }

    public Tileset(TextureRegion textureRegion, int tileWidth, int tileHeight)
    {
        TileWidth = tileWidth;
        TileHeight = tileHeight;

        Columns = (int)textureRegion.Width / tileWidth;
        Rows = (int)textureRegion.Height / tileHeight;
        Count = Columns * Rows;

        _tiles = new List<TextureRegion>(Count);

        for (int i = 0; i < Count; i++)
        {
            int x = (int)textureRegion.Source.X + (i % Columns * tileWidth);
            int y = (int)textureRegion.Source.Y + (i / Columns * tileHeight);

            _tiles.Add(new TextureRegion(
                textureRegion.Texture,
                new Rectangle(x, y, TileWidth, TileHeight)
            ));
        }
    }
    
    public TextureRegion GetTile(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index),
                $"Tile index {index} is out of range. " + 
                $"({Columns} columns x {Rows} rows)."
            );
        }

        return _tiles[index];
    }

    /// <summary>
    /// Gets the texture region for the tile from this tileset at the given location.
    /// </summary>
    /// <param name="column">The column in this tileset of the texture region.</param>
    /// <param name="row">The row in this tileset of the texture region.</param>
    /// <returns>The texture region for the tile from this tileset at given location.</returns>
    public TextureRegion GetTile(int column, int row)
    {
        int index = row * Columns + column;
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index),
                $"Tile index {index} is out of range. " +
                $"({Columns} columns x {Rows} rows)."
            );
        }
        return GetTile(index);
    }

    public List<TextureRegion> GetTileList()
    {
        return _tiles;
    }

    public void Clear()
    {
        _tiles.Clear();
    }
}