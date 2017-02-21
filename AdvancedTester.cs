using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Fougerite;
using Fougerite.Events;
using RustProto;
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
        public Dictionary<ulong, TestData> UnderTesting;
        public Dictionary<ulong, double> TestCooldown;
        public Dictionary<ulong, Vector3> LastPos;
        public Dictionary<ulong, List<ulong>> ReportC;
        public Dictionary<string, int> LanguageData;
        public Dictionary<int, Dictionary<int, string>> LanguageDict;
        public Dictionary<ulong, Angle2> Angles;
        public Dictionary<ulong, int> AnglesC;
        public Dictionary<ulong, Dictionary<string, object>> TData;
        public List<Vector3> TestPlaces;
        public List<string> DSNames; 
        public List<string> RestrictedCommands;
        public int ReportsNeeded = 3;
        public int TestAllowedEvery = 15;
        public int RecoilWait = 15;
        public int InsertWait = 0;
        public int F2Wait = 0;
        public int F5Wait = 0;
        public int F3Wait = 0;
        public int MaxTest = 2;
        public int IgnoreDropIfPing = 170;
        public int WarnSeconds = 5;
        public IniParser Settings;
        public bool GeoIPSupport = false;
        public bool AutoTestOnJoin = false;
        public bool DropTest = true;
        public bool RemoveSleeperD = false;
        public bool EnableButtonWarnMessage = false;
        public bool UnBindTab = true;
        public bool MenuHackBan = true;
        public bool WarnInventoryBeforeTest = false;

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
            get { return new Version("1.6.2"); }
        }

        public override void Initialize()
        {
            Fougerite.Hooks.OnCommand += On_Command;
            Fougerite.Hooks.OnPlayerSpawned += On_Spawn;
            Fougerite.Hooks.OnPlayerHurt += OnPlayerHurt;
            Fougerite.Hooks.OnPlayerConnected += OnPlayerConnected;
            Fougerite.Hooks.OnPlayerDisconnected += OnPlayerDisconnected;
            Fougerite.Hooks.OnChat += OnChat;
            Fougerite.Hooks.OnModulesLoaded += OnModulesLoaded;
            Fougerite.Hooks.OnShoot += OnShoot;
            RestrictedCommands = new List<string>();
            DSNames = new List<string>();
            LanguageDict = new Dictionary<int, Dictionary<int, string>>();
            LanguageData = new Dictionary<string, int>();
            if (!File.Exists(Path.Combine(ModuleFolder, "Settings.ini")))
            {
                File.Create(Path.Combine(ModuleFolder, "Settings.ini")).Dispose();
                Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                Settings.AddSetting("Settings", "TestAllowedEvery", "20");
                Settings.AddSetting("Settings", "RecoilWait", "20");
                Settings.AddSetting("Settings", "InsertWait", "0");
                Settings.AddSetting("Settings", "F2Wait", "0");
                Settings.AddSetting("Settings", "F5Wait", "0");
                Settings.AddSetting("Settings", "F3Wait", "0");
                Settings.AddSetting("Settings", "MaxTest", "2");
                Settings.AddSetting("Settings", "ReportsNeeded", "3");
                Settings.AddSetting("Settings", "DropTest", "True");
                Settings.AddSetting("Settings", "RestrictedCommands", "tpa,home,tpaccept,hg");
                Settings.AddSetting("Settings", "DSNames", "HGIG,RandomDSName");
                Settings.AddSetting("Settings", "AutoTestOnJoin", "False");
                Settings.AddSetting("Settings", "IgnoreDropIfPing", "170");
                Settings.AddSetting("Settings", "RemoveSleeperD", "False");
                Settings.AddSetting("Settings", "EnableButtonWarnMessage", "False");
                Settings.AddSetting("Settings", "MenuHackBan", "True");
                Settings.AddSetting("Settings", "UnBindTab", "True");
                Settings.AddSetting("Settings", "WarnInventoryBeforeTest", "False");
                Settings.AddSetting("Settings", "WarnSeconds", "5");
                Settings.AddSetting("Languages", "1", "English");
                Settings.AddSetting("Languages", "2", "Hungarian");
                Settings.AddSetting("Languages", "3", "Russian");
                Settings.AddSetting("Languages", "4", "Portuguese");
                Settings.AddSetting("Languages", "5", "Romanian");
                Settings.AddSetting("Languages", "6", "Spanish");
                Settings.AddSetting("Languages", "7", "Arabic");
                Settings.AddSetting("Languages", "8", "Italian");
                Settings.AddSetting("Languages", "9", "Dutch");
                Settings.AddSetting("Languages", "10", "German");
                Settings.AddSetting("LanguageData", "Spain", "6");
                Settings.AddSetting("LanguageData", "Hungary", "2");
                Settings.AddSetting("LanguageData", "Russia", "3");
                Settings.AddSetting("LanguageData", "Portugal", "4");
                Settings.AddSetting("LanguageData", "Romania", "5");
                Settings.AddSetting("LanguageData", "Saudi Arabia", "7");
                Settings.AddSetting("LanguageData", "United Arab Emirates", "7");
                Settings.AddSetting("LanguageData", "Italy", "8");
                Settings.AddSetting("LanguageData", "Dutch", "9");
                Settings.AddSetting("LanguageData", "Germany", "10");
                Settings.AddSetting("LanguageData", "Austria", "10");
                Settings.AddSetting("LanguageData", "Switzerland", "10");
                Settings.AddSetting("LanguageData", "Venezuela", "6");
                Settings.AddSetting("LanguageData", "Uruguay", "6");
                Settings.AddSetting("LanguageData", "Puerto Rico", "6");
                Settings.AddSetting("LanguageData", "Peru", "6");
                Settings.AddSetting("LanguageData", "Paraguay", "6");
                Settings.AddSetting("LanguageData", "Panama", "6");
                Settings.AddSetting("LanguageData", "Nicaragua", "6");
                Settings.AddSetting("LanguageData", "Mexico", "6");
                Settings.AddSetting("LanguageData", "Honduras", "6");
                Settings.AddSetting("LanguageData", "Guatemala", "6");
                Settings.AddSetting("LanguageData", "Equatorial Guinea", "6");
                Settings.AddSetting("LanguageData", "El Salvador", "6");
                Settings.AddSetting("LanguageData", "Ecuador", "6");
                Settings.AddSetting("LanguageData", "Dominican Republic", "6");
                Settings.AddSetting("LanguageData", "Cuba", "6");
                Settings.AddSetting("LanguageData", "Costa Rica", "6");
                Settings.AddSetting("LanguageData", "Colombia", "6");
                Settings.AddSetting("LanguageData", "Chile", "6");
                Settings.AddSetting("LanguageData", "Bolivia", "6");
                Settings.AddSetting("LanguageData", "Argentina", "6");
                Settings.AddSetting("English", "1", "Do not press F2/ F5 / Insert until the plugin says otherwise!");
                Settings.AddSetting("English", "2", "Disconnecting from the test will cause auto ban!");
                Settings.AddSetting("English", "3", "Take your M4 out, reload It, and shoot It!");
                Settings.AddSetting("English", "4", "Keep Pressing Insert/Ins/NUMPAD 0");
                Settings.AddSetting("English", "5", "Keep Pressing F2");
                Settings.AddSetting("English", "6", "Keep Pressing F5");
                Settings.AddSetting("English", "7", "Keep Pressing F3");
                Settings.AddSetting("Hungarian", "1", "Ne nyomj F2 / F5 / Insert gombokat, amíg a plugin nem kéri!");
                Settings.AddSetting("Hungarian", "2", "A lecsatlakozás autómatikus bant okoz!");
                Settings.AddSetting("Hungarian", "3", "Vedd elő az M4-et, töltsd újra, és tüzelj párszor!");
                Settings.AddSetting("Hungarian", "4", "Nyomd folyamatosan az INSERT/Ins/NUMPAD 0 gombot");
                Settings.AddSetting("Hungarian", "5", "Nyomd folyamatosan az F2 gombot");
                Settings.AddSetting("Hungarian", "6", "Nyomd folyamatosan az F5 gombot");
                Settings.AddSetting("Hungarian", "7", "Nyomd folyamatosan az F3 gombot");
                Settings.AddSetting("Russian", "1", "Не нажимайте F2 / F5 / Insert, пока плагин не говорит иначе!");
                Settings.AddSetting("Russian", "2", "Разъединители причины запрета!");
                Settings.AddSetting("Russian", "3", "Возьмите M4 из, перезагрузить его, и стрелять из него!");
                Settings.AddSetting("Russian", "4", "Продолжайте нажимать Insert/Ins/NUMPAD 0");
                Settings.AddSetting("Russian", "5", "Продолжайте нажимать F2");
                Settings.AddSetting("Russian", "6", "Продолжайте нажимать F5");
                Settings.AddSetting("Russian", "7", "Продолжайте нажимать F3");
                Settings.AddSetting("Portuguese", "1", "Não carregues no F2 / F5 / Insert sem te pedirem para o fazer!");
                Settings.AddSetting("Portuguese", "2", "Sair do teste vai resultar em autoban!");
                Settings.AddSetting("Portuguese", "3", "Pega na M4, recarrega-a e dispara-a sem parar!");
                Settings.AddSetting("Portuguese", "4", "Carrega no Insert/Ins/NUMPAD 0 continuamente.");
                Settings.AddSetting("Portuguese", "5", "Carrega no F2 continuamente.");
                Settings.AddSetting("Portuguese", "6", "Carrega no F5 continuamente.");
                Settings.AddSetting("Portuguese", "7", "Carrega no F3 continuamente.");
                Settings.AddSetting("Romanian", "1", "Nu apasa tastele F2 / F5 / Insert pana nu iti spune plug - inul sa o faci");
                Settings.AddSetting("Romanian", "2", "Daca te deconectezi in timp ce esti testat vei lua ban automat");
                Settings.AddSetting("Romanian", "3", "Echipeaza M4 - ul, incarca - l si trage!");
                Settings.AddSetting("Romanian", "4", "Apasa tasta Insert/Ins/NUMPAD 0 incontinuu");
                Settings.AddSetting("Romanian", "5", "Apasa tasta F2 incontinuu");
                Settings.AddSetting("Romanian", "6", "Apasa tasta F5 incontinuu");
                Settings.AddSetting("Romanian", "7", "Apasa tasta F3 incontinuu");
                Settings.AddSetting("Spanish", "1", "No pulses F2/ F5 / Insert hasta que el plugin lo diga");
                Settings.AddSetting("Spanish", "2", "Desconectarse del servidor durante el test causará un baneo automático");
                Settings.AddSetting("Spanish", "3", "Equipa tu M4, recarga y dispara");
                Settings.AddSetting("Spanish", "4", "Sigue pulsando Insert/Ins/NUMPAD 0");
                Settings.AddSetting("Spanish", "5", "Sigue pulsando F2");
                Settings.AddSetting("Spanish", "6", "Sigue pulsando F5");
                Settings.AddSetting("Spanish", "7", "Sigue pulsando F3");
                Settings.AddSetting("Arabic", "1", " تدغط علي F2 / F5 / Insert حتي يقول لكا الخادم.");
                Settings.AddSetting("Arabic", "2", "فصل الاتصال اثنا الاختبار سايسبب من الخادم بمنعك من الاتصال مجددآ");
                Settings.AddSetting("Arabic", "3", "اخرج ال M4 ، قم بسحبه، ثم قم باطلاق النار");
                Settings.AddSetting("Arabic", "4", "ادغط Insert/Ins/NUMPAD 0 باستمرار");
                Settings.AddSetting("Arabic", "5", "ادغط F2 باستمرار");
                Settings.AddSetting("Arabic", "6", "ادغط F5 باستمرار");
                Settings.AddSetting("Arabic", "7", "ادغط F3 باستمرار");
                Settings.AddSetting("Italian", "1", "Non premere F2 / F5 / Insert sino a quando non te lo chiede il plugin!");
                Settings.AddSetting("Italian", "2", "Se ti disconnetti dal test verrai autobannato!");
                Settings.AddSetting("Italian", "3", "Prendi il tuo M4, ricaricalo e spara!");
                Settings.AddSetting("Italian", "4", "Tieni premuto Insert/Ins/NUMPAD 0");
                Settings.AddSetting("Italian", "5", "Tieni premuto F2");
                Settings.AddSetting("Italian", "6", "Tieni premuto F5");
                Settings.AddSetting("Italian", "7", "Tieni premuto F3");
                Settings.AddSetting("Dutch", "1", "Druk niet op F2 / F5/ Insert totdat de plugin zegt dat dat moet!");
                Settings.AddSetting("Dutch", "2", "Als je disconnect in de test, wordt je gebanned / verbannen!");
                Settings.AddSetting("Dutch", "3", "Pak de M4, reload / herlaad het, en schiet!");
                Settings.AddSetting("Dutch", "4", "Blijf Insert/Ins/NUMPAD 0 ingedrukt houden");
                Settings.AddSetting("Dutch", "5", "Blijf F2 ingedrukt houden");
                Settings.AddSetting("Dutch", "6", "Blijf F5 ingedrukt houden");
                Settings.AddSetting("Dutch", "7", "Blijf F3 ingedrukt houden");
                Settings.AddSetting("German", "1", "Drücke nicht F2 / F5 / Einfg, bevor du dazu aufgefordert wirst");
                Settings.AddSetting("German", "2", "Wenn du während dem Test die Verbindung trennst oder Rust schließt, wirst du gebannt");
                Settings.AddSetting("German", "3", "Nehme die M4 und schieße!");
                Settings.AddSetting("German", "4", "Halte die Taste Einfügen / Einfg / Ziffernblock Taste 0 gedrückt");
                Settings.AddSetting("German", "5", "Halte die Taste F2 gedrückt");
                Settings.AddSetting("German", "6", "Halte die Taste F5 gedrückt");
                Settings.AddSetting("German", "7", "Halte die Taste F3 gedrückt");
                Settings.Save();
            }
            try
            {
                Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                TestAllowedEvery = int.Parse(Settings.GetSetting("Settings", "TestAllowedEvery")) * 60;
                RecoilWait = int.Parse(Settings.GetSetting("Settings", "RecoilWait"));
                ReportsNeeded = int.Parse(Settings.GetSetting("Settings", "ReportsNeeded"));
                InsertWait = int.Parse(Settings.GetSetting("Settings", "InsertWait"));
                F2Wait = int.Parse(Settings.GetSetting("Settings", "F2Wait"));
                F5Wait = int.Parse(Settings.GetSetting("Settings", "F5Wait"));
                F3Wait = int.Parse(Settings.GetSetting("Settings", "F3Wait"));
                MaxTest = int.Parse(Settings.GetSetting("Settings", "MaxTest"));
                WarnSeconds = int.Parse(Settings.GetSetting("Settings", "WarnSeconds"));
                IgnoreDropIfPing = int.Parse(Settings.GetSetting("Settings", "IgnoreDropIfPing"));
                AutoTestOnJoin = Settings.GetBoolSetting("Settings", "AutoTestOnJoin");
                DropTest = Settings.GetBoolSetting("Settings", "DropTest");
                RemoveSleeperD = Settings.GetBoolSetting("Settings", "RemoveSleeperD");
                EnableButtonWarnMessage = Settings.GetBoolSetting("Settings", "EnableButtonWarnMessage");
                UnBindTab = Settings.GetBoolSetting("Settings", "UnBindTab");
                MenuHackBan = Settings.GetBoolSetting("Settings", "MenuHackBan");
                WarnInventoryBeforeTest = Settings.GetBoolSetting("Settings", "WarnInventoryBeforeTest");
                var cmds = Settings.GetSetting("Settings", "RestrictedCommands").Split(Convert.ToChar(","));
                foreach (var x in cmds)
                {
                    RestrictedCommands.Add(x);
                }
                var dsnamesc = Settings.GetSetting("Settings", "DSNames").Split(Convert.ToChar(","));
                foreach (var x in dsnamesc)
                {
                    DSNames.Add(x);
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
                var langdata = Settings.EnumSection("LanguageData");
                foreach (var x in langdata)
                {
                    var lang = Settings.GetSetting("LanguageData", x);
                    LanguageData[x] = int.Parse(lang);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("[AdvancedTester] Failed to read the config! Fix It! " + ex);
            }
            storage = new Dictionary<ulong, Dictionary<string, int>>();
            Reports = new Dictionary<ulong, int>();
            OccupiedPositions = new Dictionary<Vector3, ulong>();
            UnderTesting = new Dictionary<ulong, TestData>();
            TestCooldown = new Dictionary<ulong, double>();
            LastPos = new Dictionary<ulong, Vector3>();
            ReportC = new Dictionary<ulong, List<ulong>>();
            Angles = new Dictionary<ulong, Angle2>();
            TData = new Dictionary<ulong, Dictionary<string, object>>();
            AnglesC = new Dictionary<ulong, int>();
            TestPlaces = new List<Vector3>
            {
                new Vector3(-5599, 403, -2989),
                new Vector3(-5594, 403, -2985),
                new Vector3(-5589, 403, -2981),
                new Vector3(-5585, 402, -2978),
                new Vector3(-5579, 401, -2973),
                new Vector3(-5570, 398, -2966),
                new Vector3(-5562, 396, -2960),
                new Vector3(-5555, 394, -2954),
                new Vector3(-5546, 393, -2947),
                new Vector3(-5520, 388, -2943)
            };
            DataStore.GetInstance().Flush("RecoilTest");
            DataStore.GetInstance().Flush("JumpTest");
            DataStore.GetInstance().Flush("ADVTESTAUTO");
        }

        public override void DeInitialize()
        {
            Fougerite.Hooks.OnCommand -= On_Command;
            Fougerite.Hooks.OnPlayerSpawned -= On_Spawn;
            Fougerite.Hooks.OnPlayerConnected -= OnPlayerConnected;
            Fougerite.Hooks.OnPlayerHurt -= OnPlayerHurt;
            Fougerite.Hooks.OnPlayerDisconnected -= OnPlayerDisconnected;
            Fougerite.Hooks.OnChat -= OnChat;
            Fougerite.Hooks.OnModulesLoaded -= OnModulesLoaded;
            Fougerite.Hooks.OnShoot -= OnShoot;
            Fougerite.Hooks.OnPlayerMove -= OnPlayerMove;
            List<ulong> list = UnderTesting.Keys.ToList();
            foreach (var x in list)
            {
                RemoveTest(Fougerite.Server.Cache[x], true);
            }
        }

        public void OnPlayerMove(HumanController hc, Vector3 origin, int encoded, ushort stateflags, uLink.NetworkMessageInfo info, Util.PlayerActions action)
        {
            if (!UnderTesting.ContainsKey(hc.netUser.userID))
            {
                return;
            }
            Fougerite.Player player = Fougerite.Server.Cache.ContainsKey(hc.netUser.userID) ? Fougerite.Server.Cache[hc.netUser.userID]
                    : Fougerite.Server.GetServer().FindPlayer(hc.netUser.userID.ToString());
            if (player == null)
            {
                if (hc.netUser == null) return;
                if (hc.netUser.connected)
                {
                    hc.netUser.Kick(NetError.NoError, true);
                }
                return;
            }
            if (action == Util.PlayerActions.TAB && MenuHackBan)
            {
                RemoveTest(player);
                Server.GetServer().BanPlayer(player, "Console", "Menu hack Detected!", null, true);
                if (RemoveSleeperD)
                {
                    ExecuteSleeperRemoval(player);
                }
            }
        }

        public void OnShoot(ShootEvent shootevent)
        {
            if (shootevent.Player != null)
            {
                if (!UnderTesting.ContainsKey(shootevent.Player.UID)) { return; }
                if (DataStore.GetInstance().ContainsKey("RecoilTest", shootevent.Player.UID))
                {
                    shootevent.IBulletWeaponItem.clipAmmo = 24;
                    return;
                }
                if (!Angles.ContainsKey(shootevent.Player.UID) || !UnderTesting.ContainsKey(shootevent.Player.UID))
                {
                    return;
                }
                if (!shootevent.BulletWeaponDataBlock.name.ToLower().Contains("m4"))
                {
                    return;
                }
                if (AnglesC.ContainsKey(shootevent.Player.UID))
                {
                    AnglesC[shootevent.Player.UID] = 0;
                }
                shootevent.IBulletWeaponItem.clipAmmo = 24;
                var player = shootevent.Player;
                Character c = player.PlayerClient.netUser.playerClient.controllable.character;
                var eyeangles = c.eyesAngles;
                if (Angles[player.UID] == eyeangles)
                {
                    if (AnglesC[player.UID] < 2)
                    {
                        AnglesC[player.UID] = AnglesC[player.UID] + 1;
                    }
                    else
                    {
                        RemoveTest(player);
                        Server.GetServer().BanPlayer(player, "Console", "NoRecoil", null, true);
                        if (RemoveSleeperD)
                        {
                            ExecuteSleeperRemoval(player);
                        }
                    }
                }
                else
                {
                    player.SendCommand("input.bind Up F4 None");
                    player.SendCommand("input.bind Down F4 None");
                    player.SendCommand("input.bind Left F4 None");
                    player.SendCommand("input.bind Right INSERT None");
                    UnderTesting[player.UID].RecoilComplete = true;
                    if (storage.ContainsKey(player.UID))
                    {
                        var n = storage[player.UID];
                        player.Inventory.RemoveItem(30);
                        player.Inventory.RemoveItem(31);
                        foreach (var x in n.Keys)
                        {
                            player.Inventory.AddItem(x, n[x]);
                        }
                        storage.Remove(player.UID);
                    }
                    var dict = TData[player.UID];
                    Vector3 pos = (Vector3)dict["Location"];
                    dict["ButtonPos"] = pos;
                    dict["SCount"] = 0;
                    dict["SCount2"] = 0;
                    dict["INSERT"] = 0;
                    dict["F2"] = 0;
                    dict["F5"] = 0;
                    dict["F3"] = 0;
                    var timedEvent2 = CreateParallelTimer(1000, dict);
                    timedEvent2.OnFire += Callback5;
                    timedEvent2.Start();
                }
            }
        }

        public void OnPlayerConnected(Fougerite.Player player)
        {
            DataStore.GetInstance().Add("ADVTESTAUTO", player.UID, 1);
            if (!GeoIPSupport)
            {
                return;
            }
            if (DataStore.GetInstance().Get("ADVTEST", player.UID) != null) { return;}
            Thread thread = new Thread(() => DetermineLocation(player));
            thread.IsBackground = true;
            thread.Start();
        }

        internal void DetermineLocation(Fougerite.Player player)
        {
            var GeoIPS = GeoIP.GeoIP.Instance;
            var data = GeoIPS.GetDataOfIP(player.IP);
            DataStore.GetInstance().Add("ADVTEST", player.UID, LanguageData.ContainsKey(data.Country) ? LanguageData[data.Country] : 1);
        }

        public void OnModulesLoaded()
        {
            foreach (var x in Fougerite.ModuleManager.Modules.Where(x => x.Plugin.Name.ToLower().Contains("geoip")))
            {
                GeoIPSupport = true;
            }
        }

        public void OnChat(Fougerite.Player player, ref ChatString text)
        {
            if (text.OriginalMessage.ToLower().Contains("hack"))
            {
                player.Notice("If you see a hacker, report him using /areport name - " + ReportsNeeded + " votes = AutoTest!");
            }
        }

        public void OnPlayerDisconnected(Fougerite.Player player)
        {
            if (!UnderTesting.ContainsKey(player.UID)) { return;}
            Server.GetServer().BanPlayer(player, "Console", "Disconnecting from AdvancedTest", null, true);
            RemoveTest(player, true);
            if (RemoveSleeperD)
            {
                ExecuteSleeperRemoval(player);
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
            switch (cmd)
            {
                case "advancedtest":
                {
                    player.MessageFrom("AdvancedTest", green + "AdvancedTest " + yellow + " V" + Version + " [COLOR#FFFFFF] By DreTaX");
                    if (player.Admin)
                    {
                        Settings = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                        TestAllowedEvery = int.Parse(Settings.GetSetting("Settings", "TestAllowedEvery")) * 60;
                        RecoilWait = int.Parse(Settings.GetSetting("Settings", "RecoilWait"));
                        ReportsNeeded = int.Parse(Settings.GetSetting("Settings", "ReportsNeeded"));
                        InsertWait = int.Parse(Settings.GetSetting("Settings", "InsertWait"));
                        F2Wait = int.Parse(Settings.GetSetting("Settings", "F2Wait"));
                        F5Wait = int.Parse(Settings.GetSetting("Settings", "F5Wait"));
                        F3Wait = int.Parse(Settings.GetSetting("Settings", "F3Wait"));
                        MaxTest = int.Parse(Settings.GetSetting("Settings", "MaxTest"));
                        IgnoreDropIfPing = int.Parse(Settings.GetSetting("Settings", "IgnoreDropIfPing"));
                        WarnSeconds = int.Parse(Settings.GetSetting("Settings", "WarnSeconds"));
                        AutoTestOnJoin = Settings.GetBoolSetting("Settings", "AutoTestOnJoin");
                        RemoveSleeperD = Settings.GetBoolSetting("Settings", "RemoveSleeperD");
                        DropTest = Settings.GetBoolSetting("Settings", "DropTest");
                        UnBindTab = Settings.GetBoolSetting("Settings", "UnBindTab");
                        MenuHackBan = Settings.GetBoolSetting("Settings", "MenuHackBan");
                        WarnInventoryBeforeTest = Settings.GetBoolSetting("Settings", "WarnInventoryBeforeTest");
                        var cmds = Settings.GetSetting("Settings", "RestrictedCommands").Split(Convert.ToChar(","));
                        RestrictedCommands.Clear();
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
                    break;
                }
                case "alang":
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
                            if (UnderTesting.ContainsKey(player.UID))
                            {
                                UnderTesting[player.UID].LangCode = i;
                            }
                            DataStore.GetInstance().Add("ADVTEST", player.UID, i);
                            player.MessageFrom("AdvancedTest",
                                green + "Language Set to: " + Settings.GetSetting("Languages", s));
                        }
                        else
                        {
                            player.MessageFrom("AdvancedTest", red + "Entered Value doesn't exist or It's not a number!");
                        }

                    }
                    break;
                }
                case "alangflush":
                {
                    if (player.Admin)
                    {
                        DataStore.GetInstance().Flush("ADVTEST");
                        player.MessageFrom("AdvancedTest", "Flushed!");
                    }
                    break;
                }
                case "recoiltest":
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
                        if (!UnderTesting.ContainsKey(p.UID))
                        {
                            player.MessageFrom("AdvancedTest", "Player is not being tested!");
                            return;
                        }
                        Send(p, false);
                        if (storage.ContainsKey(p.UID))
                        {
                            var n = storage[p.UID];
                            p.Inventory.RemoveItem(30);
                            p.Inventory.RemoveItem(31);
                            foreach (var x in n.Keys)
                            {
                                p.Inventory.AddItem(x, n[x]);
                            }
                            storage.Remove(p.UID);
                        }
                        if (UnderTesting.ContainsKey(p.UID))
                        {
                            UnderTesting.Remove(p.UID);
                        }
                        if (Angles.ContainsKey(p.UID))
                        {
                            Angles.Remove(p.UID);
                        }
                        if (AnglesC.ContainsKey(p.UID))
                        {
                            AnglesC.Remove(p.UID);
                        }
                        DataStore.GetInstance().Remove("RecoilTest", id);
                        player.MessageFrom("RecoilTest", red + "Testing ended for " + p.Name);
                        p.Notice("Recoil Test ended!");
                    }
                    else
                    {
                        if (UnderTesting.ContainsKey(p.UID))
                        {
                            player.MessageFrom("AdvancedTest", "Player is currently being tested!");
                            return;
                        }
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
                        p.Inventory.AddItemTo("556 Ammo", 31, 5);
                        int lang = 1;
                        if (DataStore.GetInstance().Get("ADVTEST", p.UID) != null)
                        {
                            lang = (int)DataStore.GetInstance().Get("ADVTEST", p.UID);
                        }
                        UnderTesting[p.UID] = new TestData(p, lang);
                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        dict["Location"] = p.Location;
                        dict["Player"] = p;
                        /*var timedEvent = CreateParallelTimer(2000, dict);
                        timedEvent.OnFire += Callback3;
                        timedEvent.Start();*/

                        DataStore.GetInstance().Add("RecoilTest", id, "1");
                        p.Notice("You are being tested for recoil!");
                        p.MessageFrom("RecoilTest", red + "=== Recoil Test ===");
                        p.MessageFrom("RecoilTest", red + "Take the M4, reload It and start shooting!");
                    }
                    break;
                }
                case "jumptest":
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
                    break;
                }
                case "fatest":
                {
                    if (!player.Admin && !player.Moderator)
                    {
                        return;
                    }
                    if (args.Length == 0)
                    {
                        player.MessageFrom("AdvancedTest", red + "Usage: /fatest playername - Test player");
                        return;
                    }
                    string s = string.Join(" ", args);
                    Fougerite.Player p = Fougerite.Server.GetServer().FindPlayer(s);
                    if (p == null)
                    {
                        player.MessageFrom("AdvancedTest", red + "Couldn't find " + s + "!");
                        return;
                    }
                    if (UnderTesting.Keys.Count == MaxTest)
                    {
                            player.MessageFrom("AdvancedTest", red 
                                + "Too many Players being tested! Wait for them to finish! (Max: " + MaxTest + ")");
                            return;
                    }
                    if (WarnInventoryBeforeTest)
                    {
                        p.Notice("", p.Name + " test starting, CLOSE your INVENTORY!", 8f);
                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        dict["Player"] = p;
                        var timedEvent = CreateParallelTimer(WarnSeconds * 1000, dict);
                        timedEvent.OnFire += DelayTest;
                        timedEvent.Start();
                    }
                    else
                    {
                        StartTest(p);
                    }
                    player.MessageFrom("AdvancedTest", green + "Test Started.");
                    break;
                }
                case "stopfatest":
                {
                    if (!player.Admin && !player.Moderator)
                    {
                        return;
                    }
                    if (args.Length == 0)
                    {
                        player.MessageFrom("AdvancedTest", red + "Usage: /stopfatest playername - Stop test");
                        return;
                    }
                    string s = string.Join(" ", args);
                    Fougerite.Player p = Fougerite.Server.GetServer().FindPlayer(s);
                    if (p == null)
                    {
                        player.MessageFrom("AdvancedTest", red + "Couldn't find " + s + "!");
                        return;
                    }
                    if (UnderTesting.ContainsKey(p.UID))
                    {
                        if (storage.ContainsKey(player.UID))
                        {
                            var n = storage[player.UID];
                            player.Inventory.RemoveItem(30);
                            player.Inventory.RemoveItem(31);
                            foreach (var x in n.Keys)
                            {
                                player.Inventory.AddItem(x, n[x]);
                            }
                            storage.Remove(player.UID);
                        }
                        RemoveTest(p);
                        player.MessageFrom("AdvancedTest", green + "Test Stopped.");
                    }
                    else
                    {
                        player.MessageFrom("AdvancedTest", green + "Player isn't being tested");
                    }
                    break;
                }
                case "areport":
                {
                    if (args.Length == 0)
                    {
                        player.MessageFrom("AdvancedTest", red + "Usage: /areport playername - Server will test player after " + ReportsNeeded + " reports");
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
                    if (DSNames.Any(x => DataStore.GetInstance().ContainsKey(x, p.UID) ||
                                         DataStore.GetInstance().ContainsKey(x, p.SteamID)))
                    {
                        player.MessageFrom("AdvancedTest", red + "Player cannot be tested this time, wait a bit!");
                        return;
                    }
                    if (UnderTesting.Keys.Count == MaxTest)
                    {
                        player.MessageFrom("AdvancedTest", red
                                + "Too many Players being tested! Wait for them to finish! (Max: " + MaxTest + ")");
                        return;
                    }
                    if (TestCooldown.ContainsKey(p.UID))
                    {
                        var Time = TestCooldown[p.UID];
                        var diff = (TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds - Time);
                        if (diff >= TestAllowedEvery)
                        {
                            var done = Math.Round(diff);
                            var done2 = Math.Round(Time);
                            player.MessageFrom("AdvancedTest",
                                "Player has report cooldown for: " + (done2 - done) + " seconds!");
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
                    }
                    else
                    {
                        Reports[p.UID] = Reports[p.UID] + 1;
                        player.MessageFrom("AdvancedTest", "Player " + p.Name + " reported. ( " + Reports[p.UID] + "/" + ReportsNeeded + " )");
                    }
                    if (Reports[p.UID] == ReportsNeeded)
                    {
                        Server.GetServer().BroadcastFrom("AdvancedTest", red + p.Name + " received enough reports. Auto test is starting.");
                        if (WarnInventoryBeforeTest)
                        {
                            p.Notice("", p.Name + " test starting, CLOSE your INVENTORY!", 8f);
                            Dictionary<string, object> dict = new Dictionary<string, object>();
                            dict["Player"] = p;
                            var timedEvent = CreateParallelTimer(WarnSeconds * 1000, dict);
                            timedEvent.OnFire += DelayTest;
                            timedEvent.Start();
                        }
                        else
                        {
                            StartTest(p);
                        }
                    }
                    else
                    {
                        player.MessageFrom("AdvancedTest", "Player " + p.Name + " reported. ( " + Reports[p.UID] + "/" + ReportsNeeded + " )");
                    }
                    break;
                }
            }
        }

        public void StartTest(Fougerite.Player player)
        {
            player.Inventory.InternalInventory.Invalidate();
            SendAutoTest(player);
            foreach (var x in RestrictedCommands)
            {
                player.RestrictCommand(x);
            }
            LastPos[player.UID] = player.Location;
            TestCooldown[player.UID] = TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds;
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
            int lang = 1;
            if (DataStore.GetInstance().Get("ADVTEST", player.UID) != null)
            {
                lang = (int) DataStore.GetInstance().Get("ADVTEST", player.UID);
            }
            if (UnderTesting.Keys.Count == 0)
            {
                // Lets do PowerSaving
                Fougerite.Hooks.OnPlayerMove -= OnPlayerMove;
                Fougerite.Hooks.OnPlayerMove += OnPlayerMove;
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
            player.Inventory.AddItemTo("556 Ammo", 31, 5);
            player.MessageFrom("AdvancedTest", yellow + "Type /alang to set a different language");
            player.MessageFrom("AdvancedTest", yellow + LanguageDict[TestDataP.LangCode][2]);
            if (EnableButtonWarnMessage)
            {
                player.MessageFrom("AdvancedTest", red + LanguageDict[TestDataP.LangCode][1]);
                player.MessageFrom("AdvancedTest", red + LanguageDict[TestDataP.LangCode][1]);
                player.MessageFrom("AdvancedTest", red + LanguageDict[TestDataP.LangCode][1]);
            }
            var dict = new  Dictionary<string, object>();
            dict["Player"] = player;
            var timedEvent = CreateParallelTimer(2500, dict);
            timedEvent.OnFire += Callback;
            timedEvent.Start();
            var timedEvent2 = CreateParallelTimer(450, dict);
            timedEvent2.OnFire += AntiMove;
            timedEvent2.Start();
        }

        public void RemoveTest(Fougerite.Player player, bool Disconnected = false)
        {
            if (!Disconnected)
            {
                SendAutoTest(player, false);
                if (LastPos.ContainsKey(player.UID))
                {
                    player.TeleportTo(LastPos[player.UID], false);
                    var dict = new Dictionary<string, object>();
                    dict["Location"] = LastPos[player.UID];
                    dict["Player"] = player;
                    var timedEvent = CreateParallelTimer(2000, dict);
                    timedEvent.OnFire += ReTeleport;
                    timedEvent.Start();
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
            if (TData.ContainsKey(player.UID))
            {
                TData.Remove(player.UID);
            }
            if (Angles.ContainsKey(player.UID))
            {
                Angles.Remove(player.UID);
            }
            if (AnglesC.ContainsKey(player.UID))
            {
                AnglesC.Remove(player.UID);
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
            if (UnderTesting.Keys.Count == 0)
            {
                // Lets do PowerSaving
                Fougerite.Hooks.OnPlayerMove -= OnPlayerMove;
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
            if (AutoTestOnJoin && DataStore.GetInstance().Get("ADVTESTAUTO", player.UID) != null)
            {
                DataStore.GetInstance().Remove("ADVTESTAUTO", player.UID);
                var dict = new Dictionary<string, object>();
                dict["Player"] = player;
                var timedEvent = CreateParallelTimer(5000, dict);
                timedEvent.OnFire += SpawnDelay;
                timedEvent.Start();
            }
        }

        private bool SendW(Fougerite.Player player)
        {
            if (!UnderTesting.ContainsKey(player.UID))
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
                return false;
            }
            var data = UnderTesting[player.UID];
            if (!data.RecoilComplete)
            {
                player.SendCommand("input.mousespeed 0");
                player.SendCommand("input.bind Up F3 None");
                player.SendCommand("input.bind Down F5 None");
                player.SendCommand("input.bind Left F2 None");
                player.SendCommand("input.bind Right INSERT None");
                player.SendCommand("input.bind Fire Mouse0 W");
                player.SendCommand("input.bind Jump None None");
                player.SendCommand("input.bind Duck None None");
                player.SendCommand("input.bind AltFire None None");
                player.SendCommand("input.bind Sprint None None");
                if (UnBindTab)
                {
                    player.SendCommand("input.bind Inventory None None");
                }
            }
            else if (!data.ButtonComplete)
            {
                player.SendCommand("input.bind Up None None");
                player.SendCommand("input.bind Down None None");
                player.SendCommand("input.bind Left None None");
                player.SendCommand("input.bind Right INSERT None");
                player.SendCommand("input.mousespeed 0");
                player.SendCommand("input.bind Fire Mouse0 W");
                player.SendCommand("input.bind Jump None None");
                player.SendCommand("input.bind Duck None None");
                player.SendCommand("input.bind AltFire None None");
                player.SendCommand("input.bind Sprint None None");
                if (UnBindTab)
                {
                    player.SendCommand("input.bind Inventory None None");
                }
            }
            else if (!data.ButtonComplete2)
            {
                player.SendCommand("input.bind Up None None");
                player.SendCommand("input.bind Down None None");
                player.SendCommand("input.bind Left F2 None");
                player.SendCommand("input.bind Right None None");
                player.SendCommand("input.mousespeed 0");
                player.SendCommand("input.bind Fire Mouse0 W");
                player.SendCommand("input.bind Jump None None");
                player.SendCommand("input.bind Duck None None");
                player.SendCommand("input.bind AltFire None None");
                player.SendCommand("input.bind Sprint None None");
                if (UnBindTab)
                {
                    player.SendCommand("input.bind Inventory None None");
                }
            }
            else if (!data.ButtonComplete3)
            {
                player.SendCommand("input.bind Up None None");
                player.SendCommand("input.bind Down F5 None");
                player.SendCommand("input.bind Left None None");
                player.SendCommand("input.bind Right None None");
                player.SendCommand("input.mousespeed 0");
                player.SendCommand("input.bind Fire Mouse0 W");
                player.SendCommand("input.bind Jump None None");
                player.SendCommand("input.bind Duck None None");
                player.SendCommand("input.bind AltFire None None");
                player.SendCommand("input.bind Sprint None None");
                if (UnBindTab)
                {
                    player.SendCommand("input.bind Inventory None None");
                }
            }
            else if (!data.ButtonComplete4)
            {
                player.SendCommand("input.bind Up F3 None");
                player.SendCommand("input.bind Down None None");
                player.SendCommand("input.bind Left None None");
                player.SendCommand("input.bind Right None None");
                player.SendCommand("input.mousespeed 0");
                player.SendCommand("input.bind Fire Mouse0 W");
                player.SendCommand("input.bind Jump None None");
                player.SendCommand("input.bind Duck None None");
                player.SendCommand("input.bind AltFire None None");
                player.SendCommand("input.bind Sprint None None");
                if (UnBindTab)
                {
                    player.SendCommand("input.bind Inventory None None");
                }
            }
            return true;
        }

        private void SendAutoTest(Fougerite.Player player, bool on = true)
        {
            if (on)
            {
                player.SendCommand("input.mousespeed 0");
                player.SendCommand("input.bind Up None None");
                player.SendCommand("input.bind Down F5 None");
                player.SendCommand("input.bind Left F2 None");
                player.SendCommand("input.bind Right INSERT None");
                player.SendCommand("input.bind Fire Mouse0 W");
                player.SendCommand("input.bind Jump None None");
                player.SendCommand("input.bind Duck None None");
                player.SendCommand("input.bind AltFire None None");
                player.SendCommand("input.bind Sprint None None");
                if (UnBindTab)
                {
                    player.SendCommand("input.bind Inventory None None");
                }
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
                player.SendCommand("input.bind Up None None");
                player.SendCommand("input.bind Down None None");
                player.SendCommand("input.bind Left F2 None");
                player.SendCommand("input.bind Right INSERT None");
                player.SendCommand("input.bind Fire Mouse0 W");
                player.SendCommand("input.bind Jump F5 None");
                player.SendCommand("input.bind Duck None None");
                player.SendCommand("input.bind AltFire None None");
                player.SendCommand("input.bind Sprint None None");
                if (UnBindTab)
                {
                    player.SendCommand("input.bind Inventory None None");
                }
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
                player.SendCommand("input.bind Down None None");
                player.SendCommand("input.bind Left F5 None");
                player.SendCommand("input.bind Right INSERT None");
                player.SendCommand("input.bind Fire Mouse0 W");
                player.SendCommand("input.bind AltFire None None");
                player.SendCommand("input.bind Sprint None None");
                player.SendCommand("input.bind Duck None None");
                if (UnBindTab)
                {
                    player.SendCommand("input.bind Inventory None None");
                }
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

        private void ExecuteSleeperRemoval(Fougerite.Player player)
        {
            if (RemoveSleeperD)
            {
                var dict = new Dictionary<string, object>();
                dict["Location"] = player.DisconnectLocation;
                dict["Player"] = player;
                var timedEvent = CreateParallelTimer(3500, dict);
                timedEvent.OnFire += RemoveSleeper;
                timedEvent.Start();
            }
        }

        public void ReTeleport(AdvancedTesterTE e)
        {
            e.Kill();
            var dict = e.Args;
            Fougerite.Player player = (Fougerite.Player)dict["Player"];
            if (!player.IsOnline)
            {
                return;
            }
            Vector3 location = (Vector3)dict["Location"];
            player.TeleportTo(location);
            player.MessageFrom("AdvancedTest", green + "Double teleported!");
        }

        public void SpawnDelay(AdvancedTesterTE e)
        {
            e.Kill();
            var dict = e.Args;
            Fougerite.Player player = (Fougerite.Player)dict["Player"];
            if (!player.IsOnline)
            {
                return;
            }
            StartTest(player);
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

        public void RemoveSleeper(AdvancedTesterTE e)
        {
            e.Kill();
            var dict = e.Args;
            Fougerite.Player player = (Fougerite.Player)dict["Player"];
            Vector3 location = (Vector3) dict["Location"];
            var sleepers = UnityEngine.Physics.OverlapSphere(location, 2f);
            foreach (var sleeper in sleepers)
            {
                var name = sleeper.name;
                if (!name.ToLower().Contains("malesleeper"))
                {
                    continue;
                }
                RustProto.Avatar playerAvatar = NetUser.LoadAvatar(player.UID);
                //Check if the player has a SLUMBER away event & a timestamp that's older than the oldest permitted, calculated above
                if (playerAvatar != null && playerAvatar.HasAwayEvent &&
                    playerAvatar.AwayEvent.Type == AwayEvent.Types.AwayEventType.SLUMBER &&
                    playerAvatar.AwayEvent.HasTimestamp)
                {
                    SleepingAvatar.TransientData transientData = SleepingAvatar.Close(player.UID);
                    if (transientData.exists)
                    {
                        transientData.AdjustIncomingAvatar(ref playerAvatar);
                        NetUser.SaveAvatar(player.UID, ref playerAvatar);
                    }
                }
            }
        }

        public void DelayTest(AdvancedTesterTE e)
        {
            var dict = e.Args;
            e.Kill();
            Fougerite.Player player = (Fougerite.Player)dict["Player"];
            Loom.QueueOnMainThread(() => {
                StartTest(player);
            });
        }

        public void Callback(AdvancedTesterTE e)
        {
            var dict = e.Args;
            e.Kill();
            Fougerite.Player player = (Fougerite.Player)dict["Player"];
            var loc = player.Location;
            dict["Location"] = loc;
            if (DropTest && player.Ping <= IgnoreDropIfPing)
            {
                player.TeleportTo(loc.x, loc.y + 35f, loc.z);
            }
            else
            {
                player.MessageFrom("AdvancedTest", yellow + "High Ping or DropTest is Disabled. Ignoring.");
                UnderTesting[player.UID].FallComplete = true;
            }
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
                        x.MessageFrom("AdvancedTest", green + player.Name + "'s ping: " + player.Ping);
                    }
                }
                if (storage.ContainsKey(player.UID))
                {
                    var n = storage[player.UID];
                    player.Inventory.RemoveItem(30);
                    player.Inventory.RemoveItem(31);
                    foreach (var x in n.Keys)
                    {
                        player.Inventory.AddItem(x, n[x]);
                    }
                    storage.Remove(player.UID);
                }
                RemoveTest(player);
                Fougerite.Server.GetServer().BanPlayer(player, "Console", "Auto DropTest Failed! (Ping: " + player.Ping + ")", null, true);
                if (RemoveSleeperD)
                {
                    ExecuteSleeperRemoval(player);
                }
                return;
            }
            player.MessageFrom("AdvancedTest", yellow + "Drop Test Complete!");
            Character c = player.PlayerClient.netUser.playerClient.controllable.character;
            var eyeangles = c.eyesAngles;
            player.FallDamage.ClearInjury();
            player.HumanBodyTakeDmg.SetBleedingLevel(0f);
            player.TeleportTo(pos, false);
            if (EnableButtonWarnMessage)
            {
                player.MessageFrom("AdvancedTest", yellow + LanguageDict[UnderTesting[player.UID].LangCode][1]);
                player.MessageFrom("AdvancedTest", yellow + LanguageDict[UnderTesting[player.UID].LangCode][1]);
                player.MessageFrom("AdvancedTest", yellow + LanguageDict[UnderTesting[player.UID].LangCode][1]);
            }

            /*var timedEvent = CreateParallelTimer(1500, dict);
            timedEvent.OnFire += Callback3;
            timedEvent.Start();*/

            Angles[player.UID] = eyeangles;
            AnglesC[player.UID] = 0;
            dict["Count"] = 0;
            var timedEvent2 = CreateParallelTimer(2000, dict);
            timedEvent2.OnFire += Callback4;
            timedEvent2.Start();
            TData[player.UID] = dict;
        }

        /*public void Callback3(AdvancedTesterTE e)
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
                if (storage.ContainsKey(player.UID))
                {
                    var n = storage[player.UID];
                    player.Inventory.RemoveItem(30);
                    player.Inventory.RemoveItem(31);
                    foreach (var x in n.Keys)
                    {
                        player.Inventory.AddItem(x, n[x]);
                    }
                    storage.Remove(player.UID);
                }
                RemoveTest(player);
                Server.GetServer().BanPlayer(player, "Console", "Hack Detected! (Movement)", null, true);
                if (RemoveSleeperD)
                {
                    ExecuteSleeperRemoval(player);
                }
            }
        }*/

        public void Callback4(AdvancedTesterTE e)
        {
            e.Kill();
            var dict = e.Args;
            Fougerite.Player player = (Fougerite.Player)dict["Player"];
            int count = (int)dict["Count"];
            if (UnderTesting[player.UID].RecoilComplete)
            {
                return;
            }
            if (RecoilWait != 0)
            {
                count++;
                player.MessageFrom("AdvancedTest", teal + LanguageDict[UnderTesting[player.UID].LangCode][3] + " ( " + count + "/" + RecoilWait + " )");
                if (count == RecoilWait)
                {
                    RemoveTest(player);
                    Server.GetServer().BanPlayer(player, "Console", "Recoil Timed out!", null, true);
                    if (RemoveSleeperD)
                    {
                        ExecuteSleeperRemoval(player);
                    }
                    return;
                }
                dict["Count"] = count;
            }
            else
            {
                player.MessageFrom("AdvancedTest", teal + LanguageDict[UnderTesting[player.UID].LangCode][3]);
            }
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
            int INSERT = (int)dict["INSERT"];
            if (InsertWait == 0)
            {
                player.MessageFrom("AdvancedTest", teal + LanguageDict[UnderTesting[player.UID].LangCode][4]);
            }
            var pll = player.Location;
            var dist = Vector3.Distance(pos, pll);
            if (SCount2 != 1)
            {
                dict["SCount2"] = 1;
            }
            if (INSERT == InsertWait && InsertWait != 0)
            {
                RemoveTest(player);
                Server.GetServer().BanPlayer(player, "Console", "Insert Press Timed Out!", null, true);
                if (RemoveSleeperD)
                {
                    ExecuteSleeperRemoval(player);
                }
                return;
            }
            /*if (dist < 0.10f && dist >= 0.001f)
            {
                SCount++;
                if (SCount == 1)
                {
                    RemoveTest(player);
                    Server.GetServer().BanPlayer(player, "Console", "Detected JACKED Hack.", null, true);
                    if (RemoveSleeperD)
                    {
                        ExecuteSleeperRemoval(player);
                    }
                    return;
                }
            }*/
            //else if (dist > 0.1f)
            if (dist > 0.10f)
            {
                player.SendCommand("input.bind Up None None");
                player.SendCommand("input.bind Down None None");
                player.SendCommand("input.bind Left F2 None");
                player.SendCommand("input.bind Right None None");
                player.MessageFrom("AdvancedTest", green + "ButtonTest Complete!");
                dict["ButtonPos"] = player.Location;
                dict["SCount"] = 0;
                UnderTesting[player.UID].ButtonComplete = true;
                var timedEvent3 = CreateParallelTimer(1500, dict);
                timedEvent3.OnFire += Callback6;
                timedEvent3.Start();
                return;
            }
            if (InsertWait > 0)
            {
                INSERT++;
                player.MessageFrom("AdvancedTest", teal + LanguageDict[UnderTesting[player.UID].LangCode][4] + " ( " + INSERT + "/" + InsertWait + " )");
            }
            dict["ButtonPos"] = pll;
            dict["SCount"] = SCount;
            dict["INSERT"] = INSERT;
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
            int F2 = (int)dict["F2"];
            var pll = player.Location;
            if (F2Wait == 0)
            {
                player.MessageFrom("AdvancedTest", teal + LanguageDict[UnderTesting[player.UID].LangCode][5]);
            }
            if (SCount2 != 2)
            {
                pos = pll;
                dict["SCount2"] = 2;
            }
            if (F2 == F2Wait && F2Wait != 0)
            {
                RemoveTest(player);
                Server.GetServer().BanPlayer(player, "Console", "F2 Press Timed Out!", null, true);
                if (RemoveSleeperD)
                {
                    ExecuteSleeperRemoval(player);
                }
                return;
            }
            var dist = Vector3.Distance(pos, pll);
            /*if (dist < 0.10f && dist >= 0.001f)
            {
                SCount++;
                if (SCount == 1)
                {
                    RemoveTest(player);
                    Server.GetServer().BanPlayer(player, "Console", "Detected Dizzy Hack.", null, true);
                    if (RemoveSleeperD)
                    {
                        ExecuteSleeperRemoval(player);
                    }
                    return;
                }
            }*/
            //else if (dist > 0.10f)
            if (dist > 0.10f)
            {
                player.SendCommand("input.bind Up None None");
                player.SendCommand("input.bind Down F5 None");
                player.SendCommand("input.bind Left None None");
                player.SendCommand("input.bind Right None None");
                player.MessageFrom("AdvancedTest", green + "ButtonTest2 Complete!");
                dict["ButtonPos"] = player.Location;
                dict["SCount"] = 0;
                UnderTesting[player.UID].ButtonComplete2 = true;
                var timedEvent3 = CreateParallelTimer(1500, dict);
                timedEvent3.OnFire += Callback7;
                timedEvent3.Start();
                return;
            }
            if (F2Wait > 0)
            {
                player.MessageFrom("AdvancedTest", teal + LanguageDict[UnderTesting[player.UID].LangCode][5] + " ( " + F2 + "/" + F2Wait + " )");
                F2++;
            }
            dict["ButtonPos"] = pll;
            dict["SCount"] = SCount;
            dict["F2"] = F2;
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
            int F5 = (int)dict["F5"];
            var pll = player.Location;
            if (F5 == 0)
            {
                player.MessageFrom("AdvancedTest", teal + LanguageDict[UnderTesting[player.UID].LangCode][6]);
            }
            if (SCount2 != 3)
            {
                pos = pll;
                dict["SCount2"] = 3;
            }
            if (F5 == F5Wait && F5Wait != 0)
            {
                RemoveTest(player);
                Server.GetServer().BanPlayer(player, "Console", "F5 Press Timed Out!", null, true);
                if (RemoveSleeperD)
                {
                    ExecuteSleeperRemoval(player);
                }
                return;
            }
            var dist = Vector3.Distance(pos, pll);
            /*if (dist < 0.10f && dist >= 0.001f)
            {
                SCount++;
                if (SCount == 1)
                {
                    RemoveTest(player);
                    Server.GetServer().BanPlayer(player, "Console", "Detected A3MON Hack.", null, true);
                    if (RemoveSleeperD)
                    {
                        ExecuteSleeperRemoval(player);
                    }
                    return;
                }
            }*/
            //else if (dist > 0.10f)
            if (dist > 0.10f)
            {
                player.SendCommand("input.bind Up None None");
                player.SendCommand("input.bind Down F3 None");
                player.SendCommand("input.bind Left None None");
                player.SendCommand("input.bind Right None None");
                player.MessageFrom("AdvancedTest", green + "ButtonTest3 Complete!");
                dict["ButtonPos"] = Vector3.zero;
                dict["SCount"] = 0;
                UnderTesting[player.UID].ButtonComplete3 = true;
                var timedEvent3 = CreateParallelTimer(1500, dict);
                timedEvent3.OnFire += Callback8;
                timedEvent3.Start();
                return;
            }
            if (F5Wait > 0)
            {
                player.MessageFrom("AdvancedTest", teal + LanguageDict[UnderTesting[player.UID].LangCode][6] + " ( " + F5 + "/" + F5Wait + " )");
                F5++;
            }
            dict["ButtonPos"] = pll;
            dict["SCount"] = SCount;
            dict["F5"] = F5;
            var timedEvent2 = CreateParallelTimer(1000, dict);
            timedEvent2.OnFire += Callback7;
            timedEvent2.Start();
        }

        public void Callback8(AdvancedTesterTE e)
        {
            e.Kill();
            var dict = e.Args;
            Vector3 pos = (Vector3)dict["ButtonPos"];
            Fougerite.Player player = (Fougerite.Player)dict["Player"];
            if (pos == Vector3.zero)
            {
                dict["ButtonPos"] = player.Location;
                pos = player.Location;
            }
            int SCount = (int)dict["SCount"];
            int SCount2 = (int)dict["SCount2"];
            int F3 = (int)dict["F3"];
            var pll = player.Location;
            if (F3 == 0)
            {
                player.MessageFrom("AdvancedTest", teal + LanguageDict[UnderTesting[player.UID].LangCode][7]);
            }
            if (SCount2 != 3)
            {
                pos = pll;
                dict["SCount2"] = 3;
            }
            if (F3 == F3Wait && F3Wait != 0)
            {
                RemoveTest(player);
                Server.GetServer().BanPlayer(player, "Console", "F3 Press Timed Out!", null, true);
                if (RemoveSleeperD)
                {
                    ExecuteSleeperRemoval(player);
                }
                return;
            }
            var dist = Vector3.Distance(pos, pll);
            /*if (dist < 0.10f && dist >= 0.001f)
            {
                SCount++;
                if (SCount == 1)
                {
                    RemoveTest(player);
                    Server.GetServer().BanPlayer(player, "Console", "Detected Dizzy Hack.", null, true);
                    if (RemoveSleeperD)
                    {
                        ExecuteSleeperRemoval(player);
                    }
                    return;
                }
            }*/
            //else if (dist > 0.10f)
            if (dist > 0.10f)
            {
                player.MessageFrom("AdvancedTest", green + "ButtonTest4 Complete!");
                UnderTesting[player.UID].ButtonComplete4 = true;
                RemoveTest(player);
                Server.GetServer().BroadcastFrom("AdvancedTest", green + player.Name + " passed all auto tests!");
                return;
            }
            if (F3Wait > 0)
            {
                player.MessageFrom("AdvancedTest", teal + LanguageDict[UnderTesting[player.UID].LangCode][7] + " ( " + F3 + "/" + F3Wait + " )");
                F3++;
            }
            dict["ButtonPos"] = pll;
            dict["SCount"] = SCount;
            dict["F3"] = F3;
            var timedEvent2 = CreateParallelTimer(1000, dict);
            timedEvent2.OnFire += Callback8;
            timedEvent2.Start();
        }

        public AdvancedTesterTE CreateParallelTimer(int timeoutDelay, Dictionary<string, object> args)
        {
            AdvancedTesterTE timedEvent = new AdvancedTesterTE(timeoutDelay);
            timedEvent.Args = args;
            return timedEvent;
        }

        public class TestData
        {
            private Fougerite.Player Player;
            private int LanguageCode = 1;
            private bool FallTest = false;
            private bool RecoilTest = false;
            private bool ButtonTest = false;
            private bool ButtonTest2 = false;
            private bool ButtonTest3 = false;
            private bool ButtonTest4 = false;

            public TestData(Fougerite.Player player, int LangC = 1)
            {
                Player = player;
                LanguageCode = LangC;
            }

            public int LangCode
            {
                get { return LanguageCode; }
                set { LanguageCode = value; }
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

            public bool ButtonComplete4
            {
                get { return ButtonTest4; }
                set { ButtonTest4 = value; }
            }
        }
    }
}
