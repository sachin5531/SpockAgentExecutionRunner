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


    }
}
