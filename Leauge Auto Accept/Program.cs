using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Reflection;

namespace Leauge_Auto_Accept
{
    class Program
    {
        private static void Main()
        {
            // Start application using conhost (Windows Terminal do not support resize)
            // Ref: https://github.com/microsoft/terminal/issues/5094
            var parentProc = ParentProcessUtilities.GetParentProcess();
            var isDebug = Debugger.IsAttached;
            if (!isDebug && parentProc.ProcessName != "conhost")
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "conhost",
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    Arguments = Process.GetCurrentProcess().MainModule.FileName,
                });
                return;
            }

            UI.InitializingWindow(); // Initializing message
            SizeHandler.initialize(); // Initlize console size
            Console.Title = "League Auto Accept";
            Console.OutputEncoding = Encoding.UTF8;
            Settings.LoadSettings(); // Attempt to load existing settings
            Updater.Initialize();

            // Start a bunch of task
            var taskKeys = new Task(Navigation.ReadKeys);
            taskKeys.Start();
            var taskQueue = new Task(MainLogic.AcceptQueue);
            taskQueue.Start();
            var taskLeagueAlive = new Task(LCU.CheckIfLeagueClientIsOpenTask);
            taskLeagueAlive.Start();
            var taskResizeHandler = new Task(SizeHandler.SizeReader);
            taskResizeHandler.Start();
            
            var tasks = new[] { taskKeys, taskQueue, taskLeagueAlive, taskResizeHandler }; // Indefinitely await tasks
            Task.WaitAll(tasks);
            Console.ReadKey();
        }
    }
}
