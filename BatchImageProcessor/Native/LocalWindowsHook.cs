// Copyright (c) 2017 Sidneys1
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace BatchImageProcessor.Native
{
    public class LocalWindowsHook
    {
        // ************************************************************************
        // Filter function delegate
        public delegate void HookEventHandler(object sender, HookEventArgs e);

        public delegate int HookProc(int code, IntPtr wParam, IntPtr lParam);

        // ************************************************************************

        // ************************************************************************
        // Internal properties
        protected HookProc MFilterFunc;
        protected IntPtr MHhook = IntPtr.Zero;
        protected HookType MHookType;
        // ************************************************************************

        // ************************************************************************
        // Event delegate

        // ************************************************************************

        // ************************************************************************
        // Class constructor(s)
        public LocalWindowsHook(HookType hook)
        {
            MHookType = hook;
            MFilterFunc = CoreHookProc;
        }

        public LocalWindowsHook(HookType hook, HookProc func)
        {
            MHookType = hook;
            MFilterFunc = func;
        }

        public event HookEventHandler HookInvoked;

        protected void OnHookInvoked(HookEventArgs e)
        {
            HookInvoked?.Invoke(this, e);
        }

        // ************************************************************************

        // ************************************************************************
        // Default filter function
        protected int CoreHookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0)
                return CallNextHookEx(MHhook, code, wParam, lParam);

            // Let clients determine what to do
            var e = new HookEventArgs {HookCode = code, WParam = wParam, LParam = lParam};
            OnHookInvoked(e);

            // Yield to the next hook in the chain
            return CallNextHookEx(MHhook, code, wParam, lParam);
        }

        // ************************************************************************

        // ************************************************************************
        // Install the hook
        public void Install()
        {
            MHhook = SetWindowsHookEx(
                MHookType,
                MFilterFunc,
                IntPtr.Zero,
                //AppDomain.GetCurrentThreadId(), 
                Thread.CurrentThread.ManagedThreadId);
        }

        // ************************************************************************

        // ************************************************************************
        // Uninstall the hook
        public void Uninstall()
        {
            UnhookWindowsHookEx(MHhook);
        }

        #region Win32 Imports

        // ************************************************************************
        // Win32: SetWindowsHookEx()
        [DllImport("user32.dll")]
        protected static extern IntPtr SetWindowsHookEx(HookType code,
            HookProc func,
            IntPtr hInstance,
            int threadId);

        // ************************************************************************

        // ************************************************************************
        // Win32: UnhookWindowsHookEx()
        [DllImport("user32.dll")]
        protected static extern int UnhookWindowsHookEx(IntPtr hhook);

        // ************************************************************************

        // ************************************************************************
        // Win32: CallNextHookEx()
        [DllImport("user32.dll")]
        protected static extern int CallNextHookEx(IntPtr hhook,
            int code, IntPtr wParam, IntPtr lParam);

        // ************************************************************************

        #endregion
    }
}