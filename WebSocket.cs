using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zack.ComObjectHelpers;


namespace Cs_ppt_controller
{
    class WebSocket
    {
        List<IWebSocketConnection> socket_list;
        WebSocketServer server;
        int page_num = 0;


        public WebSocket(dynamic ppt_obj, dynamic com_ref, dynamic page_num_text_box)
        {
            socket_list = new List<IWebSocketConnection>();
            server = new WebSocketServer("ws://127.0.0.1:3001");
            dynamic com_ref_ws = com_ref;
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Socket Open");
                    socket_list.Add(socket);
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("Socket Close");
                    socket_list.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    Console.WriteLine("Client: " + message);
                    string[] mes_combine = Detect(ppt_obj, com_ref, message, page_num_text_box);
                    
                    if (mes_combine.Length == 2){ 
                        socket_list.ToList().ForEach(s => s.Send("?PAGENUM=" + mes_combine[0]));
                        socket_list.ToList().ForEach(s => s.Send("?PAGENOTE=" + mes_combine[1]));
                    }
                    Console.WriteLine("Num and Note sent.");
                };
            });

        }

        private dynamic Trace(dynamic comObj, dynamic com_ref) => com_ref.T(comObj);

        private string GetInnerText(dynamic part, dynamic com_ref)
        {
            StringBuilder string_builder = new StringBuilder();
            dynamic shapes = Trace(Trace(part, com_ref).Shapes, com_ref);
            int shapesCount = shapes.Count;
            for (int i = 0; i < shapesCount; i++)
            {
                dynamic shape = Trace(shapes[i + 1], com_ref);
                var textFrame = Trace(shape.TextFrame, com_ref);
                // MsoTriState.msoTrue==-1
                if (textFrame.HasText == -1)
                {
                    string text = Trace(textFrame.TextRange, com_ref).Text;
                    string_builder.AppendLine(text);
                }
                string_builder.AppendLine();
            }
            return string_builder.ToString();
        }

        private string[] Detect(dynamic ppt_obj, dynamic com_ref, string message, dynamic page_num_text_box)
        {
            string[] mes_combine;

            if (message.Contains("?NEXTPAGE"))
            {
                try
                {
                    Trace(Trace(ppt_obj.SlideShowWindow, com_ref).view, com_ref).Next();
                    
                    page_num = Trace(Trace(Trace(ppt_obj.SlideShowWindow, com_ref).view, com_ref).Slide, com_ref).SlideIndex;
                    //page_num_text_box.Text = "Page: " + page_num.ToString();
                    
                    dynamic ppt_page = Trace(Trace(Trace(Trace(ppt_obj.SlideShowWindow, com_ref).view, com_ref).Slide, com_ref).NotesPage, com_ref);
                    var ppt_note = GetInnerText(ppt_page, com_ref);
                    ppt_note = ppt_note.Split(new[] { "\r\n\r\n\r\n" }, StringSplitOptions.None)[1];

                    mes_combine = new string[] { page_num.ToString(), ppt_note };

                    return mes_combine;
                }
                catch (Exception em)
                {
                    Console.WriteLine("Next page error." + em);
                }
            }
            else if (message.Contains("?PREPAGE"))
            {
                try
                {
                    if (page_num > 1)
                    {
                        Trace(Trace(ppt_obj.SlideShowWindow, com_ref).view, com_ref).Previous();

                        page_num = Trace(Trace(Trace(ppt_obj.SlideShowWindow, com_ref).view, com_ref).Slide, com_ref).SlideIndex;
                        //page_num_text_box.Text = "Page: " + page_num.ToString();

                        dynamic ppt_page = Trace(Trace(Trace(Trace(ppt_obj.SlideShowWindow, com_ref).view, com_ref).Slide, com_ref).NotesPage, com_ref);
                        var ppt_note = GetInnerText(ppt_page, com_ref);
                        ppt_note = ppt_note.Split(new[] { "\r\n\r\n\r\n" }, StringSplitOptions.None)[1];
                        mes_combine = new string[] { page_num.ToString(), ppt_note };

                        return mes_combine;
                    }
                }
                catch (Exception em)
                {
                    Console.WriteLine("Previous page error." + em);
                }
            }

            mes_combine = new string[] { "NULL", "NULL" };
            return mes_combine;

        }


        public void Close()
        {
            try
            {
                foreach (var client in socket_list)
                {
                    client.Close();
                }
                server.Dispose();
            }
            catch (Exception em)
            {
                Console.WriteLine("WebSocket close error." + em);
            }
        }
    }
}
