namespace PokeAByte.Web.Services.Notifiers;

public class ChangeNotificationService
{
    public event Action OnChange;

    public void NotifyDataChanged() => OnChange?.Invoke();
}