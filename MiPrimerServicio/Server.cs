using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiPrimerServicio
{
    internal class Server
    {
        public bool ServerIsRunning { get; set; } = true;
        public int Port { get; set; } = ReadFile("port");
        public string Command { get; set; }
        //public string Password { get; set; } = ReadFile("password");
        public Socket socketServer;
        public bool isFreePort(int port)
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, port);
            using (socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    socketServer.Bind(iPEndPoint);
                    socketServer.Listen(1);
                }
                catch (SocketException e)
                {
                    return false;
                }
            }
            return true;
        }

        public int SearchFreePort(int port)
        {
            bool portIsFree = true;
            do
            {
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, port);
                using (socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    try
                    {
                        portIsFree = true;
                        socketServer.Bind(iPEndPoint);
                        socketServer.Listen(1);
                    }
                    catch (SocketException e) when (e.ErrorCode == (int)SocketError.AddressAlreadyInUse)
                    {
                        portIsFree = false;
                        port++;
                    }
                    catch (SocketException e)
                    {
                        portIsFree = false;
                        port++;
                    }
                }
            }
            while (!portIsFree);//maxport
            return port;
        }

        public void InitServer()
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, Port);
            using (socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socketServer.Bind(iPEndPoint);
                socketServer.Listen(10);
                Console.WriteLine($"Conectado a {iPEndPoint}");
                while (ServerIsRunning)
                {
                    try
                    {
                        Socket client = socketServer.Accept();
                        Thread thread = new Thread(() => ClientManager(client));
                        thread.IsBackground = true;
                        thread.Start();
                    }
                    catch (SocketException s)
                    {
                    }
                }
            }
        }

        public void StopServer(Socket socketServer)
        {
            ServerIsRunning = false;
            socketServer.Close();
        }

        private void ClientManager(Socket socketClient)
        {
            using (socketClient)
            {
                IPEndPoint clientEndPoint = (IPEndPoint)socketClient.RemoteEndPoint;
                Console.WriteLine($"Cliente conectado desde {clientEndPoint.Address}:{clientEndPoint.Port}");
                Encoding encoding = Console.OutputEncoding;
                using (NetworkStream networkStream = new NetworkStream(socketClient))
                using (StreamReader sReader = new StreamReader(networkStream, encoding))
                using (StreamWriter sWriter = new StreamWriter(networkStream, encoding))
                {
                    sWriter.AutoFlush = true;
                    sWriter.WriteLine("CLIENT");
                    try
                    {
                        Command = sReader.ReadLine();
                        if (Command != null)
                        {
                            Command = Command.Trim();
                        }

                        if (Command == "time")
                        {
                            sWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss"));
                        }
                        else if (Command == "date")
                        {
                            sWriter.WriteLine(DateTime.Now.ToString("dd/MM/yyyy"));
                        }
                        else if (Command == "all")
                        {
                            sWriter.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                        }
                        //else if (Command == $"close {Password}")
                        //{
                        //    //close password: Junto con el comando close se debe verificar que viene
                        //    //una contraseña. Si esta es correcta el servidor ha de finalizar y se lo
                        //    //indica al cliente.Si no devuelve un mensaje de error al cliente(Debe
                        //    //diferenciarse el error de contraseña no válida o que no se haya enviado
                        //    //la contraseña).
                        //    StopServer(socketServer);
                        //    sWriter.WriteLine("Conexión con el servidor finalizada");
                        //}
                        //else if (Command == $"close")
                        //{
                        //    sWriter.WriteLine("Comando close sin contraseña");
                        //}
                        else
                        {
                            sWriter.WriteLine("ERROR, comando no valido");
                        }
                    }
                    catch (Exception ex) when (ex is SocketException || ex is IOException)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public void LogFile()
        {
            using (StreamWriter sw = new StreamWriter($"{Environment.GetEnvironmentVariable("programdata")}\\log.txt"))
            {
                try
                {
                    sw.WriteLine($"[{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}-@{IPAddress.Any}");
                }
                catch (IOException)
                {
                    sw.WriteLine($"[ERROR: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");
                }
            }
        }

        public static int ReadFile(string FileName)
        {
            try
            {
                string path = $"{Environment.GetEnvironmentVariable("programdata")}\\{FileName}.txt";
                using (StreamReader reader = new StreamReader(path))
                {
                    return int.TryParse(reader.ReadToEnd().Trim(), out int port) ? port : 31416;
                }
            }
            catch (IOException io)
            {
                return 0;
            }
        }
    }
}
