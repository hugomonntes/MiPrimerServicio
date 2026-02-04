using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiPrimerServicio
{
    public partial class MiPrimerServicio : ServiceBase
    {
        Server server;
        public MiPrimerServicio()
        {
            InitializeComponent();
            this.AutoLog = false;
        }
        public void WriteEvent(string mensaje)
        {
            // Nombre de la fuente de eventos.
            const string nombre = "MiPrimerServicio";
            // Escribe el mensaje deseado en el visor de eventos

            try
            {
                EventLog.WriteEntry(nombre, mensaje);
            }
            catch (Exception)
            {
                server.LogFile($"Eroor {nombre}", IPAddress.Any, 0);
            }
        }

        protected override void OnStart(string[] args)
        {
            WriteEvent("Iniciando MiPrimerServicio mediante OnStart");
            server = new Server();
            Thread thread = new Thread(() => server.InitServer());
            thread.Start();
        }

        protected override void OnStop()
        {
            WriteEvent("Deteniendo MiPrimerServicio");
            server.StopServer(server.socketServer);
        }
    }
}
