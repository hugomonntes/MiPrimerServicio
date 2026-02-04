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
        MiPrimerServicio servicio = new MiPrimerServicio();
        int defaultPort = 31416;
        public bool ServerIsRunning { get; set; } = true;
        public Socket socketServer;
        string errorValidacion = "Comando no válido ";
        string errorPuerto = "El archivo no esxiste o el puerto está ocupado, puerto por defecto: ";
        string puertoOcupado = "Puerto en uso, cerrando servidor";
        string errorArchivo = "El archivo no existe o tiene algún error";

        public void InitServer()
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, ReadFile("port"));
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
                        servicio.WriteEvent($"{puertoOcupado}");
                        socketServer.Close();
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
                IPEndPoint ieCliente = (IPEndPoint)socketClient.RemoteEndPoint;
                Console.WriteLine($"Cliente conectado a {ieCliente.Address} puerto {ieCliente.Port}");
                Encoding codificacion = Console.OutputEncoding;
                using (NetworkStream ns = new NetworkStream(socketClient))
                using (StreamReader sr = new StreamReader(ns))
                using (StreamWriter sw = new StreamWriter(ns))
                {
                    sw.WriteLine("Inicio");
                    sw.AutoFlush = true;
                    string msg = "";
                    DateTime fechaYHora;
                    try
                    {
                        msg = sr.ReadLine();
                        if (msg != null)
                        {
                            switch (msg.Trim())
                            {
                                case "time":
                                    fechaYHora = DateTime.Now;
                                    sw.WriteLine(fechaYHora.ToString("HH:mm:ss"));
                                    break;
                                case "date":
                                    fechaYHora = DateTime.Now;
                                    sw.WriteLine(fechaYHora.ToString("dd/MM/yyyy"));
                                    break;
                                case "all":
                                    fechaYHora = DateTime.Now;
                                    sw.WriteLine(fechaYHora.ToString("dd/MM/yyyy -- HH:mm:ss"));
                                    break;
                                default:
                                    servicio.WriteEvent($"{errorValidacion}{msg}");
                                    break;
                            }
                            Console.WriteLine($"Cliente: {msg}");
                            LogFile(msg, ieCliente.Address, ieCliente.Port);
                        }
                    }
                    catch (Exception ex) when (ex is IOException || ex is SocketException)
                    {
                        msg = null;
                    }
                    Console.WriteLine("Conexión cleinte cerrada");
                }
            }
        }

        public void LogFile(string mensaje, IPAddress ip, int puerto)
        {
            try
            {
                DateTime timeStamp = DateTime.Now;
                DirectoryInfo dir = new DirectoryInfo(Environment.GetEnvironmentVariable("programdata"));
                string path = $"{Environment.GetEnvironmentVariable("programdata")}\\log.txt";
                using (StreamWriter sw = new StreamWriter(path, true))
                {
                    sw.WriteLine($"[{timeStamp.ToString("dd/MM/yyyy - HH:mm:ss")}@{ip}{puerto}] : {mensaje}");
                }
            }
            catch (Exception e)
            {
                servicio.WriteEvent(errorArchivo);
            }
        }

        int puertoProv;
        public int ReadFile(string fileName)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(Environment.GetEnvironmentVariable("programdata"));
                string path = $"{Environment.GetEnvironmentVariable("programdata")}\\{fileName}.txt";
                using (StreamReader sr = new StreamReader(path))
                {
                    puertoProv = int.Parse(sr.ReadToEnd());
                    servicio.WriteEvent($"Puerto escucha: {puertoProv}");
                    return puertoProv;
                }
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is IOException || ex is UnauthorizedAccessException)
            {
                servicio.WriteEvent($"{errorPuerto}{defaultPort}");
                return defaultPort;
            }
        }
    }
}
