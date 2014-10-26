using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for local port forwarding
    /// </summary>
    public partial class ForwardedPortLocal
    {
        private TcpListener _listener;
        private readonly object _listenerLocker = new object();
        private int _pendingRequests;

        partial void InternalStart()
        {
            //  If port already started don't start it again
            if (this.IsStarted)
                return;

            IPAddress addr = this.BoundHost.GetIPAddress();
            var ep = new IPEndPoint(addr, (int)this.BoundPort);

            this._listener = new TcpListener(ep);
            this._listener.Start();
            //  Update bound port if original was passed as zero
            this.BoundPort = (uint)((IPEndPoint)_listener.LocalEndpoint).Port;

            this.Session.ErrorOccured += Session_ErrorOccured;
            this.Session.Disconnected += Session_Disconnected;

            this._listenerTaskCompleted = new ManualResetEvent(false);
            this.ExecuteThread(() =>
            {
                try
                {
                    while (true)
                    {
                        lock (this._listenerLocker)
                        {
                            if (this._listener == null)
                                break;
                        }

                        var socket = this._listener.AcceptSocket();

                        this.ExecuteThread(() =>
                        {
                            try
                            {
                                Interlocked.Increment(ref _pendingRequests);

                                var originatorEndPoint = (IPEndPoint) socket.RemoteEndPoint;

                                this.RaiseRequestReceived(originatorEndPoint.Address.ToString(),
                                    (uint) originatorEndPoint.Port);

                                using (var channel = this.Session.CreateChannelDirectTcpip())
                                {
                                    channel.Open(this.Host, this.Port, this, socket);
                                    channel.Bind();
                                    channel.Close();
                                }
                            }
                            catch (Exception exp)
                            {
                                this.RaiseExceptionEvent(exp);
                            }
                            finally
                            {
                                Interlocked.Decrement(ref _pendingRequests);
                            }
                        });
                    }
                }
                catch (SocketException exp)
                {
                    if (exp.SocketErrorCode != SocketError.Interrupted)
                    {
                        this.RaiseExceptionEvent(exp);
                    }
                }
                catch (Exception exp)
                {
                    this.RaiseExceptionEvent(exp);
                }
                finally
                {
                    this._listenerTaskCompleted.Set();
                }
            });

            this.IsStarted = true;
        }

        partial void InternalStop()
        {
            //  If port not started you cant stop it
            if (!this.IsStarted)
                return;

            this.Session.Disconnected -= Session_Disconnected;
            this.Session.ErrorOccured -= Session_ErrorOccured;

            this.StopListener();

            this._listenerTaskCompleted.WaitOne(this.Session.ConnectionInfo.Timeout);
            this._listenerTaskCompleted.Dispose();
            this._listenerTaskCompleted = null;

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            while (stopWatch.Elapsed < this.Session.ConnectionInfo.Timeout || this.Session.ConnectionInfo.Timeout == SshNet.Session.Infinite)
            {
                if (Interlocked.CompareExchange(ref _pendingRequests, 0, 0) == 0)
                    break;
                Thread.Sleep(50);
            }

            stopWatch.Stop();

            this.IsStarted = false;
        }

        private void StopListener()
        {
            lock (this._listenerLocker)
            {
                if (this._listener != null)
                {
                    this._listener.Stop();
                    this._listener = null;
                }
            }
        }

        private void Session_ErrorOccured(object sender, Common.ExceptionEventArgs e)
        {
            this.StopListener();
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            this.StopListener();
        }
    }
}
