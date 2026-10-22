using TestEngine.Utils;

namespace TestEngine.UI;

public class GuiManager
{
    private List<UiElement> _uiElementsList = new();

    public void Add(UiElement uiElement)
    {
        _uiElementsList.Add(uiElement);
    }

    public void Remove(UiElement uiElement)
    {
        _uiElementsList.Remove(uiElement);
    }

    public void Clear()
    {
        _uiElementsList.Clear();
    }

    public void Update(GameTime gameTime)
    {
        foreach (var element in _uiElementsList)
        {
            element.Update(gameTime);
        }
    }

    public void Draw()
    {
        foreach (var element in _uiElementsList)
        {
            element.Draw();
        }
    }

}