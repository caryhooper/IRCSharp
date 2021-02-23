//    Project:     IRC# Server 
//    File:        Program.cs
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
//using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using seaboy1234.Logging;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace IRCSharp.Server
{
    class Program
    {
        public static IrcServer Server { get; private set; }
        public static ServerConfig Config { get; private set; }

        static void Main(string[] args)
        {
            Console.Title = "IRC# Server";

            Server = new IrcServer();
            Config = new ServerConfig();

            string TLS_string = "--tls";
            foreach (string argument in args)
            {
                if (argument == TLS_string)
                {
                    IrcServer.Logger.Log(LogLevel.Info, $"--tls detected");
                    Config.TLS = true;
                    break;
                }
                else
                {
                    IrcServer.Logger.Log(LogLevel.Info, $"No TLS");
                    Config.TLS = false;
                }
            }
            Config.Load();
            Server.Hostname = Config.Host;

            for (int i = 0; i < Config.Ports.Length; i++)
            {
                int index = i;
                Thread thread = new Thread(() => Listen(index)) { IsBackground = true };
                thread.Start();
            }
            IrcServer.Logger.Log(LogLevel.Info, "Press ESC to exit.");
            while (Console.ReadKey().Key != ConsoleKey.Escape) ;
        }
        private static void Listen(int index)
        {
            //string certName = "Server.pfx";
            //checkCertificate(certName);
            try
            {
                TcpListener listener = new TcpListener(Config.Addresses[index], Config.Ports[index]);
                listener.Start();
                IrcServer.Logger.Log(LogLevel.Info, "Listening on {0}:{1}", Config.Addresses[index], Config.Ports[index]);
                while (true)
                {
                    Socket client = listener.AcceptSocket();
                    new IrcClient
                    {
                        Socket = client,
                        IrcServer = Server
                    }.OnConnect();
                }
            }
            catch (Exception ex)
            {
                IrcServer.Logger.Log(LogLevel.Error, 
                    "The IRC Server at {0}:{1} encountered an error and is no longer listening.", 
                    Config.Addresses[index], 
                    Config.Ports[index]);
                IrcServer.Logger.Log(ex);
            }
        }

        /*private static void checkCertificate(string certName)
        {
            //Check if pfx file (server.pfx) exists in directory.
            //If none exists, create one.
            if (!File.Exists(certName))
            {
                RSA privkey = RSA.Create();
                X509Certificate2 cert = new X509Certificate2(certName);


                Byte[] PKCS1Bytes = cert.ExportRSAPrivateKey();
                //do conversion of PKCS1 to PFX
                //Export to PFX with no password
                File.WriteAllBytes("Server.pfx", ca.ExportToPFX());
            }
            

        }*/
    }
}
