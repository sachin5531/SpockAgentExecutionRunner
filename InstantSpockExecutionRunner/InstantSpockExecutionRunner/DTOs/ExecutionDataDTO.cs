using System;
using System.Collections.Generic;
using System.Text;

namespace InstantSpockExecutionRunner.DTOs
{
    [Serializable]
    public class ExecutionDataDTO
    {
        public string suitePath { get; set; }
        public string agent { get; set; }
        public string plugin { get; set; }
        public string build { get; set; }
        public string session { get; set; }
        public string snap_freq { get; set; }
        public string snap_quality { get; set; }
        public int stepTimeOut { get; set; }
        public string sendReportType { get; set; }
        public bool updateOnTMT { get; set; }
        public bool ApplySkipStepValidation { get; set; }
        public bool contWithBadOR { get; set; }
        public string SpockAgentBrowser { get; set; }
        public string token { get; set; }

    }
}
