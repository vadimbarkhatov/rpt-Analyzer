using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public sealed class Logs
{
    private static Logs instance = null;
    private static readonly object padlock = new object();
    public readonly log4net.ILog log;

    Logs()
    {
        log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }

    public static Logs Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new Logs();
                }
                return instance;
            }
        }
    }
}

