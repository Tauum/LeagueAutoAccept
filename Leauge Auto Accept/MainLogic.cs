﻿using System;
using System.Linq;
using System.Threading;

//1. (play -> lanes -> search -> normal path)
//2. setup custom profile select as menu option (champs/lanes)
//3. (title, champ, lanes, rune, spells)
//4. backup ban / ban priority based on role
//5. fix option to allow "close client while ingame" - so it does not break this

// currently: auto honor and honor skip enablle but seem unmanaged state
// and enabling skip statistics will completely break?
// once enabling setting works test and start on above
namespace Leauge_Auto_Accept
{
    public class MainLogic
    {
        public static GameState gameState =  new GameState(new GameState.StateFlags(false));
        private static long lastActStartTime;
        private static long queueStartTime;
        private static long champSelectStart;
        private static string lastActId = "";
        private static string lastChatRoom = "";
        private static string lastPhase = "";

        public static void AcceptQueue()
        {
            while (true)
            {
            if(!gameState.Flags.IsAutoAcceptOn) Thread.Sleep(1000);
            string[] gameSession = LCU.clientRequest("GET", "lol-gameflow/v1/session");
            if (gameSession[0] != "200") Thread.Sleep(50);    
            string phase = gameSession[1].Split("phase").Last().Split('"')[2];
            if (Settings.autoRestartQueue) HandleQueueRestart(phase);
            if (phase != "ChampSelect") lastChatRoom = "";
                switch (phase)
                {
                    case "Lobby":
                        Thread.Sleep(5000);
                        break;
                    case "Matchmaking":
                        HandleMatchmakingCancel();
                        Thread.Sleep(2000);
                        break;
                    case "ReadyCheck":
                        HandleMatchmakingAccept();
                        break;
                    case "ChampSelect":
                        HandleChampSelect();
                        HandlePickOrderSwap();
                        break;
                    case "PreEndOfGame": // Honor screen
                        HandleHonorScreen();
                        break;
                    case "WaitingForStats": // Waiting for stats screen (nice game riot)
                        HandleWaitingForStats();
                        break;
                    case "InProgress": // In game
                    case "EndOfGame": // End of game stats screen
                        Thread.Sleep(5000);
                        break;
                    default:
                        //Debug.WriteLine(phase);
                        // TODO: add more special cases?
                        Thread.Sleep(1000);
                        break;
                }
            }
        }

        private static void HandleHonorScreen()
        {
            if (!Settings.autoHonor && !Settings.autoHonorTeammatesOrSkip) return;
            if (Settings.autoHonorTeammatesOrSkip) LCU.clientRequest("POST", "lol-honor/v1/honor-teammates", "{\"honorAll\":true}"); // honor all teammates
            LCU.clientRequest("POST", "lol-gameflow/v1/complete-honor-phase", "{}");
                Thread.Sleep(1000);
        }

        private static void HandleWaitingForStats()
        {
            if (!Settings.autoStatsSkip) return;

            // Close the stats screen by transitioning to the next phase
            LCU.clientRequest("POST", "lol-gameflow/v1/complete-stats-phase", "{}");

            // Wait a bit to ensure the phase change is processed
            Thread.Sleep(1000);
        }

        private static void HandleMatchmakingCancel()
        {
            if (!Settings.cancelQueueAfterDodge) return;
            if (lastChatRoom != "")
            {
                LCU.clientRequest("DELETE", "lol-lobby/v2/lobby/matchmaking/search");
                lastChatRoom = "";
            }
        }

        private static void HandleMatchmakingAccept()
        {
            if (!Settings.cancelQueueAfterDodge || string.IsNullOrEmpty(lastChatRoom)) LCU.clientRequest("POST", "lol-matchmaking/v1/ready-check/accept");
            else
            {
                LCU.clientRequest("POST", "lol-matchmaking/v1/ready-check/decline");
                lastChatRoom = "";
            }
        }

        private static void HandleQueueRestart(string phase)
        {
            if (phase == "Matchmaking" && phase != lastPhase) queueStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            else if (phase == "Matchmaking")
            {
                long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                if ((currentTime - Settings.queueMaxTime) > queueStartTime)
                {
                    LCU.clientRequest("DELETE", "lol-lobby/v2/lobby/matchmaking/search");
                    LCU.clientRequest("POST", "lol-lobby/v2/lobby/matchmaking/search");
                    queueStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                }
            }
            lastPhase = phase;
        }

        private static void HandleChampSelect()
        {
            // Get data for the current ongoing champ select
            string[] currentChampSelect = LCU.clientRequest("GET", "lol-champ-select/v1/session");

            if (currentChampSelect[0] != "200") return;
            string currentChatRoom = currentChampSelect[1].Split("multiUserChatId\":\"")[1].Split('"')[0]; // Get needed data from the current champ select
            if (lastChatRoom != currentChatRoom || lastChatRoom == ""){
                gameState.ResetState();
                champSelectStart = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }
            lastChatRoom = currentChatRoom;
            if (new[]
            {
                gameState.Flags.PickedChamp,
                gameState.Flags.LockedChamp,
                gameState.Flags.PickedBan,
                gameState.Flags.LockedBan,
                gameState.Flags.PickedSpell1,
                gameState.Flags.PickedSpell2,
                gameState.Flags.SentChatMessages
            }.All(flag => flag))
                Thread.Sleep(1000); // IF all conditions are met Sleep
            else
            {
                string localPlayerCellId = currentChampSelect[1].Split("localPlayerCellId\":")[1].Split(',')[0]; // Get more data from current champ select
                if (Settings.currentChamp[1] == "0")
                {
                    gameState.Flags.PickedChamp = true;
                    gameState.Flags.LockedChamp = true;
                }
                if (Settings.currentBan[1] == "0")
                {
                    gameState.Flags.PickedBan = true;
                    gameState.Flags.LockedBan = true;
                }
                if (Settings.currentSpell1[1] == "0") gameState.Flags.PickedSpell1 = true;
                if (Settings.currentSpell2[1] == "0") gameState.Flags.PickedSpell2 = true;
                if (!Settings.chatMessagesEnabled) gameState.Flags.SentChatMessages = true;
                else if (Settings.chatMessages.Count == 0) gameState.Flags.SentChatMessages = true;
                if (!gameState.Flags.PickedChamp || !gameState.Flags.LockedChamp || !gameState.Flags.PickedBan || !gameState.Flags.LockedBan) HandleChampSelectActions(currentChampSelect, localPlayerCellId);
                if (!gameState.Flags.SentChatMessages) HandleChampSelectChat(currentChatRoom);
                if (!gameState.Flags.PickedSpell1)
                {
                    string[] champSelectAction = LCU.clientRequest("PATCH", "lol-champ-select/v1/session/my-selection", "{\"spell1Id\":" + Settings.currentSpell1[1] + "}");
                    if (champSelectAction[0] == "204") gameState.Flags.PickedSpell1 = true;
                }
                if (!gameState.Flags.PickedSpell2)
                {
                    string[] champSelectAction = LCU.clientRequest("PATCH", "lol-champ-select/v1/session/my-selection", "{\"spell2Id\":" + Settings.currentSpell2[1] + "}");
                    if (champSelectAction[0] == "204") gameState.Flags.PickedSpell2 = true;
                }
            }
            
        }

        private static void HandleChampSelectChat(string chatId)
        {
            string[] chats = LCU.clientRequest("GET", "lol-chat/v1/conversations", "");
            if (chats[1].Contains(chatId))
            {
                Data.LoadPlayerChatId();
                long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                if ((currentTime - Settings.chatMessagesDelay) > champSelectStart) HandleChampSelectChatSendMsg(chatId);

            }
        }

        private static void HandleChampSelectChatSendMsg(string chatId)
        {
            foreach (var message in Settings.chatMessages)
            {
                int attempts = 0;
                string httpRes = "";
                while (httpRes != "200" && attempts < 3)
                {
                    string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                    string body = "{\"type\":\"chat\",\"fromId\":\"" + Data.currentChatId + "\",\"fromSummonerId\":" + Data.currentSummonerId + ",\"isHistorical\":false,\"timestamp\":\"" + timestamp + "\",\"body\":\"" + message + "\"}";
                    string[] response = LCU.clientRequest("POST", "lol-chat/v1/conversations/" + chatId + "/messages", body);
                    attempts++;
                    httpRes = response[0];
                    Thread.Sleep(attempts * 20);
                }
            }
            gameState.Flags.SentChatMessages = true;
        }

        private static void HandleChampSelectActions(string[] currentChampSelect, string localPlayerCellId)
        {
            string csActs = currentChampSelect[1].Split("actions\":[[{")[1].Split("}]],")[0];
            csActs = csActs.Replace("}],[{", "},{");
            string[] csActsArr = csActs.Split("},{");

            foreach (var act in csActsArr)
            {
                string ActCctorCellId = act.Split("actorCellId\":")[1].Split(',')[0];
                string ActCompleted = act.Split("completed\":")[1].Split(',')[0];
                string ActType = act.Split("type\":\"")[1].Split('"')[0];
                string championId = act.Split("championId\":")[1].Split(',')[0];
                string actId = act.Split(",\"id\":")[1].Split(',')[0];
                string ActIsInProgress = act.Split("isInProgress\":")[1].Split(',')[0];

                if (ActCctorCellId == localPlayerCellId && ActCompleted == "false" && ActType == "pick") HandlePickAction(actId, championId, ActIsInProgress, currentChampSelect);
                else if (ActCctorCellId == localPlayerCellId && ActCompleted == "false" && ActType == "ban") HandleBanAction(actId, championId, ActIsInProgress, currentChampSelect);
            }
        }

        private static void HandlePickAction(string actId, string championId, string ActIsInProgress, string[] currentChampSelect)
        {
            if (!gameState.Flags.PickedChamp)
            {
                // Hover champion when champ select starts
                string champSelectPhase = currentChampSelect[1].Split("\"phase\":\"")[1].Split('"')[0];
                long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                if ((currentTime - Settings.pickStartHoverDelay) > champSelectStart // Check if enough time has passed since planning phase has started
                    || champSelectPhase != "PLANNING" // Check if it's even planning phase at all
                    || Settings.instantHover) // Check if instahover setting is on
                {
                    HoverChampion(actId, Settings.currentChamp[1], "pick"); // attempt first choice
                    if (!gameState.Flags.PickedChamp) HoverChampion(actId, Settings.secondaryChamp[1], "pick"); // attempt second choice
                }
            }

            if (ActIsInProgress == "true")
            {
                MarkPhaseStart(actId);
                if (!gameState.Flags.LockedChamp)
                {
                    // Check the instalock setting
                    if (!Settings.instaLock) CheckLockDelay(actId, championId, currentChampSelect, "pick");
                    else LockChampion(actId, championId, "pick");
                }
            }
        }

        private static void HandleBanAction(string actId, string championId, string ActIsInProgress, string[] currentChampSelect)
        {
            string champSelectPhase = currentChampSelect[1].Split("\"phase\":\"")[1].Split('"')[0];
            if (ActIsInProgress == "true" && champSelectPhase != "PLANNING") // verify not lobby initialise before pick phase
            {
                MarkPhaseStart(actId);
                if (!gameState.Flags.PickedBan) //hover champion
                {
                    long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    if ((currentTime - Settings.banStartHoverDelay) > champSelectStart)  HoverChampion(actId, Settings.currentBan[1], "ban");
                }

                if (!gameState.Flags.LockedBan) // instaban
                {
                    if (!Settings.instaBan) CheckLockDelay(actId, championId, currentChampSelect, "ban");
                    else LockChampion(actId, championId, "ban");
                }
            }
        }

        private static void MarkPhaseStart(string actId)
        {
            if (actId != lastActId)
            {
                lastActId = actId;
                lastActStartTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            }
        }

        private static void HoverChampion(string actId, string currentChamp, string actType)
        {
            string[] champSelectAction = LCU.clientRequest("PATCH", "lol-champ-select/v1/session/actions/" + actId, "{\"championId\":" + currentChamp + "}");
            if (champSelectAction[0] == "204")
            {
                if (actType == "pick")  gameState.Flags.PickedChamp = true;
                else if (actType == "ban") gameState.Flags.PickedBan = true;
            }
        }

        private static void LockChampion(string actId, string championId, string actType)
        {
            string[] champSelectAction = LCU.clientRequest("PATCH", "lol-champ-select/v1/session/actions/" + actId, "{\"completed\":true,\"championId\":" + championId + "}");
            if (champSelectAction[0] == "204")
            {
                if (actType == "pick")  gameState.Flags.LockedChamp = true;
                else if (actType == "ban")  gameState.Flags.LockedBan = true;
            }
        }

        private static void CheckLockDelay(string actId, string championId, string[] currentChampSelect, string actType)
        {
            string timer = currentChampSelect[1].Split("totalTimeInPhase\":")[1].Split("}")[0];
            long timerInt = Convert.ToInt64(timer);
            long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            int delayPreEnd = 0;
            if (actType == "pick") delayPreEnd = Settings.pickEndlockDelay;
            else if (actType == "ban") delayPreEnd = Settings.banEndlockDelay;
            if (currentTime >= lastActStartTime + timerInt - delayPreEnd)
            {
                LockChampion(actId, championId, actType);
                return;
            }

            int delayAfterStart = 0;
            if (actType == "pick") delayAfterStart = Settings.pickStartlockDelay;
            else if (actType == "ban") delayAfterStart = Settings.banStartlockDelay;

            if (currentTime >= lastActStartTime + delayAfterStart) LockChampion(actId, championId, actType);
        }
        
        private static void HandlePickOrderSwap()
        {
            if (!Settings.autoPickOrderTrade || gameState.Flags.LockedChamp) return; // Return if already locked in or settings off
            string[] swap = LCU.clientRequest("GET", "lol-champ-select/v1/ongoing-swap");
            if (swap[0] == "200")
            {
                if (swap.Contains("initiatedByLocalPlayer\":true")) return; // called by local player
                string swapId = swap[1].Split("\"id\":")[1].Split(',')[0]; // action ID
                LCU.clientRequest("POST", "lol-champ-select/v1/session/swaps/" + swapId + "/accept");
                LCU.clientRequest("POST", "lol-champ-select/v1/ongoing-swap/" + swapId + "/clear");
            }
        }
    }
}
