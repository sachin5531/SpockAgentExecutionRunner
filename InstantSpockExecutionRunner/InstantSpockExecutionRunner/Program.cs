using InstantSpockExecutionRunner.DTOs;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace InstantSpockExecutionRunner
{
    class Program
    {
        static Process teleportingTunnelProcess = null;

        static void Main(string[] args)
        {
            InstantSpockExecutionDTO dto = validateArgs(args);
            dto.print();

            var customSpockToken = launchTeleportingTunnel(dto.environmentType(), dto.opkeyBaseUrl());
            waitForTeleportingTunnelOnline(dto, customSpockToken);

        }

        private static InstantSpockExecutionDTO validateArgs(string[] args)
        {
            InstantSpockExecutionDTO dto = new InstantSpockExecutionDTO();
            foreach (String arg in args)
            {
                if (arg.StartsWith("--") && arg.Contains("="))
                {
                    var argName = arg.Split("=")[0].Substring(2);
                    var argValue = arg.Split("=")[1];
                    dto.setArgument(argName, argValue);
                }
                else
                {
                    throw new ArgumentException("Unrecognised Argument: " + arg + ". This has to be in the format: --argName=argValue");
                }
            }
            dto.validate();
            return dto;
        }

        private static String launchTeleportingTunnel(String environmentType, String opkeyBaseUrl)
        {

            var spockToken = "";
            var stringLength = 5;
            var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            for (var i = 0; i < stringLength; i++)
            {
                var pos = Convert.ToInt16(new Random().Next(0, possible.Length));
                spockToken += possible[pos];
            }

            var spockArgument = environmentType + "_" + spockToken + "@#@" + opkeyBaseUrl;
            var customSpockToken = environmentType + "_" + spockToken;

            var encodedToken = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(spockArgument));

            var teleportingTunnelJar = new FileInfo("../../../../../Latest/OpkeyTeleportingTunnelUtility.jar");

            ProcessStartInfo psi = new ProcessStartInfo("java.exe");
            psi.ArgumentList.Add("-jar");
            psi.ArgumentList.Add(teleportingTunnelJar.FullName);
            psi.ArgumentList.Add(encodedToken);
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            teleportingTunnelProcess = Process.Start(psi);
            teleportingTunnelProcess.WaitForExit();

            return customSpockToken;
        }

        private static void waitForTeleportingTunnelOnline(InstantSpockExecutionDTO dto, String customSpockToken)
        {
            var apiUrl = dto.opkeyBaseUrl() + "/SpockAgentApi/GetTokenStatus?token=" + customSpockToken;
            HttpClient client = new HttpClient();
            var startedOn = DateTime.Now;
            while ((DateTime.Now - startedOn).TotalMinutes < 3)
            {
                var response = client.GetAsync(apiUrl).Result.Content.ReadAsStringAsync().Result;
                if (response == "Awake")
                    return;
            }
            throw new Exception("Waited for 3 minutes for the SpockAgent to come online. Exiting now.")



        }
    }
}
