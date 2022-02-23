namespace Server.Injections;

/// <summary>
/// Инъекция с параметрами из флагов команды запуска.<br/>
/// Синтаксис:
/// <code>name[=value]?</code>
/// Пример<br/>
/// В Client на сервере разработки (localhost с другим портом) могут прилететь только куки
/// со специальными опциями: Secure=True, SameSite=None,
/// задаваемыми параметром команды запуска cookie-for-dev.
/// </summary>
public class Settings
{
    public readonly KeyValuePair<string, string?>[] Args;

    public Settings(params string[] args)
    {
        Args = args
            .Select(a => {
                string[] kv = a.Split('=', 2);
                return kv.Length == 1 
                    ? new KeyValuePair<string, string?>(kv[0], null)
                    : new KeyValuePair<string, string?>(kv[0], kv[1]);
            })
            .ToArray();
    }
}