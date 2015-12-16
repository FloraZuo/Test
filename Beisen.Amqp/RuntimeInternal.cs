using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace Beisen.Amqp
{
    internal static class RuntimeInternal
    {
        public static readonly ILog Logger = Beisen.Logging.LogWrapper.GetLogger("Amqp");
    }
}
