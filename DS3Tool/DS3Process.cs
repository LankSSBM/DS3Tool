using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using MiscUtils;
using System.Xml.Linq;
using System.Net.Sockets;

namespace DS3Tool
{
    public class DS3Process : IDisposable
    {
        public const uint PROCESS_ALL_ACCESS = 2035711;
        private Process _targetProcess = null;
        public IntPtr _targetProcessHandle = IntPtr.Zero;
        public IntPtr ds3Base = IntPtr.Zero;
        public const int CodeCavePtrLoc = 0x1914670;

        protected bool disposed = false;

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAcess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int iSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int iSize, int lpNumberOfBytesWritten);

        [DllImport("ntdll.dll")]
        static extern int NtWriteVirtualMemory(IntPtr ProcessHandle, IntPtr BaseAddress, byte[] Buffer, UInt32 NumberOfBytesToWrite, ref UInt32 NumberOfBytesWritten); //TODO: replace all read/write process memory with this and the equivalent read func.

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll")]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        public uint RunThread(IntPtr address, uint timeout = 0xFFFFFFFF)
        {
            IntPtr thread = CreateRemoteThread(_targetProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, IntPtr.Zero);
            var ret = WaitForSingleObject(thread, timeout);
            CloseHandle(thread); //return value unimportant
            return ret;
        }

        Thread freezeThread = null;
        bool _running = true;
        public DS3Process()
        {
            findAttach();
            findBaseAddress();

            freezeThread = new Thread(() => { freezeFunc(); });
            freezeThread.Start();

        }

        public void Dispose()
        {
            if (!disposed)
            {
                _running = false;
                if (freezeThread != null)
                {
                    freezeThread.Abort();
                    freezeThread = null;
                }
                detach();
                disposed = true;
            }
        }

        ~DS3Process()
        {
            Dispose();
        }

        const string ds3ProName = "darksoulsiii";

        private void findAttach()
        {
            var processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                if (string.Equals(process.ProcessName.ToLowerInvariant(), ds3ProName.ToLowerInvariant(), StringComparison.InvariantCulture) && !process.HasExited)
                {
                    attach(process);
                    return;
                }
            }
            throw new Exception("DS3 not running");
        }

        private void attach(Process proc)
        {
            if (_targetProcessHandle == IntPtr.Zero)
            {
                _targetProcess = proc;
                _targetProcessHandle = OpenProcess(PROCESS_ALL_ACCESS, bInheritHandle: false, _targetProcess.Id);
                if (_targetProcessHandle == IntPtr.Zero)
                {
                    throw new Exception("Attach failed");
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Already attached");
            }
        }

        private void findBaseAddress()
        {//kinda pointless since the base address is always the same (0x140000000), however this isn't true in other games. (this is due to ASLR)
            try
            {
                foreach (var module in _targetProcess.Modules)
                {
                    var processModule = module as ProcessModule;
                    //Utils.debugWrite(processModule.ModuleName);
                    switch (processModule.ModuleName.ToLower())
                    {
                        case ds3ProName + ".exe":
                            ds3Base = processModule.BaseAddress;
                            break;
                    }
                }
            }
            catch (Exception ex) { Utils.debugWrite(ex.ToString()); }
            if (ds3Base == IntPtr.Zero)
            {
                throw new Exception("Couldn't find DS3 base address");
            }
        }

        private void detach()
        {
            if (!(_targetProcessHandle == IntPtr.Zero))
            {
                _targetProcess = null;
                try
                {
                    CloseHandle(_targetProcessHandle);
                    _targetProcessHandle = IntPtr.Zero;
                }
                catch (Exception ex)
                {
                    Utils.debugWrite(ex.ToString());
                }
            }
        }

        //all read/write funcs just fail silently, except this one:
        public bool ReadTest(IntPtr addr)
        {
            var array = new byte[1];
            var lpNumberOfBytesRead = 1;
            return ReadProcessMemory(_targetProcessHandle, addr, array, 1, ref lpNumberOfBytesRead) && lpNumberOfBytesRead == 1;
        }

        public int ReadInt32(IntPtr addr)
        {
            var bytes = ReadBytes(addr, 4);
            return BitConverter.ToInt32(bytes, 0);
        }

        public long ReadInt64(IntPtr addr)
        {
            var bytes = ReadBytes(addr, 8);
            return BitConverter.ToInt64(bytes, 0);
        }

        public byte ReadUInt8(IntPtr addr)
        {
            var bytes = ReadBytes(addr, 1);
            return bytes[0];
        }

        public uint ReadUInt32(IntPtr addr)
        {
            var bytes = ReadBytes(addr, 4);
            return BitConverter.ToUInt32(bytes, 0);
        }

        public ulong ReadUInt64(IntPtr addr)
        {
            var bytes = ReadBytes(addr, 8);
            return BitConverter.ToUInt64(bytes, 0);
        }

        public float ReadFloat(IntPtr addr)
        {
            var bytes = ReadBytes(addr, 4);
            return BitConverter.ToSingle(bytes, 0);
        }

        public double ReadDouble(IntPtr addr)
        {
            var bytes = ReadBytes(addr, 8);
            return BitConverter.ToDouble(bytes, 0);
        }

        public byte[] ReadBytes(IntPtr addr, int size)
        {
            var array = new byte[size];
            var targetProcessHandle = _targetProcessHandle;
            var lpNumberOfBytesRead = 1;
            ReadProcessMemory(targetProcessHandle, addr, array, size, ref lpNumberOfBytesRead);
            return array;
        }

        public string ReadString(IntPtr addr, int maxLength = 32)
        {
            var bytes = ReadBytes(addr, maxLength * 2);

            int stringLength = 0;
            for (int i = 0; i < bytes.Length - 1; i += 2)
            {
                if (bytes[i] == 0 && bytes[i + 1] == 0)
                {
                    stringLength = i;
                    break;
                }
            }

            if (stringLength == 0)
            {
                stringLength = bytes.Length - (bytes.Length % 2);
            }

            return System.Text.Encoding.Unicode.GetString(bytes, 0, stringLength);
        }

        public void WriteInt32(IntPtr addr, int val)
        {
            WriteBytes(addr, BitConverter.GetBytes(val));
        }

        public void WriteUInt32(IntPtr addr, uint val)
        {
            WriteBytes(addr, BitConverter.GetBytes(val));
        }

        public void WriteFloat(IntPtr addr, float val)
        {
            WriteBytes(addr, BitConverter.GetBytes(val));
        }

        public void WriteUInt8(IntPtr addr, byte val)
        {
            var bytes = new byte[] { val };
            WriteBytes(addr, bytes);
        }

        public void WriteBytes(IntPtr addr, byte[] val, bool useNewWrite = true)
        {
            if (useNewWrite)
            {
                uint written = 0;
                NtWriteVirtualMemory(_targetProcessHandle, addr, val, (uint)val.Length, ref written); //MUCH faster, <1ms
            }
            else
            {
                WriteProcessMemory(_targetProcessHandle, addr, val, val.Length, 0); //can take as long as 15ms!
            }
        }

        public void WriteString(IntPtr addr, string value, int maxLength = 32)
        {
            var bytes = new byte[maxLength];
            var stringBytes = System.Text.Encoding.Unicode.GetBytes(value);
            Array.Copy(stringBytes, bytes, Math.Min(stringBytes.Length, maxLength));
            WriteBytes(addr, bytes);
        }

        public enum DebugOpts
        {
            COL_MESH_MAIN, COL_MESH_VISUAL, COL_MESH_HIGH_PERF, COL_MESH_COLOURS,
            CHARACTER_MESH,
            ALL_CHRS_DBG_DRAW_FLAG,
            HITBOX_VIEW, IMPACT_VIEW,
            DISABLE_MAP, DISABLE_CHARACTER,
            NO_DEATH, ALL_CHR_NO_DEATH,
            INSTANT_QUITOUT,
            ONE_HP, MAX_HP,
            TARGET_HP,
            DISABLE_AI, NO_STAM, NO_FP, NO_ARROW_CONSUM,
            NO_GOODS_CONSUM,
            DISABLE_STEAM_INPUT_ENUM,
            EVENT_STOP, EVENT_DRAW,
            HIDDEN_DEBUG_MENU,
            ALL_DEBUG_DRAWING, //this is what they call it. it's a 'master switch' for some debug draws.
            ENEMY_TARGETING_A, ENEMY_TARGETING_B, SOUND_VIEW,
            FREE_CAMERA,
            NO_GRAVITY,
            ONE_SHOT,
        }

        public enum TargetInfo
        {
            HP, MAX_HP,
            POISE, MAX_POISE, POISE_TIMER,
            POISON, POISON_MAX, //resists must match memory order
            TOXIC, TOXIC_MAX,
            BLEED, BLEED_MAX,
            CURSE, CURSE_MAX,
            FROST, FROST_MAX,
        }

        public enum PlayerStats
        {
            VIGOR,
            ATTUNEMENT,
            ENDURANCE,
            VITALITY,
            STRENGTH,
            DEXTERITY,
            INTELLIGENCE,
            FAITH,
            LUCK,
            SOULS
        }

        private readonly Dictionary<PlayerStats, int> statOffsets = new Dictionary<PlayerStats, int>
    {
        { PlayerStats.VIGOR, 0x44 },
        { PlayerStats.ATTUNEMENT, 0x48 },
        { PlayerStats.ENDURANCE, 0x4C },
        { PlayerStats.VITALITY, 0x6C },
        { PlayerStats.STRENGTH, 0x50 },
        { PlayerStats.DEXTERITY, 0x54 },
        { PlayerStats.INTELLIGENCE, 0x58 },
        { PlayerStats.FAITH, 0x5C },
        { PlayerStats.LUCK, 0x60 },
        {PlayerStats.SOULS, 0x74 }
    };


        //1.15 stuff by shilkey
        const int worldChrManOff = 0x4768E78;
        const int hitboxOff = 0x4766B80;
        const int gameDataManOff = 0x4740178;
        const int menuManOff = 0x474c2e8;
        const int debug_flagsOff = 0x4768f68;
        const int meshesOff = 0x4743A98;
        const int enemyTargetDrawAOff = 0x41E6CA;
        const int GameFlagDataOff = 0x473BE28;
        const int globalSpeedOff = 0x999C28;
        const int targetHookLoc = 0x85A74A;
        const int codeCavePtrLoc = 0x1914670;
        const int codeCaveCodeLoc = codeCavePtrLoc + 0x10;
        const int enemyRepeatActionOff = 0x3E2510 + 4 + 3;
        const int fieldAreaOff = 0x4743A80;
        const int SprjDebugEvent = 0x473AD78; //BaseF
        const int newMenuSystemOff = 0x4776B08;



        //offsets of main pointers/statics.
        //see aob scanner for aobs.
        // const int gameDataManOff = 0x47572B8; //NS_SPRJ::GameDataMan
        //const int worldChrManOff = 0x477FDB8; //NS_SPRJ::WorldChrManImp

        const int gameManOff = 0x475AC00; //NS_SPRJ::GameMan
        //const int fieldAreaOff = 0x475ABD0; //NS_SPRJ::FieldArea
        const int BaseEOff = 0x4756E48; //NS_SPRJ::FrpgNetManImp
        const int BaseFOff = 0x4751EB8; //no name, seems lua related? //SprjDebugEvent ?
        const int worldChrManDbgOff = 0x477FED8; //NS_SPRJ::WorldChrManDbgImp. all debug drawing is under here. presumably others like 'all no death' and such
        const int ParamOff = 0x4798118; //NS_SPRJ::SoloParamRepositoryImp
        //const int GameFlagDataOff = 0x4752F68; //no name //SprjEventFlagMan ?
        const int LockBonus_ptrOff = 0x477DBE0; //NS_SPRJ::LockTgtManImp
        //const int DrawNearOnly_ptrOff = 0x4766555; //not a pointer - static debug flag? (no refs to this addr) //not updated for 1.15.1
        //const int debug_flagsOff = 0x477FEA8; //also static? "all" debug flags, not specific to any character.
        const int GROUP_MASKOff = 0x456CBA8; //also static
                                             //const int menuManOff = 0x4763258; //NS_SPRJ::MenuMan
                                             //const int hitboxOff = 0x477DAC0; //no name. damage management?

        //const int newMenuSystemOff = 0x478DA50; //AppMenu::NewMenuSystem
        const int worldAIManOff = 0x4751550; //NS_SPRJ::SprjWorldAiManagerImp

        //targeting is static, but maybe ?$DLRuntimeClassImpl@VSprjTargetingSystem@NS_SPRJ@@$0A@@DLRF@@ + 54
        //const int enemyTargetDrawAOff = 0x4750C04; //in 1.15, this is accessed at +41E6CA, which sadly is obfuscated in the exe. in 1.15.1, +41e74a (barely moved)
        const int enemyTargetDrawBOff = 0x4750C05;

        //const int meshesOff = 0x477DBAC; //no name. lots of static stuff around here.
        //maybe ?$DLRuntimeClassImpl @VSprjDrawStep@NS_SPRJ@@$0A@@DLRF@@ + 18C

        //offsets from a main pointer
        const int playerInsModulesOff = 0x1F90;
        const int XB = 0x1FA0;
        const int XC = 0x950;

        const int worldChrManPlayerInsOff = 0x80; //NS_SPRJ::PlayerIns?
        //modules
        const int chrDataModuleOff = 0x18; //NS_SPRJ::SprjChrDataModule
        const int chrResistModuleOff = 0x20;
        const int chrSuperArmorModuleOff = 0x40;
        const int chrPhysModuleOff = 0x68;

        //others
        const int playerDebugFlagsOff = 0x1EEA; //expect this to change if any patches occur

        //const int globalSpeedOff = 0x9A3D48;
        public float getSetGameSpeed(float? val = null)
        {
            var ptr = ds3Base + globalSpeedOff;
            var ret = ReadFloat(ptr);
            if (val.HasValue)
            {
                WriteFloat(ptr, val.Value);
            }
            return ret;
        }

        //DbgGetForceActIdx. patch changes it to use the addr from DbgSetLastActIdx
        //const int enemyRepeatActionOff = 0x3E2590 + 4 + 3;
        /*  00000001403E2510 | 48:8B41 08               | mov rax, qword ptr ds:[rcx+8]                         |
            00000001403E2514 | 0FBE80 81B60000          | movsx eax,byte ptr ds:[rax+B681]                      |
            00000001403E251B | C3                       | ret                                                   |*/
        //note: taken from live process. code is obfuscated when analysing offline. //TODO: investigate this? unpacking with steamless doesn't help. just have to do live aob scans with ds3 i guess.
        const byte enemyRepeatActionPatchVal = 0x82;
        const byte enemyRepeatActionOrigVal = 0x81;

        //just search for string refs
        /*
        00000001426427D0 | 48:83EC 38                      | sub rsp,38                                                             |
        00000001426427D4 | 48:C74424 20 FEFFFFFF           | mov qword ptr ss:[rsp+20],FFFFFFFFFFFFFFFE                             |
        00000001426427DD | 48:8D05 9C3A1000                | lea rax, qword ptr ds:[142746280]                                       |
        00000001426427E4 | 48:8905 8539EE01                | mov qword ptr ds:[144526170],rax                                       |
        00000001426427EB | 48:8D05 2EFDD9FD                | lea rax, qword ptr ds:[<DbgSetLastActIdx>]                              | <---- here
        00000001426427F2 | 48:8905 7F39EE01                | mov qword ptr ds:[144526178],rax                                       |
        00000001426427F9 | 48:8B05 B06F0F02                | mov rax, qword ptr ds:[1447397B0]                                       |
        0000000142642800 | 48:85C0                         | test rax, rax                                                           |
        0000000142642803 | 75 05                           | jne<darksoulsiii_1.15.sub_14264280A>                                  |
        0000000142642805 | E8 8644DBFD                     | call<darksoulsiii_1.15.sub_1403F6C90>                                 |
        000000014264280A | 4C:8B10                         | mov r10, qword ptr ds:[rax]                                             |
        000000014264280D | 4C:8D0D 346A1000                | lea r9, qword ptr ds:[142749248]                                        | 0000000142749248:L"DbgSetLastActIdx"
        0000000142642814 | 4C:8D05 556A1000                | lea r8, qword ptr ds:[142749270]                                        | 0000000142749270:"DbgSetLastActIdx"
        000000014264281B | 48:8D15 4E39EE01                | lea rdx, qword ptr ds:[144526170]                                       |
        0000000142642822 | 48:8BC8                         | mov rcx, rax                                                            |
        0000000142642825 | 41:FF52 58                      | call qword ptr ds:[r10+58]                                             |
        0000000142642829 | 90                              | nop                                                                    |
        000000014264282A | 48:8D0D CF0D0B00                | lea rcx, qword ptr ds:[<sub_1426F3600>]                                 |
        0000000142642831 | 48:83C4 38                      | add rsp,38                                                             |
        0000000142642835 | E9 222198FF                     | jmp darksoulsiii_1.15.141FC495C                                        |
        */

        const int usrInputMgrImplOff = 0x49644C8; //virtually static pointing to 14494e9f0. no AOB but the class instance is DLUID::DLUserInputManagerImpl<DLKR::DLMultiThreadingPolicy>.
        const int usrInputMgrImpSteamInputFlagOff = 0x24b;

        const int fontDrawFirstPatchLoc = 0x237C736;

        //const int freeCamPlayerControlPatchLoc = 0x62C408; //address with debug menu ???
        const int freeCamPlayerControlPatchLoc = 0x62E821; //address without? why did it change? do we need to AOB scan in this region?
        readonly byte[] freeCamPlayerControlPatchOrig = new byte[] { 0x8B, 0x83, 0xE0, 0, 0, 0 };
        readonly byte[] freeCamPlayerControlPatchReplacement = new byte[] { 0x31, 0xC0, 0x90, 0x90, 0x90, 0x90 };

        const int noLogoPatchLoc = 0xBF42BF;
        readonly byte[] noLogoPatchCode = new byte[] { 0x48, 0x31, 0xC0, 0x48, 0x89, 0x02, 0x49, 0x89, 0x04, 0x24, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 };


        //const int codeCavePtrLoc = 0x271AC00; //end-of-file zero block
        // const int targetHookLoc = 0x862CBA;

        //const int codeCaveCodeLoc = codeCavePtrLoc + 0x10;

        static readonly byte[] targetHookOrigCode = new byte[] { 0x48, 0x8B, 0x80, 0x90, 0x1F, 0x00, 0x00, };
        /*
000000014085A74A | 48:8B80 901F0000                | mov rax,qword ptr ds:[rax+1F90]                       |
000000014085A751 | 48:8B08                         | mov rcx,qword ptr ds:[rax]                            |
000000014085A754 | 48:8B51 58                      | mov rdx,qword ptr ds:[rcx+58]                         |
*/
        static readonly byte[] targetHookReplacementCodeTemplate = new byte[] { 0xE9,
            0, 0, 0, 0, //address offset
            0x90, 0x90, };
        //replacement code contains the offset from the following instruction (basically hook loc + 5) to the code cave.
        //then it just nops to fill out the rest of the old instructions
        static byte[] getTargetHookReplacementCode()
        {
            var ret = new byte[targetHookReplacementCodeTemplate.Length];
            int addrOffset = codeCaveCodeLoc - (targetHookLoc + 5); //target minus next instruction location (ie. the NOP 5 bytes in)
            Array.Copy(targetHookReplacementCodeTemplate, ret, ret.Length);
            Array.Copy(BitConverter.GetBytes(addrOffset), 0, ret, 1, 4);
            return ret;
        }

        static readonly byte[] targetHookCaveCodeTemplate = new byte[] { 0x48, 0xA3,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //full 64 bit ptr address goes here
        0x48, 0x8B, 0x80, 0x90, 0x1F, 0x00, 0x00, //should be identical to orig code from here to just before E9
        0xE9,
        0, 0, 0, 0, //address offset
         };

        static byte[] getTargetHookCaveCodeTemplate()
        {
            var ret = new byte[targetHookCaveCodeTemplate.Length];
            int addrOffset = targetHookLoc + targetHookReplacementCodeTemplate.Length - (codeCaveCodeLoc + ret.Length); //again, target (after the hook) minus next instruction location (the NOPs after the end of our injection)
            Array.Copy(targetHookCaveCodeTemplate, ret, ret.Length);
            Array.Copy(BitConverter.GetBytes(addrOffset), 0, ret, ret.Length - 4, 4);
            return ret;
        }

        public bool installTargetHook()


        {

            //generate code first
            var targetHookReplacementCode = getTargetHookReplacementCode();
            var targetHookCaveCode = getTargetHookCaveCodeTemplate(); //still needs to have ptr addr added in

            var code = ReadBytes(ds3Base + targetHookLoc, targetHookOrigCode.Length);
            if (code.SequenceEqual(targetHookReplacementCode))
            {
                Console.WriteLine("Already hooked");
                return true;
            }
            if (!code.SequenceEqual(targetHookOrigCode))
            {
                Console.WriteLine("Unexpected code at hook location");
                return false;
            }

            var caveCheck1 = ReadUInt64(ds3Base + codeCavePtrLoc);
            var caveCheck2 = ReadUInt64(ds3Base + codeCaveCodeLoc);
            if (caveCheck1 != 0 || caveCheck2 != 0)
            {
                Console.WriteLine("Code cave not empty");
                return false;
            }

            //set up cave first
            var targetHookFullAddr = ds3Base + codeCavePtrLoc;
            var caveCode = new byte[targetHookCaveCode.Length];
            Array.Copy(targetHookCaveCode, caveCode, targetHookCaveCode.Length);
            var fullAddrBytes = BitConverter.GetBytes((Int64)targetHookFullAddr);
            Array.Copy(fullAddrBytes, 0, caveCode, 2, 8);
            //patch cave
            WriteBytes(ds3Base + codeCaveCodeLoc, caveCode);
            //patch hook loc
            WriteBytes(ds3Base + targetHookLoc, targetHookReplacementCode);
            return true;
        }


        public void setEnemyRepeatActionPatch(bool on)
        {
            var b = ReadUInt8(ds3Base + enemyRepeatActionOff);
            if (on && b == enemyRepeatActionOrigVal)
            {
                WriteUInt8(ds3Base + enemyRepeatActionOff, enemyRepeatActionPatchVal);
            }
            else if (!on && b == enemyRepeatActionPatchVal)
            {
                WriteUInt8(ds3Base + enemyRepeatActionOff, enemyRepeatActionOrigVal);
            }
            else
            {
                Utils.debugWrite("Unexpected value trying to apply enemy repeat action patch");
            }
        }

        public bool patchLogos()
        {
            var code = ReadBytes(ds3Base + noLogoPatchLoc, noLogoPatchCode.Length);
            if (code.SequenceEqual(noLogoPatchCode))
            {
                Utils.debugWrite("Already patched");
                return true;
            }
            else if (code[0] == 0xE8) //just check first byte
            {//original code
                WriteBytes(ds3Base + noLogoPatchLoc, noLogoPatchCode);
                Utils.debugWrite("Patched");
                return true;
            }
            else
            {
                Utils.debugWrite("Unexpected data for logo patch, unknown version?");
                return false;
            }
        }

        const long SANE_MINIMUM = 0x140000000; //base addr
        const long SANE_MAXIMUM = 0x800000000000; //TODO: refine

        ulong getPlayerInsPtr()
        {
            var ptr1 = ReadUInt64(ds3Base + worldChrManOff);
            var ptr2 = ReadUInt64((IntPtr)(ptr1 + worldChrManPlayerInsOff));
            return ptr2;
        }

        ulong getCharPtrModules()
        {
            var ptr2 = getPlayerInsPtr();
            var ptr3 = ReadUInt64((IntPtr)(ptr2 + playerInsModulesOff));
            return ptr3;
        }

        public (float x, float y, float z, float dir) getSetPlayerLocalCoords((float, float, float, float)? pos = null)
        {
            var ptr4 = getCharPtrModules();
            var ptr5 = ReadUInt64((IntPtr)(ptr4 + chrPhysModuleOff));
            var ptrX = (IntPtr)(ptr5 + 0x80); //another copy at +170
            var ptrY = (IntPtr)(ptr5 + 0x84);
            var ptrZ = (IntPtr)(ptr5 + 0x88);
            var ptrDir = (IntPtr)(ptr5 + 74); //chr facing direction

            float x = ReadFloat(ptrX);
            float y = ReadFloat(ptrY);
            float z = ReadFloat(ptrZ);
            float dir = ReadFloat(ptrDir);

            if (pos != null)
            {
                WriteFloat(ptrX, pos.Value.Item1);
                WriteFloat(ptrY, pos.Value.Item2);
                WriteFloat(ptrZ, pos.Value.Item3);
                if (!float.IsNaN(pos.Value.Item4)) { WriteFloat(ptrDir, pos.Value.Item4); } //direction is optional; set NaN if not caring //doesn't seem to work?
            }

            return (x, y, z, dir);
        }
        //playerins also has some coords at +E0 onwards but they don't update...seems to be spawn coords?
        //also at +1050 but these are live coords. can't easily find map ID. should try again and double check the map i'm looking for.

        public IntPtr getFreeCamPtr()
        {//pointer to CSDebugCam
            var ptr1 = ReadUInt64(ds3Base + fieldAreaOff);
            var ptr2 = ReadUInt64((IntPtr)(ptr1 + 0x18)); //GameRend
            var ptr3 = ReadUInt64((IntPtr)(ptr2 + 0xE8)); //SprjDebugCam
            return (IntPtr)ptr3;
        }
        //free cam 'look at matrix' is at +10

        public (float x, float y, float z) getSetFreeCamCoords((float, float, float)? pos = null)
        {//literally identical to ER
            var ptr3 = getFreeCamPtr();
            var ptrX = (IntPtr)(ptr3 + 0x40);
            var ptrY = (IntPtr)(ptr3 + 0x44);
            var ptrZ = (IntPtr)(ptr3 + 0x48);

            float x = ReadFloat(ptrX);
            float y = ReadFloat(ptrY);
            float z = ReadFloat(ptrZ);

            if (pos != null)
            {
                WriteFloat(ptrX, pos.Value.Item1);
                WriteFloat(ptrY, pos.Value.Item2);
                WriteFloat(ptrZ, pos.Value.Item3);
            }

            return (x, y, z);
        }

        public void moveCamToPlayer()
        {
            var player = getSetPlayerLocalCoords();
            player.y += 1.7f; //offset to roughly player head
            getSetFreeCamCoords((player.x, player.y, player.z));
        }

        public void movePlayerToCam()
        {
            var cam = getSetFreeCamCoords();
            getSetPlayerLocalCoords((cam.x, cam.y, cam.z, float.NaN));
        }

        bool _fontPatchesDone = false;
        void doFontPatch()
        {//all from DS3-Debug-Patch. not 100% sure but i assume it prevents drawing with a font that doesn't exist. only first one seems necessary for sound draw.
            if (_fontPatchesDone) { return; }
            WriteBytes(ds3Base + fontDrawFirstPatchLoc, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 });//all nop
            /*WriteUInt8(ds3Base + 0x2352600, 0xC3);
            WriteUInt8(ds3Base + 0x23B7670, 0xC3);
            WriteUInt8(ds3Base + 0x1915370, 0xC3);
            WriteUInt8(ds3Base + 0x03A44A0, 0xC3);
            WriteUInt8(ds3Base + 0x0CDB500, 0xC3);*/
            _fontPatchesDone = true;
        }

        public void setAllSoundDebug(bool on)
        {//seems that a patch is needed or else it will crash the game when it tries to draw
            //alternative to this enabling method is to patch the instruction that reads it
            //40 56 48 83EC ?? 8079 ?? 00 48 8BF2 74 ??    <--- works in sekiro and DS3 but in DS3 it's obfuscated in the exe. patch the cmp of 00 to 01.
            doFontPatch();
            var ptr1 = (IntPtr)ReadUInt64(ds3Base + worldAIManOff);
            var ptr2 = (IntPtr)ReadUInt64(ptr1 + 0x28); //WorldBlockAi
            for (int off = 0; off <= 0x3200; off += 0x200)
            {
                var ptr3 = ptr2 + off;
                var ptr4 = (IntPtr)ReadUInt64(ptr3 + 0xB8);
                if (ptr4.ToInt64() >= 0x7FF000000000 && ptr4.ToInt64() <= 0x800000000000) //just a null check is probably adequate (that's all the game does)
                {
                    var addr = ptr4 + 0x28;
                    WriteUInt8(addr, on ? (byte)1 : (byte)0);
                }
            }
        }

        (IntPtr, byte) lookupOpt(DebugOpts opt)
        {//second value is "value for on state" if it's just one byte.
            (IntPtr, byte) badVal = (IntPtr.Zero, 0);
            switch (opt)
            {
                case DebugOpts.COL_MESH_MAIN: return (ds3Base + meshesOff + 0, 1); //6c
                case DebugOpts.COL_MESH_VISUAL: return (ds3Base + meshesOff + 1, 1); //6d
                                                                                     //there's a character mesh at +3, but it requires 'all debug drawing' to also be on. (or switched on individually for a character)
                case DebugOpts.COL_MESH_HIGH_PERF: return (ds3Base + meshesOff + 5, 1); //71
                case DebugOpts.COL_MESH_COLOURS: return (ds3Base + meshesOff + 8, 1); //74
                case DebugOpts.CHARACTER_MESH: return (ds3Base + meshesOff + 3, 1); //6f
                case DebugOpts.HITBOX_VIEW:
                case DebugOpts.IMPACT_VIEW:
                    {
                        var ptr = ReadUInt64(ds3Base + hitboxOff);
                        if (ptr < SANE_MINIMUM) { return badVal; }
                        ptr += 0x30;
                        if (opt == DebugOpts.IMPACT_VIEW) { ptr += 1; }
                        //Utils.debugWrite(ptr.ToString("X16"));
                        return ((IntPtr)ptr, 1);
                    }
                case DebugOpts.DISABLE_MAP: return (ds3Base + GROUP_MASKOff, 0);
                case DebugOpts.DISABLE_CHARACTER: return (ds3Base + GROUP_MASKOff + 2, 0); //TODO - what was todo?
                case DebugOpts.NO_DEATH:
                    {
                        var ptr3 = getCharPtrModules();
                        var ptr4 = ReadUInt64((IntPtr)(ptr3 + chrDataModuleOff));
                        var ptr5 = (IntPtr)(ptr4 + 0x1C0); //character debug flags?
                        return (ptr5, 0x12); //bitfield, bit 2
                    }
                case DebugOpts.ALL_CHR_NO_DEATH: return (ds3Base + debug_flagsOff + 0x8, 1);
                case DebugOpts.INSTANT_QUITOUT:
                    {

                        var ptr = ReadUInt64(ds3Base + menuManOff);
                        return ((IntPtr)(ptr + 0x250), 1); //likely other menu functions nearby
                    }
                case DebugOpts.ONE_HP:
                case DebugOpts.MAX_HP:
                    {
                        var ptr3 = getCharPtrModules();
                        var ptr4 = ReadUInt64((IntPtr)(ptr3 + chrDataModuleOff));
                        var ptr5 = (IntPtr)(ptr4 + 0xD8);
                        if (opt == DebugOpts.MAX_HP) { return (ptr5, 0xfe); }
                        return (ptr5, 0xff);
                    }
                case DebugOpts.ALL_CHRS_DBG_DRAW_FLAG:
                    {
                        var ptr = ReadUInt64((IntPtr)(ds3Base + worldChrManDbgOff));
                        return ((IntPtr)(ptr + 0x65), 1);
                    }
                case DebugOpts.DISABLE_AI: return (ds3Base + debug_flagsOff + 0xD, 1);
                case DebugOpts.NO_STAM: return (ds3Base + debug_flagsOff + 0x2, 1);
                case DebugOpts.NO_FP: return (ds3Base + debug_flagsOff + 0x3, 1);
                case DebugOpts.NO_ARROW_CONSUM: return (ds3Base + debug_flagsOff + 0x4, 1);
                case DebugOpts.ONE_SHOT: return (ds3Base + debug_flagsOff + 1, 1);
                case DebugOpts.NO_GOODS_CONSUM:
                    {
                        var ptr = getPlayerInsPtr();
                        return ((IntPtr)(ptr + playerDebugFlagsOff), 0x13);
                    }
                case DebugOpts.DISABLE_STEAM_INPUT_ENUM:
                    {
                        var ptr = ReadUInt64(ds3Base + usrInputMgrImplOff);
                        return ((IntPtr)(ptr + usrInputMgrImpSteamInputFlagOff), 1);
                    }
                case DebugOpts.EVENT_STOP:
                    {
                        var ptr = ReadUInt64(ds3Base + SprjDebugEvent);
                        return ((IntPtr)(ptr + 0xD4), 1); //was D4, changed in 1.15.1 to E4
                    }
                case DebugOpts.EVENT_DRAW:
                    {
                        var ptr = ReadUInt64(ds3Base + SprjDebugEvent);
                        return ((IntPtr)(ptr + 0xA8), 1);
                    }
                case DebugOpts.HIDDEN_DEBUG_MENU:
                    {
                        var ptr = ReadUInt64(ds3Base + newMenuSystemOff);
                        return ((IntPtr)(ptr + 0x3083), 1);
                    }
                case DebugOpts.ALL_DEBUG_DRAWING:
                    {
                        var ptr = ReadUInt64(ds3Base + worldChrManDbgOff);
                        return ((IntPtr)(ptr + 0x65), 1);
                    }
                case DebugOpts.ENEMY_TARGETING_A:
                    {
                        return (ds3Base + enemyTargetDrawAOff, 1);
                    }
                case DebugOpts.ENEMY_TARGETING_B:
                    {
                        return (ds3Base + enemyTargetDrawBOff, 1);
                    }
                case DebugOpts.FREE_CAMERA:
                    {
                        var ptr = ReadUInt64(ds3Base + fieldAreaOff);
                        var ptr2 = ReadUInt64((IntPtr)ptr + 0x18); //GameRend
                        return ((IntPtr)(ptr2 + 0xE0), 1);
                    }
                case DebugOpts.NO_GRAVITY:
                    {
                        var ptr3 = getCharPtrModules();
                        var ptr4 = ReadUInt64((IntPtr)(ptr3 + chrPhysModuleOff));
                        return ((IntPtr)(ptr4 + 0x1DC), 1); //may be other gravity flags; ER has multiple
                                                            //1DA may be equivalent? this is the one the debug menu sets. seems to unset itself sometimes.
                                                            // gravity: bitflag!(0b1000000; world_chr_man, 0x80, 0x1a08), //alternative?
                    }
            }
            return badVal;
        }

        public int getSetPlayerHP(int? val = null)
        {
            var ptr3 = getCharPtrModules();
            var ptr4 = ReadUInt64((IntPtr)(ptr3 + chrDataModuleOff));
            var ptr5 = (IntPtr)(ptr4 + 0xD8);
            int ret = ReadInt32(ptr5);
            if (val.HasValue) { WriteInt32(ptr5, val.Value); }
            return ret;
        }

        public void doFreeCamPlayerControlPatch()
        {
            if (ReadBytes(ds3Base + freeCamPlayerControlPatchLoc, 6).SequenceEqual(freeCamPlayerControlPatchOrig))
            {
                WriteBytes(ds3Base + freeCamPlayerControlPatchLoc, freeCamPlayerControlPatchReplacement);
            }
        }

        public void undoFreeCamPlayerControlPatch()
        {
            if (ReadBytes(ds3Base + freeCamPlayerControlPatchLoc, 6).SequenceEqual(freeCamPlayerControlPatchReplacement))
            {
                WriteBytes(ds3Base + freeCamPlayerControlPatchLoc, freeCamPlayerControlPatchOrig);
            }
        }

        public void cycleMeshColours()
        {
            IntPtr addr = ds3Base + meshesOff + 8;
            int meshColours = ReadInt32(addr);
            meshColours++;
            if (meshColours > 3) { meshColours = 0; }
            WriteInt32(addr, meshColours);
        }

        public void enableOpt(DebugOpts opt)
        {
            if (opt == DebugOpts.TARGET_HP)
            {//special case
                getSetTargetInfo(TargetInfo.HP, targetHpFreeze);
                return;
            }
            if (opt == DebugOpts.SOUND_VIEW)
            {//special case
                setAllSoundDebug(true);
                return;
            }

            var tuple = lookupOpt(opt);
            if (tuple.Item1 == IntPtr.Zero) { Utils.debugWrite("Can't enable " + opt); return; }

            if ((long)tuple.Item1 < SANE_MINIMUM || (long)tuple.Item1 > SANE_MAXIMUM) { return; }

            var val = tuple.Item2;
            if (val == 0 || val == 1)
            {
                WriteUInt8(tuple.Item1, val);
            }
            else if (val >= 0x10 && val <= 0x17)
            {//bitfield (set to enable)
                int setMask = 1 << (val - 0x10);
                var oldVal = ReadUInt8(tuple.Item1);
                var newVal = oldVal | setMask;
                WriteUInt8(tuple.Item1, (byte)newVal);
            }
            else if (val == 0xff)
            {//special case, write 1 as 32 bit
                WriteUInt32(tuple.Item1, 1);
            }
            else if (val == 0xfe)
            {//special case, read next 32 bit int and write
                var nextVal = ReadUInt32(tuple.Item1 + 4); //max HP is after hp in the character struct (i think)
                WriteUInt32(tuple.Item1, nextVal);
            }
        }
        public void disableOpt(DebugOpts opt)
        {
            if (opt == DebugOpts.SOUND_VIEW)
            {//special case
                setAllSoundDebug(false);
                return;
            }

            var tuple = lookupOpt(opt);
            if (tuple.Item1 == IntPtr.Zero) { return; }

            if ((long)tuple.Item1 < SANE_MINIMUM || (long)tuple.Item1 > SANE_MAXIMUM) { return; }

            var val = tuple.Item2;
            if (val == 0 || val == 1)
            {
                var newVal = (tuple.Item2 == 1) ? (byte)0 : (byte)1;
                WriteUInt8(tuple.Item1, newVal);
            }
            else if (val >= 0x10 && val <= 0x17)
            {//bitfield (clear to disable)
                int setMask = 1 << (val - 0x10);
                var oldVal = ReadUInt8(tuple.Item1);
                var newVal = oldVal & ~setMask;
                WriteUInt8(tuple.Item1, (byte)newVal);
            }
            else if (val == 0xff)
            {//special case, write 9999 as 32 bit
                WriteUInt32(tuple.Item1, 9999);
            }
            else if (val == 0xfe)
            {//nothing to do
            }
        }

        object setLock = new object();

        public void freezeOn(DebugOpts opt)
        {
            if (opt == DebugOpts.TARGET_HP)
            {//special case
                targetHpFreeze = (int)getSetTargetInfo(TargetInfo.HP);
            }

            lock (setLock)
            {
                freezeSet.Add(opt);
            }
        }
        public void offAndUnFreeze(DebugOpts opt)
        {
            lock (setLock)
            {
                unFreezeSet.Add(opt);
            }
        }

        HashSet<DebugOpts> freezeSet = new HashSet<DebugOpts>();
        HashSet<DebugOpts> unFreezeSet = new HashSet<DebugOpts>();

        public bool weGood { get; set; } = true;

        void freezeFunc()
        {
            while (_running)
            {
                weGood = ReadTest(ds3Base); //is it possible to come good later, or should we just fail immediately and dispose ourself?
                lock (setLock)
                {
                    foreach (var opt in unFreezeSet)
                    {
                        disableOpt(opt);
                        if (freezeSet.Contains(opt)) { freezeSet.Remove(opt); }
                    }
                    unFreezeSet.Clear();
                    foreach (var opt in freezeSet)
                    {
                        enableOpt(opt);
                    }
                }
                Thread.Sleep(100);
            }
        }

        int? targetHpFreeze = null;

        public double getSetTargetInfo(TargetInfo info, int? setVal = null)
        {//most are actually ints but it's easier just to use a common type. double can store fairly large ints exactly.
            double ret = double.NaN;
            var targetPtr = ReadUInt64(ds3Base + codeCavePtrLoc); //NS_SPRJ::EnemyIns
            if (targetPtr < SANE_MINIMUM || targetPtr > SANE_MAXIMUM) { return ret; }
            var p1 = ReadUInt64((IntPtr)(targetPtr + playerInsModulesOff));
            if (p1 < SANE_MINIMUM || p1 > SANE_MAXIMUM) { return ret; }

            uint p2off = 0;
            switch (info)
            {
                case TargetInfo.HP:
                case TargetInfo.MAX_HP:
                    p2off = 0x18; //NS_SPRJ::SprjChrDataModule
                    break;
                case TargetInfo.POISE:
                case TargetInfo.MAX_POISE:
                case TargetInfo.POISE_TIMER:
                    p2off = 0x40; //NS_SPRJ::SprjChrSuperArmorModule
                    break;
                default: //assume resists.
                    p2off = 0x20; //NS_SPRJ::SprjChrResistModule
                    break;
            }
            var p2 = ReadUInt64((IntPtr)(p1 + p2off));

            uint p3off = 0;
            switch (info)
            {
                case TargetInfo.HP:
                    p3off = 0xD8; break;
                case TargetInfo.MAX_HP:
                    p3off = 0xDC; break; //apparently this is 'base max HP', and there's another max hp at ...E0? FC?
                case TargetInfo.POISE:
                    p3off = 0x28; break;
                case TargetInfo.MAX_POISE:
                    p3off = 0x28 + 4; break;
                case TargetInfo.POISE_TIMER:
                    p3off = 0x28 + 4 + 8; break;
                default:
                    {//assume resists
                        int poisonOff = info - TargetInfo.POISON;
                        int statIndex = poisonOff / 2;
                        bool isMax = (poisonOff % 2) == 1;
                        p3off = (uint)((isMax ? 0x24 : 0x10) + 4 * statIndex);
                        break;
                    }
            }
            var pFinal = (IntPtr)(p2 + p3off);

            var fourBytes = ReadBytes(pFinal, 4);

            if (setVal.HasValue)
            {//TODO: support setting float? pass in object i guess
                WriteInt32(pFinal, setVal.Value);
                if (info == TargetInfo.HP)
                {//special case
                    targetHpFreeze = (int)setVal; //change our freeze value. the freeze itself will keep setting this, but that's harmless.
                }
                return (double)BitConverter.ToInt32(fourBytes, 0); //no real need to return anything tbh
            }

            switch (info)
            {
                case TargetInfo.POISE:
                case TargetInfo.MAX_POISE:
                case TargetInfo.POISE_TIMER:
                    {
                        return (double)BitConverter.ToSingle(fourBytes, 0);
                    }
            }

            return (double)BitConverter.ToInt32(fourBytes, 0);
        }

        public string GetSetTargetEnemyID(string newValue = null)
        {
            var targetPtr = ReadInt64(ds3Base + CodeCavePtrLoc);
            var modulesPtr = ReadInt64((IntPtr)targetPtr + 0x1F90);
            var chrDataPtr = ReadInt64((IntPtr)modulesPtr + 0x18);
            var enemyIdPtr = (IntPtr)(chrDataPtr + 0x130);

            if (newValue != null)
            {
                WriteString(enemyIdPtr, newValue);
            }

            return ReadString(enemyIdPtr);
        }

        public int GetSetPlayerStat(PlayerStats stat, int? newValue = null)
        {
            var gameDataPtr = ReadUInt64(ds3Base + gameDataManOff);
            var playerStatsPtr = ReadUInt64((IntPtr)(gameDataPtr + 0x10));
            var statAddress = (IntPtr)(playerStatsPtr + (ulong)statOffsets[stat]);

            if (newValue.HasValue)
            {
                UpdatePlayerStat(stat, statAddress, playerStatsPtr, newValue.Value);
            }

            return ReadInt32(statAddress);
        }

        private void UpdatePlayerStat(PlayerStats stat, IntPtr statAddress, ulong playerStatsPtr, int newValue)
        {
            WriteInt32(statAddress, newValue);

            if (stat == PlayerStats.SOULS)
            {
                var totalSoulsAddress = (IntPtr)(playerStatsPtr + 0x78);
                int currentTotalSouls = ReadInt32(totalSoulsAddress);
                WriteInt32(totalSoulsAddress, currentTotalSouls + newValue);
            }
        }

        public int GetSetNewGameLevel(int? newValue = null)
        {
            var ptr1 = ReadUInt64(ds3Base + gameDataManOff);
            var finalAddress = (IntPtr)(ptr1 + 0x78);
            if (newValue.HasValue)
            {
                WriteInt32(finalAddress, newValue.Value);
            }
            return ReadInt32(finalAddress);
        }
    }
}
