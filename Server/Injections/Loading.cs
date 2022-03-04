using System.Text.RegularExpressions;
namespace Server.Injections;

#nullable disable

public class Loading
{
    public string UploadingFolder { get; set; }

    /// <summary>
    /// При установке данного ограничения в builder.Services.Configure&lt;KestrelServerOptions&gt; в Program.cs<br/>
    /// при его превышении периодически создается ошибка 0: Response без заголовков.<br/>
    /// Создается ложное впечатление, что не заданы разрешения Cors.<br/>
    /// Поэтому в Program.cs данной параметр установлен в null.
    /// </summary>
    public long? MaxRequestBodySize { get; set; }

    public string[] AllowedNamesRegex { get; set; } = new string[0];

    public bool RegexIgnoreCase { get; set; } = true;

    private RegexOptions regexOptions => RegexIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;

    public bool NameAllowed(string name)
    {
        return AllowedNamesRegex.Length == 0
            || AllowedNamesRegex.Any(an => Regex.IsMatch(name, an, regexOptions));
    }
}
