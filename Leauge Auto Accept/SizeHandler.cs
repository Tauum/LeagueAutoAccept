using System;
using System.Diagnostics;
using System.Threading;

namespace Leauge_Auto_Accept
{
    internal class SizeHandler
    {
        public static int minWidth = 120;
        public static int minHeight = 30;

        public static int WindowWidth = minWidth;
        public static int WindowHeight = minHeight;
        public static int WidthCenter = WindowWidth / 2;
        public static int HeightCenter = WindowHeight / 2;

        public static void initialize()
        {
            Console.CursorVisible = false;
            Console.SetWindowSize(minWidth, minHeight);
        }

        public static void SizeReader()
        {
            while (true)
            {
                int currentWidth = Console.WindowWidth;
                int currentHeight = Console.WindowHeight;
                if (WindowWidth != currentWidth || WindowHeight != currentHeight)
                {
                    WindowWidth = currentWidth;
                    WindowHeight = currentHeight;
                    CalculateCenter();
                    HandleResize();
                }
                Thread.Sleep(1000);
            }
        }

        public static void HandleResize()
        {
            Console.CursorVisible = false;  // Hide because it shows EVERY time
            UI.totalRows = WindowHeight - 2;
            if (WindowWidth < minWidth) UI.ConsoleTooSmallMessage("width");
            else if (WindowHeight < minHeight) UI.ConsoleTooSmallMessage("height");
            else if (UI.currentWindow == "consoleTooSmallMessage") UI.ReloadWindow("previous");
            else UI.ReloadWindow("current");
        }

        public static void CalculateCenter()
        {
            WidthCenter = WindowWidth / 2;
            HeightCenter = WindowHeight / 2;
        }

        public static void ResizeBasedOnChampsCount()
        {
            // Calculate the items current application size can have
            int totalRows = minHeight - 2;
            int totalItems = totalRows * minWidth / 20; // 20 column size for champion name
            int totalOptions = Data.champsSorterd.Count + 2; // 2 "Unselected" and "None"
            if (totalItems < totalOptions) // verify application is not too small
            {
                double neededHeight = totalOptions / 6; // 6 is current columns in champions list
                int newHeight = (int)Math.Ceiling(neededHeight) + 3;
                minHeight = newHeight;
                if (WindowHeight < minHeight)  initialize(); // Resize if application too small
            }
        }
    }
}
