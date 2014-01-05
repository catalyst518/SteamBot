/*
~ TokenUserHandler.cs ~

Token selling bot written by Catalyst (http://steamcommunity.com/id/Catalyst7/)

~ Credit where credit is due ~
Thanks to: 
   * waylaidwanderer for writing the scrapbanking bot which was the foundation for this bot
   * Jessecar96 and Geel9 for coding SteamBot
*/


using SteamKit2;
using SteamTrade;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using System.IO;
using System.Linq;
using System.Text;
using SteamKit2.GC;

namespace SteamBot
{
    public class TokenUserHandler : UserHandler
    {
        public static int InviteTimerInterval = 2000;

        public uint userScrapAdded = 0;
        public uint botScrapAdded = 0;
        public uint botTokenAdded = 0;
        public uint invalidItem = 0;
        public uint inventoryTokensBefore = 0;
        public uint inventoryTokensAfter = 0;
        public uint inventoryScrap = 0;
        public uint inventoryReclaimed = 0;
        public uint inventoryRefined = 0;
        public uint inventoryWeps = 0;
        public uint inventoryMedicToken = 0;
        public uint inventoryPyroToken = 0;
        public uint inventorySoldierToken = 0;
        public uint inventoryEngineerToken = 0;
        public uint inventoryDemomanToken = 0;
        public uint inventoryScoutToken = 0;
        public uint inventorySpyToken = 0;
        public uint inventorySniperToken = 0;
        public uint inventoryHeavyToken = 0;
        public uint inventoryPrimaryToken = 0;
        public uint inventorySecondaryToken = 0;
        public uint inventoryMeleeToken = 0;
        public uint inventoryPDAToken = 0;
        public uint inventoryMedicWeapon = 0;
        public uint inventoryPyroWeapon = 0;
        public uint inventorySoldierWeapon = 0;
        public uint inventoryEngineerWeapon = 0;
        public uint inventoryDemomanWeapon = 0;
        public uint inventoryScoutWeapon = 0;
        public uint inventorySpyWeapon = 0;
        public uint inventorySniperWeapon = 0;
        public uint inventoryHeavyWeapon = 0;
        public uint inventoryPrimaryWeapon = 0;
        public uint inventorySecondaryWeapon = 0;
        public uint inventoryMeleeWeapon = 0;
        public uint inventoryPDAWeapon = 0;
        public uint desiredTokenInventory = 0;
        public uint desiredQuantity = 0;
        public uint sortytype = 0;
        public uint inventorySaw = 0;

        public bool errorMsgRun = false;
        public bool remove = false;
        public bool timerEnabled = false;
        public bool askType = false;
        public bool askQuantity = false;
        public bool craftSuccess = false;
        public bool goToNextCraft = false;
        //public bool inTF2 = false;
        public bool tradeThanksMessageSent = false;
        public bool tradeBugsMessageSent = false;

        public short recipenum = 7;

        public string desiredToken = "";

        public System.Threading.Thread checkTrade;

        public List<ulong> craftedIDs = new List<ulong>();

        public System.Timers.Timer inviteMsgTimer = new System.Timers.Timer(InviteTimerInterval);

        static SteamID currentSID;

        public TokenUserHandler(Bot bot, SteamID sid)
            : base(bot, sid)
        {

        }

        public override void OnLoginCompleted()
        {
        }

        public override bool OnFriendAdd()
        {
            Bot.log.Success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ToString() + ") added me!");
            if (Bot.SteamFriends.GetFriendCount()>=250)
                RemoveAllFriends();
            inviteMsgTimer = new System.Timers.Timer();
            inviteMsgTimer.Interval = InviteTimerInterval;
            inviteMsgTimer.Elapsed += (sender, e) => OnInviteTimerElapsed(sender, e, EChatEntryType.ChatMsg);
            inviteMsgTimer.Enabled = true;
            return true;
        }

        public override void OnFriendRemove()
        {
            Bot.log.Success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ToString() + ") removed me!");
        }

        public override void OnMessage(string message, EChatEntryType type)
        {
            message = message.ToLower();

            //REGULAR chat commands
            if ((message.Contains("love") || message.Contains("luv") || message.Contains("<3")) && (message.Contains("y") || message.Contains("u")))
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "I love you lots. <3");
            }
            else if (message.Contains("<3"))
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "<3");
            }
            else if (message.Contains("fuck") || message.Contains("suck") || message.Contains("dick") || message.Contains("cock") || message.Contains("tit") || message.Contains("boob") || message.Contains("pussy") || message.Contains("vagina") || message.Contains("cunt") || message.Contains("penis"))
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "Sorry, but as a robot I cannot perform sexual functions.");
            }
            else if (message.Contains("thank"))
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, "You're welcome!");
            }
            else if (message.Contains("help"))
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Send a trade invite and then simply reply to my questions in the trade window with the type and quantity of tokens that you would like. Add the correct amount of metal due and click \"Ready\" to finish the trade at any time. You may type \"Clear\" in the trade window at any time to remove all added items. There is a 3 minute time limit per trade.");
            }
            // ADMIN commands
            else if (message == "self.restart" && IsAdmin)
            {
                // Starts a new instance of the program itself
                var filename = System.Reflection.Assembly.GetExecutingAssembly().Location;
                System.Diagnostics.Process.Start(filename);

                // Closes the current process
                Environment.Exit(0);
            }
            else if (message == ".friends" && IsAdmin)
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Removing friends...");
                RemoveAllFriends();
            }
            else if (message == ".canceltrade" && IsAdmin)
            {
                Trade.CancelTrade();
                Bot.SteamFriends.SendChatMessage(currentSID, EChatEntryType.ChatMsg, "My creator has forcefully canceled the trade. Whatever you were doing, he probably wants you to stop.");
            }
            else if (message == ".tf2" && IsAdmin)
            {
                LaunchTF2();
            }
            else if (message == ".qtf2" && IsAdmin)
            {
                Bot.SetGamePlaying(0);
                Bot.log.Warn("Current AppID is " + Bot.CurrentGame);
            }
            else if (message == "stock" && IsAdmin)
            {
                CountAllInventory();
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Tokens: " + inventoryMedicToken + "/12 Medic, " + inventoryPyroToken + "/15 Pyro, " + inventorySoldierToken + "/15 Soldier, " + inventoryEngineerToken + "/12 Engineer, " + inventoryDemomanToken + "/15 Demoman, " + inventoryScoutToken + "/18 Scout, " + inventorySpyToken + " Spy, " + inventorySniperToken + " Sniper, " + inventoryHeavyToken + "/15 Heavy, " + inventoryPrimaryToken + "/30 Primary, " + inventorySecondaryToken + "/30 Secondary, " + inventoryMeleeToken + "/30 Melee, and " + inventoryPDAToken + "/30 PDA tokens.");
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Weapons: " + inventoryMedicWeapon + " Medic, " + inventoryPyroWeapon + " Pyro, " + inventorySoldierWeapon + " Soldier, " + inventoryEngineerWeapon + " Engineer, " + inventoryDemomanWeapon + " Demoman, " + inventoryScoutWeapon + " Scout, " + inventorySpyWeapon + " Spy, " + inventorySniperWeapon + " Sniper, " + inventoryHeavyWeapon + " Heavy, " + inventoryPrimaryWeapon + " Primary, " + inventorySecondaryWeapon + " Secondary, " + inventoryMeleeWeapon + " Melee, and " + inventoryPDAWeapon + " PDA weapons.");
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Metal: " + inventoryScrap + " Scrap, " + inventoryReclaimed + " Reclaimed, and " + inventoryRefined + " Refined metal.");
                Bot.log.Success("Current stock:");
                Bot.log.Success("Tokens: " + inventoryMedicToken + " Medic, " + inventoryPyroToken + " Pyro, " + inventorySoldierToken + " Soldier, " + inventoryEngineerToken + " Engineer, " + inventoryDemomanToken + " Demoman, " + inventoryScoutToken + " Scout, " + inventorySpyToken + " Spy, " + inventorySniperToken + " Sniper, " + inventoryHeavyToken + " Heavy, " + inventoryPrimaryToken + " Primary, " + inventorySecondaryToken + " Secondary, " + inventoryMeleeToken + " Melee, and " + inventoryPDAToken + " PDA tokens.");
                Bot.log.Success("Weapons: " + inventoryMedicWeapon + " Medic, " + inventoryPyroWeapon + " Pyro, " + inventorySoldierWeapon + " Soldier, " + inventoryEngineerWeapon + " Engineer, " + inventoryDemomanWeapon + " Demoman, " + inventoryScoutWeapon + " Scout, " + inventorySpyWeapon + " Spy, " + inventorySniperWeapon + " Sniper, " + inventoryHeavyWeapon + " Heavy, " + inventoryPrimaryWeapon + " Primary, " + inventorySecondaryWeapon + " Secondary, " + inventoryMeleeWeapon + " Melee, and " + inventoryPDAWeapon + " PDA weapons.");
                Bot.log.Success("Metal: " + inventoryScrap + " Scrap, " + inventoryReclaimed + " Reclaimed, and " + inventoryRefined + " Refined metal.");
            }
            else if (message == "craft" && IsAdmin)
            {
                CountAllInventory();
                craftedIDs.Clear();
                LaunchTF2();
                System.Timers.Timer craftTimer = new System.Timers.Timer(40000);
                craftTimer.Elapsed += new ElapsedEventHandler(OnCraftElapsedInterval);
                timerEnabled = true;
                craftTimer.Enabled = true;
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Now moving to Demoman tokens...");
                goToNextCraft = false;
                while (inventoryDemomanWeapon >= 3 && inventoryDemomanToken < 15 && timerEnabled && !goToNextCraft)
                {
                    classTokenCraft("Demoman");
                    CountAllInventory();
                }
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Now moving to Soldier tokens...");
                goToNextCraft = false;
                while (inventorySoldierWeapon >= 3 && inventorySoldierToken < 15 && timerEnabled && !goToNextCraft)
                {
                    classTokenCraft("Soldier");
                    CountAllInventory();
                }
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Now moving to PDA tokens...");
                goToNextCraft = false;
                while (inventoryPDAWeapon >= 3 && timerEnabled && !goToNextCraft)
                {
                    slotTokenCraft("pda2");
                    CountAllInventory();
                }
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Now moving to Spy tokens...");
                goToNextCraft = false;
                while (inventorySpyWeapon >= 3 && timerEnabled && !goToNextCraft)
                {
                    classTokenCraft("Spy");
                    CountAllInventory();
                }
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Now moving to Sniper tokens...");
                goToNextCraft = false;
                while (inventorySniperWeapon >= 3 && timerEnabled && !goToNextCraft)
                {
                    classTokenCraft("Sniper");
                    CountAllInventory();
                }
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Now moving to Heavy tokens...");
                goToNextCraft = false;
                while (inventoryHeavyWeapon >= 3 && inventoryHeavyToken < 15 && timerEnabled && !goToNextCraft)
                {
                    classTokenCraft("Heavy");
                    CountAllInventory();
                }
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Now moving to Scout tokens...");
                goToNextCraft = false;
                while (inventoryScoutWeapon >= 3 && inventoryScoutToken < 18 && timerEnabled && !goToNextCraft)
                {
                    classTokenCraft("Scout");
                    CountAllInventory();
                }
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Now moving to Pyro tokens...");
                goToNextCraft = false;
                while (inventoryPyroWeapon >= 3 && inventoryPyroToken < 15 && timerEnabled && !goToNextCraft)
                {
                    classTokenCraft("Pyro");
                    CountAllInventory();
                }
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Now moving to Medic tokens...");
                goToNextCraft = false;
                while (inventoryMedicWeapon >= 3 && inventoryMedicToken < 12 && timerEnabled && !goToNextCraft)
                {
                    classTokenCraft("Medic");
                    CountAllInventory();
                }
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Now moving to Engineer tokens...");
                goToNextCraft = false;
                while (inventoryEngineerWeapon >= 3 && inventoryEngineerToken < 12 && timerEnabled && !goToNextCraft)
                {
                    classTokenCraft("Engineer");
                    CountAllInventory();
                }
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Now moving to Primary tokens...");
                goToNextCraft = false;
                while (inventoryPrimaryWeapon >= 3 && inventoryPrimaryToken < 30 && timerEnabled && !goToNextCraft)
                {
                    slotTokenCraft("primary");
                    CountAllInventory();
                }
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Now moving to Secondary tokens...");
                goToNextCraft = false;
                while (inventorySecondaryWeapon >= 3 && inventorySecondaryToken < 30 && timerEnabled && !goToNextCraft)
                {
                    slotTokenCraft("secondary");
                    CountAllInventory();
                }
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Now moving to Melee tokens...");
                goToNextCraft = false;
                while (inventoryMeleeWeapon >= 3 && inventoryMeleeToken < 30 && timerEnabled && !goToNextCraft)
                {
                    slotTokenCraft("melee");
                    CountAllInventory();
                }
                craftTimer.Enabled = false;
                craftTimer.Close();
                Bot.SetGamePlaying(0);
                Bot.log.Warn("Current AppID is " + Bot.CurrentGame);
            }
            else if (message == ".slot" && IsAdmin)
            {
                CountAllInventory();
                craftedIDs.Clear();
                LaunchTF2();
                while (inventoryPrimaryWeapon >= 3)
                {
                    slotTokenCraft("primary");
                    CountWeaponInventory();
                }
                while (inventorySecondaryWeapon >= 3)
                {
                    slotTokenCraft("secondary");
                    CountWeaponInventory();
                }
                while (inventoryMeleeWeapon >= 3)
                {
                    slotTokenCraft("Melee");
                    CountWeaponInventory();
                }
                while (inventoryPDAWeapon >= 3)
                {
                    slotTokenCraft("pda2");
                    CountWeaponInventory();
                }
                Bot.SetGamePlaying(0);
                Bot.log.Warn("Current AppID is " + Bot.CurrentGame);
            }
            else
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, type, Bot.ChatResponse);
            }
        }

        private void OnCraftElapsedInterval(object sender, ElapsedEventArgs e)
        {
            timerEnabled = false;
            Bot.log.Warn("Craft timer timed out");
        }

        public override bool OnTradeRequest()
        {
            Bot.log.Success(Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ToString() + ") has requested to trade with me!");
            return true;
        }

        public override void OnTradeError(string error)
        {
            Bot.SteamFriends.SendChatMessage(OtherSID,
                                              EChatEntryType.ChatMsg,
                                              "Error: " + error + "."
                                              );
            //Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Bugs or problems? Please contact my owner: http://steamcommunity.com/id/Catalyst7");
            Bot.log.Warn(error);
            Bot.SteamFriends.SetPersonaState(EPersonaState.LookingToTrade);
        }

        public override void OnTradeTimeout()
        {
            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Sorry, but you were either AFK or took too long, and the trade was canceled.");
            Bot.log.Warn("User was kicked because he was AFK.");
            Bot.SteamFriends.SetPersonaState(EPersonaState.LookingToTrade);
        }

        public override void OnTradeInit()
        {
            Bot.SteamFriends.SetPersonaState(EPersonaState.Busy);
            checkTrade = new System.Threading.Thread(CheckTradeState);
            checkTrade.Start(); 
            tradeThanksMessageSent = false;
            tradeBugsMessageSent = false;
            ReInit();
            Bot.log.Warn("Opened trade with user.");
            Trade.SendMessage("Welcome to the Token Bot! This bot sells tokens for 2 scrap each.");
            TradeCountInventory(true);
            inventoryTokensBefore = inventoryDemomanToken + inventoryEngineerToken + inventoryHeavyToken + inventoryMedicToken + inventoryMeleeToken + inventoryPDAToken + inventoryPrimaryToken + inventoryPyroToken + inventoryScoutToken + inventorySecondaryToken + inventorySniperToken + inventorySoldierToken + inventorySpyToken;
            Trade.SendMessage("Add the correct amount of metal due and click \"Ready\" to finish the trade at any time. Type \"clear\" at any time to remove all added items. You have 3 minutes to complete the trade.");
            Trade.SendMessage("What type of token would you like to buy?");
            askType = true;
        }

        private void CheckTradeState()
        {
            while (true)
            {
                if (Bot.CurrentTrade == null)
                {
                    OnTradeClose();
                    checkTrade.Abort();
                    break;
                }
                System.Threading.Thread.Sleep(30000);
            }
        }


        public void GiveChange()
        {
            if (userScrapAdded > botTokenAdded * 2 + botScrapAdded && botTokenAdded>0)
            {
                botScrapAdded += Trade.AddAllItemsByDefindex(5000, userScrapAdded - botTokenAdded * 2 + botScrapAdded);
                Bot.log.Success("I've added " + botScrapAdded + " scrap total!");
                if (botScrapAdded >= inventoryScrap)
                    Trade.SendMessage("Sorry, but I am out of scrap metal and am unable to provide anymore change.");
            }
            if (userScrapAdded < botTokenAdded * 2 + botScrapAdded && botTokenAdded > 0 && botScrapAdded > 0)
            {
                botScrapAdded -= Trade.RemoveAllItemsByDefindex(5000, botTokenAdded * 2 + botScrapAdded - userScrapAdded);
            }
        }

        public override void OnTradeAddItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
        {
            var item = Trade.CurrentSchema.GetItem(schemaItem.Defindex);
            Bot.log.Success("User added: " + schemaItem.Name);
            if (item.Defindex == 5000)
            {
                userScrapAdded++;
                GiveChange();
            }
            else if (item.Defindex == 5001)
            {
                userScrapAdded+=3;
                GiveChange();
            }
            else if (item.Defindex == 5002)
            {
                userScrapAdded+=9;
                GiveChange();
            }
            else if (!IsAdmin)
            {
                Trade.SendMessage(schemaItem.Name + " is not a valid item! Please remove it from the trade. I only accept metal.");
                invalidItem++;
                if (invalidItem >= 4)
                {
                    Trade.CancelTrade();
                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Stop messing around. This bot sells tokens and only accepts metal.");
                }
            }
        }

        public override void OnTradeRemoveItem(Schema.Item schemaItem, Inventory.Item inventoryItem)
        {
            var item = Trade.CurrentSchema.GetItem(schemaItem.Defindex);
            Bot.log.Success("User removed: " + schemaItem.Name);
            if (item.Defindex == 5000)
            {
                userScrapAdded--;
                GiveChange();
            }
            else if (item.Defindex == 5001)
            {
                userScrapAdded -= 3;
                GiveChange();
            }
            else if (item.Defindex == 5002)
            {
                userScrapAdded -= 9;
                GiveChange();
            }
            else if (!IsAdmin)
            {
                invalidItem--;
            }
        }

        public override void OnTradeMessage(string message)
        {
            Bot.log.Info("[TRADE MESSAGE] " + message);
            message = message.ToLower();
            if (message == "clear")
            {
                Trade.SendMessage("All added items will now be removed.");
                Bot.log.Warn("They asked to remove all items.");
                Trade.RemoveAllItems();
                botTokenAdded = 0;
                botScrapAdded = 0;
                TradeCountInventory(false);
                Trade.SendMessage("What type of token would you like to buy?");
                askType = true;
            }
            else if (message == "empty" && IsAdmin)
            {
                Trade.AddAllItemsByDefindex(5002, 0);
                Trade.AddAllItemsByDefindex(5021, 0);
                Bot.log.Warn("Giving all refined and keys to admin!");
            }
            else if (askType)
            {
                if (message == "medic")
                {
                    desiredToken = "Medic";
                    desiredTokenInventory = inventoryMedicToken;
                    TokenMessage();
                }
                else if (message == "spy")
                {
                    desiredToken = "Spy";
                    desiredTokenInventory = inventorySpyToken;
                    TokenMessage();
                }
                else if (message == "scout")
                {
                    desiredToken = "Scout";
                    desiredTokenInventory = inventoryScoutToken;
                    TokenMessage();
                }
                else if (message == "soldier")
                {
                    desiredToken = "Soldier";
                    desiredTokenInventory = inventorySoldierToken;
                    TokenMessage();
                }
                else if (message == "sniper")
                {
                    desiredToken = "Sniper";
                    desiredTokenInventory = inventorySniperToken;
                    TokenMessage();
                }
                else if (message == "pyro")
                {
                    desiredToken = "Pyro";
                    desiredTokenInventory = inventoryPyroToken;
                    TokenMessage();
                }
                else if (message == "heavy")
                {
                    desiredToken = "Heavy";
                    desiredTokenInventory = inventoryHeavyToken;
                    TokenMessage();
                }
                else if (message == "engineer")
                {
                    desiredToken = "Engineer";
                    desiredTokenInventory = inventoryEngineerToken;
                    TokenMessage();
                }
                else if (message == "demoman" || message == "demo")
                {
                    desiredToken = "Demoman";
                    desiredTokenInventory = inventoryDemomanToken;
                    TokenMessage();
                }
                else if (message == "primary")
                {
                    desiredToken = "Primary";
                    desiredTokenInventory = inventoryPrimaryToken;
                    TokenMessage();
                }
                else if (message == "secondary")
                {
                    desiredToken = "Secondary";
                    desiredTokenInventory = inventorySecondaryToken;
                    TokenMessage();
                }
                else if (message == "melee")
                {
                    desiredToken = "Melee";
                    desiredTokenInventory = inventoryMeleeToken;
                    TokenMessage();
                }
                else if (message == "pda" || message == "pda2")
                {
                    desiredToken = "PDA";
                    desiredTokenInventory = inventoryPDAToken;
                    TokenMessage();
                }
                else
                {
                    Trade.SendMessage("I'm sorry, but \"" + message + "\" is not a valid response. Please use one of the following responses: Medic, Scout, Spy, Soldier, Sniper, Pyro, Demoman, Engineer, Heavy, Primary, Secondary, Melee, PDA.");
                    Trade.SendMessage("What type of token would you like to buy?");
                }
            }
            else if (askQuantity)
            {
                if (message == "00")
                {
                    askQuantity = false;
                    Trade.SendMessage("What type of token would you like to buy?");
                    askType = true;
                }
                else
                {
                    int messageQuantity;
                    bool result = Int32.TryParse(message, out messageQuantity);
                    if (result)
                    {
                        uint umessagequantity = (uint)messageQuantity;
                        desiredQuantity = umessagequantity;
                        if (desiredQuantity <= desiredTokenInventory)
                        {
                            Trade.SendMessage("Okay. I will now add " + desiredQuantity + " " + desiredToken + " tokens.");
                            Bot.log.Success("I am adding " + desiredQuantity + " " + desiredToken + " tokens.");
                            if (desiredToken == "Scout")
                            {
                                botTokenAdded += Trade.AddAllItemsByDefindex(5003, desiredQuantity);
                                inventoryScoutToken -= desiredQuantity;
                            }
                            else if (desiredToken == "Sniper")
                            {
                                botTokenAdded += Trade.AddAllItemsByDefindex(5004, desiredQuantity);
                                inventorySniperToken -= desiredQuantity;
                            }
                            else if (desiredToken == "Soldier")
                            {
                                botTokenAdded += Trade.AddAllItemsByDefindex(5005, desiredQuantity);
                                inventorySoldierToken -= desiredQuantity;
                            }
                            else if (desiredToken == "Demoman")
                            {
                                botTokenAdded += Trade.AddAllItemsByDefindex(5006, desiredQuantity);
                                inventoryDemomanToken -= desiredQuantity;
                            }
                            else if (desiredToken == "Heavy")
                            {
                                botTokenAdded += Trade.AddAllItemsByDefindex(5007, desiredQuantity);
                                inventoryHeavyToken -= desiredQuantity;
                            }
                            else if (desiredToken == "Medic")
                            {
                                botTokenAdded += Trade.AddAllItemsByDefindex(5008, desiredQuantity);
                                inventoryMedicToken -= desiredQuantity;
                            }
                            else if (desiredToken == "Pyro")
                            {
                                botTokenAdded += Trade.AddAllItemsByDefindex(5009, desiredQuantity);
                                inventoryPyroToken -= desiredQuantity;
                            }
                            else if (desiredToken == "Spy")
                            {
                                botTokenAdded += Trade.AddAllItemsByDefindex(5010, desiredQuantity);
                                inventorySpyToken -= desiredQuantity;
                            }
                            else if (desiredToken == "Engineer")
                            {
                                botTokenAdded += Trade.AddAllItemsByDefindex(5011, desiredQuantity);
                                inventoryEngineerToken -= desiredQuantity;
                            }
                            else if (desiredToken == "Primary")
                            {
                                botTokenAdded += Trade.AddAllItemsByDefindex(5012, desiredQuantity);
                                inventoryPrimaryToken -= desiredQuantity;
                            }
                            else if (desiredToken == "Secondary")
                            {
                                botTokenAdded += Trade.AddAllItemsByDefindex(5013, desiredQuantity);
                                inventorySecondaryToken -= desiredQuantity;
                            }
                            else if (desiredToken == "Melee")
                            {
                                botTokenAdded += Trade.AddAllItemsByDefindex(5014, desiredQuantity);
                                inventoryMeleeToken -= desiredQuantity;
                            }
                            else if (desiredToken == "PDA")
                            {
                                botTokenAdded += Trade.AddAllItemsByDefindex(5018, desiredQuantity);
                                inventoryPDAToken -= desiredQuantity;
                            }
                            GiveChange();
                            askQuantity = false;
                            Trade.SendMessage("What type of token would you like to buy?");
                            askType = true;
                        }
                        else
                        {
                            Trade.SendMessage("I'm sorry, but \"" + message + "\" is not a valid quantity. Please enter a number between 0 and " + desiredTokenInventory + ".");
                            Trade.SendMessage("How many " + desiredToken + " tokens would you like to buy? I currently have " + desiredTokenInventory + ". Please enter \"00\" if you would like 0 " + desiredToken + " tokens.");
                        }
                    }
                    else
                    {
                        Trade.SendMessage("I'm sorry, but \"" + message + "\" is not a valid quantity. Please enter a number between 0 and " + desiredTokenInventory + ".");
                        Trade.SendMessage("How many " + desiredToken + " tokens would you like to buy? I currently have " + desiredTokenInventory + ". Please enter \"00\" if you would like 0 " + desiredToken + " tokens.");
                    }
                }
            }
        }

        public override void OnTradeReady(bool ready)
        {
            Trade.Poll();
            if (!ready)
            {
                Trade.SetReady(false);
                Bot.log.Warn("Unreadied trade.");
            }
            else if (IsAdmin)
            {
                Bot.log.Success("Admin is ready to trade!");
                Trade.SetReady(true);
            }
            else
            {
                Bot.log.Success("User is ready to trade!");
                if (Validate())
                {
                    Trade.SetReady(true);
                    Bot.log.Warn("I am ready.");
                }
            }
        }

        public override void OnTradeAccept()
        {
            if (IsAdmin || Validate())
            {
                bool success = Trade.AcceptTrade();
                if (success)
                {
                    Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Thanks for the trade! Please spread the word if you found this bot useful!");
                    Log.Success("Trade was successful!");
                    StreamWriter sold = new StreamWriter("sold.txt", true);
                    sold.WriteLine(DateTime.Now + " " + botTokenAdded + " " + OtherSID.ToString());
                    sold.Close();
                    Bot.SteamFriends.SendChatMessage(76561198040958199, EChatEntryType.ChatMsg, "Sold " + botTokenAdded + " tokens to " + Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ToString() + ")");
                    Bot.SteamFriends.SetPersonaState(EPersonaState.LookingToTrade);
                    tradeThanksMessageSent = true;
                }
                else
                {
                    Log.Warn("Trade might have failed.");
                    Bot.SteamFriends.SetPersonaState(EPersonaState.LookingToTrade);
                }
            }
            OnTradeClose();
        }

        public override void OnTradeClose()
        {
            CountAllInventory();
            inventoryTokensAfter = inventoryDemomanToken + inventoryEngineerToken + inventoryHeavyToken + inventoryMedicToken + inventoryMeleeToken + inventoryPDAToken + inventoryPrimaryToken + inventoryPyroToken + inventoryScoutToken + inventorySecondaryToken + inventorySniperToken + inventorySoldierToken + inventorySpyToken;
            if (!tradeThanksMessageSent && (inventoryTokensAfter < inventoryTokensBefore))
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Thanks for the trade! Please spread the word if you found this bot useful!");
                Log.Success("Trade was successful!");
                if (!IsAdmin)
                {
                    StreamWriter sold = new StreamWriter("sold.txt", true);
                    sold.WriteLine(DateTime.Now + " " + botTokenAdded + " " + OtherSID.ToString());
                    sold.Close();
                    Bot.SteamFriends.SendChatMessage(76561198040958199, EChatEntryType.ChatMsg, "Sold " + botTokenAdded + " tokens to " + Bot.SteamFriends.GetFriendPersonaName(OtherSID) + " (" + OtherSID.ToString() + ")");
                }
                Bot.SteamFriends.SetPersonaState(EPersonaState.LookingToTrade);
                tradeThanksMessageSent = true;
            }
            if (!tradeBugsMessageSent)
            {
                Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Bugs or problems? Please contact my owner: http://steamcommunity.com/id/Catalyst7");
                tradeBugsMessageSent = true;
            }
            Bot.SteamFriends.SetPersonaState(EPersonaState.LookingToTrade);
            base.OnTradeClose();
            CraftScrap();
        }

        public bool Validate()
        {
            userScrapAdded = 0;
            //botScrapAdded = 0;
            //botTokenAdded = 0;
            invalidItem = 0;
            Trade.Poll();
            List<string> errors = new List<string>();
            foreach (ulong id in Trade.OtherOfferedItems)
            {
                var item = Trade.OtherInventory.GetItem(id);
                if (item.Defindex == 5000)
                    userScrapAdded++;
                else if (item.Defindex == 5001)
                    userScrapAdded += 3;
                else if (item.Defindex == 5002)
                    userScrapAdded += 9;
                else
                {
                    var schemaItem = Trade.CurrentSchema.GetItem(item.Defindex);
                    errors.Add(schemaItem.Name + " is not a valid item! Please remove it from the trade. I only accept metal.");
                }
            }
            //foreach (ulong id in Trade.BotsOfferedItems)
            //{
            //    var item = Trade.OtherInventory.GetItem(id);
            //    if (item.Defindex == 5000)
            //        botScrapAdded++;
            //    else if (item.Defindex == 5001)
            //        botScrapAdded += 3;
            //    else if (item.Defindex == 5002)
            //        botScrapAdded += 9;
            //    else if (item.Defindex == 5018 || (item.Defindex >= 5003 && item.Defindex <= 5014))
            //        botTokenAdded++;
            //}

            if (invalidItem > 0)
            {
                errors.Add("You have given me invalid items! Please remove them!");
                Bot.log.Warn("User has invalid items put up!");
            }
            if (userScrapAdded > botTokenAdded * 2 + botScrapAdded)
            {
                //if (botScrapAdded < inventoryScrap)
                //{
                //    GiveChange();
                //    return false;
                //}
               // else
                {
                    if (botTokenAdded > 0)
                    {
                        errors.Add("You have added too much metal! Please remove " + (userScrapAdded - botTokenAdded * 2 - botScrapAdded) + " scrap.");
                        Bot.log.Warn("User has added too much metal!");
                    }
                    else
                    {
                        errors.Add("You have not requested any tokens yet, and I cannot accept your generosity at this time. If you would like to make a donation, please contact my owner: http://steamcommunity.com/id/Catalyst7");
                        Bot.log.Warn("User tried to give me free metal!");
                    }
                }
            }
            if (userScrapAdded < botTokenAdded * 2 && botTokenAdded > 0)
            {
                errors.Add("You have not added enough metal! You need to add " + (botTokenAdded * 2 - userScrapAdded) + " more scrap.");
                Bot.log.Warn("User has added too little metal!");
            }

            // send the errors
            if (errors.Count != 0)
                Trade.SendMessage("There were errors in your trade: ");

            foreach (string error in errors)
            {
                Trade.SendMessage(error);
            }

            return errors.Count == 0;
        }

        public void LaunchTF2()
        {
            System.Timers.Timer launchTimer = new System.Timers.Timer(2500);
            launchTimer.Elapsed += new ElapsedEventHandler(OnLaunchElapsedInterval);
            timerEnabled = true;
            //inTF2 = false;

            launchTimer.Enabled = true;
            Bot.SetGamePlaying(440);
            while (!Bot.inTF2 && timerEnabled)
            {
                
                System.Threading.Thread.Sleep(1000);
                //CallbackMsg msg = Bot.SteamClient.WaitForCallback(false);
                //msg.Handle<SteamGameCoordinator.MessageCallback>(callback =>
                //{
                //    if (callback.EMsg == 4004)
                //    {
                //        inTF2 = true;
                //        Bot.log.Warn("Current AppID is " + Bot.CurrentGame);
                //    }
                //});
            }
            launchTimer.Enabled = false;
            launchTimer.Close();
        }

        private void OnLaunchElapsedInterval(object sender, ElapsedEventArgs e)
        {
            timerEnabled = false;
            Bot.log.Warn("Launch timer timed out");
        }

        public void CraftScrap()
        {
            CountMetalInventory();
            uint possibleScrap = inventoryScrap + 3 * inventoryReclaimed + 9 * inventoryRefined;
            if ((inventoryScrap < 9 && (inventoryReclaimed+inventoryRefined)>0)||inventoryScrap>=12||inventoryReclaimed>=3)
            {
                craftedIDs.Clear();
                LaunchTF2();
                //smelt down to make scrap
                while (inventoryScrap < 9 && inventoryScrap!=possibleScrap)
                {
                    if (inventoryReclaimed > 0)
                    {
                        Inventory.Item reclaimed = Array.Find(Bot.MyInventory.Items, item => item.Defindex == 5001 && !craftedIDs.Contains(item.Id));
                        if (reclaimed != null)
                        {
                            Crafting.CraftItems(Bot, 22, reclaimed.Id);
                            craftedIDs.Add(reclaimed.Id);
                            Bot.log.Success("Smelted reclaimed");
                        }
                    }
                    else if (inventoryRefined > 0)
                    {
                        Inventory.Item refined = Array.Find(Bot.MyInventory.Items, item => item.Defindex == 5002 && !craftedIDs.Contains(item.Id));
                        if (refined != null)
                        {
                            Crafting.CraftItems(Bot, 23, refined.Id);
                            craftedIDs.Add(refined.Id);
                            Bot.log.Success("Smelted refined");
                        }
                    }
                    CountMetalInventory();
                }
                while (inventoryScrap >= 12)
                {
                    Inventory.Item scrap1 = Array.Find(Bot.MyInventory.Items, item => item.Defindex == 5000 && !craftedIDs.Contains(item.Id));
                    craftedIDs.Add(scrap1.Id);
                    Inventory.Item scrap2 = Array.Find(Bot.MyInventory.Items, item => item.Defindex == 5000 && !craftedIDs.Contains(item.Id));
                    craftedIDs.Add(scrap2.Id);
                    Inventory.Item scrap3 = Array.Find(Bot.MyInventory.Items, item => item.Defindex == 5000 && !craftedIDs.Contains(item.Id));
                    craftedIDs.Add(scrap3.Id);
                    if (scrap1 != null && scrap2 != null && scrap3 != null)
                    {
                        Crafting.CraftItems(Bot, 4, scrap1.Id, scrap2.Id, scrap3.Id);
                        Bot.log.Success("Combined scrap");
                        Thread.Sleep(500);
                    }
                    CountMetalInventory();
                }
                while (inventoryReclaimed >= 3)
                {
                    Inventory.Item reclaimed1 = Array.Find(Bot.MyInventory.Items, item => item.Defindex == 5001 && !craftedIDs.Contains(item.Id));
                    if (reclaimed1 != null)
                        craftedIDs.Add(reclaimed1.Id);
                    Inventory.Item reclaimed2 = Array.Find(Bot.MyInventory.Items, item => item.Defindex == 5001 && !craftedIDs.Contains(item.Id));
                    if (reclaimed2 != null)
                        craftedIDs.Add(reclaimed2.Id);
                    Inventory.Item reclaimed3 = Array.Find(Bot.MyInventory.Items, item => item.Defindex == 
                        5001 && !craftedIDs.Contains(item.Id));
                    if (reclaimed3 != null)
                        craftedIDs.Add(reclaimed3.Id);
                    if (reclaimed1 != null && reclaimed2 != null && reclaimed3 != null)
                    {
                        Crafting.CraftItems(Bot, 5, reclaimed1.Id, reclaimed2.Id, reclaimed3.Id);
                        Bot.log.Success("Combined reclaimed");
                    }
                    CountMetalInventory();
                }
                Bot.SetGamePlaying(0);
                Bot.log.Warn("Current AppID is " + Bot.CurrentGame);
            }
        }

        public void CountMetalInventory()
        {
            inventoryScrap = 0;
            inventoryReclaimed = 0;
            inventoryRefined = 0;
            Bot.GetInventory();
            foreach (Inventory.Item item in Bot.MyInventory.Items)
            {
                if (item.Defindex == 5000)
                    inventoryScrap++;
                else if (item.Defindex == 5001)
                    inventoryReclaimed++;
                else if (item.Defindex == 5002)
                    inventoryRefined++;
            }
        }

        public void TradeCountInventory(bool message)
        {
            // Let's count our inventory
            Inventory.Item[] inventory = Trade.MyInventory.Items;
            inventoryScrap = 0;
            inventoryMedicToken = 0;
            inventoryPyroToken = 0;
            inventorySoldierToken = 0;
            inventoryEngineerToken = 0;
            inventoryDemomanToken = 0;
            inventoryScoutToken = 0;
            inventorySpyToken = 0;
            inventorySniperToken = 0;
            inventoryHeavyToken = 0;
            inventoryPrimaryToken = 0;
            inventorySecondaryToken = 0;
            inventoryMeleeToken = 0;
            inventoryPDAToken = 0;
            foreach (Inventory.Item item in inventory)
            {
                if (item.Defindex == 5000)
                    inventoryScrap++;
                else if (item.Defindex == 5003)
                    inventoryScoutToken++;
                else if (item.Defindex == 5004)
                    inventorySniperToken++;
                else if (item.Defindex == 5005)
                    inventorySoldierToken++;
                else if (item.Defindex == 5006)
                    inventoryDemomanToken++;
                else if (item.Defindex == 5007)
                    inventoryHeavyToken++;
                else if (item.Defindex == 5008)
                    inventoryMedicToken++;
                else if (item.Defindex == 5009)
                    inventoryPyroToken++;
                else if (item.Defindex == 5010)
                    inventorySpyToken++;
                else if (item.Defindex == 5011)
                    inventoryEngineerToken++;
                else if (item.Defindex == 5012)
                    inventoryPrimaryToken++;
                else if (item.Defindex == 5013)
                    inventorySecondaryToken++;
                else if (item.Defindex == 5014)
                    inventoryMeleeToken++;
                else if (item.Defindex == 5018)
                    inventoryPDAToken++;
            }
            
            if (message)
            {
                Trade.SendMessage("Current stock: "+inventoryMedicToken+" Medic, "+inventoryPyroToken+" Pyro, "+inventorySoldierToken+" Soldier, "+inventoryEngineerToken+" Engineer, "+inventoryDemomanToken+" Demoman, "+inventoryScoutToken+" Scout, "+inventorySpyToken+" Spy, "+inventorySniperToken+" Sniper, "+inventoryHeavyToken+" Heavy, "+inventoryPrimaryToken+" Primary, "+inventorySecondaryToken+" Secondary, "+inventoryMeleeToken+" Melee, and "+inventoryPDAToken+" PDA tokens.");
                Bot.log.Success("Current stock: "+inventoryMedicToken+" Medic, "+inventoryPyroToken+" Pyro, "+inventorySoldierToken+" Soldier, "+inventoryEngineerToken+" Engineer, "+inventoryDemomanToken+" Demoman, "+inventoryScoutToken+" Scout, "+inventorySpyToken+" Spy, "+inventorySniperToken+" Sniper, "+inventoryHeavyToken+" Heavy, "+inventoryPrimaryToken+" Primary, "+inventorySecondaryToken+" Secondary, "+inventoryMeleeToken+" Melee, and "+inventoryPDAToken+" PDA tokens, and "+inventoryScrap+" scrap.");
            }
        }

        public void ReInit()
        {
            userScrapAdded = 0;
            botScrapAdded = 0;
            botTokenAdded = 0;
            inventoryScrap = 0;
            invalidItem = 0;
            errorMsgRun = false;
            remove = false;
            currentSID = OtherSID;
        }

        private void OnInviteTimerElapsed(object source, ElapsedEventArgs e, EChatEntryType type)
        {
            Bot.SteamFriends.SendChatMessage(OtherSID, EChatEntryType.ChatMsg, "Hi. You have added the Token Bot! I sell tokens for two scrap each. Check my backpack to see my current stock, and then trade me to begin!");
            Bot.log.Success("Sent welcome message.");
            inviteMsgTimer.Enabled = false;
            inviteMsgTimer.Stop();
        }

        public void TokenMessage()
        {
            if (desiredTokenInventory == 0)
            {
                Trade.SendMessage("I'm sorry, but I do not have any " + desiredToken + " tokens at the moment.");
                Trade.SendMessage("What type of token would you like to buy?");
            }
            else
            {
                askType = false;
                Trade.SendMessage("How many " + desiredToken + " tokens would you like to buy? I currently have " + desiredTokenInventory + ". Please enter \"00\" if you would like 0 " + desiredToken + " tokens.");
                askQuantity = true;
            }
        }

        public void CountAllInventory()
        {
            inventoryMedicWeapon = 0;
            inventoryPyroWeapon = 0;
            inventorySoldierWeapon = 0;
            inventoryEngineerWeapon = 0;
            inventoryDemomanWeapon = 0;
            inventoryScoutWeapon = 0;
            inventorySpyWeapon = 0;
            inventorySniperWeapon = 0;
            inventoryHeavyWeapon = 0;
            inventoryPrimaryWeapon = 0;
            inventorySecondaryWeapon = 0;
            inventoryMeleeWeapon = 0;
            inventoryPDAWeapon = 0;
            inventoryScrap = 0;
            inventoryReclaimed = 0;
            inventoryRefined = 0;
            inventoryMedicToken = 0;
            inventoryPyroToken = 0;
            inventorySoldierToken = 0;
            inventoryEngineerToken = 0;
            inventoryDemomanToken = 0;
            inventoryScoutToken = 0;
            inventorySpyToken = 0;
            inventorySniperToken = 0;
            inventoryHeavyToken = 0;
            inventoryPrimaryToken = 0;
            inventorySecondaryToken = 0;
            inventoryMeleeToken = 0;
            inventoryPDAToken = 0;
            Bot.GetInventory();
            foreach (Inventory.Item item in Bot.MyInventory.Items)
            {
                var itemToCheck = Trade.CurrentSchema.GetItem(item.Defindex);
                if (itemToCheck.CraftClass == "weapon")
                {
                    foreach (string usableclass in itemToCheck.UsableByClasses)
                    {
                        if (usableclass == "Medic")
                            inventoryMedicWeapon++;
                        else if (usableclass == "Pyro")
                            inventoryPyroWeapon++;
                        else if (usableclass == "Soldier")
                            inventorySoldierWeapon++;
                        else if (usableclass == "Engineer")
                            inventoryEngineerWeapon++;
                        else if (usableclass == "Demoman")
                            inventoryDemomanWeapon++;
                        else if (usableclass == "Scout")
                            inventoryScoutWeapon++;
                        else if (usableclass == "Spy" && itemToCheck.ItemSlot != "pda2")
                            inventorySpyWeapon++;
                        else if (usableclass == "Sniper")
                            inventorySniperWeapon++;
                        else if (usableclass == "Heavy")
                            inventoryHeavyWeapon++;
                    }
                    if (itemToCheck.ItemSlot == "primary")
                        inventoryPrimaryWeapon++;
                    else if (itemToCheck.ItemSlot == "secondary")
                        inventorySecondaryWeapon++;
                    else if (itemToCheck.ItemSlot == "melee")
                        inventoryMeleeWeapon++;
                    else if (itemToCheck.ItemSlot == "pda2")
                        inventoryPDAWeapon++;
                }
                else
                {
                    if (item.Defindex == 5000)
                        inventoryScrap++;
                    else if (item.Defindex == 5001)
                        inventoryReclaimed++;
                    else if (item.Defindex == 5002)
                        inventoryRefined++;
                    else if (item.Defindex == 5003)
                        inventoryScoutToken++;
                    else if (item.Defindex == 5004)
                        inventorySniperToken++;
                    else if (item.Defindex == 5005)
                        inventorySoldierToken++;
                    else if (item.Defindex == 5006)
                        inventoryDemomanToken++;
                    else if (item.Defindex == 5007)
                        inventoryHeavyToken++;
                    else if (item.Defindex == 5008)
                        inventoryMedicToken++;
                    else if (item.Defindex == 5009)
                        inventoryPyroToken++;
                    else if (item.Defindex == 5010)
                        inventorySpyToken++;
                    else if (item.Defindex == 5011)
                        inventoryEngineerToken++;
                    else if (item.Defindex == 5012)
                        inventoryPrimaryToken++;
                    else if (item.Defindex == 5013)
                        inventorySecondaryToken++;
                    else if (item.Defindex == 5014)
                        inventoryMeleeToken++;
                    else if (item.Defindex == 5018)
                        inventoryPDAToken++;
                }
            }
        }

        public void CountWeaponInventory()
        {
            inventoryMedicWeapon = 0;
            inventoryPyroWeapon = 0;
            inventorySoldierWeapon = 0;
            inventoryEngineerWeapon = 0;
            inventoryDemomanWeapon = 0;
            inventoryScoutWeapon = 0;
            inventorySpyWeapon = 0;
            inventorySniperWeapon = 0;
            inventoryHeavyWeapon = 0;
            inventoryPrimaryWeapon = 0;
            inventorySecondaryWeapon = 0;
            inventoryMeleeWeapon = 0;
            inventoryPDAWeapon = 0;
            Bot.GetInventory();
            foreach (Inventory.Item item in Bot.MyInventory.Items)
            {
                var itemToCheck = Trade.CurrentSchema.GetItem(item.Defindex);
                if (itemToCheck.CraftClass == "weapon")
                {
                    foreach (string usableclass in itemToCheck.UsableByClasses)
                    {
                        if (usableclass == "Medic")
                            inventoryMedicWeapon++;
                        else if (usableclass == "Pyro")
                            inventoryPyroWeapon++;
                        else if (usableclass == "Soldier")
                            inventorySoldierWeapon++;
                        else if (usableclass == "Engineer")
                            inventoryEngineerWeapon++;
                        else if (usableclass == "Demoman")
                            inventoryDemomanWeapon++;
                        else if (usableclass == "Scout")
                            inventoryScoutWeapon++;
                        else if (usableclass == "Spy")
                            inventorySpyWeapon++;
                        else if (usableclass == "Sniper")
                            inventorySniperWeapon++;
                        else if (usableclass == "Heavy")
                            inventoryHeavyWeapon++;
                    }
                    if (itemToCheck.ItemSlot == "primary")
                        inventoryPrimaryWeapon++;
                    else if (itemToCheck.ItemSlot == "secondary")
                        inventorySecondaryWeapon++;
                    else if (itemToCheck.ItemSlot == "melee")
                        inventoryMeleeWeapon++;
                    else if (itemToCheck.ItemSlot == "pda2")
                        inventoryPDAWeapon++;
                }
            }
        }

        private bool IsCorrectClass(Inventory.Item item, string correctClass)
        {
            var itemToCheck = Trade.CurrentSchema.GetItem(item.Defindex);
            if (itemToCheck.CraftClass == "weapon")
            {
                if (itemToCheck.ItemSlot == "pda2")
                    return false;
                foreach (string usableClass in itemToCheck.UsableByClasses)
                {
                    if (usableClass == correctClass)
                        return true;
                }
            }
            return false;
        }

        private bool IsCorrectSlot(Inventory.Item item, string correctSlot)
        {
            var itemToCheck = Trade.CurrentSchema.GetItem(item.Defindex);
            if (itemToCheck.CraftClass == "weapon")
            {
                foreach (string usableClass in itemToCheck.UsableByClasses)
                {
                    if (usableClass == "sniper" || usableClass == "spy")
                        return false;
                    else if (usableClass == "Medic" && inventoryMedicToken<12)
                        return false;
                    else if (usableClass == "Pyro" && inventoryPyroToken<15)
                        return false;
                    else if (usableClass == "Soldier" && inventorySoldierToken<15)
                        return false;
                    else if (usableClass == "Engineer" && inventoryEngineerToken<12)
                        return false;
                    else if (usableClass == "Demoman" && inventoryDemomanToken<15)
                        return false;
                    else if (usableClass == "Scout" && inventoryScoutToken<18)
                        return false;
                    else if (usableClass == "Heavy" && inventoryHeavyToken<15)
                        return false;
                }
                if (itemToCheck.ItemSlot == correctSlot)
                    return true;
            }
            return false;
        }

        public void classTokenCraft(string craftClass)
        {
            Bot.GetInventory();
            Inventory.Item weapon1 = Array.Find(Bot.MyInventory.Items, item => IsCorrectClass(item, craftClass) && !craftedIDs.Contains(item.Id));
            if (weapon1 != null)
                craftedIDs.Add(weapon1.Id);
            Inventory.Item weapon2 = Array.Find(Bot.MyInventory.Items, item => IsCorrectClass(item, craftClass) && !craftedIDs.Contains(item.Id));
            if (weapon2 != null)
                craftedIDs.Add(weapon2.Id);
            Inventory.Item weapon3 = Array.Find(Bot.MyInventory.Items, item => IsCorrectClass(item, craftClass) && !craftedIDs.Contains(item.Id));
            if (weapon3 != null)
                craftedIDs.Add(weapon3.Id);
            if (weapon1 != null && weapon2 != null && weapon3 != null)
            {
                Crafting.CraftItems(Bot, 7, weapon1.Id, weapon2.Id, weapon3.Id);
                Bot.log.Success("Crafted " + craftClass + " token!");
            }
            else
                goToNextCraft = true;
        }

        public void slotTokenCraft(string craftSlot)
        {
            Bot.GetInventory();
            Inventory.Item weapon1 = Array.Find(Bot.MyInventory.Items, item => IsCorrectSlot(item, craftSlot) && !craftedIDs.Contains(item.Id));
            if (weapon1 != null)
                craftedIDs.Add(weapon1.Id);
            Inventory.Item weapon2 = Array.Find(Bot.MyInventory.Items, item => IsCorrectSlot(item, craftSlot) && !craftedIDs.Contains(item.Id));
            if (weapon2 != null)
                craftedIDs.Add(weapon2.Id);
            Inventory.Item weapon3 = Array.Find(Bot.MyInventory.Items, item => IsCorrectSlot(item, craftSlot) && !craftedIDs.Contains(item.Id));
            if (weapon3 != null)
                craftedIDs.Add(weapon3.Id);
            if (weapon1 != null && weapon2 != null && weapon3 != null)
            {
                Crafting.CraftItems(Bot, 8, weapon1.Id, weapon2.Id, weapon3.Id);
                Bot.log.Success("Crafted " + craftSlot + " token!");
            }
            else
                goToNextCraft = true;
        }

        public void RemoveAllFriends()
        {
            Bot.log.Warn("Friends list is full. Removing 5 friends...");
            for (int count = 0; count < 5; count++)
            {
                SteamID friend = Bot.SteamFriends.GetFriendByIndex(count);
                ulong friendID = friend.ConvertToUInt64();
                bool isAdmin = false;
                for (int i = 0; i < Bot.Admins.Length; i++)
                {
                    if (Bot.Admins[i] == friendID)
                    {
                        isAdmin = true;
                        break;
                    }
                }
                if (isAdmin)
                {
                    count--;
                    continue;
                }
                else
                {
                    Bot.SteamFriends.SendChatMessage(friendID, EChatEntryType.ChatMsg, "I am about to remove you as a Steam Friend in order to allow more people to buy tokens from me. Thanks for trading with me. I hope you enjoyed our time together as much as I did. If you wish to buy tokens from me in the future, you can bookmark my Steam profile (http://steamcommunity.com/id/tokenbot/) in your favorite web browser and then add me again whenever you need me.");
                    Bot.SteamFriends.RemoveFriend(friendID);
                    Bot.log.Success("Removed friend: " + friendID);
                    System.Threading.Thread.Sleep(500);
                }
            }
        }
    }
}