using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cs_ppt_controller
{
    public class WebServer
    {
        private readonly Semaphore _sem;

        private readonly HttpListener _listener;

        public WebServer(int con_current_count)
        {
            _sem = new Semaphore(con_current_count, con_current_count);
            _listener = new HttpListener();
        }

        public void Bind(string url)
        {
            _listener.Prefixes.Add(url);
        }

        public void Start()
        {
            _listener.Start();

            Task.Run(async () =>
            {
                while (true)
                {
                    _sem.WaitOne();
                    var content = await _listener.GetContextAsync();
                    _sem.Release();
                    HandleRequest(content);
                }
            });
        }

        private void HandleRequest(HttpListenerContext content)
        {
            var request = content.Request;
            var response = content.Response;
            var url_path = request.Url.LocalPath.TrimStart('/');
            Console.WriteLine($"url path={url_path}");

            try
            {
                string file_path = Path.Combine("WebApp", url_path);
                byte[] data = File.ReadAllBytes(file_path);
                response.ContentType = "text/html";
                response.ContentLength64 = data.Length;
                response.ContentEncoding = Encoding.UTF8;
                response.StatusCode = 200;
                response.OutputStream.Write(data, 0, data.Length);
                response.OutputStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine(e.StackTrace);
            }
        }

        public void Close()
        {
            _listener.Close();
        }
    }
}
