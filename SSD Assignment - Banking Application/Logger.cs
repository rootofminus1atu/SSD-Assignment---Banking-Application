using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace Banking_Application
{
    internal class Logger
    {
        private const string SOURCE = "SSD Banking Application";

        static Logger()
        {
            //if (!EventLog.SourceExists(SOURCE))
            //{
            //    EventLog.CreateEventSource(SOURCE, "Application");
            //}
            try
            {
                if (!EventLog.SourceExists(SOURCE))
                {
                    EventLog.CreateEventSource(SOURCE, "Application");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EventLog unavailable: " + ex.Message);
            }
        }

        public static void Log(
            string tellerName,
            string accountNo,
            string accountHolder,
            string transactionType,
            string reason = "N/A"
        )
        {
            string macAddress = GetMacAddress();
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var asm = Assembly.GetExecutingAssembly();
            string appMeta = $"{asm.GetName().Name}, Version {asm.GetName().Version}, Hash {asm.ManifestModule.ModuleVersionId}";

            string message = $@"
WHO (Teller): {tellerName}
WHO (Account): {accountNo} - {accountHolder}
WHAT: {transactionType}
WHERE (MAC): {macAddress}
WHEN: {timestamp}
WHY: {reason}
HOW: {appMeta}
";

            Console.WriteLine(message);
            return;

            EventLog.WriteEntry(SOURCE, message, EventLogEntryType.Information);
        }

        private static string GetMacAddress()
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                    return nic.GetPhysicalAddress().ToString();
            }
            return "Unknown";
        }
    }
}
