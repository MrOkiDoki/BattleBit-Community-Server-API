#region

using System.Net;
using System.Net.Sockets;

#endregion

namespace CommunityServerAPI.BattleBitAPI.Common.Extentions;

public static class Extensions
{
	public static long TickCount
	{
		get
		{
#if NETCOREAPP
			return Environment.TickCount64;
#else
                return (long)Environment.TickCount;
#endif
		}
	}

	public static unsafe uint ToUInt(this IPAddress address)
	{
#if NETCOREAPP
		return BitConverter.ToUInt32(address.GetAddressBytes());
#else
        return BitConverter.ToUInt32(address.GetAddressBytes(), 0);
#endif
	}

	public static void SafeClose(this TcpClient client)
	{
		try
		{
			client.Close();
		}
		catch
		{
		}
		try
		{
			client.Dispose();
		}
		catch
		{
		}
	}
}
