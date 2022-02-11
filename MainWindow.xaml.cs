using QRCoder;
using QRCoder.Xaml;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Zack.ComObjectHelpers;

namespace Cs_ppt_controller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            RestoreUserConfig();
        }



        /// <summary>
        /// Click events also PPT manipulations
        /// </summary>

        private COMReferenceTracker com_ref = new COMReferenceTracker();
        private dynamic ppt_obj;
        private OpenFileDialog openFileDialog = new OpenFileDialog();



        string file_name = null;
        bool host_status = false;
        int page_num = 0;
        string ppt_note = null;
        int show_type = 0;

        dynamic http_service;
        dynamic ws_service;


        private void RestoreUserConfig()
        {
            try
            {
                show_type = Cs_ppt_controller.Properties.Settings.Default.show_type;

                switch (show_type)
                {
                    case 2:
                        normal_mode.IsChecked = true;
                        full_screen.IsChecked = false;
                        read_mode.IsChecked = false;
                        break;

                    case 3:
                        normal_mode.IsChecked = false;
                        full_screen.IsChecked = true;
                        read_mode.IsChecked = false;
                        break;

                    case 4:
                        normal_mode.IsChecked = false;
                        full_screen.IsChecked = false;
                        read_mode.IsChecked = true;
                        break;
                }
            }
            catch (Exception)
            {
                show_type = 2;
            }
        }

        // Com object tracker for Garbage Collect
        private dynamic Trace(dynamic comObj) => this.com_ref.T(comObj);

        // Clear Com Reference (GC)
        private void Clear_Com_Ref()
        {
            try
            {
                if (this.ppt_obj != null)
                {
                    Trace(this.ppt_obj.Application).Quit();
                }
            }
            catch (COMException ex)
            {
                Debug.WriteLine(ex);
            }
            this.com_ref.Dispose();
            this.com_ref = new COMReferenceTracker();

            // update status bar filepath
            status_path.Text = "None";
        }

        private string GetInnerText(dynamic part)
        {
            StringBuilder string_builder = new StringBuilder();
            dynamic shapes = Trace(Trace(part).Shapes);
            int shapesCount = shapes.Count;
            for (int i = 0; i < shapesCount; i++)
            {
                dynamic shape = Trace(shapes[i + 1]);
                var textFrame = Trace(shape.TextFrame);
                // MsoTriState.msoTrue==-1
                if (textFrame.HasText == -1)
                {
                    string text = Trace(textFrame.TextRange).Text;
                    string_builder.AppendLine(text);
                }
                string_builder.AppendLine();
            }
            return string_builder.ToString();
        }

        // open file
        private void Open_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // filter config for open window
            openFileDialog.Filter = "PowerPoint file|*.ppt;*.pptx;*.pptm";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                file_name = openFileDialog.FileName;

                // clear memory
                Clear_Com_Ref();

                // open PPT app
                dynamic ppt_app = Trace(PowerPointHelper.CreatePowerPointApplication());
                ppt_app.Visible = true;
                dynamic Presentations = Trace(ppt_app.Presentations);

                // create PPT object and open PPT file
                ppt_obj = Trace(Presentations.Open(file_name));
                Trace(ppt_obj.SlideShowSettings).ShowType = show_type;
                //Trace(ppt_obj.SlideShowSettings).AdvanceMode = 2;
                Trace(ppt_obj.SlideShowSettings).Run();

                // update status bar filepath
                status_path.Text = file_name;

                dynamic ppt_page = Trace(Trace(Trace(Trace(ppt_obj.SlideShowWindow).view).Slide).NotesPage);

                page_num = Trace(Trace(Trace(ppt_obj.SlideShowWindow).view).Slide).SlideIndex;
                page_num_text_box.Text = "Page: " + page_num.ToString();

                ppt_note = GetInnerText(ppt_page);

                try
                {
                    ppt_note = ppt_note.Split(new[] { "\r\n\r\n\r\n" }, StringSplitOptions.None)[1];
                }
                catch (Exception em)
                {
                    Console.WriteLine("page note read fail." + em);
                    ppt_note = "";
                }
                page_note_text_box.Text = ppt_note;
            }
            else
            {
                return;
            }

        }

        // host web
        private void Host_Web_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!host_status)
                {
                    http_service = new HttpService();
                    //http_service = new HttpServer();
                    ws_service = new WebSocket(ppt_obj, com_ref, page_num_text_box);

                    string ip_address = GetLocalIPAddress();

                    address_text_box.Text = "http://" + ip_address + ":3000/" + http_service.GetHttpAddress();

                    host_status = true;
                    System.Windows.MessageBox.Show("Succeeded", "Web Host", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {

                    ws_service.Close();

                    http_service.Close();

                    address_text_box.Text = "Host Closed";

                    host_status = false;
                }
            }
            catch (Exception em)
            {
                Debug.WriteLine(em);
            }
        }

        private void Exit_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // clear memory
            Clear_Com_Ref();

            System.Windows.Application.Current.Shutdown();
        }

        private void Next_Page_Button_Click(object sender, RoutedEventArgs e)
        {
            Next_Page();
        }

        public void Next_Page()
        {
            try
            {
                Trace(Trace(ppt_obj.SlideShowWindow).view).Next();

                page_num = Trace(Trace(Trace(ppt_obj.SlideShowWindow).view).Slide).SlideIndex;

                page_num_text_box.Text = "Page: " + page_num.ToString();

                dynamic ppt_page = Trace(Trace(Trace(Trace(ppt_obj.SlideShowWindow).view).Slide).NotesPage);
                ppt_note = GetInnerText(ppt_page);
                try
                {
                    ppt_note = ppt_note.Split(new[] { "\r\n\r\n\r\n" }, StringSplitOptions.None)[1];
                }
                catch (Exception em)
                {
                    Console.WriteLine("page note read fail." + em);
                    ppt_note = "";
                }
                page_note_text_box.Text = ppt_note;
            }
            catch (Exception)
            {
                Console.WriteLine("Next page error.");
            }
        }

        private void Pre_Page_Button_Click(object sender, RoutedEventArgs e)
        {
            Pre_Page();
        }

        public void Pre_Page()
        {
            try
            {
                if (page_num > 1)
                {
                    Trace(Trace(ppt_obj.SlideShowWindow).view).Previous();

                    page_num = Trace(Trace(Trace(ppt_obj.SlideShowWindow).view).Slide).SlideIndex;
                    page_num_text_box.Text = "Page: " + page_num.ToString();

                    dynamic ppt_page = Trace(Trace(Trace(Trace(ppt_obj.SlideShowWindow).view).Slide).NotesPage);
                    ppt_note = GetInnerText(ppt_page);

                    try
                    {
                        ppt_note = ppt_note.Split(new[] { "\r\n\r\n\r\n" }, StringSplitOptions.None)[1];
                    }
                    catch (Exception em)
                    {
                        Console.WriteLine("page note read fail." + em);
                        ppt_note = "";
                    }
                    
                    page_note_text_box.Text = ppt_note;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Previous page error.");
            }
        }

        private void normal_pres_setting_MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            normal_mode.IsChecked = true;
            full_screen.IsChecked = false;
            read_mode.IsChecked = false;

            show_type = 2;
            Cs_ppt_controller.Properties.Settings.Default.show_type = 2;
            Cs_ppt_controller.Properties.Settings.Default.Save();
        }

        private void full_pres_setting_MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            normal_mode.IsChecked = false;
            full_screen.IsChecked = true;
            read_mode.IsChecked = false;

            show_type = 3;
            Cs_ppt_controller.Properties.Settings.Default.show_type = 3;
            Cs_ppt_controller.Properties.Settings.Default.Save();
        }

        private void read_pres_setting_MenuItem_Checked(object sender, RoutedEventArgs e)
        {
            normal_mode.IsChecked = false;
            full_screen.IsChecked = false;
            read_mode.IsChecked = true;

            show_type = 4;
            Cs_ppt_controller.Properties.Settings.Default.show_type = 4;
            Cs_ppt_controller.Properties.Settings.Default.Save();
        }


        private void QR_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(address_text_box.Text, QRCodeGenerator.ECCLevel.Q);
                XamlQRCode qrCode = new XamlQRCode(qrCodeData);
                DrawingImage qrCodeAsXaml = qrCode.GetGraphic(20);
                qr_image.Source = qrCodeAsXaml;
            }
            catch (Exception em)
            {
                Debug.WriteLine("Qr code generation error: " + em);
            }
        }


        private void refresh_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                page_num = Trace(Trace(Trace(ppt_obj.SlideShowWindow).view).Slide).SlideIndex;

                page_num_text_box.Text = "Page: " + page_num.ToString();

                dynamic ppt_page = Trace(Trace(Trace(Trace(ppt_obj.SlideShowWindow).view).Slide).NotesPage);
                ppt_note = GetInnerText(ppt_page);
                try
                {
                    ppt_note = ppt_note.Split(new[] { "\r\n\r\n\r\n" }, StringSplitOptions.None)[1];
                }
                catch (Exception em)
                {
                    Console.WriteLine("page note read fail." + em);
                    ppt_note = "";
                }
                page_note_text_box.Text = ppt_note;
            }
            catch (Exception em)
            {
                Console.WriteLine("Refresh error." + em);
            }
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                IPInterfaceProperties properti = adapter.GetIPProperties();
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
                //Console.WriteLine(properties.HostName);//properties.DnsSuffix);
            }
            /*
            foreach (var ip in host.AddressList)
            {
                //Console.WriteLine("host" + IPGlobalProperties.GetIPGlobalProperties().DomainName);
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }

            }*/
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    IPInterfaceProperties properties = ni.GetIPProperties();
                    //Console.WriteLine(properties.DnsSuffix);
                    //Console.WriteLine(ni.Name);
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            //Console.WriteLine(ip.Address.ToString());
                            if (properties.DnsSuffix == "lan")
                            {
                                return ip.Address.ToString();
                            }
                        }
                    }
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }


    }



}
