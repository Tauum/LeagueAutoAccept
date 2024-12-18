using System;
using System.Collections.Generic;

namespace Leauge_Auto_Accept
{
    internal class UI
    {
        public static string currentWindow = "";
        public static string previousWindow = "";

        public static int currentChampPicker = 0;
        public static int currentSpellSlot = 0;

        public static int totalChamps = 0;
        public static int totalSpells = 0;

        // normal/+grid/pages/nocursor/messageEdit
        public static string windowType = "";
        public static int messageIndex = 0; //index for the message currently being edit

        public static int totalRows = SizeHandler.WindowHeight - 2;
        public static int columnSize = 20;
        public static int topPad = 0;
        public static int leftPad = 0;
        public static int maxPos = 0;
        public static int currentPage = 0;
        public static int totalPages = 0;

        private static int[] cursorPositionValue = [0, 0];
        public static int[] cursorPosition
        {
            get { return cursorPositionValue; }
            set
            {
                cursorPositionValue = value;
                Console.SetCursorPosition(value[0], value[1]);
            }
        }
        
        private static bool showCursorValue = false;
        public static bool showCursor
        {
            get { return showCursorValue; }
            set
            {
                if (showCursorValue != value) Console.CursorVisible = value; 
                showCursorValue = value;
            }
        }   

        public static void InitializingWindow()
        {
            Print.canMovePos = false;
            Console.Clear();
            currentWindow = "initializing";
            windowType = "nocursor";
            showCursor = false;
            Print.printCentered("Initializing...", SizeHandler.HeightCenter);
        }

        public static void LeagueClientIsClosedMessage()
        {
            Print.canMovePos = false;
            currentWindow = "leagueClientIsClosedMessage";
            windowType = "nocursor";
            showCursor = false;
            Console.Clear();
            Print.printCentered("League client cannot be found.", SizeHandler.HeightCenter);
        }

        public static void ConsoleTooSmallMessage(string direction)
        {
            if (currentWindow != "consoleTooSmallMessage") previousWindow = currentWindow; // persist previous window
            currentWindow = "consoleTooSmallMessage";
            windowType = "nocursor";
            showCursor = false;
            Console.Clear();
            if (direction == "width")
            {
                Print.printCentered("Applicaiton width too small. Resize it to continue.", SizeHandler.HeightCenter);
                Print.printCentered("Minimum width:" + SizeHandler.minWidth + " | Current width:" + SizeHandler.WindowWidth);
            }
            else
            {
                Print.printCentered("Applicaiton height too small. Resize it to continue.", SizeHandler.HeightCenter);
                Print.printCentered("Minimum height:" + SizeHandler.minHeight + " | Current height:" + SizeHandler.WindowHeight);
            }
        }

        public static void ReloadWindow(string windowToReload = "current")
        {
            if (windowToReload == "current") windowToReload = currentWindow;
            else windowToReload = previousWindow;
            switch (windowToReload)
            {
                case "mainScreen":
                    MainScreen();
                    break;
                case "settingsMenu":
                    SettingsMenu();
                    break;
                case "delayMenu":
                    DelayMenu();
                    break;
                case "leagueClientIsClosedMessage":
                    LeagueClientIsClosedMessage();
                    break;
                case "infoMenu":
                    InfoMenu();
                    break;
                case "exitMenu":
                    ExitMenu();
                    break;
                case "champSelector":
                    ChampSelector();
                    break;
                case "spellSelector":
                    SpellSelector();
                    break;
                case "chatMessagesWindow":
                    ChatMessagesWindow();
                    break;
                case "initializing":
                    InitializingWindow();
                    break;
            }
        }

        public static void MainScreen()
        {
            Print.canMovePos = false;
            Navigation.currentPos = Navigation.lastPosMainNav;
            Navigation.consolePosLast = Navigation.lastPosMainNav;

            currentWindow = "mainScreen";
            windowType = "normal";
            showCursor = false;
            topPad = SizeHandler.HeightCenter - 1;
            leftPad = SizeHandler.WidthCenter - 25;
            maxPos = 9;

            Console.Clear();

            // Define logo
            string[] logo =
            {
                @"  _                                                 _                                     _   ",
                @" | |                                     /\        | |            /\                     | |  ",
                @" | |     ___  __ _  __ _ _   _  ___     /  \  _   _| |_ ___      /  \   ___ ___ ___ _ __ | |_ ",
                @" | |    / _ \/ _` |/ _` | | | |/ _ \   / /\ \| | | | __/ _ \    / /\ \ / __/ __/ _ \ '_ \| __|",
                @" | |___|  __/ (_| | (_| | |_| |  __/  / ____ \ |_| | || (_) |  / ____ \ (_| (_|  __/ |_) | |_ ",
                @" |______\___|\__,_|\__, |\__,_|\___| /_/    \_\__,_|\__\___/  /_/    \_\___\___\___| .__/ \__|",
                @"                    __/ |                                                          | |        ",
                @"                   |___/                                                           |_|        "
            };

            // Print logo
            for (int i = 0; i < logo.Length; i++)
            {
                Print.printCentered(logo[i], SizeHandler.HeightCenter - 12 + i);
            }

            // Define options
            string[] optionName = {
                "Select champion",
                "Select backup-champion",
                "Select ban",
                "Select summoner spell [D]",
                "Select summoner spell [F]",
                "Instant chat messages",
                "Enable auto accept"
            };
            string[] optionValue = {
                Settings.currentChamp[0],
                Settings.secondaryChamp[0],
                Settings.currentBan[0],
                Settings.currentSpell1[0],
                Settings.currentSpell2[0],
                Settings.chatMessagesEnabled ? "Enabled, " + Settings.chatMessages.Count : "Disabled",
                MainLogic.gameState.Flags.IsAutoAcceptOn ? "Enabled" : "Disabled"
            };

            // Print options
            for (int i = 0; i < optionName.Length; i++)
            {
                Print.printCentered(AddDotsInBetween(optionName[i], optionValue[i]), topPad + i);
            }

            // Print two bottom buttons that are not actaul settings
            Print.printWhenPossible("Info", SizeHandler.HeightCenter + 7, leftPad + 43);
            Print.printWhenPossible("Settings", SizeHandler.HeightCenter + 7, leftPad + 3);
            Print.printWhenPossible("v" + Updater.appVersion, SizeHandler.WindowHeight - 1, 0, false);
            Navigation.HandlePointerMovementPrint();
            Print.canMovePos = true;
        }

        public static void ToggleAutoAcceptSettingUI()
        {
            Print.printWhenPossible(MainLogic.gameState.Flags.IsAutoAcceptOn ? ". Enabled" : " Disabled", topPad + 6, leftPad + 38);
        }

        public static void SettingsMenu()
        {
            Print.canMovePos = false;
            Navigation.currentPos = 0;
            Navigation.consolePosLast = 0;

            currentWindow = "settingsMenu";
            windowType = "normal";
            showCursor = false;
            topPad = SizeHandler.HeightCenter - 4;
            leftPad = SizeHandler.WidthCenter - 25;
            maxPos = 13;
            Console.Clear();

            var options = new Dictionary<string, string>
            {
                { "Save settings/config", Settings.saveSettings ? "Yes" : "No" },
                { "Preload data", Settings.preloadData ? "Yes" : "No" },
                { "Instalock pick", Settings.instaLock ? "Yes" : "No" },
                { "Instalock ban", Settings.instaBan ? "Yes" : "No" },
                { "Disable update check", Settings.disableUpdateCheck ? "Yes" : "No" },
                { "Automatically trade pick order", Settings.autoPickOrderTrade ? "Yes" : "No" },
                { "Instantly hover pick", Settings.instantHover ? "Yes" : "No" },
                { "Automatically restart queue", Settings.autoRestartQueue ? "Yes" : "No" },
                { "Cancel queue after dodge", Settings.cancelQueueAfterDodge ? "Yes" : "No" },
                { "Automatically honor", Settings.autoHonor ? "Yes" : "No" },
                { "Honor teammates or skip", Settings.autoHonorTeammatesOrSkip ? "Yes" : "No" },
                { "Automatically skip post lobby statistics", Settings.autoStatsSkip ? "Yes" : "No" },
                { "Delay settings", "" } // Empty string can be handled if necessary
            };

            foreach (var option in options)
            {
                Print.printCentered(AddDotsInBetween(option.Key, option.Value), topPad++);
            }
            Navigation.HandlePointerMovementPrint();
            Print.canMovePos = true;
            SettingsMenuDesc(0);
        }

        public static void SettingsMenuDesc(int item)
        {
            // settings descrptions
            switch (item)
            {
                case 0:
                    Print.printCentered("Save settings for the next time you open the app.", topPad + maxPos + 2);
                    Print.printCentered("This will create a settings file in the %AppData% folder.");
                    break;
                case 1:
                    Print.printCentered("Preload all data the app will need on launch.", topPad + maxPos + 2);
                    Print.printCentered("This includes champions list, summoner spells list and more.");
                    break;
                case 2:
                    Print.printCentered("Instanly lock in when it's your turn to pick.", topPad + maxPos + 2);
                    Print.printCentered("This will bypass the lock in delay setting.");
                    break;
                case 3:
                    Print.printCentered("Instanly lock in when it's your turn to ban.", topPad + maxPos + 2);
                    Print.printCentered("This will bypass the lock in delay setting.");
                    break;
                case 4:
                    Print.printCentered("Disable update check on startup.", topPad + maxPos + 2);
                    Print.printCentered("");
                    break;
                case 5:
                    Print.printCentered("Automatically trade pick order when someone requests to.", topPad + maxPos + 2);
                    Print.printCentered("");
                    break;
                case 6:
                    Print.printCentered("Instantly hover champion as soon as joining champ select.", topPad + maxPos + 2);
                    Print.printCentered("In draft pick, it will hover before you are normally able to.");
                    break;
                case 7:
                    Print.printCentered("Automatically restart queue every few minutes.", topPad + maxPos + 2);
                    Print.printCentered("Default is 5 mintues, can be configured in the delays settings.");
                    break;
                case 8:
                    Print.printCentered("Automatically cancel the queue after someone dodges the lobby.", topPad + maxPos + 2);
                    Print.printCentered("");
                    break;
                case 9:
                    Print.printCentered("Automatically honor all teammates post-game lobby.", topPad + maxPos + 2);
                    Print.printCentered("");
                    break;
                case 10:
                    Print.printCentered("Automatically skip honoring post-game lobby.", topPad + maxPos + 2);
                    Print.printCentered("");
                    break;
                case 11:
                    Print.printCentered("Automatically skip post-game statistics lobby.", topPad + maxPos + 2);
                    Print.printCentered("");
                    break; 
                case 12:
                    Print.printCentered("Adjust different delays.", topPad + maxPos + 2);
                    Print.printCentered("");
                    break;
            }
        }

        public static void SettingsMenuUpdateUI(int item)
        {
            // Select item to toggle from settings
            string outputText = item switch
            {
                0 => Settings.saveSettings ? " Yes" : ". No",
                1 => Settings.preloadData ? " Yes" : ". No",
                2 => Settings.instaLock ? " Yes" : ". No",
                3 => Settings.instaBan ? " Yes" : ". No",
                4 => Settings.disableUpdateCheck ? " Yes" : ". No",
                5 => Settings.autoPickOrderTrade ? " Yes" : ". No",
                6 => Settings.instantHover ? " Yes" : ". No",
                7 => Settings.autoRestartQueue ? " Yes" : ". No",
                8 => Settings.cancelQueueAfterDodge ? " Yes" : ". No",
                9 => Settings.autoHonor ? " Yes" : ". No",
                10 => Settings.autoHonorTeammatesOrSkip ? " Yes" : ". No",
                11 => Settings.autoStatsSkip ? " Yes" : ". No",
                _ => ""
            };
            Print.printWhenPossible(outputText, item + topPad, SizeHandler.WidthCenter + 22 - outputText.Length);
        }

        public static void DelayMenu()
        {
            Print.canMovePos = false;
            Navigation.currentPos = 0;
            Navigation.consolePosLast = 0;
            currentWindow = "delayMenu";
            windowType = "normal";
            showCursor = false;
            topPad = SizeHandler.HeightCenter - 3;
            leftPad = SizeHandler.WidthCenter - 25;
            maxPos = 8;
            Console.Clear();
            // Define options
            string[] optionName = {
                "Pick hover delay upon phase start",
                "Pick lock delay upon phase start",
                "Pick lock delay before phase end",
                "Ban hover delay upon phase start",
                "Ban lock delay upon phase start",
                "Ban lock delay before phase end",
                "Max queue time before restart",
                "Chat Messages Delay"
            };
            string[] optionValue = {
                Settings.pickStartHoverDelay.ToString(),
                Settings.pickStartlockDelay.ToString(),
                Settings.pickEndlockDelay.ToString(),
                Settings.banStartHoverDelay.ToString(),
                Settings.banStartlockDelay.ToString(),
                Settings.banEndlockDelay.ToString(),
                Settings.queueMaxTime.ToString(),
                Settings.chatMessagesDelay.ToString()
            };

            // Print options
            for (int i = 0; i < optionName.Length; i++) Print.printCentered(AddDotsInBetween(optionName[i], optionValue[i]), topPad + i);
            Navigation.HandlePointerMovementPrint();
            Print.canMovePos = true;
            DelayMenuDesc(0);
        }

        public static void DelayMenuDesc(int item)
        {
            // settings descrptions
            switch (item)
            {
                case 0:
                    Print.printCentered("Delay after which to hover your pick.", topPad + maxPos + 2);
                    Print.printCentered("Default is 10000.");
                    break;
                case 1:
                    Print.printCentered("Delay after which to lock in your pick, after you are able to.", topPad + maxPos + 2);
                    Print.printCentered("Default is 999999999.");
                    break;
                case 2:
                    Print.printCentered("Time to lock in before your time runs out.", topPad + maxPos + 2);
                    Print.printCentered("Do not set too low (<300), it will cause you to dodge. Default is 1000.");
                    break;
                case 3:
                    Print.printCentered("Delay after which to hover your ban.", topPad + maxPos + 2);
                    Print.printCentered("Default is 1500.");
                    break;
                case 4:
                    Print.printCentered("Delay after which to lock in your pick, after you are able to.", topPad + maxPos + 2);
                    Print.printCentered("Default is 999999999.");
                    break;
                case 5:
                    Print.printCentered("Time to lock in before your time runs out", topPad + maxPos + 2);
                    Print.printCentered("Default is 1000.");
                    break;
                case 6:
                    Print.printCentered("How long should the queue be before cancelling and restarting it?", topPad + maxPos + 2);
                    Print.printCentered("Default is 300000.");
                    break;
                case 7:
                    Print.printCentered("Delay after which the chat messages will be sent", topPad + maxPos + 2);
                    Print.printCentered("Default is 100.");
                    break;
            }
        }

        public static void DelayMenuUpdateUI(int item)
        {
            string outputText = item switch // Select item to toggle from settings
            {
                0 => (" " + Settings.pickStartHoverDelay).PadLeft(10, '.'),
                1 => (" " + Settings.pickStartlockDelay).PadLeft(10, '.'),
                2 => (" " + Settings.pickEndlockDelay).PadLeft(10, '.'),
                3 => (" " + Settings.banStartHoverDelay).PadLeft(10, '.'),
                4 => (" " + Settings.banStartlockDelay).PadLeft(10, '.'),
                5 => (" " + Settings.banEndlockDelay).PadLeft(10, '.'),
                6 => (" " + Settings.queueMaxTime).PadLeft(10, '.'),
                7 => (" " + Settings.chatMessagesDelay).PadLeft(10, '.'),
                _ => ""
            };
            Print.printWhenPossible(outputText, item + topPad, SizeHandler.WidthCenter + 22 - outputText.Length);
        }

        public static void InfoMenu()
        {
            Print.canMovePos = false;
            Navigation.currentPos = 0;
            Navigation.consolePosLast = 0;
            currentWindow = "infoMenu";
            windowType = "nocursor";
            showCursor = false;
            Console.Clear();
            Print.printCentered(AddDotsInBetween("Made by", "Artem"), SizeHandler.HeightCenter - 3);
            Print.printCentered(AddDotsInBetween("Enhanced By", "Taum"), SizeHandler.HeightCenter - 2);
            Print.printCentered(AddDotsInBetween("Version", Updater.appVersion), SizeHandler.HeightCenter - 1);
            Print.printCentered("Source code:", SizeHandler.HeightCenter + 1);
            Print.printCentered(" github.com/sweetriverfish/LeagueAutoAccept", SizeHandler.HeightCenter + 2);
        }

        public static void ExitMenu()
        {
            Print.canMovePos = false;
            Navigation.currentPos = 0;
            Navigation.consolePosLast = 0;
            currentWindow = "exitMenu";
            windowType = "sideways";
            showCursor = false;
            topPad = SizeHandler.HeightCenter + 1;
            leftPad = SizeHandler.WidthCenter - 19;
            maxPos = 2;
            Console.Clear();
            Print.printCentered("Are you sure you want to close this app?", topPad - 2);
            Print.printWhenPossible((" No").PadLeft(32, ' '), topPad, leftPad + 3, false);
            Print.printWhenPossible("Yes ", topPad, leftPad + 3, false);
            Navigation.HandlePointerMovementPrint();
            Print.canMovePos = true;
        }

        public static void ChampSelector()
        {
            Print.canMovePos = false;
            totalRows = SizeHandler.WindowHeight - 2;
            currentWindow = "champSelector";
            windowType = "grid";
            showCursor = false;
            Navigation.currentInput = "";
            Console.Clear();
            Data.LoadChampionsList();
            DisplayChamps();
            UpdateCurrentFilter();
        }

        private static void DisplayChamps()
        {
            Console.CursorVisible = false;
            Navigation.currentPos = 0;
            Navigation.consolePosLast = 0;
            topPad = 0;
            leftPad = 0;
            maxPos = totalChamps;
            Console.SetCursorPosition(0, 0);
            List<ItemList> champsFiltered = new List<ItemList>();
            if ("unselected".Contains(Navigation.currentInput.ToLower())) champsFiltered.Add(new ItemList() { Name = "Unselected", Id = "0" });
            if (currentChampPicker == 1 && "none".Contains(Navigation.currentInput.ToLower())) champsFiltered.Add(new ItemList() { Name = "None", Id = "-1" });
            foreach (var champ in Data.champsSorterd)
            {
                if (champ.Name.ToLower().Contains(Navigation.currentInput.ToLower()))
                {
                    // Verify champ available and not banned before adding
                    if (champ.Free || currentChampPicker == 1) champsFiltered.Add(new ItemList() { Name = champ.Name, Id = champ.Id });
                }
            }

            totalChamps = champsFiltered.Count;
            int currentRow = 0;
            string[] champsOutput = new string[totalRows];

            foreach (var champ in champsFiltered)
            {
                string line = "   " + champ.Name;
                line = line.PadRight(columnSize, ' ');
                champsOutput[currentRow] += line;
                currentRow++;
                if (currentRow >= totalRows) currentRow = 0;
            }

            foreach (var line in champsOutput)
            {
                string lineNew;
                if (line != null)
                {
                    lineNew = line.Remove(line.Length - 1);
                    lineNew = lineNew.PadRight(119, ' ');
                }
                else lineNew = "".PadRight(119, ' ');
                Print.printWhenPossible(lineNew);
            }
            Navigation.HandlePointerMovementPrint();
            Print.canMovePos = true;
            DisplayCursorIfNeeded();
        }

        public static void SpellSelector()
        {
            Print.canMovePos = false;
            totalRows = SizeHandler.WindowHeight - 2;
            currentWindow = "spellSelector";
            windowType = "grid";
            showCursor = false;
            Navigation.currentInput = "";
            Console.Clear();
            Data.LoadSpellsList();
            DisplaySpells();
            UpdateCurrentFilter();
        }

        private static void DisplaySpells()
        {
            Console.CursorVisible = false;
            Navigation.currentPos = 0;
            Navigation.consolePosLast = 0;
            topPad = 0;
            leftPad = 0;
            maxPos = totalSpells;
            Console.SetCursorPosition(0, 0);
            List<ItemList> spellsFiltered = new List<ItemList>();
            if ("unselected".Contains(Navigation.currentInput.ToLower())) spellsFiltered.Add(new ItemList() { Name = "Unselected", Id = "0" });
            foreach (var spell in Data.spellsSorted)
            {
                if (spell.Name.ToLower().Contains(Navigation.currentInput.ToLower())) spellsFiltered.Add(new ItemList() { Name = spell.Name, Id = spell.Id });
            }
            totalSpells = spellsFiltered.Count;
            int currentRow = 0;
            string[] spelloutput = new string[totalRows];

            foreach (var spell in spellsFiltered)
            {
                string line = "   " + spell.Name;
                line = line.PadRight(columnSize, ' ');
                spelloutput[currentRow] += line;
                currentRow++;
                if (currentRow >= totalRows) currentRow = 0;
            }

            foreach (var line in spelloutput)
            {
                string lineNew;
                if (line != null)
                {
                    lineNew = line.Remove(line.Length - 1);
                    lineNew = lineNew.PadRight(119, ' ');
                }
                else lineNew = "".PadRight(119, ' ');
                Print.printWhenPossible(lineNew);
            }
            Navigation.HandlePointerMovementPrint();
            Print.canMovePos = true;
            DisplayCursorIfNeeded();
        }

        public static void UpdateCurrentFilter()
        {
            showCursor = true;
            if (currentWindow == "champSelector") DisplayChamps();
            else if (currentWindow == "spellSelector")  DisplaySpells();
            Navigation.currentPos = 0;
            Console.CursorVisible = false;
            string consoleLine = "Search: " + Navigation.currentInput;
            Print.printCentered(consoleLine, Console.WindowHeight - 1, false, true);
            Console.SetCursorPosition(0, 0);
            Console.CursorVisible = true;
            UpdateCursorPosition();
        }

        public static void PrintHeart()
        {
            int[] position = [54, 10];
            string[] lines = [
                "  oooo   oooo",
                " o    o o    o ",
                "o      o      o",
                " o           o",
                "  o         o",
                "   o       o",
                "    o     o",
                "     o   o",
                "      o o",
                "       o"
            ];

            foreach (string line in lines)
            {
                Console.SetCursorPosition(position[0], position[1]++);
                Console.WriteLine(line);
            }
            UpdateCursorPosition();
        }

        private static string AddDotsInBetween(string firstString, string secondString, int totalLength = 44)
        {
            int firstStringLength = firstString.Length + 1;
            int secondStringLength = secondString.Length + 1;
            int dotsCount = totalLength - firstStringLength - secondStringLength;
            return firstString + " " + new string('.', dotsCount) + " " + secondString;
        }

        public static void ChatMessagesWindow(int pageToLoad = 0)
        {
            Print.canMovePos = false;
            Console.Clear();
            Navigation.currentPos = 0;
            Navigation.consolePosLast = 0;

            currentWindow = "chatMessagesWindow";
            windowType = "pages";
            showCursor = false;
            topPad = 1;
            leftPad = 2;
            maxPos = Settings.chatMessages.Count + 1; // +1 for "new message" row
            int messageWidth = SizeHandler.WindowWidth - (leftPad * 2) - 6; // calclate the amount of characters to display of each messages before cropping it
            totalRows = SizeHandler.WindowHeight - 4; // calculate rows per page
            if (maxPos > totalRows) maxPos = totalRows;

            {
                double totalPagesTmp = ((double)Settings.chatMessages.Count + 1) / (double)totalRows;
                int totalPagesTmp2 = (int)Math.Ceiling(totalPagesTmp);
                totalPages = totalPagesTmp2;
            }

            int currentConsoleRow = topPad;
            int currentMessagePrint = 0;
            int startingIndex = pageToLoad * totalRows;

            // Print all messages
            foreach (var message in Settings.chatMessages)
            {
                if (startingIndex > 0)
                {
                    startingIndex--;
                    continue;
                }
                if (currentMessagePrint + 1 > totalRows) break;
                // Limit messages to console width, crop & ellipsis if message length exeeded
                string messageOutput = message.Length > messageWidth ? message.Substring(0, messageWidth - 3) + "..." : message;
                Print.printWhenPossible(messageOutput, currentConsoleRow++, leftPad + 3, false);
                currentMessagePrint++;
            }

            // Add a button to create a new message
            if (!(currentMessagePrint + 1 > totalRows)) Print.printWhenPossible("[new message]", currentConsoleRow++, leftPad + 3, false); // +1 for "new message" row
            if (totalPages > 1)
            {
                string pagesPrint = Print.centerString("Current page: " + (pageToLoad + 1) + " / " + totalPages)[0];
                pagesPrint = Print.replaceAt(pagesPrint, "<- previous page", leftPad + 3);
                pagesPrint = Print.replaceAt(pagesPrint, "next page ->", SizeHandler.WindowWidth - 17);
                Print.printWhenPossible(pagesPrint, SizeHandler.WindowHeight - 2, 0, false);
            }
            Print.canMovePos = true;
            Navigation.HandlePointerMovementPrint();
        }

        public static void ChatMessagesEdit()
        {
            Print.canMovePos = false;
            Console.Clear();
            Navigation.currentPos = 0;
            Navigation.consolePosLast = 0;

            currentWindow = "chatMessagesEdit";
            windowType = "messageEdit";
            showCursor = true;
            topPad = SizeHandler.HeightCenter - 2;
            leftPad = SizeHandler.WidthCenter;
            maxPos = 3;

            if (Settings.chatMessages.Count > messageIndex) Navigation.currentInput = Settings.chatMessages[messageIndex];
            else  Navigation.currentInput = "";
            UpdateMessageEdit();
            Print.printCentered("Save          Delete         Cancel", topPad + 3, false);
            Print.canMovePos = true;
            Navigation.HandlePointerMovementPrint();
            UpdateCursorPosition();
        }

        public static void UpdateMessageEdit() // this is wrong somewhere, prints line before it should, eg: 
        // xyz:
        // xyz: youtube.com/watch?qwerffgqwefqwefwqefqew
        {
            string message = Navigation.currentInput;
            int chunkLength = 100; // Length of each chunk

            List<string> chunks = new List<string>();

            // Extract chunks from the input message
            for (int i = 0; i < message.Length; i += chunkLength)
            {
                int length = Math.Min(chunkLength, message.Length - i);
                chunks.Add(message.Substring(i, length));
            }

            string chunk1 = chunks.Count > 0 ? chunks[0] : "";
            string chunk2 = chunks.Count > 1 ? chunks[1] : "";

            Console.CursorVisible = false;
            if (chunk2.Length == 0) Print.printCentered(chunk2, topPad + 1, false, true); // second line wiped if contents
            Print.printCentered(chunk1, topPad, false, true); // first line
            if (chunk2.Length > 0) Print.printCentered(chunk2, topPad + 1, false, true); // only print second line if needed
            DisplayCursorIfNeeded();
        }

        public static void UpdateCursorPosition()
        {
            Console.SetCursorPosition(cursorPosition[0], cursorPosition[1]);
        }

        public static void DisplayCursorIfNeeded()
        {
            if (showCursor) Console.CursorVisible = true;
        }
    }
}
