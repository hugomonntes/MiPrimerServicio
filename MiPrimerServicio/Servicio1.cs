using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace MiPrimerServicio
{
    public partial class MiPrimerServicio : ServiceBase
    {
        Server server = new Server();
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
            EventLog.WriteEntry(nombre, mensaje);
            if (server.isFreePort(server.Port))
            {
                EventLog.WriteEntry($"Puerto de escucha: {server.Port}");
            }
            else if (server.Port == 0)
            {
                EventLog.WriteEntry("Error al leer el archivo");
            }
            else if (server.Command != "time" || server.Command != "date" || server.Command != "all")
            {
                EventLog.WriteEntry($"Comando no válido {server.Command}");
            }

            //Si al final tanto el puerto leido del archivo como el puerto por defecto
            //estuvieran ocupados, se informará en el visor de eventos y finalizará el
            //servicio.
            if (!server.isFreePort(server.Port))
            {

            }
        }

        private System.Timers.Timer timer;
        protected override void OnStart(string[] args)
        {
            WriteEvent("Iniciando MiPrimerServicio mediante OnStart");
            timer = new System.Timers.Timer();
            timer.Interval = 10000; // cada 10 segundos
            timer.Elapsed += this.TimerTick;
            timer.Start();

            server.InitServer();
        }

        private int t = 0;
        public void TimerTick(object sender, System.Timers.ElapsedEventArgs args)
        {
            WriteEvent("MiPrimerServicio lleva ejecutándose {t} segundos.");
            t += 10;
        }

        protected override void OnPause()
        {
            WriteEvent("MiPrimerServicio en Pausa");
            timer.Stop();
        }
        protected override void OnContinue()
        {
            WriteEvent("Continuando MiPrimerServicio");
            timer.Start();
        }
        protected override void OnStop()
        {
            WriteEvent("Deteniendo MiPrimerServicio");
            timer.Stop();
            timer.Dispose();
            t = 0;
        }
    }
}
