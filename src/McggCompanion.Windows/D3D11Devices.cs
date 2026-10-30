using System.Runtime.InteropServices;
using Windows.Graphics.DirectX.Direct3D11;

namespace McggCompanion.Windows;

internal static class D3D11Devices
{
    public static IDirect3DDevice Create()
    {
        nint devicePointer = 0;
        nint dxgiPointer = 0;
        nint winRtPointer = 0;

        try
        {
            const uint hardwareDriver = 1;
            const uint bgraSupport = 0x20;
            const uint sdkVersion = 7;
            Marshal.ThrowExceptionForHR(D3D11CreateDevice(
                0, hardwareDriver, 0, bgraSupport, 0, 0, sdkVersion,
                out devicePointer, out _, 0));

            var dxgiDeviceId = new Guid("54EC77FA-1377-44E6-8C32-88FD5F44C84C");
            Marshal.ThrowExceptionForHR(Marshal.QueryInterface(devicePointer, in dxgiDeviceId, out dxgiPointer));
            Marshal.ThrowExceptionForHR(CreateDirect3D11DeviceFromDXGIDevice(dxgiPointer, out winRtPointer));

            // C#/WinRT owns a separate reference; release the original ABI pointers below.
            // https://github.com/microsoft/CsWinRT/blob/master/docs/interop.md
            return WinRT.MarshalInterface<IDirect3DDevice>.FromAbi(winRtPointer)
                ?? throw new InvalidOperationException("Windows tidak mengembalikan perangkat grafis capture.");
        }
        finally
        {
            if (winRtPointer != 0)
            {
                Marshal.Release(winRtPointer);
            }

            if (dxgiPointer != 0)
            {
                Marshal.Release(dxgiPointer);
            }

            if (devicePointer != 0)
            {
                Marshal.Release(devicePointer);
            }
        }
    }

    [DllImport("d3d11.dll", ExactSpelling = true)]
    private static extern int D3D11CreateDevice(
        nint adapter,
        uint driverType,
        nint software,
        uint flags,
        nint featureLevels,
        uint featureLevelCount,
        uint sdkVersion,
        out nint device,
        out uint selectedFeatureLevel,
        nint immediateContext);

    [DllImport("d3d11.dll", ExactSpelling = true)]
    private static extern int CreateDirect3D11DeviceFromDXGIDevice(nint dxgiDevice, out nint graphicsDevice);
}
