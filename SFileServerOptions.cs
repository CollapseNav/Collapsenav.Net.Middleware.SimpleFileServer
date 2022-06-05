using Microsoft.AspNetCore.Builder;

namespace Collapsenav.Net.Middleware.SimpleFileServer;

public class SFileServerOptions : DirectoryBrowserOptions
{
    // TODO 后续可能会有其他的配置参数
}


public class SFileServerConfig
{
    /// <summary>
    /// 文件存储位置
    /// </summary>
    public string FileStore { get; set; }
    /// <summary>
    /// 匹配路径
    /// </summary>
    public string MapPath { get; set; }
}