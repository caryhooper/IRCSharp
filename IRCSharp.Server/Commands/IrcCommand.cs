﻿//    Project:     IRC# Server 
//    File:        IrcCommand.cs
//    Copyright:   Copyright (C) 2014 Christian Wilson. All rights reserved.
//    Website:     https://github.com/seaboy1234/IRCSharp
//    Description: An Internet Relay Chat (IRC) Server written in C#.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace IRCSharp.Server.Commands
{
    public abstract class IrcCommand
    {
        private static List<IrcCommand> _Commands = new List<IrcCommand>();

        static IrcCommand()
        {
            AppDomain.CurrentDomain.GetAssemblies().ForEach(assembly => assembly.GetTypes().Where(type =>
                    type.IsSubclassOf(typeof(IrcCommand)) &&
                    !type.IsAbstract &&
                    !type.IsInterface
                ).ForEach(type =>
                {
                    IrcCommand command = (IrcCommand)type.GetConstructor(new Type[0]).Invoke(null);
                    _Commands.Add(command);
                }));
        }

        public static IrcCommand Find(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
            {
                return null;
            }
            return _Commands.Where(command => command.Name.ToLower() == commandName.ToLower()).FirstOrDefault();
        }

        public virtual bool RequireRegistered { get { return true; } }
        public abstract string Name { get; }

        public abstract void Run(IIrcUser client, IrcMessage message);

        public void SendMessage(IrcNumericResponceId responceId, IIrcUser client, string message)
        {
            IrcNumericResponce Response = new IrcNumericResponce();
            Response.Host = client.IrcServer.Hostname;
            Response.To = client.Nick;
            Response.NumericId = responceId;
            Response.Message = message;
            client.Write(Response);
        }
        public void Say(IIrcUser client, string command, string message)
        {
            IrcMessage Response = new IrcMessage();
            Response.Prefix = client.Nick;
            Response.Command = command.ToUpper();
            Response.Params = new string[] { message };
            client.Write(Response);
        }
        public void NeedMoreParamsError(IIrcUser client)
        {
            client.Write(new IrcNumericResponce
            {
                NumericId = IrcNumericResponceId.ERR_NEEDMOREPARAMS,
                To = Name.ToUpper(),
                Message = "Need more params"
            });
        }
        public void NotFoundError(IIrcUser client, bool isChannel)
        {
            IrcNumericResponce Response = new IrcNumericResponce();
            Response.Message = (isChannel ? "channel" : "nick") + " not found.";
            Response.NumericId = isChannel ? IrcNumericResponceId.ERR_NOSUCHCHANNEL : IrcNumericResponceId.ERR_NOSUCHNICK;
            Response.To = Name.ToUpper();
            client.Write(Response);
        }
        public void PermissionsError(IIrcUser client)
        {
            IrcMessage Response = new IrcMessage();
            Response.Params = (client.Nick + ":Insufficiant permissions for " + Name.ToUpper()).Split(':');
            Response.Prefix = client.IrcServer.Hostname;
            Response.Command = "NOTICE";
            client.Write(Response);
        }
    }
}
