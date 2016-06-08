using System;
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
            get { return new Version("1.4.1"); }
        }

        public override void Initialize()
        {
            Fougerite.Hooks.OnCommand += On_Command;
            Fougerite.Hooks.OnPlayerSpawned += On_Spawn;
            Fougerite.Hooks.OnPlayerHurt += OnPlayerHurt;
            Fougerite.Hooks.OnPlayerDisconnected += OnPlayerDisconnected;
            if (!File.Exists(Path.Combine(ModuleFolder, "Settings.ini")))
            {
                File.Create(Path.Combine(ModuleFolder, "Settings.ini")).Dispose();
                Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                Settings.AddSetting("Settings", "TestAllowedEvery", "15");
                Settings.AddSetting("Settings", "RecoilWait", "15");
                Settings.AddSetting("Settings", "ReportsNeeded", "3");
                Settings.Save();
            }
            else
            {
                Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                TestAllowedEvery = int.Parse(Settings.GetSetting("Settings", "TestAllowedEvery"));
                RecoilWait = int.Parse(Settings.GetSetting("Settings", "RecoilWait"));
                ReportsNeeded = int.Parse(Settings.GetSetting("Settings", "ReportsNeeded"));
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
                        //p.Inventory.AddItemTo();
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
            var TestDataP = new TestData(player);
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
            player.Inventory.RemoveItem(30);
            player.Inventory.RemoveItem(31);
            player.Inventory.AddItemTo("M4", 30);
            player.Inventory.AddItemTo("556 Ammo", 31, 250);
            player.MessageFrom("AdvancedTest", yellow + "Disconnecting from test will cause an auto ban!");
            player.MessageFrom("AdvancedTest", yellow + "Disconnecting from test will cause an auto ban!");
            player.MessageFrom("AdvancedTest", yellow + "Pressing F2/F5/INSERT will cause an auto ban!");
            var dict = new  Dictionary<string, object>();
            dict["Player"] = player;
            var timedEvent = CreateParallelTimer(1600, dict);
            timedEvent.OnFire += Callback;
            timedEvent.Start();
        }

        public void RemoveTest(Fougerite.Player player, bool Disconnected = false)
        {
            if (!Disconnected)
            {
                SendAutoTest(player, false);
                var n = storage[player.UID];
                player.Inventory.RemoveItem(30);
                player.Inventory.RemoveItem(31);
                foreach (var x in n.Keys)
                {
                    player.Inventory.AddItem(x, n[x]);
                }
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
            if (!UnderTesting.ContainsKey(player.UID))
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
                UnderTesting[player.UID].RecoilComplete = true;
                RemoveTest(player);
                Server.GetServer().BroadcastFrom("AdvancedTest", green + player.Name + " passed all auto tests!");
                return;
            }
            player.MessageFrom("AdvancedTest", teal + "Take your M4 out, Reload It, and Shoot It! ( " + count + "/" + RecoilWait + " )");
            if (count == 10)
            {
                Server.GetServer().BanPlayer(player, "Console", "Having NoRecoil!", null, true);
                return;
            }
            dict["Count"] = count;
            var timedEvent = CreateParallelTimer(2000, dict);
            timedEvent.OnFire += Callback4;
            timedEvent.Start();
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
            private bool FallTest = false;
            private bool RecoilTest = false;
            private bool ButtonTest = false;
            private bool ButtonTest2 = false;
            private bool ButtonTest3 = false;

            public TestData(Fougerite.Player player)
            {
                Player = player;
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
