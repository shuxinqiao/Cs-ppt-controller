using System;

namespace Cs_ppt_controller
{
    class HttpService
    {
        private const int con_current_count = 20;

        //netsh http add urlacl url=http://+:80/MyUri user=DOMAIN\user
        // Server address
        private const string server_url = "http://*:3000/";

        dynamic server;

        public HttpService()
        {
            //System.Diagnostics.Process.Start("netsh.exe", "http add urlacl url=http://*:3000/ user=Everyone");
            server = new WebServer(con_current_count);

            server.Bind(server_url);
            server.Start();

            Console.WriteLine($"Web server started at {server_url}");
        }

        public string GetHttpAddress()
        {
            return "webapp.html";
        }

        public void Close()
        {
            server.Close();
        }
    }
}
