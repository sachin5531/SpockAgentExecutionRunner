using InstantSpockExecutionRunner.DTOs;
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
        static Process teleportingTunnelProcess = null;

        static void Main(string[] args)
        {
            InstantSpockExecutionDTO dto = validateArgs(args);
            dto.print();

            var customSpockToken = launchTeleportingTunnel(dto.environmentType(), dto.opkeyBaseUrl());
            waitForTeleportingTunnelOnline(dto, customSpockToken);

            //now do FinalLocalExecutionInner()
            InitiateSpockExecution(dto, customSpockToken);

            teleportingTunnelProcess.WaitForExit();
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

            ProcessStartInfo psi = new ProcessStartInfo("java.exe");
            psi.ArgumentList.Add("-jar");
            psi.ArgumentList.Add(teleportingTunnelJar.FullName);
            psi.ArgumentList.Add("OpKeyTeleportingTunnel:" + encodedToken);
            psi.ArgumentList.Add(javaPath);
            psi.ArgumentList.Add("8");
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;

            teleportingTunnelProcess = Process.Start(psi);

            teleportingTunnelProcess.EnableRaisingEvents = true;
            teleportingTunnelProcess.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_OutputDataReceived);
            teleportingTunnelProcess.ErrorDataReceived += new System.Diagnostics.DataReceivedEventHandler(process_ErrorDataReceived);
            teleportingTunnelProcess.Exited += new System.EventHandler(process_Exited);


            teleportingTunnelProcess.BeginErrorReadLine();
            teleportingTunnelProcess.BeginOutputReadLine();


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
                Thread.Sleep(1000);
            }
            throw new Exception("Waited for 3 minutes for the SpockAgent to come online. Exiting now.");



        }

        static void process_Exited(object sender, EventArgs e)
        {
            Console.WriteLine(string.Format("process exited with code {0}\n", teleportingTunnelProcess.ExitCode.ToString()));
        }

        static void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data.Trim().Length > 0)
                Console.Error.WriteLine(e.Data.Trim());
        }

        static void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data.Trim().Length > 0)
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
