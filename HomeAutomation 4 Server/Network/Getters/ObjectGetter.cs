﻿using HomeAutomation.Network.APIStatus;
using HomeAutomation.Objects;
using HomeAutomation.Objects.Switches;
using HomeAutomation.Rooms;
using HomeAutomation.Users;
using HomeAutomationCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomation.Network.Getters
{
    class ObjectGetter
    {
        public static string SendParameters(string method, string[] request, Identity login)
        {
            if (method.Equals("rooms"))
            {
                List<Room> rooms = new List<Room>();
                foreach (Room iobj in HomeAutomationServer.server.Rooms)
                {
                    if (login.HasAccess(iobj)) rooms.Add(iobj);
                }
                return JsonConvert.SerializeObject(rooms);
            }
            if (method.Equals("devices"))
            {
                List<IObject> devices = new List<IObject>();
                foreach(IObject iobj in HomeAutomationServer.server.Objects)
                {
                    if (login.HasAccess(iobj)) devices.Add(iobj);
                }
                return JsonConvert.SerializeObject(devices);
            }
            if (method.Equals("switchable_devices"))
            {
                List<IObject> devices = new List<IObject>();
                foreach (IObject iobj in HomeAutomationServer.server.Objects)
                {
                    if (iobj is ISwitch)
                    {
                        if (login.HasAccess(iobj)) devices.Add(iobj);
                    }
                }
                return JsonConvert.SerializeObject(devices);
            }
            if (method.Equals("clients"))
            {
                return JsonConvert.SerializeObject(HomeAutomationServer.server.Clients);
            }
            if (method.Equals("users"))
            {
                List<string> usernames = new List<string>();
                foreach (Identity identity in HomeAutomationServer.server.ObjectNetwork.Objects.Identities)
                {
                    usernames.Add(identity.Name);
                }
                return JsonConvert.SerializeObject(usernames);
            }
            return new ReturnStatus(CommonStatus.ERROR_NOT_IMPLEMENTED).Json();
        }
    }
}
