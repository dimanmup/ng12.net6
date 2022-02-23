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
}
