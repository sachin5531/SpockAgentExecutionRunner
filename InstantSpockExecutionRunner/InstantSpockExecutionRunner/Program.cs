using InstantSpockExecutionRunner.DTOs;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InstantSpockExecutionRunner
{
    class Program
    {
        static Process runTeleportingTunnelProcess = null;
        static Process installTeleportingTunnelProcess = null;

        static void Main(string[] args)
        {
            InstantSpockExecutionDTO dto = validateArgs(args);
            dto.print();

            var customSpockToken = launchTeleportingTunnel(dto.environmentType(), dto.opkeyBaseUrl());
            waitForTeleportingTunnelOnline(dto, customSpockToken);

            //now do FinalLocalExecutionInner()
            InitiateSpockExecution(dto, customSpockToken);

            runTeleportingTunnelProcess.WaitForExit();
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

            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");

            if (String.IsNullOrEmpty(javaHome))
                throw new Exception("JAVA_HOME is not set in environment variables.");

            var javaPath = Path.Combine(javaHome, "bin", "java.exe");

            if (!System.IO.File.Exists(javaPath))
                throw new Exception($"Java{javaPath} not found.");

            for (var i = 0; i < stringLength; i++)
            {
                var pos = Convert.ToInt16(new Random().Next(0, possible.Length));
                spockToken += possible[pos];
            }

            var spockArgument = environmentType + "_" + spockToken + "@#@" + opkeyBaseUrl;
            var customSpockToken = environmentType + "_" + spockToken;

            var encodedToken = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(spockArgument));

            var teleportingTunnelJar = new FileInfo("../../../../../Latest/OpkeyTeleportingTunnelUtility.jar");
            if (!teleportingTunnelJar.Exists)
            {
                teleportingTunnelJar = new FileInfo("./Latest/OpkeyTeleportingTunnelUtility.jar");
            }

            if (!teleportingTunnelJar.Exists)
                throw new FileNotFoundException(teleportingTunnelJar.FullName);

            //Now Need to install Teleporting Tunnel Utility to extract files is user dir
            ProcessStartInfo installTunnelPSI = new ProcessStartInfo("java.exe");
            installTunnelPSI.ArgumentList.Add("-cp");
            installTunnelPSI.ArgumentList.Add(teleportingTunnelJar.FullName);
            installTunnelPSI.ArgumentList.Add("com.ssts.sshTeleportingTunnel.CLI_Startup");
            installTunnelPSI.ArgumentList.Add("-quietInstall");
            installTunnelPSI.RedirectStandardError = true;
            installTunnelPSI.RedirectStandardOutput = true;

            installTeleportingTunnelProcess = Process.Start(installTunnelPSI);
            installTeleportingTunnelProcess.EnableRaisingEvents = true;
            installTeleportingTunnelProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_OutputDataReceived);
            installTeleportingTunnelProcess.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_ErrorDataReceived);
            installTeleportingTunnelProcess.Exited += new System.EventHandler(installTeleportingTunnelProcess_Exited);

            installTeleportingTunnelProcess.BeginErrorReadLine();
            installTeleportingTunnelProcess.BeginOutputReadLine();

            ProcessStartInfo runTeleportingTunnelPSI = new ProcessStartInfo("java.exe");
            runTeleportingTunnelPSI.ArgumentList.Add("-cp");
            runTeleportingTunnelPSI.ArgumentList.Add(teleportingTunnelJar.FullName);
            runTeleportingTunnelPSI.ArgumentList.Add("com.ssts.sshTeleportingTunnel.CLI_Startup");
            runTeleportingTunnelPSI.ArgumentList.Add("OpKeyTeleportingTunnel:" + encodedToken);
            runTeleportingTunnelPSI.ArgumentList.Add(javaPath);
            runTeleportingTunnelPSI.ArgumentList.Add("8");
            runTeleportingTunnelPSI.RedirectStandardError = true;
            runTeleportingTunnelPSI.RedirectStandardOutput = true;

            runTeleportingTunnelProcess = Process.Start(runTeleportingTunnelPSI);

            runTeleportingTunnelProcess.EnableRaisingEvents = true;
            runTeleportingTunnelProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_OutputDataReceived);
            runTeleportingTunnelProcess.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_ErrorDataReceived);
            runTeleportingTunnelProcess.Exited += new System.EventHandler(runTeleportingtunelProcess_Exited);


            runTeleportingTunnelProcess.BeginErrorReadLine();
            runTeleportingTunnelProcess.BeginOutputReadLine();


            return customSpockToken;
        }

        private static void waitForTeleportingTunnelOnline(InstantSpockExecutionDTO dto, String customSpockToken)
        {
            var apiUrl = dto.opkeyBaseUrl() + "/SpockAgentApi/GetTokenStatus?token=" + customSpockToken;
            HttpClient client = new HttpClient();
            var startedOn = DateTime.Now;
            while ((DateTime.Now - startedOn).TotalMinutes < 3)
            {
                var jsonResponse = client.GetAsync(apiUrl).Result.Content.ReadAsStringAsync().Result;

                var result = JsonConvert.DeserializeObject(jsonResponse).ToString();

                if (result == "Awake")
                    return;

                Thread.Sleep(1000);
            }
            throw new Exception("Waited for 3 minutes for the SpockAgent to come online. Exiting now.");



        }

        static void runTeleportingtunelProcess_Exited(object sender, EventArgs e)
        {
            Console.WriteLine(string.Format("process exited with code {0}\n", runTeleportingTunnelProcess.ExitCode.ToString()));
        }

        static void installTeleportingTunnelProcess_Exited(object sender, EventArgs e)
        {
            Console.WriteLine(string.Format("process exited with code {0}\n", installTeleportingTunnelProcess.ExitCode.ToString()));
        }

        static void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                Console.Error.WriteLine(e.Data.Trim());
        }

        static void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                Console.Out.WriteLine(e.Data.Trim());
        }

        private static async void InitiateSpockExecution(InstantSpockExecutionDTO dto, String customSpockToken)
        {

            var url = dto.opkeyBaseUrl() + "/api/SpockRestAPI/RunSpockExecution";
            HttpClient client = new HttpClient();

            ExecutionDataDTO executionDataDTO = new ExecutionDataDTO()
            {
                suitePath = dto.suitepath(),
                build = dto.build(),
                session = dto.sessionName(),
                plugin = dto.defaultPlugin(),
                SpockAgentBrowser = dto.browser(),
                token = customSpockToken
            };

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(executionDataDTO);
            var data = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            var base64EncodedAuthenticationString = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{dto.username()}:{dto.apikey()}"));

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            requestMessage.Content = data;

            requestMessage.Headers.Add("project", dto.project());
            requestMessage.Headers.Add("serverURL", dto.opkeyBaseUrl());

            var response = await client.SendAsync(requestMessage);

            string result = response.Content.ReadAsStringAsync().Result;

            Console.WriteLine(result);


        }


    }
}
