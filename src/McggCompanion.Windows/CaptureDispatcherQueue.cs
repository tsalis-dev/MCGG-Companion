using System.Runtime.InteropServices;
using Windows.System;

namespace McggCompanion.Windows;

public static class CaptureDispatcherQueue
{
    public static DispatcherQueueController? CreateForCurrentThreadIfNeeded()
    {
        if (DispatcherQueue.GetForCurrentThread() is not null)
        {
            return null;
        }

        var options = new DispatcherQueueOptions
        {
            Size = (uint)Marshal.SizeOf<DispatcherQueueOptions>(),
            ThreadType = 2,
            // Current-thread queues keep the caller's existing COM apartment.
            // https://learn.microsoft.com/windows/win32/api/dispatcherqueue/ns-dispatcherqueue-dispatcherqueueoptions
            ApartmentType = 0
        };

        nint controllerPointer = 0;
        try
        {
            Marshal.ThrowExceptionForHR(CreateDispatcherQueueController(options, out controllerPointer));
            return WinRT.MarshalInterface<DispatcherQueueController>.FromAbi(controllerPointer)
                ?? throw new InvalidOperationException("Windows tidak mengembalikan antrean pesan capture.");
        }
        finally
        {
            if (controllerPointer != 0)
            {
                Marshal.Release(controllerPointer);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DispatcherQueueOptions
    {
        public uint Size;
        public int ThreadType;
        public int ApartmentType;
    }

    [DllImport("CoreMessaging.dll", ExactSpelling = true)]
    private static extern int CreateDispatcherQueueController(DispatcherQueueOptions options, out nint controller);
}
