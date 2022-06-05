using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Collapsenav.Net.Middleware.SimpleFileServer;

public static class FileServerExt
{
    // TODO 通过传入的 args 作为参数启动文件服务
    // public static void UseSFileServer(this IApplicationBuilder app, string[] args)
    // {
    // }

    public static void UseSFileServer(this IApplicationBuilder app, SFileServerConfig config)
    {
        // 使用原生 StaticFile
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(config.FileStore),
            RequestPath = new PathString(config.MapPath)
        });
        // 使用自己的 SFileServerMiddleware
        app.UseMiddleware<SFileServerMiddleware>(Options.Create(new SFileServerOptions
        {
            FileProvider = new PhysicalFileProvider(config.FileStore),
            RequestPath = new PathString(config.MapPath)
        }));
    }
}