using System;
using System.Text;
//using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Collections;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Net.Http;
using Share;
//using System.Text.Encodings.Web;
//using System.Text.Json.Serialization;
//using System.Text.Json;
using Yandex.Checkout.V3;
using System.Web;
using static System.Net.Mime.MediaTypeNames;

namespace Mafia_Server
{
    public class WebServer
    {
        private TcpListener Listener;

        private Task listenerTask;

        public WebServer(int port)
        {
            IPAddress localIP = IPAddress.Parse("192.168.1.240");
            Listener = new TcpListener(localIP, port);

            Listener.Start();

            listenerTask =  Task.Run(() => Listen());
        }

        public void Stop()
        {
            if(Listener!=null) Listener.Stop();
            if(listenerTask!=null) listenerTask.Dispose();
        }

        public void Listen()
        {
            while (true) CheckTCPListener();
        }

        private void CheckTCPListener()
        {
            var TCPClient = Listener.AcceptTcpClient();

            try
            {
                //byte[] buffer = new byte[1024];
                //string request = "";

                //int count = TCPClient.GetStream().Read(buffer, 0, 1024);

                //request += Encoding.ASCII.GetString(buffer, 0, count);

                //Logger.Log.Debug($"web request {request}");

                Message message = Yandex.Checkout.V3.Client.ParseMessage(
                    "GET", "application/json", TCPClient.GetStream());

                Payment payment = message?.Object;

                if (message?.Event == Event.PaymentWaitingForCapture && payment.Paid)
                {
                    Logger.Log.Debug($"Got message: payment.id={payment.Id}, payment.paid={payment.Paid}");

                    // 4. Подтвердите готовность принять платеж
                   UMoney.client.CapturePayment(payment.Id);
                }
            }
            catch (Exception exception)
            {
                Logger.Log.Debug("CheckTCPListener: " + exception.ToString());
            }


        }


            //string[] lines = request.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            //foreach (var L in lines)
            //{
            //    var content = new StringContent(L, Encoding.UTF8);
            //    Logger.Log.Debug($"request Lines => {content}");

            //    if (L.Contains("app_id"))
            //    {
            //        //еслис запрос содержит инфо о покупке
            //        //обрабатываем его и создаем ответ
            //        //CreateResponse(L, TCPClient);
            //    }
            //}

            //if (request.IndexOf("\r\n\r\n") >= 0) // Запрос обрывается \r\n\r\n последовательностью
            //{
            //    //Logger.Log.Debug($"web request {request}");

            //    //string[] lines = theText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            //    //string[] lines = request.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            //    //var Params = request.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            //    //Params = Params[1].Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

            //    //foreach (var P in Params)
            //    //{
            //    //    Logger.Log.Debug($"web request => {P}");
            //    //}

            //    //WriteToClient(request);
            //    //request = "";
            //}

        private void CreateResponse(string Request,TcpClient TCPClient)
        {
            var Result = Request.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

            var AppId = "";
            var ItemName = "";
            var ItemId = "";
            var NotificationType = "";
            var OrderId = "";
            var RecieverId = "";
            var UserId = "";
            var Sig = "";
            var Status = "";
            var ItemPrice = "";
            var Date = "";

            foreach (var R in Result)
            {
                var Parameters = R.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                //Logger.Log.Debug($"{R}");

                switch (Parameters[0])
                {
                    case "app_id":
                        {
                            AppId = Parameters[1];
                            //Logger.Log.Debug($"parsed parameter => App_Id {App_Id}");
                        }
                        break;

                    case "item":
                        {
                            ItemName = Parameters[1];
                            //Logger.Log.Debug($"parsed parameter => ItemName {ItemName}");
                        }
                        break;

                    case "item_id":
                        {
                            ItemId = Parameters[1];
                            //Logger.Log.Debug($"parsed parameter => ItemId {ItemId}");
                        }
                        break;

                    case "notification_type":
                        {
                            NotificationType = Parameters[1];
                            //Logger.Log.Debug($"parsed parameter => Notification_Type {Notification_Type}");
                        }
                        break;

                    case "order_id":
                        {
                            OrderId = Parameters[1];
                            //Logger.Log.Debug($"parsed parameter => Order_Id {Order_Id}");
                        }
                        break;

                    case "receiver_id":
                        {
                            RecieverId = Parameters[1];
                            //Logger.Log.Debug($"parsed parameter => Reciever_Id {Reciever_Id}");
                        }
                        break;

                    case "user_id":
                        {
                            UserId = Parameters[1];
                            //Logger.Log.Debug($"parsed parameter => User_Id {User_Id}");
                        }
                        break;

                    case "sig":
                        {
                            Sig = Parameters[1];
                            //Logger.Log.Debug($"parsed parameter => Sig {Sig}");
                        }
                        break;
                    case "status":
                        {
                            Status = Parameters[1];
                            //Logger.Log.Debug($"parsed parameter => Status {Status}");
                        }
                        break;

                    case "item_price":
                        {
                            ItemPrice = Parameters[1];
                            //Logger.Log.Debug($"parsed parameter => ItemPrice {ItemPrice}");
                        }
                        break;

                    case "date":
                        {
                            Date = Parameters[1];
                            //Logger.Log.Debug($"parsed parameter => Date {Date}");
                        }
                        break;                       

                        //lang = ru_RU

                }
            }

            //var Header = "Content-Type: application/json; encoding=utf-8";
            var SecretKey = "LZLmdRE6Re2alQHm4oSl"; // Защищенный ключ приложения

            var Response = "";

            if(Sig == CreateMD5(SecretKey))
            {
                Logger.Log.Debug("Sig != SecretKey");
                Response = @"{""error"":{"+
                           @"""error_code"":10," +
                           @"""error_msg"":""Несовпадение вычисленной и переданной подписи запроса.""," +
                           @"""critical"":true}}";
                //{"error":{"error_code":10,"error_msg":"*****","critical":true}}
            }
            else
            {
                Logger.Log.Debug("Sig = SecretKey! test buy!");

                switch (NotificationType)
                {            
                    case "get_item_test":
                        {
                            //ищем товар в БД
                            var FindShopItem = DBManager.Inst.GetShopItem(ItemName);

                            //если товар не найден
                            if(FindShopItem.item_id == 0)
                            {
                                Response = @"{""error"":{" +
                                    @"""error_code"":20," +
                                    @"""error_msg"":""Товара не существует.""," +
                                    @"""critical"":true}}";

                                Logger.Log.Debug($"requested item => error: no item in shop");
                            }
                            else
                            {
                                Response += @"{""response"":";                               

                                Response += JsonConvert.SerializeObject(FindShopItem);

                                Response += "}";

                                Logger.Log.Debug($"requested item => FindShopItem {FindShopItem.title}");

                                //сохраняем заказ
                                //Logger.Log.Debug($"{OrderId} {FindShopItem} {ItemPrice} {UserId} {Date}");
                                DBManager.Inst.AddShopOrder(OrderId, 
                                    FindShopItem.item_id.ToString(),
                                    FindShopItem.price.ToString(), 
                                    UserId,
                                    DateTime.Now.ToString());
                            }
                        }
                        break;

                    case "order_status_change_test":
                        {
                            Logger.Log.Debug($"order_status_change_test");

                            var FindShopOrder = DBManager.Inst.GetShopOrder(OrderId);

                            if(FindShopOrder != null)
                            {
                                if (Status == "chargeable")
                                {
                                    //проверяем, чтобы в заказе все совпало
                                    var OrderIsCorrect = true;

                                    if (FindShopOrder.item_id != ItemId ||
                                        FindShopOrder.item_price != ItemPrice ||
                                        FindShopOrder.user_id != UserId) 
                                    {
                                        OrderIsCorrect = false;
                                        Logger.Log.Debug($"order is not correct");
                                    }

                                    if (OrderIsCorrect)
                                    {
                                        Response += @"{""response"":{";
                                        Response += @"""order_id"":" + OrderId + ",";
                                        Response += @"""app_order_id"":" + OrderId + ",";
                                        Response += "}";

                                        Logger.Log.Debug($"buy end");
                                    }                                   
                                }
                                else
                                {
                                    Response = CreateOrderError(100, "плохой chargeable", true);

                                    Logger.Log.Debug($"buy error");
                                }
                            }
                            else
                            {
                                Logger.Log.Debug($"order {OrderId} not found");
                            }

                           
                        }
                        break;
                } 
            }

            WriteHeaderToClient("text/php", Response.Length, TCPClient);

            Logger.Log.Debug($"Response => {Response}");            

            TCPClient.GetStream().Write(Encoding.ASCII.GetBytes(Response), 0, Response.Length);            
        }    
        
        private string CreateOrderError(int ErrorCode, string ErrorMessage, bool IsCritical)
        {
            var Result = "";
            Result += @"{""error"":{";
            Result += @"""error_code"":" + ErrorCode.ToString() + ",";
            Result += @"""error_msg"":" + ErrorMessage + ",";
            Result += @"""critical"":"+ IsCritical + "}";
            Result += "}";

            return Result;
        }

        public void WriteHeaderToClient(string content_type, long length, TcpClient TCPClient)
        {
            string str = "HTTP/1.1 200 OK\nContent-type: " + content_type
                   + "\nContent-Encoding: 8bit\nContent-Length:" + length.ToString()
                   + "\n\n";
            TCPClient.GetStream().Write(Encoding.ASCII.GetBytes(str), 0, str.Length);
        }

        public string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        //private void CheckHTTPRequest()
        //{
        //    //HttpListenerContext context = Listener.Getcontext();
        //    //HttpListenerRequest request = context.Request;



        //    //Logger.Log.Debug($"web request Url => {request.Url}");
        //    //Logger.Log.Debug($"web request {request.Headers.ToString()}");

        //    //Logger.Log.Debug($"web request Url => {context.User.Identity}");


        //    ////using (Stream body = request.InputStream)
        //    ////{
        //    ////    using (StreamReader reader = new StreamReader(body))
        //    ////    {
        //    ////        string text = reader.ReadToEnd();
        //    ////        Logger.Log.Debug($"web request Url => {text}");
        //    ////    }
        //    ////}

        //    //using (Stream body = request.InputStream) // here we have data
        //    //{
        //    //    using (var reader = new StreamReader(body, request.ContentEncoding))
        //    //    {
        //    //        string text = reader.ReadToEnd();
        //    //        Logger.Log.Debug($"web request body => {text}");
        //    //    }
        //    //}

        //    ////Logger.Log.Debug($"web ContentType {request.ContentType}");

        //    ////Logger.Log.Debug($"web GetType {request.GetType()}");          

        //    ////Logger.Log.Debug($"web Headers {request.Headers.ToString()}");


        //    ////var encoding = ASCIIEncoding.ASCII;
        //    ////using (var reader = new StreamReader(response.GetResponseStream(), encoding))
        //    ////{
        //    ////    string responseText = reader.ReadToEnd();
        //    ////}

        //    ////Logger.Log.Debug($"web request {}");


        //    //// Obtain a response object.
        //    //HttpListenerResponse response = context.Response;
        //    //// Construct a response.
        //    //string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
        //    //byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        //    //// Get a response stream and write the response to it.
        //    //response.ContentLength64 = buffer.Length;
        //    //Stream output = response.OutputStream;
        //    //output.Write(buffer, 0, buffer.Length);
        //    //// You must close the output stream.
        //    //output.Close();
        //}
       
    }

    //public class Interaction
    //{
    //    const string Server_Directory = "C:\\Server\\";
    //    const string Error_Message = "None";
    //    const string Main_Page = "1.txt";

    //    TcpClient Client;
    //    Hashtable Contents = new Hashtable();
    //    /// <summary>
    //    /// По строке запроса вычисляем путь к файлу.
    //    /// </summary>
    //    public string GetPath(string request)
    //    {
    //        int space1 = request.IndexOf(" ");
    //        int space2 = request.IndexOf(" ", space1 + 1);
    //        string url = request.Substring(space1 + 2, space2 - space1 - 2);
    //        if (url == "")
    //            url = Main_Page;
    //        return Server_Directory + url;
    //    }
    //    /// <summary>
    //    /// По файлу вычисляем тип содержимого в нём
    //    /// </summary>
    //    public string GetContent(string file_path)
    //    {
    //        string ext = "";
    //        int dot = file_path.LastIndexOf(".");
    //        if (dot >= 0)
    //            ext = file_path.Substring(dot, file_path.Length - dot).ToUpper();
    //        if (Contents[ext] == null)
    //            return "application/" + ext;
    //        else
    //            return (string)Contents[ext];
    //    }
    //    /// <summary>
    //    /// Отправляем заголовок клиенту.
    //    /// </summary>
    //    public void WriteHeaderToClient(string content_type, long length)
    //    {
    //        string str = "HTTP/1.1 200 OK\nContent-type: " + content_type
    //               + "\nContent-Encoding: 8bit\nContent-Length:" + length.ToString()
    //               + "\n\n";
    //        Client.GetStream().Write(Encoding.ASCII.GetBytes(str), 0, str.Length);
    //    }
    //    /// <summary>
    //    ///  Отвечаем на запрос клиенту
    //    /// </summary>
    //    public void WriteToClient(string request)
    //    {
    //        string file_path = GetPath(request);
    //        if (file_path.IndexOf("..") >= 0 || !File.Exists(file_path))
    //        {
    //            WriteHeaderToClient("text/plain", Error_Message.Length);
    //            Client.GetStream().Write(Encoding.ASCII.GetBytes(Error_Message), 0, Error_Message.Length);
    //            return;
    //        }
    //        FileStream file = File.Open(file_path, FileMode.Open);
    //        WriteHeaderToClient(GetContent(file_path), file.Length);
    //        byte[] buf = new byte[1024];
    //        int len;
    //        while ((len = file.Read(buf, 0, 1024)) != 0)
    //            Client.GetStream().Write(buf, 0, len);
    //        file.Close();
    //    }

    //    public void Interact()
    //    {
    //        try
    //        {
    //            byte[] buffer = new byte[1024];
    //            string request = "";
    //            while (true)
    //            {
    //                int count = Client.GetStream().Read(buffer, 0, 1024);

    //                request += Encoding.ASCII.GetString(buffer, 0, count);

    //                Logger.Log.Debug($"web request {request}");

    //                if (request.IndexOf("\r\n\r\n") >= 0) // Запрос обрывается \r\n\r\n последовательностью
    //                {
    //                    WriteToClient(request);
    //                    request = "";
    //                }
    //            }
    //        }
    //        catch (Exception)
    //        {
    //        }
    //    }

    //    protected void SetContents()
    //    {
    //        Contents.Add("", "application/unknown");
    //        Contents.Add(".HTML", "text/html");
    //        Contents.Add(".PHP", "text/php");
    //        Contents.Add(".HTM", "text/html");
    //        Contents.Add(".TXT", "text/plain");
    //        Contents.Add(".GIF", "image/gif");
    //        Contents.Add(".JPG", "image/jpeg");
    //    }

    //    public Interaction(TcpClient client)
    //    {
    //        Logger.Log.Debug("some client");
    //        Client = client;
    //        SetContents();
    //        Thread interact = new Thread(new ThreadStart(Interact));
    //        interact.Start();
    //    }        
    //}

   
}

public class ShopItem
{
    public int item_id;
    public string title;
    public string photo_url;
    public int price;    
}

public class ShopError
{
    public int error_code;
    public string error_msg;
    public bool critical;
}

public class ShopOrder
{
    public string order_id;
    public string item_id;
    public string item_price;
    public string user_id;
}
