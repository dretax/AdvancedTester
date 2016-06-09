﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fougerite;
using Fougerite.Events;
using UnityEngine;
using Random = System.Random;

namespace AdvancedTester
{
    public class AdvancedTester : Fougerite.Module
    {
        public const string blue = "[color #0099FF]";
        public const string red = "[color #FF0000]";
        public const string pink = "[color #CC66FF]";
        public const string teal = "[color #00FFFF]";
        public const string green = "[color #009900]";
        public const string purple = "[color #6600CC]";
        public const string white = "[color #FFFFFF]";
        public const string yellow = "[color #FFFF00]";

        public Dictionary<ulong, Dictionary<string, int>> storage;
        public Dictionary<ulong, int> Reports;
        public Dictionary<Vector3, ulong> OccupiedPositions;
        public List<Vector3> TestPlaces; 
        public Dictionary<ulong, TestData> UnderTesting;
        public Dictionary<ulong, int> TestCooldown;
        public Dictionary<ulong, Vector3> LastPos;
        public Dictionary<ulong, List<ulong>> ReportC;
        public Dictionary<int, Dictionary<int, string>> LanguageDict; 
        public List<string> RestrictedCommands; 
        public int ReportsNeeded = 3;
        public int TestAllowedEvery = 15;
        public int RecoilWait = 15;
        public IniParser Settings;

        public override string Name
        {
            get { return "AdvancedTester"; }
        }

        public override string Author
        {
            get { return "DreTaX"; }
        }

        public override string Description
        {
            get { return "Recoil/Jump Tester"; }
        }

        public override Version Version
        {
            get { return new Version("1.4.6"); }
        }

        public override void Initialize()
        {
            Fougerite.Hooks.OnCommand += On_Command;
            Fougerite.Hooks.OnPlayerSpawned += On_Spawn;
            Fougerite.Hooks.OnPlayerHurt += OnPlayerHurt;
            Fougerite.Hooks.OnPlayerDisconnected += OnPlayerDisconnected;
            RestrictedCommands = new List<string>();
            LanguageDict = new Dictionary<int, Dictionary<int, string>>();
            if (!File.Exists(Path.Combine(ModuleFolder, "Settings.ini")))
            {
                File.Create(Path.Combine(ModuleFolder, "Settings.ini")).Dispose();
                Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                Settings.AddSetting("Settings", "TestAllowedEvery", "15");
                Settings.AddSetting("Settings", "RecoilWait", "15");
                Settings.AddSetting("Settings", "ReportsNeeded", "3");
                Settings.AddSetting("Settings", "RestrictedCommands", "tpa,home,tpaccept,hg");
                Settings.AddSetting("Languages", "1", "English");
                Settings.AddSetting("Languages", "2", "Hungarian");
                Settings.AddSetting("Languages", "3", "Russian");
                Settings.AddSetting("English", "1", "Do not press F2/ F5 / Insert until the plugin says otherwise!");
                Settings.AddSetting("English", "2", "Disconnecting from the test will cause auto ban!");
                Settings.AddSetting("English", "3", "Take your M4 out, reload It, and shoot It!");
                Settings.AddSetting("English", "4", "Keep Pressing Insert");
                Settings.AddSetting("English", "5", "Keep Pressing F2");
                Settings.AddSetting("English", "6", "Keep Pressing F5");
                Settings.AddSetting("Hungarian", "1", "Ne nyomj F2 / F5 / Insert gombokat, amíg a plugin nem kéri!");
                Settings.AddSetting("Hungarian", "2", "A lecsatlakozás autómatikus bant okoz!");
                Settings.AddSetting("Hungarian", "3", "Vedd elő az M4-et, töltsd újra, és tüzelj párszor!");
                Settings.AddSetting("Hungarian", "4", "Nyomd folyamatosan az INSERT gombot");
                Settings.AddSetting("Hungarian", "5", "Nyomd folyamatosan az F2 gombot");
                Settings.AddSetting("Hungarian", "6", "Nyomd folyamatosan az F5 gombot");
                Settings.AddSetting("Russian", "1", "Не нажимайте F2 / F5 / Insert, пока плагин не говорит иначе!");
                Settings.AddSetting("Russian", "2", "Разъединители причины запрета!");
                Settings.AddSetting("Russian", "3", "Возьмите M4 из, перезагрузить его, и стрелять из него!");
                Settings.AddSetting("Russian", "4", "Продолжайте нажимать Insert");
                Settings.AddSetting("Russian", "5", "Продолжайте нажимать F2");
                Settings.AddSetting("Russian", "6", "Продолжайте нажимать F5");
                Settings.Save();
            }
            Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
            TestAllowedEvery = int.Parse(Settings.GetSetting("Settings", "TestAllowedEvery"));
            RecoilWait = int.Parse(Settings.GetSetting("Settings", "RecoilWait"));
            ReportsNeeded = int.Parse(Settings.GetSetting("Settings", "ReportsNeeded"));
            var cmds = Settings.GetSetting("Settings", "RestrictedCommands").Split(Convert.ToChar(","));
            foreach (var x in cmds)
            {
                RestrictedCommands.Add(x);
            }
            var langcodes = Settings.EnumSection("Languages");
            foreach (var x in langcodes)
            {
                var lang = Settings.GetSetting("Languages", x);
                var langms = Settings.EnumSection(lang);
                Dictionary<int, string> langmdata = new Dictionary<int, string>();
                foreach (var y in langms)
                {
                    langmdata[int.Parse(y)] = Settings.GetSetting(lang, y);
                }
                LanguageDict[int.Parse(x)] = langmdata;
            }
            storage = new Dictionary<ulong, Dictionary<string, int>>();
            Reports = new Dictionary<ulong, int>();
            OccupiedPositions = new Dictionary<Vector3, ulong>();
            UnderTesting = new Dictionary<ulong, TestData>();
            TestCooldown = new Dictionary<ulong, int>();
            LastPos = new Dictionary<ulong, Vector3>();
            ReportC = new Dictionary<ulong, List<ulong>>();
            TestPlaces = new List<Vector3>();
            TestPlaces.Add(new Vector3(-5599, 403, -2989));
            TestPlaces.Add(new Vector3(-5594, 403, -2985));
            TestPlaces.Add(new Vector3(-5589, 403, -2981));
            TestPlaces.Add(new Vector3(-5585, 402, -2978));
            TestPlaces.Add(new Vector3(-5579, 401, -2973));
            TestPlaces.Add(new Vector3(-5570, 398, -2966));
            TestPlaces.Add(new Vector3(-5562, 396, -2960));
            TestPlaces.Add(new Vector3(-5555, 394, -2954));
            TestPlaces.Add(new Vector3(-5546, 393, -2947));
            TestPlaces.Add(new Vector3(-5520, 388, -2943));
            DataStore.GetInstance().Flush("RecoilTest");
            DataStore.GetInstance().Flush("JumpTest");
        }

        public override void DeInitialize()
        {
            Fougerite.Hooks.OnCommand -= On_Command;
            Fougerite.Hooks.OnPlayerSpawned -= On_Spawn;
            Fougerite.Hooks.OnPlayerHurt -= OnPlayerHurt;
            Fougerite.Hooks.OnPlayerDisconnected -= OnPlayerDisconnected;
        }

        public void OnPlayerDisconnected(Fougerite.Player player)
        {
            if (UnderTesting.ContainsKey(player.UID))
            {
                Server.GetServer().BanPlayer(player, "Console", "Disconnecting from AdvancedTest", null, true);
                RemoveTest(player, true);
            }
        }

        public void OnPlayerHurt(HurtEvent he)
        {
            if (he.VictimIsPlayer && he.Victim != null)
            {
                Fougerite.Player player = (Fougerite.Player) he.Victim;
                if (UnderTesting.ContainsKey(player.UID))
                {
                    if (he.WeaponName == "Fall Damage")
                    {
                        UnderTesting[player.UID].FallComplete = true;
                    }
                    he.DamageAmount = 0f;
                }
            }
        }

        public void On_Command(Fougerite.Player player, string cmd, string[] args)
        {
            if (cmd == "advancedtest")
            {
                player.MessageFrom("AdvancedTest", green + "AdvancedTest " + yellow + " V" + Version + " [COLOR#FFFFFF] By DreTaX");
                if (player.Admin)
                {
                    Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                    TestAllowedEvery = int.Parse(Settings.GetSetting("Settings", "TestAllowedEvery"));
                    RecoilWait = int.Parse(Settings.GetSetting("Settings", "RecoilWait"));
                    ReportsNeeded = int.Parse(Settings.GetSetting("Settings", "ReportsNeeded"));
                    var cmds = Settings.GetSetting("Settings", "RestrictedCommands").Split(Convert.ToChar(","));
                    foreach (var x in cmds)
                    {
                        RestrictedCommands.Add(x);
                    }
                    var langcodes = Settings.EnumSection("Languages");
                    foreach (var x in langcodes)
                    {
                        var lang = Settings.GetSetting("Languages", x);
                        var langms = Settings.EnumSection(lang);
                        Dictionary<int, string> langmdata = new Dictionary<int, string>();
                        foreach (var y in langms)
                        {
                            langmdata[int.Parse(y)] = Settings.GetSetting(lang, y);
                        }
                        LanguageDict[int.Parse(x)] = langmdata;
                    }
                }
            }
            else if (cmd == "alang")
            {
                if (args.Length == 0)
                {
                    var langcodes = Settings.EnumSection("Languages");
                    string langs = langcodes.Aggregate("",
                        (current, x) => current + x + "=" + Settings.GetSetting("Languages", x) + ", ");
                    player.MessageFrom("AdvancedTest", green + "Usage /alang number");
                    player.MessageFrom("AdvancedTest", langs);
                }
                else
                {
                    string s = string.Join(" ", args);
                    int i;
                    if (Settings.GetSetting("Languages", s) != null && int.TryParse(s, out i))
                    {
                        DataStore.GetInstance().Add("ADVTEST", player.UID, i);
                        player.MessageFrom("AdvancedTest", green + "Language Set to: " + Settings.GetSetting("Languages", s));
                    }
                    else
                    {
                        player.MessageFrom("AdvancedTest", red + "Entered Value doesn't exist or It's not a number!");
                    }
                }
            }
            else if (cmd.Equals("recoiltest"))
            {
                if (!player.Admin && !player.Moderator)
                {
                    return;
                }
                if (args.Length == 0)
                {
                    player.MessageFrom("RecoilTest", red + "Usage: /recoiltest playername - To Start/Stop Testing");
                    return;
                }
                string s = string.Join(" ", args);
                Fougerite.Player p = Fougerite.Server.GetServer().FindPlayer(s);
                if (p == null)
                {
                    player.MessageFrom("RecoilTest", red + "Couldn't find " + s + "!");
                    return;
                }
                ulong id = p.UID;
                if (DataStore.GetInstance().ContainsKey("RecoilTest", id))
                {
                    Send(p, false);
                    var n = storage[p.UID];
                    p.Inventory.RemoveItem(30);
                    p.Inventory.RemoveItem(31);
                    foreach (var x in n.Keys)
                    {
                        p.Inventory.AddItem(x, n[x]);
                    }
                    DataStore.GetInstance().Remove("RecoilTest", id);
                    player.MessageFrom("RecoilTest", red + "Testing ended for " + p.Name);
                    p.Notice("Recoil Test ended!");
                    storage.Remove(p.UID);
                }
                else
                {
                    player.MessageFrom("RecoilTest", red + "Testing started for " + p.Name);
                    player.MessageFrom("RecoilTest", red + "/recoiltest name - to finish");
                    Send(p);
                    Dictionary<string, int> itemcount = new Dictionary<string, int>();
                    p.MessageFrom("RecoilTest", red + "=== Recoil Test ===");
                    p.MessageFrom("RecoilTest", red + p.Name + "'s two hotbar items were: ");
                    if (!p.Inventory.BarItems[0].IsEmpty())
                    {
                        itemcount[p.Inventory.BarItems[0].Name] = p.Inventory.BarItems[0].Quantity;
                        player.MessageFrom("RecoilTest", red + p.Inventory.BarItems[0].Name + " / " + p.Inventory.BarItems[0].Quantity);
                    }
                    if (!p.Inventory.BarItems[1].IsEmpty())
                    {
                        itemcount[p.Inventory.BarItems[1].Name] = p.Inventory.BarItems[1].Quantity;
                        player.MessageFrom("RecoilTest", red + p.Inventory.BarItems[1].Name + " / " + p.Inventory.BarItems[1].Quantity);
                    }
                    storage[p.UID] = itemcount;
                    p.Inventory.RemoveItem(30);
                    p.Inventory.RemoveItem(31);
                    p.Inventory.AddItemTo("M4", 30);
                    p.Inventory.AddItemTo("556 Ammo", 31, 100);
                    DataStore.GetInstance().Add("RecoilTest", id, "1");
                    p.Notice("You are being tested for recoil!");
                    p.MessageFrom("RecoilTest", red + "=== Recoil Test ===");
                    p.MessageFrom("RecoilTest", red + "Take the M4, reload It and start shooting!");
                }
            }
            else if (cmd.Equals("jumptest"))
            {
                if (!player.Admin && !player.Moderator)
                {
                    return;
                }
                if (args.Length == 0)
                {
                    player.MessageFrom("JumpTest", red + "Usage: /jumptest playername - To Start/Stop Testing");
                    return;
                }
                string s = string.Join(" ", args);
                Fougerite.Player p = Fougerite.Server.GetServer().FindPlayer(s);
                if (p == null)
                {
                    player.MessageFrom("JumpTest", red + "Couldn't find " + s + "!");
                    return;
                }
                ulong id = p.UID;
                if (DataStore.GetInstance().ContainsKey("JumpTest", id))
                {
                    Send2(p, false);
                    DataStore.GetInstance().Remove("JumpTest", id);
                    player.MessageFrom("JumpTest", red + "Testing ended for " + p.Name);
                    p.Notice("Jump Test ended!");
                }
                else
                {
                    Send2(p);
                    DataStore.GetInstance().Add("JumpTest", id, "1");
                    player.MessageFrom("JumpTest", red + "Testing started for " + p.Name);
                    player.MessageFrom("JumpTest", red + "/jumptest name - to finish");
                    p.Notice("You are being tested for jump hacks!");
                    p.MessageFrom("RecoilTest", red + "=== Jump Test ===");
                    p.MessageFrom("RecoilTest", red + "Start jumping rapidly!");
                }
            }
            else if (cmd == "fatest")
            {
                if (!player.Admin && !player.Moderator)
                {
                    return;
                }
                if (args.Length == 0)
                {
                    player.MessageFrom("AdvancedTest", red + "Usage: /areport playername - Server will test player after 3 reports");
                    return;
                }
                string s = string.Join(" ", args);
                Fougerite.Player p = Fougerite.Server.GetServer().FindPlayer(s);
                if (p == null)
                {
                    player.MessageFrom("AdvancedTest", red + "Couldn't find " + s + "!");
                    return;
                }
                StartTest(p);
                player.MessageFrom("AdvancedTest", green + "Test Started.");
            }
            else if (cmd == "areport")
            {
                if (args.Length == 0)
                {
                    player.MessageFrom("AdvancedTest", red + "Usage: /areport playername - Server will test player after 3 reports");
                    return;
                }
                string s = string.Join(" ", args);
                Fougerite.Player p = Fougerite.Server.GetServer().FindPlayer(s);
                if (p == null)
                {
                    player.MessageFrom("AdvancedTest", red + "Couldn't find " + s + "!");
                    return;
                }
                if (p.Admin || p.Moderator)
                {
                    player.MessageFrom("AdvancedTest", red + "Admins cannot be tested!");
                    return;
                }
                if (TestCooldown.ContainsKey(p.UID))
                {
                    var ticks = TestCooldown[p.UID];
                    var calc = System.Environment.TickCount - ticks;
                    var time = TestCooldown[p.UID];
                    if (calc > 0 || !double.IsNaN(calc) || !double.IsNaN(ticks))
                    {
                        var done = Math.Round((float) ((calc/1000)/60));
                        player.MessageFrom("AdvancedTest",
                            "Player has report cooldown for: " + (time - done) + " minutes!");
                        return;
                    }
                    if (calc < (TestAllowedEvery + time)*60000)
                    {
                        var done = Math.Round((float) ((calc/1000)/60));
                        player.MessageFrom("AdvancedTest",
                            "Player has report cooldown for: " + (time - done) + " minutes!");
                        return;
                    }
                }
                if (UnderTesting.ContainsKey(p.UID))
                {
                    player.MessageFrom("AdvancedTest", "Player is currently being tested!");
                    return;
                }
                if (ReportC.ContainsKey(p.UID))
                {
                    var list = ReportC[p.UID];
                    if (list.Contains(player.UID))
                    {
                        player.MessageFrom("AdvancedTest", "Already Reported!");
                        return;
                    }
                    list.Add(player.UID);
                    ReportC[p.UID] = list;
                }
                else
                {
                    var list = new List<ulong>();
                    list.Add(player.UID);
                    ReportC[p.UID] = list;
                }
                if (!Reports.ContainsKey(p.UID))
                {
                    Reports[p.UID] = 1;
                    player.MessageFrom("AdvancedTest", "Player " + p.Name + " reported. ( 1/" + ReportsNeeded + " )");
                    return;
                }
                Reports[p.UID] = Reports[p.UID] + 1;
                if (Reports[p.UID] == 3)
                {
                    Server.GetServer().BroadcastFrom("AdvancedTest", red + p.Name + " received enough reports. Auto test is starting.");
                    StartTest(p);
                }
                else
                {
                    player.MessageFrom("AdvancedTest", "Player " + p.Name + " reported. ( " + Reports[p.UID] + "/" + ReportsNeeded + " )");
                }
            }
        }

        public void StartTest(Fougerite.Player player)
        {
            foreach (var x in RestrictedCommands)
            {
                player.RestrictCommand(x);
            }
            LastPos[player.UID] = player.Location;
            TestCooldown[player.UID] = Environment.TickCount;
            if (Reports.ContainsKey(player.UID))
            {
                Reports.Remove(player.UID);
            }
            int i = 1;
            Vector3 pos = Vector3.zero;
            foreach (var x in TestPlaces)
            {
                if (OccupiedPositions.ContainsKey(x))
                {
                    if (i == OccupiedPositions.Keys.Count)
                    {
                        pos = TestPlaces[0];
                        Random r = new Random();
                        var xx = r.Next(10);
                        var zz = r.Next(10);
                        pos.x = pos.x + xx;
                        pos.z = pos.z + zz;
                        var yy = World.GetWorld().GetGround(pos);
                        pos.y = yy + 1f;
                        break;
                    }
                    i++;
                    continue;
                }
                pos = x;
                OccupiedPositions[x] = player.UID;
            }
            player.TeleportTo(pos, false);
            SendAutoTest(player);
            int lang = 1;
            if (DataStore.GetInstance().Get("ADVTEST", player.UID) != null)
            {
                lang = (int) DataStore.GetInstance().Get("ADVTEST", player.UID);
            }
            var TestDataP = new TestData(player, lang);
            UnderTesting[player.UID] = TestDataP;
            Dictionary<string, int> itemcount = new Dictionary<string, int>();
            if (!player.Inventory.BarItems[0].IsEmpty())
            {
                itemcount[player.Inventory.BarItems[0].Name] = player.Inventory.BarItems[0].Quantity;
            }
            if (!player.Inventory.BarItems[1].IsEmpty())
            {
                itemcount[player.Inventory.BarItems[1].Name] = player.Inventory.BarItems[1].Quantity;
            }
            storage[player.UID] = itemcount;
            if (player.IsHungry)
            {
                player.PlayerClient.controllable.GetComponent<Metabolism>().AddCalories(100);
            }
            player.Inventory.RemoveItem(30);
            player.Inventory.RemoveItem(31);
            player.Inventory.AddItemTo("M4", 30);
            player.Inventory.AddItemTo("556 Ammo", 31, 250);
            player.MessageFrom("AdvancedTest", yellow + LanguageDict[TestDataP.LangCode][2]);
            player.MessageFrom("AdvancedTest", red + LanguageDict[TestDataP.LangCode][1]);
            player.MessageFrom("AdvancedTest", red + LanguageDict[TestDataP.LangCode][1]);
            player.MessageFrom("AdvancedTest", red + LanguageDict[TestDataP.LangCode][1]);
            var dict = new  Dictionary<string, object>();
            dict["Player"] = player;
            var timedEvent = CreateParallelTimer(1600, dict);
            timedEvent.OnFire += Callback;
            timedEvent.Start();
            var timedEvent2 = CreateParallelTimer(500, dict);
            timedEvent2.OnFire += AntiMove;
            timedEvent2.Start();
        }

        public void RemoveTest(Fougerite.Player player, bool Disconnected = false)
        {
            if (!Disconnected)
            {
                SendAutoTest(player, false);
                if (TestCooldown.ContainsKey(player.UID))
                {
                    TestCooldown.Remove(player.UID);
                }
                if (LastPos.ContainsKey(player.UID))
                {
                    player.TeleportTo(LastPos[player.UID], false);
                    LastPos.Remove(player.UID);
                }
            }
            foreach (var x in RestrictedCommands)
            {
                player.UnRestrictCommand(x);
            }
            if (ReportC.ContainsKey(player.UID))
            {
                ReportC.Remove(player.UID);
            }
            if (UnderTesting.ContainsKey(player.UID))
            {
                UnderTesting.Remove(player.UID);
            }
            if (OccupiedPositions.ContainsValue(player.UID))
            {
                var vec = OccupiedPositions.FirstOrDefault(x => x.Value == player.UID).Key;
                OccupiedPositions.Remove(vec);
            }
        }

        public void On_Spawn(Fougerite.Player player, SpawnEvent e)
        {
            if (DataStore.GetInstance().ContainsKey("RecoilTest", player.UID))
            {
                Send(player, false);
                if (storage.ContainsKey(player.UID)) {storage.Remove(player.UID);}
                DataStore.GetInstance().Remove("RecoilTest", player.UID);
            }
            else if (DataStore.GetInstance().ContainsKey("JumpTest", player.UID))
            {
                Send2(player, false);
                DataStore.GetInstance().Remove("JumpTest", player.UID);
            }
        }

        private bool SendW(Fougerite.Player player)
        {
            if (!UnderTesting.ContainsKey(player.UID))
            {
                return false;
            }
            var data = UnderTesting[player.UID];
            if (!data.RecoilComplete)
            {
                player.SendCommand("input.mousespeed 0");
                player.SendCommand("input.bind Up F4 None");
                player.SendCommand("input.bind Down F5 None");
                player.SendCommand("input.bind Left F2 None");
                player.SendCommand("input.bind Right INSERT None");
                player.SendCommand("input.bind Fire Mouse0 W");
                player.SendCommand("input.bind Jump F4 None");
                player.SendCommand("input.bind Duck F4 None");
                player.SendCommand("input.bind AltFire F4 None");
                player.SendCommand("input.bind Sprint F4 None");
                player.SendCommand("input.bind Inventory 7 None");
            }
            else if (!data.ButtonComplete)
            {
                player.SendCommand("input.bind Up F4 None");
                player.SendCommand("input.bind Down F4 None");
                player.SendCommand("input.bind Left F4 None");
                player.SendCommand("input.bind Right INSERT None");
                player.SendCommand("input.mousespeed 0");
                player.SendCommand("input.bind Fire Mouse0 W");
                player.SendCommand("input.bind Jump F4 None");
                player.SendCommand("input.bind Duck F4 None");
                player.SendCommand("input.bind AltFire F4 None");
                player.SendCommand("input.bind Sprint F4 None");
                player.SendCommand("input.bind Inventory 7 None");
            }
            else if (!data.ButtonComplete)
            {
                player.SendCommand("input.bind Up F4 None");
                player.SendCommand("input.bind Down F4 None");
                player.SendCommand("input.bind Left F2 None");
                player.SendCommand("input.bind Right F4 None");
                player.SendCommand("input.mousespeed 0");
                player.SendCommand("input.bind Fire Mouse0 W");
                player.SendCommand("input.bind Jump F4 None");
                player.SendCommand("input.bind Duck F4 None");
                player.SendCommand("input.bind AltFire F4 None");
                player.SendCommand("input.bind Sprint F4 None");
                player.SendCommand("input.bind Inventory 7 None");
            }
            else if (!data.ButtonComplete)
            {
                player.SendCommand("input.bind Up F4 None");
                player.SendCommand("input.bind Down F5 None");
                player.SendCommand("input.bind Left F4 None");
                player.SendCommand("input.bind Right F4 None");
                player.SendCommand("input.mousespeed 0");
                player.SendCommand("input.bind Fire Mouse0 W");
                player.SendCommand("input.bind Jump F4 None");
                player.SendCommand("input.bind Duck F4 None");
                player.SendCommand("input.bind AltFire F4 None");
                player.SendCommand("input.bind Sprint F4 None");
                player.SendCommand("input.bind Inventory 7 None");
            }
            return true;
        }

        private void SendAutoTest(Fougerite.Player player, bool on = true)
        {
            if (on)
            {
                player.SendCommand("input.mousespeed 0");
                player.SendCommand("input.bind Up F4 None");
                player.SendCommand("input.bind Down F5 None");
                player.SendCommand("input.bind Left F2 None");
                player.SendCommand("input.bind Right INSERT None");
                player.SendCommand("input.bind Fire Mouse0 W");
                player.SendCommand("input.bind Jump F4 None");
                player.SendCommand("input.bind Duck F4 None");
                player.SendCommand("input.bind AltFire F4 None");
                player.SendCommand("input.bind Sprint F4 None");
                player.SendCommand("input.bind Inventory 7 None");
            }
            else
            {
                player.SendCommand("input.mousespeed 5");
                player.SendCommand("input.bind Up W None");
                player.SendCommand("input.bind Down S None");
                player.SendCommand("input.bind Left A None");
                player.SendCommand("input.bind Right D None");
                player.SendCommand("input.bind Fire Mouse0 None");
                player.SendCommand("input.bind AltFire Mouse1 none");
                player.SendCommand("input.bind Sprint LeftShift none");
                player.SendCommand("input.bind Duck LeftControl None");
                player.SendCommand("input.bind Jump Space None");
                player.SendCommand("input.bind Inventory Tab None");
            }
        }

        private void Send(Fougerite.Player player, bool on = true)
        {
            if (on)
            {
                player.SendCommand("input.mousespeed 0");
                player.SendCommand("input.bind Up F4 None");
                player.SendCommand("input.bind Down F4 None");
                player.SendCommand("input.bind Left F2 None");
                player.SendCommand("input.bind Right INSERT None");
                player.SendCommand("input.bind Fire Mouse0 W");
                player.SendCommand("input.bind Jump F5 None");
                player.SendCommand("input.bind Duck F4 None");
                player.SendCommand("input.bind AltFire F4 None");
                player.SendCommand("input.bind Sprint F4 None");
                player.SendCommand("input.bind Inventory 7 None");
            }
            else
            {
                player.SendCommand("input.mousespeed 5");
                player.SendCommand("input.bind Up W None");
                player.SendCommand("input.bind Down S None");
                player.SendCommand("input.bind Left A None");
                player.SendCommand("input.bind Right D None");
                player.SendCommand("input.bind Fire Mouse0 None");
                player.SendCommand("input.bind AltFire Mouse1 none");
                player.SendCommand("input.bind Sprint LeftShift none");
                player.SendCommand("input.bind Duck LeftControl None");
                player.SendCommand("input.bind Jump Space None");
                player.SendCommand("input.bind Inventory Tab None");
            }
        }

        private void Send2(Fougerite.Player player, bool on = true)
        {
            if (on)
            {
                player.SendCommand("input.mousespeed 0");
                player.SendCommand("input.bind Up F2 None");
                player.SendCommand("input.bind Down F4 None");
                player.SendCommand("input.bind Left F5 None");
                player.SendCommand("input.bind Right INSERT None");
                player.SendCommand("input.bind Fire Mouse0 W");
                player.SendCommand("input.bind AltFire F4 None");
                player.SendCommand("input.bind Sprint F4 None");
                player.SendCommand("input.bind Duck F4 None");
                player.SendCommand("input.bind Inventory 7 None");
            }
            else
            {
                player.SendCommand("input.mousespeed 5");
                player.SendCommand("input.bind Up W None");
                player.SendCommand("input.bind Down S None");
                player.SendCommand("input.bind Left A None");
                player.SendCommand("input.bind Right D None");
                player.SendCommand("input.bind Fire Mouse0 None");
                player.SendCommand("input.bind AltFire Mouse1 none");
                player.SendCommand("input.bind Sprint LeftShift none");
                player.SendCommand("input.bind Duck LeftControl None");
                player.SendCommand("input.bind Jump Space None");
                player.SendCommand("input.bind Inventory Tab None");
            }
        }

        public void AntiMove(AdvancedTesterTE e)
        {
            var dict = e.Args;
            Fougerite.Player player = (Fougerite.Player)dict["Player"];
            bool b = SendW(player);
            if (!b)
            {
                e.Kill();
            }
        }

        public void Callback(AdvancedTesterTE e)
        {
            var dict = e.Args;
            e.Kill();
            Fougerite.Player player = (Fougerite.Player)dict["Player"];
            dict["Location"] = player.Location;
            player.TeleportTo(player.X, player.Y + 35f, player.Z);
            var timedEvent = CreateParallelTimer(3500, dict);
            timedEvent.OnFire += Callback2;
            timedEvent.Start();

        }

        public void Callback2(AdvancedTesterTE e)
        {
            var dict = e.Args;
            e.Kill();
            Fougerite.Player player = (Fougerite.Player) dict["Player"];
            Vector3 pos = (Vector3) dict["Location"];
            if (!UnderTesting[player.UID].FallComplete)
            {
                foreach (var x in Server.GetServer().Players)
                {
                    if (x.Admin || x.Moderator)
                    {
                        x.MessageFrom("AdvancedTest", green + player.Name + "'s ping: " + x.Ping);
                    }
                }
                RemoveTest(player);
                Fougerite.Server.GetServer().BanPlayer(player, "Console", "Auto DropTest Failed!", null, true);
                return;
            }
            player.MessageFrom("AdvancedTest", yellow + "Drop Test Complete!");
            Character c = player.PlayerClient.netUser.playerClient.controllable.character;
            var eyeangles = c.eyesAngles;
            player.FallDamage.ClearInjury();
            player.HumanBodyTakeDmg.SetBleedingLevel(0f);
            player.TeleportTo(pos, false);

            var timedEvent = CreateParallelTimer(1500, dict);
            timedEvent.OnFire += Callback3;
            timedEvent.Start();

            dict["Angle"] = eyeangles;
            dict["Count"] = 0;
            var timedEvent2 = CreateParallelTimer(2000, dict);
            timedEvent2.OnFire += Callback4;
            timedEvent2.Start();
        }

        public void Callback3(AdvancedTesterTE e)
        {
            var dict = e.Args;
            Fougerite.Player player = (Fougerite.Player)dict["Player"];
            Vector3 pos = (Vector3)dict["Location"];
            if (!UnderTesting.ContainsKey(player.UID) || UnderTesting[player.UID].RecoilComplete)
            {
                e.Kill();
                return;
            }
            var dist = Vector3.Distance(pos, player.Location);
            //Todo: Sometimes there is a big distance bug, but It doesn't cause instaban. Better Safe than Sound. Probs Unity.
            //If the player moves, his position will be updated, and banned. Everything is okay with It.
            if (dist > 50)
            {
                return;
            }
            if (dist >= 0.1f)
            {
                if (!UnderTesting.ContainsKey(player.UID) || UnderTesting[player.UID].RecoilComplete)
                {
                    e.Kill();
                    return;
                }
                RemoveTest(player);
                Server.GetServer().BanPlayer(player, "Console", "Hack Detected! (Movement)", null, true);
            }
        }

        public void Callback4(AdvancedTesterTE e)
        {
            //Todo: Add on shooting event to Fougerite and fuck up everyone
            e.Kill();
            var dict = e.Args;
            Fougerite.Player player = (Fougerite.Player)dict["Player"];
            int count = (int)dict["Count"];
            Angle2 angle = (Angle2) dict["Angle"];
            Character c = player.PlayerClient.netUser.playerClient.controllable.character;
            var eyeangles = c.eyesAngles;
            if (angle == eyeangles)
            {
                count++;
            }
            else
            {
                player.SendCommand("input.bind Up F4 None");
                player.SendCommand("input.bind Down F4 None");
                player.SendCommand("input.bind Left F4 None");
                player.SendCommand("input.bind Right INSERT None");
                UnderTesting[player.UID].RecoilComplete = true;
                var n = storage[player.UID];
                player.Inventory.RemoveItem(30);
                player.Inventory.RemoveItem(31);
                foreach (var x in n.Keys)
                {
                    player.Inventory.AddItem(x, n[x]);
                }
                Vector3 pos = (Vector3)dict["Location"];
                dict["ButtonPos"] = pos;
                dict["SCount"] = 0;
                dict["SCount2"] = 0;
                var timedEvent2 = CreateParallelTimer(1000, dict);
                timedEvent2.OnFire += Callback5;
                timedEvent2.Start();
                return;
            }

            player.MessageFrom("AdvancedTest", teal + LanguageDict[UnderTesting[player.UID].LangCode][3] + " ( " + count + "/" + RecoilWait + " )");
            if (count == RecoilWait)
            {
                RemoveTest(player);
                Server.GetServer().BanPlayer(player, "Console", "Having NoRecoil!", null, true);
                return;
            }
            dict["Count"] = count;
            var timedEvent = CreateParallelTimer(2000, dict);
            timedEvent.OnFire += Callback4;
            timedEvent.Start();
        }

        public void Callback5(AdvancedTesterTE e)
        {
            e.Kill();
            var dict = e.Args;
            Vector3 pos = (Vector3)dict["ButtonPos"];
            Fougerite.Player player = (Fougerite.Player)dict["Player"];
            int SCount = (int) dict["SCount"];
            int SCount2 = (int)dict["SCount2"];
            player.MessageFrom("AdvancedTest", teal + LanguageDict[UnderTesting[player.UID].LangCode][4]);
            var pll = player.Location;
            var dist = Vector3.Distance(pos, pll);
            if (SCount2 != 1)
            {
                dict["SCount2"] = 1;
            }
            if (dist < 0.10f && dist >= 0.001f)
            {
                SCount++;
                if (SCount == 1)
                {
                    RemoveTest(player);
                    Server.GetServer().BanPlayer(player, "Console", "Detected JACKED Hack.", null, true);
                    return;
                }
            }
            else if (dist > 0.1f)
            {
                player.SendCommand("input.bind Up F4 None");
                player.SendCommand("input.bind Down F4 None");
                player.SendCommand("input.bind Left F2 None");
                player.SendCommand("input.bind Right F4 None");
                player.MessageFrom("AdvancedTest", green + "ButtonTest Complete!");
                dict["ButtonPos"] = pll;
                dict["SCount"] = 0;
                UnderTesting[player.UID].ButtonComplete = true;
                var timedEvent3 = CreateParallelTimer(1500, dict);
                timedEvent3.OnFire += Callback6;
                timedEvent3.Start();
                return;
            }
            dict["ButtonPos"] = pll;
            dict["SCount"] = SCount;
            var timedEvent2 = CreateParallelTimer(1000, dict);
            timedEvent2.OnFire += Callback5;
            timedEvent2.Start();
        }

        public void Callback6(AdvancedTesterTE e)
        {
            e.Kill();
            var dict = e.Args;
            Vector3 pos = (Vector3)dict["ButtonPos"];
            Fougerite.Player player = (Fougerite.Player)dict["Player"];
            int SCount = (int)dict["SCount"];
            int SCount2 = (int)dict["SCount2"];
            player.MessageFrom("AdvancedTest", teal + LanguageDict[UnderTesting[player.UID].LangCode][5]);
            var pll = player.Location;
            if (SCount2 != 2)
            {
                pos = pll;
                dict["SCount2"] = 2;
            }
            var dist = Vector3.Distance(pos, pll);
            if (dist < 0.10f && dist >= 0.001f)
            {
                SCount++;
                if (SCount == 1)
                {
                    RemoveTest(player);
                    Server.GetServer().BanPlayer(player, "Console", "Detected Dizzy Hack.", null, true);
                    return;
                }
            }
            else if (dist > 0.10f)
            {
                player.SendCommand("input.bind Up F4 None");
                player.SendCommand("input.bind Down F5 None");
                player.SendCommand("input.bind Left F4 None");
                player.SendCommand("input.bind Right F4 None");
                player.MessageFrom("AdvancedTest", green + "ButtonTest2 Complete!");
                dict["ButtonPos"] = pll;
                dict["SCount"] = 0;
                UnderTesting[player.UID].ButtonComplete2 = true;
                var timedEvent3 = CreateParallelTimer(1500, dict);
                timedEvent3.OnFire += Callback7;
                timedEvent3.Start();
                return;
            }
            dict["ButtonPos"] = pll;
            dict["SCount"] = SCount;
            var timedEvent2 = CreateParallelTimer(1000, dict);
            timedEvent2.OnFire += Callback6;
            timedEvent2.Start();
        }

        public void Callback7(AdvancedTesterTE e)
        {
            e.Kill();
            var dict = e.Args;
            Vector3 pos = (Vector3)dict["ButtonPos"];
            Fougerite.Player player = (Fougerite.Player)dict["Player"];
            int SCount = (int)dict["SCount"];
            int SCount2 = (int)dict["SCount2"];
            player.MessageFrom("AdvancedTest", teal + LanguageDict[UnderTesting[player.UID].LangCode][6]);
            var pll = player.Location;
            if (SCount2 != 3)
            {
                pos = pll;
                dict["SCount2"] = 3;
            }
            var dist = Vector3.Distance(pos, pll);
            if (dist < 0.10f && dist >= 0.001f)
            {
                SCount++;
                if (SCount == 1)
                {
                    RemoveTest(player);
                    Server.GetServer().BanPlayer(player, "Console", "Detected A3MON Hack.", null, true);
                    return;
                }
            }
            else if (dist > 0.10f)
            {
                player.MessageFrom("AdvancedTest", green + "ButtonTest3 Complete!");
                UnderTesting[player.UID].ButtonComplete3 = true;
                RemoveTest(player);
                Server.GetServer().BroadcastFrom("AdvancedTest", green + player.Name + " passed all auto tests!");
                return;
            }
            dict["ButtonPos"] = pll;
            dict["SCount"] = SCount;
            var timedEvent2 = CreateParallelTimer(1000, dict);
            timedEvent2.OnFire += Callback7;
            timedEvent2.Start();
        }

        public AdvancedTesterTE CreateParallelTimer(int timeoutDelay, Dictionary<string, object> args)
        {
            AdvancedTesterTE timedEvent = new AdvancedTesterTE(timeoutDelay);
            timedEvent.Args = args;
            return timedEvent;
        }

        // Todo: Put this for an advanced usage after the next Fougerite comes
        public class TestData
        {
            private Fougerite.Player Player;
            private int LanguageCode = 1;
            private bool FallTest = false;
            private bool RecoilTest = false;
            private bool ButtonTest = false;
            private bool ButtonTest2 = false;
            private bool ButtonTest3 = false;

            public TestData(Fougerite.Player player, int LangC = 1)
            {
                Player = player;
                LanguageCode = LangC;
            }

            public int LangCode
            {
                get { return LanguageCode; }
            }

            public bool RecoilComplete 
            {
                get { return RecoilTest; }
                set { RecoilTest = value; }
            }

            public bool FallComplete
            {
                get { return FallTest; }
                set { FallTest = value; }
            }

            public bool ButtonComplete
            {
                get { return ButtonTest; }
                set { ButtonTest = value; }
            }

            public bool ButtonComplete2
            {
                get { return ButtonTest2; }
                set { ButtonTest2 = value; }
            }

            public bool ButtonComplete3
            {
                get { return ButtonTest3; }
                set { ButtonTest3 = value; }
            }
        }
    }
}
