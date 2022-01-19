using InstantSpockExecutionRunner.DTOs;
using System;
using System.Diagnostics;
using System.IO;

namespace InstantSpockExecutionRunner
{
    class Program
    {
        static Process teleportingTunnelProcess = null;

        static void Main(string[] args)
        {
            InstantSpockExecutionDTO dto = validateArgs(args);
            dto.print();

            launchTeleportingTunnel(dto.environmentType(), dto.opkeyBaseUrl());
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
                var pos = Convert.ToInt16(Math.Floor(Convert.ToDouble(new Random().Next(0, 1) * possible.Length)));
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

        private static void waitForTeleportingTunnelOnline(InstantSpockExecutionDTO dto)
        {
            var apiUrl = dto.opkeyBaseUrl() + "/SpockAgentApi/GetTokenStatus";
        }
    }
}
