using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshNet
{
    internal interface IClientAuthentication
    {
        void Authenticate(IConnectionInfoInternal connectionInfo, ISession session);
    }
}
