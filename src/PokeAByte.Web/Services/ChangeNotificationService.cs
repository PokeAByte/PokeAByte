namespace PokeAByte.Web.Services;

public class ChangeNotificationService
{
    public event Action OnChange;

    public void NotifyDataChanged() => OnChange?.Invoke();
}