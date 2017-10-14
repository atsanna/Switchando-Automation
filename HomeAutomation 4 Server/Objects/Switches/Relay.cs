﻿using Homeautomation.GPIO;
using HomeAutomation.Network;
using HomeAutomation.Network.APIStatus;
using HomeAutomation.ObjectInterfaces;
using HomeAutomation.Objects.Switches;
using HomeAutomation.Rooms;
using HomeAutomationCore;
using HomeAutomationCore.Client;
using System;
using System.Collections.Generic;

namespace HomeAutomation.Objects.Fans
{
    class Relay : ISwitch
    {
        Client Client;
        public string ClientName;
        public uint Pin { get; set; }
        public string Name;
        public string[] FriendlyNames;
        public bool Switch;
        public string Description;

        public string ObjectType = "GENERIC_SWITCH";
        public string ObjectModel = "SWITCH";

        public Relay()
        {
            NetworkInterface.Delegate requestHandler;
            requestHandler = SendParameters;
            NetworkInterface networkInterface = new NetworkInterface(ObjectType, requestHandler);

            new ObjectInterface(null, "Pin", typeof(uint), "testing things");
            new MethodInterface(NetworkInterface.FromId("relay"), "switch_TEST", "another testing thing");
        }
        public Relay(Client client, string name, uint pin, string description, string[] friendlyNames)
        {
            this.Client = client;
            this.ClientName = client.Name;
            this.FriendlyNames = friendlyNames;

            this.Description = description;
            this.Pin = pin;
            this.Name = name;
            HomeAutomationServer.server.Objects.Add(this);

            NetworkInterface.Delegate requestHandler;
            requestHandler = SendParameters;
            NetworkInterface networkInterface = new NetworkInterface(ObjectType, requestHandler);

            new ObjectInterface(null, "Pin", typeof(uint), "testing things");
            new MethodInterface(NetworkInterface.FromId("relay"), "switch_TEST", "another testing thing");
        }
        public void SetClient(Client client)
        {
            this.Client = client;
        }

        public void Start()
        {
            Console.WriteLine("Switch `" + this.Name + "` has been turned on.");
            //HomeAutomationServer.server.Telegram.Log("Switch `" + this.Name + "` has been turned on.");
            if (Client.Name.Equals("local"))
            {
                PIGPIO.gpio_write(0, Pin, 1);
            }
            else
            {
                UploadValues(true);
            }
            Switch = true;
        }
        public void Stop()
        {
            Console.WriteLine("Switch `" + this.Name + "` has been turned off.");
            //HomeAutomationServer.server.Telegram.Log("Switch `" + this.Name + "` has been turned off.");
            if (Client.Name.Equals("local"))
            {
                PIGPIO.gpio_write(0, Pin, 0);
            }
            else
            {
                UploadValues(false);
            }
            Switch = false;
        }
        public bool IsOn()
        {
            return Switch;
        }
        public string GetName()
        {
            return Name;
        }
        public string GetId()
        {
            return Name;
        }
        public string GetObjectType()
        {
            return "GENERIC_SWITCH";
        }
        public string[] GetFriendlyNames()
        {
            return FriendlyNames;
        }
        void UploadValues(bool Value)
        {
            Client.Sendata("interface=relay&objname=" + this.Name + "&enabled=" + Value.ToString());
        }
        public NetworkInterface GetInterface()
        {
            return NetworkInterface.FromId("relay");
        }
        private static Relay FindRelayFromName(string name)
        {
            Relay relay = null;
            foreach (IObject obj in HomeAutomationServer.server.Objects)
            {
                if (obj.GetName().ToLower().Equals(name.ToLower()))
                {
                    relay = (Relay)obj;
                    break;
                }
                if (obj.GetFriendlyNames() == null) continue;
                if (Array.IndexOf(obj.GetFriendlyNames(), name.ToLower()) > -1)
                {
                    relay = (Relay)obj;
                    break;
                }
            }
            return relay;
        }
        public static string SendParameters(string method, string[] request)
        {
            if (method.Equals("switch"))
            {
                Relay relay = null;
                bool status = false;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            relay = FindRelayFromName(command[1]);
                            break;
                        case "switch":
                            status = bool.Parse(command[1]);
                            break;
                    }
                    if (relay == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
                }
                if (status) relay.Start(); else relay.Stop();
                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }

            if (method.Equals("switch_TEST")) //REMOVE THIS PLS
            {
                Relay relay = null;
                bool status = false;

                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    switch (command[0])
                    {
                        case "objname":
                            relay = FindRelayFromName(command[1]);
                            break;
                        case "switch":
                            status = bool.Parse(command[1]);
                            break;
                    }
                    if (relay == null) return new ReturnStatus(CommonStatus.ERROR_NOT_FOUND).Json();
                }
                if (status) relay.Start(); else relay.Stop();
                return new ReturnStatus(CommonStatus.SUCCESS).Json();
            }

            if (string.IsNullOrEmpty(method))
            {
                Relay fan = null;
                foreach (string cmd in request)
                {
                    string[] command = cmd.Split('=');
                    if (command[0].Equals("interface")) continue;
                    switch (command[0])
                    {
                        case "objname":
                            foreach (IObject obj in HomeAutomationServer.server.Objects)
                            {
                                if (obj.GetName().Equals(command[1]))
                                {
                                    fan = (Relay)obj;
                                }
                                if (obj.GetFriendlyNames() == null) continue;
                                if (Array.IndexOf(obj.GetFriendlyNames(), command[1].ToLower()) > -1)
                                {
                                    fan = (Relay)obj;
                                }
                            }
                            break;

                        case "switch":
                            if (command[1].ToLower().Equals("true"))
                            {
                                fan.Start();
                            }
                            else
                            {
                                fan.Stop();
                            }
                            break;
                    }
                }
            }
            return "";
        }
        public static void Setup(Room room, dynamic device)
        {
            Relay relay = new Relay();
            relay.Pin = (uint)device.Pin;
            relay.Name = device.Name;
            relay.Description = device.Description;
            relay.FriendlyNames = Array.ConvertAll(((List<object>)device.FriendlyNames).ToArray(), x => x.ToString());
            relay.Switch = device.Switch;
            relay.ClientName = device.Client.Name;
            relay.SetClient(device.Client);

            HomeAutomationServer.server.Objects.Add(relay);
            room.AddItem(relay);
        }
    }
}