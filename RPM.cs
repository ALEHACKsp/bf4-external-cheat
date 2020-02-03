using SharpDX;
using System;
using System.Text;

namespace BF4_Private_By_Tejisav
{
    class RPM
    {
        private static IntPtr pHandle = IntPtr.Zero;

        public static IntPtr OpenProcess(int pId)
        {
            pHandle = Managed.OpenProcess(Managed.PROCESS_VM_READ | Managed.PROCESS_VM_WRITE | Managed.PROCESS_VM_OPERATION, false, pId);
            return pHandle;
        }

        public static IntPtr GetHandle()
        {
            return pHandle;
        }

        public static void CloseProcess()
        {
            Managed.CloseHandle(pHandle);
        }

        public static long ReadInt64(long _lpBaseAddress)
        {
            byte[] Buffer = new byte[8];
            IntPtr ByteRead;
            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, Buffer, 8, out ByteRead);
            return BitConverter.ToInt64(Buffer, 0);
        }

        public static Int32 ReadInt32(long _lpBaseAddress)
        {
            byte[] Buffer = new byte[4];
            IntPtr ByteRead;
            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, Buffer, 4, out ByteRead);
            return BitConverter.ToInt32(Buffer, 0);
        }

        public static float ReadFloat(long _lpBaseAddress)
        {
            byte[] Buffer = new byte[sizeof(float)];
            IntPtr ByteRead;
            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, Buffer, sizeof(float), out ByteRead);
            return BitConverter.ToSingle(Buffer, 0);
        }

        public static bool WriteMemory(long MemoryAddress, byte[] Buffer)
        {
            uint oldProtect;
            Managed.VirtualProtectEx(pHandle, (IntPtr)MemoryAddress, (uint)Buffer.Length, Managed.PAGE_READWRITE, out oldProtect);
            IntPtr ptrBytesWritten;
            return Managed.WriteProcessMemory(pHandle, MemoryAddress, Buffer, (uint)Buffer.Length, out ptrBytesWritten);
        }
        public static bool WriteNop(long MemoryAddress, byte[] Buffer)
        {
            IntPtr ptrBytesWritten;
            return Managed.WriteProcessMemory(pHandle, MemoryAddress, Buffer, (uint)Buffer.Length, out ptrBytesWritten);
        }

        public static bool WriteFloat(long _lpBaseAddress, float _Value)
        {
            byte[] Buffer = BitConverter.GetBytes(_Value);
            return WriteMemory(_lpBaseAddress, Buffer);
        }

        public static bool WriteInt32(long _lpBaseAddress, int _Value)
        {
            byte[] Buffer = BitConverter.GetBytes(_Value);
            return WriteMemory(_lpBaseAddress, Buffer);
        }

        public static bool WriteByte(long _lpBaseAddress, byte _Value)
        {
            byte[] Buffer = BitConverter.GetBytes(_Value);
            return WriteMemory(_lpBaseAddress, Buffer);
        }

        public static bool WriteVector3(long _lpBaseAddress, Vector3 _Value)
        {
            byte[] Buff = new byte[sizeof(float) * 3];
            Buffer.BlockCopy(BitConverter.GetBytes(_Value.X), 0, Buff, 0 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(_Value.Y), 0, Buff, 1 * sizeof(float), sizeof(float));
            Buffer.BlockCopy(BitConverter.GetBytes(_Value.Z), 0, Buff, 2 * sizeof(float), sizeof(float));
            return WriteMemory(_lpBaseAddress, Buff);
        }

        public static byte ReadByte(long _lpBaseAddress)
        {
            byte[] Buffer = new byte[sizeof(byte)];
            IntPtr ByteRead;
            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, Buffer, sizeof(byte), out ByteRead);
            return Buffer[0];
        }

        public static void WriteAngle(float _Yaw, float _Pitch)
        {
            long pBase = ReadInt64(Offsets.OFFSET_VIEWANGLES);
            long m_authorativeAiming = ReadInt64(pBase + Offsets.ClientSoldierWeapon.m_authorativeAiming);
            long m_fpsAimer = ReadInt64(m_authorativeAiming + Offsets.ClientSoldierAimingSimulation.m_fpsAimer);
            
            WriteFloat(m_fpsAimer + Offsets.AimAssist.m_yaw, _Yaw);
            WriteFloat(m_fpsAimer + Offsets.AimAssist.m_pitch, _Pitch);
        }

        public static string ReadString(long _lpBaseAddress, UInt64 _Size)
        {
            byte[] buffer = new byte[_Size];
            IntPtr BytesRead;

            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, buffer, _Size, out BytesRead);

            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == 0)
                {
                    byte[] _buffer = new byte[i];
                    Buffer.BlockCopy(buffer, 0, _buffer, 0, i);
                    return Encoding.ASCII.GetString(_buffer);
                }
            }
            return Encoding.ASCII.GetString(buffer);
        }

        public static string ReadString2(long _lpBaseAddress, UInt64 _Size)
        {
            byte[] buffer = new byte[_Size];
            IntPtr BytesRead;

            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, buffer, _Size, out BytesRead);
            return Encoding.ASCII.GetString(buffer);
        }

        public static Vector2 ReadVector2(long _lpBaseAddress)
        {
            Vector2 tmp = new Vector2();

            byte[] Buffer = new byte[8];
            IntPtr ByteRead;

            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, Buffer, 8, out ByteRead);
            tmp.X = BitConverter.ToSingle(Buffer, (0 * 4));
            tmp.Y = BitConverter.ToSingle(Buffer, (1 * 4));
            return tmp;
        }

        public static Vector3 ReadVector3(long _lpBaseAddress)
        {
            Vector3 tmp = new Vector3();

            byte[] Buffer = new byte[12];
            IntPtr ByteRead;

            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, Buffer, 12, out ByteRead);
            tmp.X = BitConverter.ToSingle(Buffer, (0 * 4));
            tmp.Y = BitConverter.ToSingle(Buffer, (1 * 4));
            tmp.Z = BitConverter.ToSingle(Buffer, (2 * 4));
            return tmp;
        }

        public static AxisAlignedBox ReadAABB(long _lpBaseAddress)
        {
            AxisAlignedBox tmp = new AxisAlignedBox();
            byte[] Buffer = new byte[32];
            IntPtr ByteRead;

            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, Buffer, 32, out ByteRead);
            tmp.Min.X = BitConverter.ToSingle(Buffer, (0 * 4));
            tmp.Min.Y = BitConverter.ToSingle(Buffer, (1 * 4));
            tmp.Min.Z = BitConverter.ToSingle(Buffer, (2 * 4));
            tmp.Max.X = BitConverter.ToSingle(Buffer, (4 * 4));
            tmp.Max.Y = BitConverter.ToSingle(Buffer, (5 * 4));
            tmp.Max.Z = BitConverter.ToSingle(Buffer, (6 * 4));
            return tmp;
        }

        public static Vector4 ReadVector4(long _lpBaseAddress)
        {
            Vector4 tmp = new Vector4();

            byte[] Buffer = new byte[16];
            IntPtr ByteRead;

            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, Buffer, 16, out ByteRead);
            tmp.X = BitConverter.ToSingle(Buffer, (0 * 4));
            tmp.Y = BitConverter.ToSingle(Buffer, (1 * 4));
            tmp.Z = BitConverter.ToSingle(Buffer, (2 * 4));
            tmp.W = BitConverter.ToSingle(Buffer, (3 * 4));
            return tmp;
        }

        public static Matrix ReadMatrix(long _lpBaseAddress)
        {
            Matrix tmp = new Matrix();

            byte[] Buffer = new byte[64];
            IntPtr ByteRead;

            Managed.ReadProcessMemory(pHandle, _lpBaseAddress, Buffer, 64, out ByteRead);

            tmp.M11 = BitConverter.ToSingle(Buffer, (0 * 4));
            tmp.M12 = BitConverter.ToSingle(Buffer, (1 * 4));
            tmp.M13 = BitConverter.ToSingle(Buffer, (2 * 4));
            tmp.M14 = BitConverter.ToSingle(Buffer, (3 * 4));

            tmp.M21 = BitConverter.ToSingle(Buffer, (4 * 4));
            tmp.M22 = BitConverter.ToSingle(Buffer, (5 * 4));
            tmp.M23 = BitConverter.ToSingle(Buffer, (6 * 4));
            tmp.M24 = BitConverter.ToSingle(Buffer, (7 * 4));

            tmp.M31 = BitConverter.ToSingle(Buffer, (8 * 4));
            tmp.M32 = BitConverter.ToSingle(Buffer, (9 * 4));
            tmp.M33 = BitConverter.ToSingle(Buffer, (10 * 4));
            tmp.M34 = BitConverter.ToSingle(Buffer, (11 * 4));

            tmp.M41 = BitConverter.ToSingle(Buffer, (12 * 4));
            tmp.M42 = BitConverter.ToSingle(Buffer, (13 * 4));
            tmp.M43 = BitConverter.ToSingle(Buffer, (14 * 4));
            tmp.M44 = BitConverter.ToSingle(Buffer, (15 * 4));
            return tmp;
        }

        public static bool IsValid(long Address)
        {
            return (Address >= 0x10000 && Address < 0x000F000000000000);
        } 
    }
}
