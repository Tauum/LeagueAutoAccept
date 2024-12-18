﻿using System;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Leauge_Auto_Accept
{
    internal class Navigation
    {
        public static int currentPos = 0;
        public static int consolePosLast = 0;
        public static int searchPos = 0;
        public static int lastPosMainNav = 0;

        public static string currentInput = "";

        public static void ReadKeys()
        {
            while (true)
            {
                if (UI.currentWindow == "consoleTooSmallMessage")
                {
                    Thread.Sleep(1000);
                    break;
                }

                bool movePointer = false;

                ConsoleKeyInfo key = new ConsoleKeyInfo();
                key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.LeftArrow:
                        movePointer = HandlePointerMovement(key.Key);
                        break;

                    case ConsoleKey.Escape:
                        HandleNavEscape();
                        break;

                    case ConsoleKey.Enter:
                        HandleNavEnter();
                        break;

                    case ConsoleKey.Backspace:
                        HandleInputBackspace();
                        break;

                    default:
                        HandleInput(key.KeyChar);
                        break;
                }

                if (currentInput == "Lochel" && UI.windowType == "grid") UI.PrintHeart();
                else if (movePointer)
                {
                    Print.isMovingPos = true;
                    HandlePointerMovementPrint();
                    consolePosLast = currentPos;
                    if (UI.currentWindow == "settingsMenu") UI.SettingsMenuDesc(currentPos);
                    else if (UI.currentWindow == "delayMenu") UI.DelayMenuDesc(currentPos);
                    Print.isMovingPos = false;
                }
            }
        }

        private static bool HandlePointerMovement(ConsoleKey key)
        {
            switch (UI.windowType)
            {
                case "normal":
                    return HandlePointerMovementNormal(key);

                case "messageEdit":
                case "sideways":
                    return HandlePointerMovementSideways(key);

                case "grid":
                    return HandlePointerMovementGrid(key);

                case "nocursor":
                    return false;

                case "pages":
                    return HandlePointerMovementPages(key);
            }
            return false;
        }

        private static bool HandlePointerMovementNormal(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    if (currentPos == 0) return false;
                    currentPos--;
                    return true;

                case ConsoleKey.DownArrow:
                    if (currentPos + 1 == UI.maxPos) return false;
                    currentPos++;
                    return true;
            }
            return false;
        }

        private static bool HandlePointerMovementSideways(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.LeftArrow:
                    if (currentPos == 0) return false;
                    currentPos--;
                    return true;

                case ConsoleKey.UpArrow:
                    if (currentPos == 0) return false;
                    currentPos--;
                    return true;

                case ConsoleKey.RightArrow:
                    if (currentPos + 1 == UI.maxPos)  return false;
                    currentPos++;
                    return true;

                case ConsoleKey.DownArrow:
                    if (currentPos + 1 == UI.maxPos) return false;
                    currentPos++;
                    return true;
            }
            return false;
        }

        private static bool HandlePointerMovementGrid(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    if (currentPos == 0) return false;
                    currentPos--;
                    return true;

                case ConsoleKey.DownArrow:
                    if (currentPos + 1 == UI.maxPos) return false;
                    currentPos++;
                    return true;

                case ConsoleKey.RightArrow:
                    if (currentPos + UI.totalRows >= UI.maxPos) return false;
                    currentPos += UI.totalRows;
                    return true;

                case ConsoleKey.LeftArrow:
                    if (currentPos <= 0)
                    {
                        currentPos = 0;
                        return false;
                    }
                    currentPos -= UI.totalRows;
                    return true;
            }
            return false;
        }

        private static bool HandlePointerMovementPages(ConsoleKey key)
        {
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    if (currentPos == 0) return false;
                    currentPos--;
                    return true;

                case ConsoleKey.DownArrow:
                    if (currentPos + 1 == UI.maxPos) return false;
                    currentPos++;
                    return true;

                case ConsoleKey.RightArrow:
                    if (UI.currentPage + 1 == UI.totalPages) return false;
                    // TODO: improve this somehow
                    UI.currentPage++;
                    currentPos = 0;
                    UI.ChatMessagesWindow(UI.currentPage);
                    return true;

                case ConsoleKey.LeftArrow:
                    if (UI.currentPage == 0) return false;
                    // TODO: improve this somehow
                    UI.currentPage--;
                    currentPos = 0;
                    UI.ChatMessagesWindow(UI.currentPage);
                    return true;
            }
            return false;
        }

        private static void HandleNavEscape()
        {
            switch (UI.currentWindow)
            {
                case "delayMenu":
                    UI.SettingsMenu();
                    break;
                case "mainScreen":
                    UI.ExitMenu();
                    break;
                case "exitMenu":
                    if (LCU.isLeagueOpen) UI.MainScreen();
                    else UI.LeagueClientIsClosedMessage();
                    break;
                case "leagueClientIsClosedMessage":
                    UI.ExitMenu();
                    break;
                case "initializing":
                    // do nothing
                    break;
                case "chatMessagesEdit":
                    UI.ChatMessagesWindow();
                    break;
                default:
                    if (currentInput == "Lochel" && UI.windowType == "grid") currentInput = "";
                    UI.MainScreen();
                    break;
            }
        }

        private static void HandleNavEnter()
        {
            switch (UI.currentWindow)
            {
                case "mainScreen":
                    MainMenuNav();
                    break;
                case "settingsMenu":
                    Settings.SettingsModify(currentPos);
                    if (UI.currentWindow == "settingsMenu") UI.SettingsMenuUpdateUI(currentPos);
                    break;
                case "delayMenu":
                    //Settings.delayModify(currentPos);
                    //UI.delayMenuUpdateUI(currentPos);
                    break;
                case "exitMenu":
                    ExitMenuNav();
                    break;
                case "champSelector":
                case "spellSelector":
                    if (currentInput != "Lochel")
                    {
                        if (UI.currentWindow == "champSelector") Settings.SaveSelectedChamp();
                        else if (UI.currentWindow == "spellSelector") Settings.SaveSelectedSpell();
                        UI.MainScreen();
                    }
                    break;
                case "chatMessagesWindow":
                    UI.messageIndex = currentPos;
                    UI.ChatMessagesEdit();
                    break;
                case "chatMessagesEdit":
                    chatMessagesEditNav();
                    break;
            }
        }

        private static void HandleInputBackspace()
        {
            if (UI.currentWindow == "champSelector" || UI.currentWindow == "spellSelector")
            {
                if (currentInput.Length > 0)
                {
                    currentInput = currentInput.Remove(currentInput.Length - 1);
                    UI.UpdateCurrentFilter();
                }
            }
            else if (UI.currentWindow == "chatMessagesEdit")
            {
                if (currentInput.Length > 0)
                {
                    currentInput = currentInput.Remove(currentInput.Length - 1);
                    UI.UpdateMessageEdit();
                }
            }
            else if (UI.currentWindow == "delayMenu")
            {
                Settings.DelayModify(currentPos, -1);
                UI.DelayMenuUpdateUI(currentPos);
            }
        }

        private static void HandleInput(char key)
        {
            if (UI.currentWindow == "champSelector" || UI.currentWindow == "spellSelector")
            {
                if (currentInput.Length < 100)
                {
                    currentInput += key;
                    UI.UpdateCurrentFilter();
                }
            }
            if (UI.currentWindow == "chatMessagesEdit")
            {
                if (currentInput.Length < 200)
                {
                    currentInput += key;
                    UI.UpdateMessageEdit();
                }
            }
            else if (UI.currentWindow == "delayMenu")
            {
                if (Functions.IsNumeric(key))
                {
                    Settings.DelayModify(currentPos, Int32.Parse(key.ToString()));
                    UI.DelayMenuUpdateUI(currentPos);
                }
            }
        }

        public static void HandlePointerMovementPrint()
        {
            Console.CursorVisible = false;
            while (!Print.canMovePos) Thread.Sleep(2);
            {
                int positionLeft = 0;
                int positionTop = 0;
                if (UI.currentWindow == "mainScreen" && consolePosLast > 6)
                {
                    // Handles the weird main menu navigation
                    if (consolePosLast == 7)
                    {
                        positionLeft = UI.leftPad;
                        positionTop = SizeHandler.HeightCenter + 7;
                    }
                    else if (consolePosLast == 8)
                    {
                        positionLeft = UI.leftPad + 40;
                        positionTop = SizeHandler.HeightCenter + 7;
                    }
                }
                else if (UI.currentWindow == "exitMenu" && consolePosLast == 1)
                {
                    // Handles the weird exit menu navigation
                    positionLeft = UI.leftPad + 30;
                    positionTop = UI.topPad;
                }
                else if (UI.currentWindow == "chatMessagesEdit")
                {
                    positionTop = UI.topPad + 3;
                    switch (consolePosLast)
                    {
                        case 0:
                            positionLeft = UI.leftPad - 21;
                            break;
                        case 1:
                            positionLeft = UI.leftPad - 7;
                            break;
                        case 2:
                            positionLeft = UI.leftPad + 8;
                            break;
                    }
                }
                else
                {
                    int[] consolePos = GetPositionOnConsole(consolePosLast);
                    positionLeft = consolePos[1];
                    positionTop = consolePos[0];
                }
                Print.printWhenPossible("  ", positionTop, positionLeft, false);
            }


            {
                int positionLeft = 0;
                int positionTop = 0;
                if (UI.currentWindow == "mainScreen") lastPosMainNav = currentPos;

                if (UI.currentWindow == "mainScreen" && currentPos > 6)
                {
                    // Handles the weird main menu navigation
                    if (currentPos == 7)
                    {
                        positionLeft = UI.leftPad;
                        positionTop = SizeHandler.HeightCenter + 7;
                    }
                    else if (currentPos == 8)
                    {
                        positionLeft = UI.leftPad + 40;
                        positionTop = SizeHandler.HeightCenter + 7;
                    }
                }
                else if (UI.currentWindow == "exitMenu" && currentPos == 1)
                {
                    // Handles the weird exit menu navigation
                    positionLeft = UI.leftPad + 30;
                    positionTop = UI.topPad;
                }
                else if (UI.currentWindow == "chatMessagesEdit")
                {
                    positionTop = UI.topPad + 3;
                    switch (currentPos)
                    {
                        case 0:
                            positionLeft = UI.leftPad - 21;
                            break;
                        case 1:
                            positionLeft = UI.leftPad - 7;
                            break;
                        case 2:
                            positionLeft = UI.leftPad + 8;
                            break;
                    }
                }
                else
                {
                    int[] consolePos = GetPositionOnConsole(currentPos);
                    positionLeft = consolePos[1];
                    positionTop = consolePos[0];
                }
                Print.printWhenPossible("->", positionTop, positionLeft, false);
            }
            if (UI.showCursor)
            {
                Console.CursorVisible = true;
                UI.UpdateCursorPosition();
            }
        }

        private static int[] GetPositionOnConsole(int pos)
        {
            // current row
            double row1 = pos / UI.totalRows;       //  1.111111111111111
            double row2 = Math.Floor(row1);         //  1
            // current column
            double column1 = row2 * UI.totalRows;   //  27
            double column2 = pos - column1;         //  3
            // Convert integer, caclulate column width
            int column = Convert.ToInt32(column2);
            int row = Convert.ToInt32(row2) * UI.columnSize;
            if (column < 0) column = 0;
            if (row < 0) row = 0;
            return new int[] { column + UI.topPad, row + UI.leftPad };
        }

        private static void chatMessagesEditNav()
        {
            if (currentPos == 0) //save
            {
                if (currentInput.Length == 0) Settings.DeleteChatMessage();
                else Settings.UpdateChatMessage();
                UI.ChatMessagesWindow();
            }
            else if (currentPos == 1) //delete
            {
                Settings.DeleteChatMessage();
                UI.ChatMessagesWindow();
            }
            else if (currentPos == 2) UI.ChatMessagesWindow(); //cancel
        }

        private static void ExitMenuNav()
        {
            if (currentPos == 0) Environment.Exit(0); 
            else if (currentPos == 1)  // Swap sub menu
            {
                if (LCU.isLeagueOpen) UI.MainScreen();
                else UI.LeagueClientIsClosedMessage();
            }
        }

        private static void MainMenuNav()
        {
            switch (currentPos)
            {
                case 0:
                    UI.currentChampPicker = 0;
                    UI.ChampSelector();
                    break;
                case 1:
                    UI.currentChampPicker = 2;  // New value for secondary champ
                    UI.ChampSelector();
                    break;
                case 2:
                    UI.currentChampPicker = 1;
                    UI.ChampSelector();
                    break;
                case 3:
                    UI.currentSpellSlot = 0;
                    UI.SpellSelector();
                    break;
                case 4:
                    UI.currentSpellSlot = 1;
                    UI.SpellSelector();
                    break;
                case 5:
                    UI.ChatMessagesWindow();
                    break;
                case 6:
                    Settings.ToggleAutoAcceptSetting();
                    UI.ToggleAutoAcceptSettingUI();
                    break;
                case 7:
                    UI.SettingsMenu();
                    break;
                case 8:
                    UI.InfoMenu();
                    break;
            }
        }
    }
}
