using Kusto.Data.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kustox.Runtime
{
    public class ConnectionProvider
    {
        public ConnectionProvider(
            ICslQueryProvider queryProvider,
            ICslAdminProvider commandProvider)
        {
            QueryProvider = queryProvider;
            CommandProvider = commandProvider;
        }

        public ICslQueryProvider QueryProvider { get; }

        public ICslAdminProvider CommandProvider { get; }
    }
}