using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace Collapsenav.Net.Middleware.SimpleFileServer;

/// <summary>
/// 提供页面和对应的js请求处理
/// 除了第一次请求页面
/// 之后的操作都靠js完成
/// 通过请求数据而不是html页面
/// 可以减小(微乎其微)带宽压力
/// </summary>
public class SFileServerFormatter : IDirectoryFormatter
{

    private string Html = """
<html lang="en">

<head>
    <meta charset="UTF-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>FileServer</title>

    <script>
        var http = {};
        http.quest = function (option, callback) {
            var url = option.url;
            var method = option.method;
            var data = option.data;
            var timeout = option.timeout || 0;
            var xhr = new XMLHttpRequest();
            (timeout > 0) && (xhr.timeout = timeout);
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4) {
                    if (xhr.status >= 200 && xhr.status < 400) {
                        var result = xhr.responseText;
                        try { result = JSON.parse(xhr.responseText); } catch (e) { }
                        callback && callback(null, result);
                    } else {
                        callback && callback('status: ' + xhr.status);
                    }
                }
            }.bind(this);
            xhr.open(method, url, true);
            if (typeof data === 'object') {
                try {
                    data = JSON.stringify(data);
                } catch (e) { }
            }
            xhr.send(data);
            xhr.ontimeout = function () {
                callback && callback('timeout');
            };
        };
        http.get = function (url, callback) {
            var option = url.url ? url : { url: url };
            option.method = 'get';
            this.quest(option, callback);
        };
        http.post = function (option, callback) {
            option.method = 'post';
            this.quest(option, callback);
        };

        var origin = window.location.origin;
        origin = 'http://localhost:5104';

        var currentFiles = [];
        function getFiles (path) {
            http.get(`${origin}/${path ?? ''}?flag=true`, (_, result) => {
                currentFiles = result;
                if (currentFiles && Array.isArray(currentFiles)) {
                    genTable(currentFiles);
                }
            });
        }

        function genTable (files) {
            let table = document.getElementById('tablebody');
            table.innerHTML = '';
            files.forEach(file => {
                let tr = document.createElement('tr');
                let nametd = document.createElement('td');
                let sizetd = document.createElement('td');
                let lasttd = document.createElement('td');
                let ahref = document.createElement('a');
                let filepath = origin + '/' + file.name;
                ahref.text = file.name;
                ahref.href = filepath;
                ahref.download = file.name;
                sizetd.innerText = file.length;
                lasttd.innerText = file.lastModified;
                nametd.appendChild(ahref);
                tr.appendChild(nametd);
                tr.appendChild(sizetd);
                tr.appendChild(lasttd);
                table.appendChild(tr);
            });
        }
        getFiles();


    </script>
</head>

<body>
    <div>
        <h1>Title</h1>
    </div>

    <table>
        <thead>
            <th>Name</th>
            <th>Size</th>
            <th>CreateTime</th>
        </thead>
        <tbody id="tablebody">
            <!-- <tr>
                <td></td>
                <td></td>
            </tr> -->
        </tbody>
    </table>
</body>

</html>
""";
    public Task GenerateContentAsync(HttpContext context, IEnumerable<IFileInfo> contents)
    {
        return context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes(Html), 0, Html.Length);
    }
}