using System;
using System.Collections.Generic;
using System.Text;

namespace InstantSpockExecutionRunner.Exceptions
{
    class SpockArgumentException : ArgumentException
    {
        public SpockArgumentException(String argumentName) : base(argumentName + " cannot be null. Pass in --"+argumentName+"=<value>")
        {

        }
    }
}
