using Collapsenav.Net.Tool;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Collapsenav.Net.Middleware.SimpleFileServer;
public class SFileServerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SFileServerOptions _options;
    private readonly IFileProvider _provider;
    public SFileServerMiddleware(RequestDelegate next, IOptions<SFileServerOptions> options)
    {
        _options = options.Value;
        _next = next;
        _provider = _options.FileProvider;
    }

    public Task Invoke(HttpContext context)
    {
        var path = context.Request.Path;
        var querys = context.Request.Query;
        var method = context.Request.Method;
        // 只在get&&head请求时进行匹配
        if (!HttpMethods.IsGet(method) && !HttpMethods.IsHead(method))
            return _next(context);

        // 获取数据的请求都会打上 flag 标记
        // TODO 添加 实际使用中需要注意的 XXX种 判断
        if (querys.ContainsKey("flag"))
        {
            var (ismatch, files) = GetMatchFiles(path, _options.RequestPath);
            // 需处理files为空的情况
            if (files != null && files.Any())
            {
            }
            else
            {
                return context.Response.Body.WriteAsync(null, 0, 0);
            }
            var result = files.Select(item => new
            {
                item.Name,
                item.Length,
                item.IsDirectory,
                item.LastModified,
            }).ToList();
            // TODO　暂时使用 Collapsenav.Net.Tool 处理的json数据,后期考虑手动拼接json数据,减少不必要的多余的依赖
            var bytes = result.ToJson().ToBytes();
            return context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
        }
        // 没有flag标记时则返回页面
        else
        {
            return new SFileServerFormatter().GenerateContentAsync(context, null);
        }
    }

    public (bool, IEnumerable<IFileInfo>) GetMatchFiles(PathString path, PathString target)
    {
        // TODO 需要根据配置的 RequestPath(target) 调整匹配策略
        if (path.Value.EndsWith("/"))
            path += new PathString("/");
        return (true, _provider.GetDirectoryContents(path));
    }
}