using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using EasyHook;

namespace SnowRunnerStutterHook
{
    public class InjectionEntryPoint : IEntryPoint
    {
        readonly ServerInterface _server;
        readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();
        private bool _hooked;
        private WinAPI.WndProcDelegate _wndProcHook;
        private IntPtr _originalWndProc;
        public InjectionEntryPoint(
            RemoteHooking.IContext context,
            string channelName)
        {
            _server = RemoteHooking.IpcConnectClient<ServerInterface>(channelName);

            // If Ping fails then the Run method will be not be called
            _server.Ping();
        }

        public void Run(
            RemoteHooking.IContext context,
            string channelName)
        {
            Thread.CurrentThread.IsBackground = true;
            // Injection is now complete and the server interface is connected
            _server.IsInstalled(RemoteHooking.GetCurrentProcessId());

            // Install hooks
            try
            {
                _server.ReportMessage("Installing RegOpenKeyExW hook");

                var regQueryInfoKeyHook = LocalHook.Create(
                    LocalHook.GetProcAddress("advapi32.dll", "RegOpenKeyExW"),
                    new WinAPI.RegOpenKeyExDelegate(RegOpenKeyExW_Hook),
                    this);
                
                //prevent hook in this thread
                regQueryInfoKeyHook.ThreadACL.SetExclusiveACL(new[] { 0 });
                _server.ReportMessage("RegOpenKeyExW hook installed");

                // Wake up the process (required if using RemoteHooking.CreateAndInject)
                RemoteHooking.WakeUpProcess();

                try
                {
                    while (true)
                    {
                        System.Threading.Thread.Sleep(500);

                        while (_messageQueue.TryDequeue(out string result))
                        {
                            _server.ReportMessage(result);
                        }
                        _server.Ping();
                    }
                }
                catch
                {
                    // Ping() or ReportMessages() will raise an exception if host is unreachable
                }

                // Remove hooks
                _server.ReportMessage("Uninstalling RegOpenKeyExW hook");

                regQueryInfoKeyHook.Dispose();
                // Finalise cleanup of hooks
                LocalHook.Release();
            }
            catch (Exception e)
            {
                _server.ReportMessage(e.ToString());
            }
        }
        int RegOpenKeyExW_Hook(UIntPtr hKey, string subKey, int ulOptions, int samDesired, out UIntPtr hkResult)
        {
            if (subKey.StartsWith("VID_") && !_hooked)
            {
                //Game is accessing devices in the registry. It's probably safe now to hook WM_DEVICECHANGE
                //maybe this is not even needed
                HookWndProcForMainWindow();
                _hooked = true;
            }

            return WinAPI.RegOpenKeyEx(hKey, subKey, ulOptions, samDesired, out hkResult);
        }

        private void Log(string log)
        {
            _messageQueue.Enqueue(log);
        }
        private void HookWndProcForMainWindow()
        {
            Log("Hooking WndProc");
            var hwnd = Process.GetCurrentProcess().MainWindowHandle;
            _originalWndProc = WinAPI.GetWindowLong(hwnd, WinAPI.GWL_WNDPROC);
            Log(WinAPI.GetWindowLongPtr(hwnd, WinAPI.GWL_WNDPROC).ToInt64().ToString("X"));
            //save this explicitly in a variable to prevent garbage collection
            _wndProcHook = WndProcDetour;
            
            WinAPI.SetWindowLong(hwnd, WinAPI.GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(_wndProcHook));
            Log(WinAPI.GetWindowLongPtr(hwnd, WinAPI.GWL_WNDPROC).ToInt64().ToString("X"));
            Log("WndProc hook installed");
        }
        IntPtr WndProcDetour(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam) 
        {
            if (msg == WinAPI.WM_DEVICECHANGE)
            {
                Log("WM_DEVICECHANGE found and skipped");
                return IntPtr.Zero;
            }

            return WinAPI.CallWindowProc(_originalWndProc, hwnd, msg, wparam, lparam);
        }
    }
}
