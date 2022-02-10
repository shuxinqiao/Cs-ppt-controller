using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cs_ppt_controller
{
    class HttpService
    {
        private const int con_current_count = 20;

        // Server address
        private const string server_url = "http://localhost:3000/";

        dynamic server;

        public HttpService()
        {
            server = new WebServer(con_current_count);

            server.Bind(server_url);
            server.Start();

            Console.WriteLine($"Web server started at {server_url}");
        }

        public string GetHttpAddress()
        {
            return server_url + "webapp.html";
        }

        public void Close()
        {
            server.Close();
        }
    }
}
