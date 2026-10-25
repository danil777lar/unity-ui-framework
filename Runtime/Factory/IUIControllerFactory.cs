namespace Larje.UI
{
    public interface IUIControllerFactory
    {
        UIObject Create(string key);
    }
}
