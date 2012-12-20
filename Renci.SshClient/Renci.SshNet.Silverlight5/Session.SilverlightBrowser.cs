using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Renci.SshNet.Messages;

namespace Renci.SshNet
{
    public partial class Session
    {
        partial void HandleMessageCore(Message message)
        {
            this.HandleMessage((dynamic)message);
        }
    }
}
