using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS3Tool.services
{
    internal class CinderService
    {

        private readonly DS3Process _ds3Process;
        const int codeCavePtrLoc = 0x1914670;

        public CinderService(DS3Process ds3Process)
        {
            _ds3Process = ds3Process;
        }

        public class PhaseInfo
        {
            public string Name { get; }
            public int AnimationId { get; }
            public int Offset { get; }
            public int Act { get; }

            public PhaseInfo(string name, int animationId, int offset, int act)
            {
                Name = name;
                AnimationId = animationId;
                Offset = offset;
                Act = act;
            }
        }

        private readonly Dictionary<int, PhaseInfo> _phases = new Dictionary<int, PhaseInfo>
        {
            { 0, new PhaseInfo("Sword", 20000, 0, 30) },
            { 1, new PhaseInfo("Lance", 20001, 1000000, 31) },
            { 2, new PhaseInfo("Curved", 20002, 2000000, 32) },
            { 3, new PhaseInfo("Staff", 20004, 4000000, 33) },
            { 4, new PhaseInfo("Gwyn", 20010, 5000000, 0) }
        };

        public void ForcePhaseTransition(int phaseIndex)
        {
            var phase = _phases[phaseIndex];
            ForceAnimation(phase.AnimationId);

            
            SetLuaNumber(0, 0);
            SetLuaNumber(1, 0);
            SetLuaNumber(2, 0);
        }

        public void ForceAnimation(int animationId)
        {
            var storedPtr = _ds3Process.ReadInt64(_ds3Process.ds3Base + codeCavePtrLoc);
            if (storedPtr == 0)
            {
                throw new InvalidOperationException("No target selected!");
            }

            var basePtr = new IntPtr(storedPtr);
            var ptr1 = _ds3Process.ReadInt64(basePtr + 0x1F90);
           

            var ptr2 = _ds3Process.ReadInt64(new IntPtr(ptr1) + 0x58);
           

            var finalAddr = new IntPtr(ptr2) + 0x20;
            _ds3Process.WriteInt32(finalAddr, animationId);
        }


    }
}
