using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowRunnerStutterHook
{

public class ServerInterface : MarshalByRefObject
{
    public void IsInstalled(int clientPID)
    {
        Console.WriteLine("SnowRunnerStutterStarter has injected SnowRunnerStutterHook into process {0}.\r\n", clientPID);
    }
    public void ReportMessages(string[] messages)
    {
        
        foreach (var t in messages)
        {
            Console.WriteLine(t);
        }
    }

    public void ReportMessage(string message)
    {
        Console.WriteLine(message);
    }

    public void Ping()
    {
    }
}
}
