﻿using Homeautomation.GPIO;
using HomeAutomation.Application.ConfigRetriver;
using HomeAutomation.ConfigRetriver;
using HomeAutomation.Logging.Telegram;
using HomeAutomation.Network;
using HomeAutomation.Network.Getters;
using HomeAutomation.Network.WebUI;
using HomeAutomation.ObjectInterfaces;
using HomeAutomation.Objects;
using HomeAutomation.Objects.Blinds;
using HomeAutomation.Objects.External;
using HomeAutomation.Objects.External.Plugins;
using HomeAutomation.Objects.Fans;
using HomeAutomation.Objects.Inputs;
using HomeAutomation.Objects.Lights;
using HomeAutomation.Rooms;
using HomeAutomation.Scenarios;
using HomeAutomation.Users;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace HomeAutomationCore
{
    static class HomeAutomationMain
    {
        static void Main(string[] args)
        {
            int pos = Array.IndexOf(args, "--nogpio");
            bool noGPIO = false;
            if (pos <= -1)
            {
                noGPIO = true;
            }
            new HomeAutomationServer("A Switchando family", "password");

            if (!noGPIO) Console.WriteLine(PIGPIO.pigpio_start(null, null));

            Console.WriteLine("Welcome to Switchando Automation 4 BETA 4 (Bountiful Update) Server by Marco Realacci!");

            if (HomeAutomationServer.server.ObjectNetwork.Objects.MQTTClient == null)
            {
                Console.WriteLine("\n\nIt seems MQTT is not setted up yet, write down your MQTT broker's address (or leave it blank to skip MQTT configuration):");
                string addr = Console.ReadLine();
                if (!string.IsNullOrEmpty(addr))
                {
                    Console.WriteLine("MQTT broker username (leave it blank if your broker doesn't need login):");
                    string uname = Console.ReadLine();
                    if (!string.IsNullOrEmpty(uname))
                    {
                        Console.WriteLine("MQTT broker password:");
                        string passwd = Console.ReadLine();
                        MQTTClient mqtt = new MQTTClient(addr, uname, passwd);
                        HomeAutomationServer.server.ObjectNetwork.Objects.MQTTClient = mqtt;
                    }
                    else
                    {
                        MQTTClient mqtt = new MQTTClient(addr);
                        HomeAutomationServer.server.ObjectNetwork.Objects.MQTTClient = mqtt;
                    }
                }
            }
            else
            {
                HomeAutomationServer.server.ObjectNetwork.Objects.MQTTClient.Init();
            }

            new NetworkInterface("OBJECT_INTERFACE", ObjectInterface.SendParameters);
            new NetworkInterface("METHOD_INTERFACE", MethodInterface.SendParameters);
            new NetworkInterface("ACTION", HomeAutomation.ObjectInterfaces.Action.SendParameters);
            new NetworkInterface("ROOM", Room.SendParameters);
            new NetworkInterface("SCENARIO", Scenario.SendParameters);
            new NetworkInterface("GET", ObjectGetter.SendParameters);
            new NetworkInterface("USER", Identity.SendParameters);

            var gpio_switch = new NetworkInterface("GENERIC_SWITCH", Relay.SendParameters);
            new ObjectInterface(gpio_switch, "Switch", typeof(uint), "ON / OFF state");
            var gpio_switch_mi = new MethodInterface(gpio_switch, "switch", "Switch (on / off)");
            gpio_switch_mi.AddParameter(new MethodParameter("objname", typeof(string), "Device name"));
            gpio_switch_mi.AddParameter(new MethodParameter("switch", typeof(string), "Switch on (true / false)"));


            var http_switch = new NetworkInterface("HTTP_SWITCH", WebRelay.SendParameters);
            new HTMLFragment("SWITCH", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/web/standard-fragments/switch.htmlfragment");
            var prova = new ObjectInterface(http_switch, "Switch", typeof(uint), "ON / OFF state");
            var http_switch_mi = new MethodInterface(http_switch, "switch", "Switch (on / off)");
            http_switch_mi.AddParameter(new MethodParameter("objname", typeof(string), "Device name"));
            http_switch_mi.AddParameter(new MethodParameter("switch", typeof(string), "Switch on (true / false)"));

            var rgb = new NetworkInterface("LIGHT_GPIO_RGB", RGBLight.SendParameters);
            new HTMLFragment("COLORABLE_LIGHT", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/web/standard-fragments/colorable_light.htmlfragment");
            new ObjectInterface(rgb, "Switch", typeof(uint), "ON / OFF state");
            new ObjectInterface(rgb, "ValueR", typeof(uint), "Red value");
            new ObjectInterface(rgb, "ValueG", typeof(uint), "Green value");
            new ObjectInterface(rgb, "ValueB", typeof(uint), "Blue value");
            var rgb_mi = new MethodInterface(rgb, "switch", "Switch (on / off)");
            rgb_mi.AddParameter(new MethodParameter("objname", typeof(string), "Device name"));
            rgb_mi.AddParameter(new MethodParameter("switch", typeof(string), "Switch on (true / false)"));
            var rgb_chgClr = new MethodInterface(rgb, "changeColor/RGB", "Change color from RGB values");
            rgb_chgClr.AddParameter(new MethodParameter("objname", typeof(string), "Device name"));
            rgb_chgClr.AddParameter(new MethodParameter("R", typeof(string), "Red value"));
            rgb_chgClr.AddParameter(new MethodParameter("G", typeof(string), "Green value"));
            rgb_chgClr.AddParameter(new MethodParameter("B", typeof(string), "Blue value"));
            rgb_chgClr.AddParameter(new MethodParameter("dimmer", typeof(string), "Dimmer transition (in ms)"));
            var rgb_chgVal = new MethodInterface(rgb, "dimm", "Change brightness");
            rgb_chgVal.AddParameter(new MethodParameter("objname", typeof(string), "Device name"));
            rgb_chgVal.AddParameter(new MethodParameter("percentage", typeof(string), "Brightness (%)"));
            rgb_chgVal.AddParameter(new MethodParameter("dimmer", typeof(string), "Dimmer transition (in ms)"));

            var w = new NetworkInterface("LIGHT_GPIO_W", WLight.SendParameters);
            new HTMLFragment("DIMMABLE_LIGHT", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/web/standard-fragments/dimmable_light.htmlfragment");
            new ObjectInterface(w, "Switch", typeof(uint), "ON / OFF state");
            new ObjectInterface(w, "Value", typeof(uint), "Brightness (0-255)");
            var w_mi = new MethodInterface(w, "switch", "Switch (on / off)");
            w_mi.AddParameter(new MethodParameter("objname", typeof(string), "Device name"));
            w_mi.AddParameter(new MethodParameter("switch", typeof(string), "Switch on (true / false)"));
            var w_chgVal = new MethodInterface(w, "changeValue", "Change brightness");
            w_chgVal.AddParameter(new MethodParameter("objname", typeof(string), "Device name"));
            w_chgVal.AddParameter(new MethodParameter("value", typeof(string), "Brightness (0-255)"));
            w_chgVal.AddParameter(new MethodParameter("dimmer", typeof(string), "Dimmer transition (in ms)"));

            new NetworkInterface("BUTTON", Button.SendParameters);
            new NetworkInterface("SWITCH_BUTTON", SwitchButton.SendParameters);

            var blinds = new NetworkInterface("BLINDS", Blinds.SendParameters);
            new HTMLFragment("BLINDS", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/web/standard-fragments/blinds.htmlfragment");
            new ObjectInterface(blinds, "Switch", typeof(uint), "ON / OFF state");
            //add step in percentage
            var blinds_mi = new MethodInterface(blinds, "switch", "Open / close");
            blinds_mi.AddParameter(new MethodParameter("objname", typeof(string), "Device name"));
            blinds_mi.AddParameter(new MethodParameter("switch", typeof(string), "Switch on (true / false)"));
            var blinds_mv = new MethodInterface(blinds, "move", "");
            blinds_mv.AddParameter(new MethodParameter("objname", typeof(string), "Device name"));
            blinds_mv.AddParameter(new MethodParameter("value", typeof(string), "Closing percentage (%)"));

            new SetupTool("LIGHT_GPIO_RGB", RGBLight.Setup);
            new SetupTool("LIGHT_GPIO_W", WLight.Setup);
            new SetupTool("GENERIC_SWITCH", Relay.Setup);
            new SetupTool("BUTTON", Button.Setup);
            new SetupTool("SWITCH_BUTTON", SwitchButton.Setup);
            new SetupTool("HTTP_SWITCH", WebRelay.Setup);
            new SetupTool("BLINDS", Blinds.Setup);

            if (!noGPIO) new Client.Client(null, 0, "local");

            if (Identity.GetAdminUser() == null)
            {
                Console.WriteLine("\n\nIt seems you don't have an admin account yet, please wite down the new password for the 'admin' user:");
                string passwd = Console.ReadLine();
                new Identity("admin", passwd, Identity.UserType.ADMINISTRATOR);
            }

            string jsonned = null;
            if (File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/configuration.json"))
                jsonned = File.ReadAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/configuration.json");

            if (File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/telegram.config"))
            {
                string telegramRaw = File.ReadAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/telegram.config"); //token@log@alert
                string[] telegramData = telegramRaw.Split('@');
                if (telegramData.Length == 3)
                {
                    HomeAutomationServer.server.Telegram = new TelegramBot(telegramData[0]);
                    HomeAutomationServer.server.Telegram.SetLogChat(long.Parse(telegramData[1]));
                    HomeAutomationServer.server.Telegram.SetAlertChat(long.Parse(telegramData[2]));
                }
            }
            Plugins.LoadAll("plugins");

            if (!string.IsNullOrEmpty(jsonned))
            {
                ModelRoom[] rooms = JsonConvert.DeserializeObject<ModelRoom[]>(jsonned);
                foreach (ModelRoom mRoom in rooms)
                {
                    Room room = new Room(mRoom.Name, mRoom.FriendlyNames, mRoom.Hidden);
                    foreach (dynamic device in mRoom.Objects)
                    {
                        Console.WriteLine(device.ClientName + " <<->> " + device.Name + " -> " + device.ObjectType.ToString());

                        Client.Client client = null;

                        bool toAdd = true;

                        if (device.ClientName != null)
                        {
                            foreach (Client.Client clnt in HomeAutomationServer.server.Clients)
                            {
                                if (clnt.Name.Equals(device.ClientName))
                                {
                                    client = clnt;
                                    toAdd = false;
                                }
                            }
                            if (toAdd) client = new Client.Client(null, 0, device.ClientName);

                            if (HomeAutomationServer.server.Clients.Count == 0)
                            {
                                client = new Client.Client(null, 0, device.ClientName);
                            }
                        }
                        bool exit = false;
                        foreach (IObject iobj in HomeAutomationServer.server.Objects)
                        {
                            if (iobj.GetName().Equals(device.Name))
                            {
                                room.AddItem(iobj);
                                exit = true;
                            }
                        }
                        if (exit) continue;

                        if (SetupTool.Exists(device.ObjectType))
                        {
                            device.Client = client;
                            SetupTool.FromId(device.ObjectType).Run(room, device);
                        }
                    }
                }
            }
            foreach (Room room in HomeAutomationServer.server.Rooms)
            {
                Console.WriteLine(room.Name + " -> ");
                foreach (IObject iobj in room.Objects)
                {
                    Console.WriteLine(iobj.GetName());
                }
                Console.WriteLine();
            }
            foreach (IObject iobj in HomeAutomationServer.server.Objects)
            {
                Console.WriteLine(iobj.GetName());
            }
            new HTTPHandler(new string[] { "http://*:8080/api/" });

            HomeAutomationServer.server.ObjectNetwork.Objects.MQTTClient.Connect();
            new HTTPWebUI("web", 8080);
            Console.WriteLine(">> Switchando is ready <<");
            Console.ReadLine();
        }
    }
}