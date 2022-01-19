using InstantSpockExecutionRunner.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace InstantSpockExecutionRunner.DTOs
{
    public class InstantSpockExecutionDTO
    {
        private Dictionary<String, String> _dict = new Dictionary<string, string>();

        public InstantSpockExecutionDTO()
        {

        }

        public void validate()
        {
            if (String.IsNullOrEmpty(environmentType()))
                throw new SpockArgumentException("environmentType");

            if (String.IsNullOrEmpty(opkeyBaseUrl()))
                throw new SpockArgumentException("opkeyBaseUrl");

            if (String.IsNullOrEmpty(sessionName()))
                throw new SpockArgumentException("sessionName");

            if (String.IsNullOrEmpty(defaultPlugin()))
                throw new SpockArgumentException("defaultPlugin");

            if (String.IsNullOrEmpty(build()))
                throw new SpockArgumentException("build");

            if (String.IsNullOrEmpty(suitepath()))
                throw new SpockArgumentException("suitepath");

            if (String.IsNullOrEmpty(browser()))
                throw new SpockArgumentException("browser");

            if (String.IsNullOrEmpty(username()))
                throw new SpockArgumentException("username");

            if (String.IsNullOrEmpty(apikey()))
                throw new SpockArgumentException("apikey");

            if (String.IsNullOrEmpty(project()))
                throw new SpockArgumentException("project");

        }

        public void print()
        {
            Console.WriteLine("\t\t\t=============================");
            foreach(String key in _dict.Keys)
            {
                Console.WriteLine(String.Format("{0,-27}",  key) + " = \t" + _dict[key]);
            }
            Console.WriteLine("\t\t\t=============================");
        }
        public void setArgument(String argName, String argValue)
        {
            _dict.Add(argName, argValue);
        }

        private String getArgValue()
        {
            var methodName = new StackTrace().GetFrame(1).GetMethod().Name;
            return getArgValue(methodName);
        }
        private String getArgValue(String argName)
        {
            if (_dict.ContainsKey(argName))
                return _dict[argName];
            else
                return null;
        }

        public String environmentType()
        {
            return getArgValue();
        }

        public String opkeyBaseUrl()
        {
            return getArgValue();
        }

        public String sessionName()
        {
            return getArgValue();
        }

        public String defaultPlugin()
        {
            return getArgValue();
        }

        public String build()
        {
            return getArgValue();
        }

        public String suitepath()
        {
            return getArgValue();
        }

        public String browser()
        {
            return getArgValue();
        }

        public String username()
        {
            return getArgValue();
        }

        public String apikey()
        {
            return getArgValue();
        }

        public String project()
        {
            return getArgValue();
        }

    }
}
