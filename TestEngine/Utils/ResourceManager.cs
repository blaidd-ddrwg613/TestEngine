using Raylib_cs;

namespace TestEngine.Utils;

public class ResourceManager
{
    private static readonly Dictionary<string, Texture2D> _textures = new();

    private static readonly Dictionary<string, Image> _images = new();

    private static readonly Dictionary<string, Font> _fonts = new();

    private static readonly Dictionary<string, Shader> _shaders = new();

    private static readonly Dictionary<string, Sound> _sounds = new();

    private static readonly Dictionary<string, Music> _music = new();

    private static string _resourceFolderPath = "resources/";
    
    
    /// <summary>
    /// Load a Texture file into GPU memory.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Texture2D LoadTexture(string path)
    {
        if (!_textures.TryGetValue(path, out var texture))
        {
            texture = Raylib.LoadTexture(Path.Combine(_resourceFolderPath, path));
            _textures[path] = texture;
        }
        return texture;
    }

    /// <summary>
    /// Load an Image into CPU memory.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Image LoadImage(string path)
    {
        if (!_images.TryGetValue(path, out var image))
        {
            image = Raylib.LoadImage(Path.Combine(_resourceFolderPath, path));
            _images[path] = image;
        }

        return image;
    }

    /// <summary>
    /// Load shader from files and bind default locations.
    /// </summary>
    /// <param name="name">Shader Name</param>
    /// <param name="vsPath">Vertex Shader Path.</param>
    /// <param name="fsPath">Fragment Shader Path.</param>
    /// <returns></returns>
    public static Shader LoadShader(string name, string vsPath, string fsPath)
    {
        if (!_shaders.TryGetValue(name, out var shader))
        {
            shader = Raylib.LoadShader(Path.Combine(_resourceFolderPath, vsPath),
                Path.Combine(_resourceFolderPath, fsPath));
            _shaders[name] = shader;
        }

        return shader;
    }

    /// <summary>
    /// Load a Sound from a file.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Sound LoadSound(string path)
    {
        if (!_sounds.TryGetValue(path, out var sound))
        {
            sound = Raylib.LoadSound(Path.Combine(_resourceFolderPath, path));
            _sounds[path] = sound;
        }

        return sound;
    }

    /// <summary>
    /// Load a sound file from a WAV file.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Sound LoadWaveSound(string path)
    {
        if (!_sounds.TryGetValue(path, out var sound))
        {
            var temp = Raylib.LoadWave(Path.Combine(_resourceFolderPath, path));
            sound = Raylib.LoadSoundFromWave(temp);
            _sounds[path] = sound;
        }

        return sound;
    }

    /// <summary>
    /// Load Music Stream from a file.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Music LoadMusic(string path)
    {
        if (!_music.TryGetValue(path, out var music))
        {
            music = Raylib.LoadMusicStream(Path.Combine(_resourceFolderPath, path));
            _music[path] = music;
        }

        return music;
    }

    /// <summary>
    /// Load font file into GPU Memory
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static Font LoadFont(string path)
    {
        if (!_fonts.TryGetValue(path, out var font))
        {
            font = Raylib.LoadFont(Path.Combine(_resourceFolderPath, path));
            _fonts[path] = font;
        }

        return font;
    }
    

    public static void UnloadAll()
    {
        foreach (var texture in _textures.Values) Raylib.UnloadTexture(texture);
        _textures.Clear();

        foreach (var font in _fonts.Values) Raylib.UnloadFont(font);
        _fonts.Clear();

        foreach (var shader in _shaders.Values) Raylib.UnloadShader(shader);
        _shaders.Clear();

        foreach (var sound in _sounds.Values) Raylib.UnloadSound(sound);
        _sounds.Clear();

        foreach (var music in _music.Values) Raylib.UnloadMusicStream(music);
        _music.Clear();

        foreach (var image in _images.Values) Raylib.UnloadImage(image);
        _images.Clear();
    }

    /// <summary>
    /// Change the resource path location, Default is "GAME_FOLDER/resources/"
    /// </summary>
    /// <param name="path"></param>
    public static void SetResourcePath(string path)
    {
        _resourceFolderPath = path;
    }
}