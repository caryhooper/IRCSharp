﻿//    Project:     IRC# Server 
//    File:        WhoCommand.cs
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
    public class WhoCommand : IrcCommand
    {
        public override string Name
        {
            get { return "who"; }
        }

        public override void Run(IIrcUser client, IrcMessage message)
        {
            if (message.Params.Length != 1)
            {
                SendMessage(IrcNumericResponceId.ERR_NEEDMOREPARAMS, client, "Not enough parameters");
                return;
            }
            if (IRC.IsChannel(message.Params[0]))
            {
                IrcChannel channel = client.IrcServer.Channels.Where(ch => ch.Name == message.Params[0]).FirstOrDefault();
                if (channel == null)
                {
                    return;
                }
                channel.Users.ForEach(cli => cli.Write(new IrcNumericResponce()
                {
                    NumericId = IrcNumericResponceId.RPL_WHOREPLY,
                    Message = cli.UserString
                }));
                client.Write(new IrcNumericResponce()
                {
                    NumericId = IrcNumericResponceId.RPL_ENDOFWHO,
                    Message = "End of who"
                });
            }
        }
    }
}
