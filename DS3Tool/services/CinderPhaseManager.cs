using DS3Tool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

internal class CinderPhaseManager : IDisposable

{
    private const string CinderEnemyId = "c5280_0000";
    private const int CodeCavePointerOffset = 0x1914670;
    private const int AnimationPointerChainOffset1 = 0x1F90;
    private const int AnimationPointerChainOffset2 = 0x58;
    private const int AnimationFinalOffset = 0x20;
    private const int ComManipulatorOffset = 0x58;
    private const int AiInsOffset = 0x320;
    private const int LuaNumbersOffset = 0x6BC;

    private const int CurrentAnimationPtr = 0x28;
    private const int CurrentAnimationOffset = 0x898;
    private const int CurrentPhaseOffset = 0xC80;

    private const int Gwyn5HitComboNumberIndex = 0;
    private const int GwynLightningRainNumberIndex = 1;
    private const int PhaseTransitionCounterNumberIndex = 2;

    private readonly DS3Process _ds3Process;

    private CancellationTokenSource _monitorCancellation;
    private Task _monitoringTask;
    private int _currentPhase;
    private bool _isLocked;


    public CinderPhaseManager(DS3Process ds3Process)
    {
        _ds3Process = ds3Process;

        _isLocked = false;
    }

    public class PhaseInfo
    {
        public string Name { get; }
        public int AnimationId { get; }
        public PhaseInfo(string name, int animationId)
        {
            Name = name;
            AnimationId = animationId;

        }
    }

    private readonly Dictionary<int, PhaseInfo> _phases = new Dictionary<int, PhaseInfo>
    {
        { 0, new PhaseInfo("Sword", 20000)},
        { 1, new PhaseInfo("Lance", 20001)},
        { 2, new PhaseInfo("Curved", 20002)},
        { 3, new PhaseInfo("Staff", 20004)},
        { 4, new PhaseInfo("Gwyn", 20010)}
    };

    private readonly Dictionary<int, int> _currentPhaseLookUp = new Dictionary<int, int>
    {
        {2, 0},
        {16, 1},
        {4, 2},
        {8, 3},
        {32, 4},
    };


    public void SetPhase(int phaseIndex, bool lockPhase)
    {

        if (!_phases.ContainsKey(phaseIndex))
            throw new ArgumentException($"Invalid phase index: {phaseIndex}");

        if (!ValidateTargetIsCinder())
        {
            MessageBox.Show("Please ensure you are locked onto Soul of Cinder before changing phases.",
                          "Invalid Target",
                          MessageBoxButton.OK);
            return;
        }

        _currentPhase = phaseIndex;
        var phase = _phases[phaseIndex];

        ForceAnimation(phase.AnimationId);
        ClearCounters();

        if (lockPhase && !_isLocked)
        {
            StartPhaseLock();
        }
        else if (!lockPhase && _isLocked)
        {
            StopPhaseLock();
        }
    }
    public void TogglePhaseLock(bool enableLock)
    {
        if (enableLock && !_isLocked)
        {
            StartPhaseLock();
        }
        else if (!enableLock && _isLocked)
        {
            StopPhaseLock();
        }
    }

    private void StartPhaseLock()
    {
        if (_monitoringTask == null || _monitoringTask.IsCompleted)
        {
            _monitorCancellation = new CancellationTokenSource();
            _monitoringTask = MonitorAndResetCounter(_monitorCancellation.Token);
            _isLocked = true;
        }
    }

    private void StopPhaseLock()
    {
        if (_monitorCancellation != null)
        {
            _monitorCancellation.Cancel();

            var cancelTask = Task.Run(() =>
            {
                try
                {
                    _monitoringTask?.Wait(TimeSpan.FromSeconds(2));
                }
                catch (AggregateException) { }
                catch (TaskCanceledException) { }
            });


            _monitorCancellation?.Dispose();
            _monitorCancellation = null;
            _monitoringTask = null;
            _isLocked = false;
        }
    }


    private async Task MonitorAndResetCounter(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (IsCurrentPhaseGwyn)
                {
                    await Task.Delay(1000, token);
                    continue;
                }

                var codeCavePointer = _ds3Process.ReadInt64(_ds3Process.ds3Base + CodeCavePointerOffset);
                if (codeCavePointer != 0)
                {
                    CheckAndResetLuaCounter(codeCavePointer);
                }
                await Task.Delay(1000, token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {

                Debug.WriteLine($"Error in phase monitoring: {ex.Message}");
                await Task.Delay(1000, token);
            }
        }
    }

    private bool IsCurrentPhaseGwyn => _currentPhase == 4;

    private void CheckAndResetLuaCounter(long targetPtr)
    {
        var comManipulator = _ds3Process.ReadInt64(new IntPtr(targetPtr) + ComManipulatorOffset);
        if (comManipulator != 0)
        {
            var aiIns = _ds3Process.ReadInt64(new IntPtr(comManipulator) + AiInsOffset);
            if (aiIns != 0)
            {
                var phaseTransitionCounter = GetLuaNumber(aiIns, PhaseTransitionCounterNumberIndex);

                if (phaseTransitionCounter > 50)
                {
                    Console.WriteLine("Resetting phase counter");
                    SetLuaNumber(aiIns, PhaseTransitionCounterNumberIndex, 0);
                }
            }
        }
    }

    private void ForceAnimation(int animationId)
    {
        var codeCavePointer = _ds3Process.ReadInt64(_ds3Process.ds3Base + CodeCavePointerOffset);
        if (codeCavePointer == 0)
        {
            MessageBox.Show("Please lock on to Cinder and Enable target options before selecting a phase", "Error", MessageBoxButton.OK);
            return;
        }

        var basePtr = new IntPtr(codeCavePointer);
        var ptr1 = _ds3Process.ReadInt64(basePtr + AnimationPointerChainOffset1);
        var ptr2 = _ds3Process.ReadInt64(new IntPtr(ptr1) + AnimationPointerChainOffset2);
        var finalAddr = new IntPtr(ptr2) + AnimationFinalOffset;
        _ds3Process.WriteInt32(finalAddr, animationId);
    }

    private void SetLuaNumber(long aiIns, int index, float value) 
    {
        _ds3Process.WriteFloat(new IntPtr(aiIns + LuaNumbersOffset + 4 * index), value);
    }
    
    private float GetLuaNumber(long aiIns, int index)
    {
        return _ds3Process.ReadFloat(new IntPtr(aiIns + LuaNumbersOffset + 4 * index));
    }

    private void ClearCounters()
    {
        var codeCavePointer = _ds3Process.ReadInt64(_ds3Process.ds3Base + CodeCavePointerOffset);
        var comManipulator = _ds3Process.ReadInt64(new IntPtr(codeCavePointer) + ComManipulatorOffset);
        var aiIns = _ds3Process.ReadInt64(new IntPtr(comManipulator) + AiInsOffset);
        SetLuaNumber(aiIns, Gwyn5HitComboNumberIndex, 0);
        SetLuaNumber(aiIns, GwynLightningRainNumberIndex, 0);
        SetLuaNumber(aiIns, PhaseTransitionCounterNumberIndex, 0);
    }

    public void Dispose()
    {
        StopPhaseLock();
    }

    private bool ValidateTargetIsCinder()
    {
        var codeCavePointer = _ds3Process.ReadInt64(_ds3Process.ds3Base + CodeCavePointerOffset);
        if (codeCavePointer == 0)
            return false;

        string targetId = _ds3Process.GetSetTargetEnemyID();
        return targetId == CinderEnemyId;
    }

    public void CastSoulMass()
    {
        var targetPtr = _ds3Process.ReadInt64(_ds3Process.ds3Base + CodeCavePointerOffset);
        var comManipulatorPtr = _ds3Process.ReadInt64(new IntPtr(targetPtr) + ComManipulatorOffset);
        var currentPhaseAddr = (IntPtr)(comManipulatorPtr + CurrentPhaseOffset);
        int phaseBeforeSoulmass = _ds3Process.ReadInt32(currentPhaseAddr);

        //First sword phase doesnt change this number, so set to 2 (Sword) if 0
        if (phaseBeforeSoulmass == 0)
        {
            phaseBeforeSoulmass = 2;
        }

        ForceAnimation(_phases[3].AnimationId);

        int maxWaitTime = 500;
        int waitCounter = 0;
        bool isStaffPhase = false;

        while (isStaffPhase == false)
        {
            int currentPhase = _ds3Process.ReadInt32(currentPhaseAddr);
            isStaffPhase = currentPhase == 8;
            Thread.Sleep(10);
            
            waitCounter++;
            if (waitCounter > maxWaitTime) {
                MessageBox.Show("Something went wrong with the forced transition, please try again or reload the game", "Operation Failed");
                return;
            }
        }


        var ptr1 = _ds3Process.ReadInt64(new IntPtr(targetPtr) + AnimationPointerChainOffset1);
        var ptr2 = _ds3Process.ReadInt64(new IntPtr(ptr1) + AnimationPointerChainOffset2);
        var finalAddr = new IntPtr(ptr2) + AnimationFinalOffset;
        _ds3Process.WriteInt32(finalAddr, 3003);

        var currentAnimPtr2 = _ds3Process.ReadInt64(new IntPtr(ptr1) + CurrentAnimationPtr);


        bool hasStartedSoulmass = false;
        waitCounter = 0;
        while (hasStartedSoulmass == false)
        {
            string currentAnim = _ds3Process.ReadString(new IntPtr(currentAnimPtr2) + CurrentAnimationOffset, 20);
            hasStartedSoulmass = currentAnim == "Attack3003";
            Thread.Sleep(10);
            
            waitCounter++;
            if (waitCounter > maxWaitTime) {
                MessageBox.Show("Something went wrong with the forced soulmass, please try again or reload the game", "Operation Failed");
                return;
            }
        }
        bool hasFinishedSoulmass = false;
        waitCounter = 0;
        while (hasFinishedSoulmass == false)
        {
            string currentAnim = _ds3Process.ReadString(new IntPtr(currentAnimPtr2) + CurrentAnimationOffset, 20);

            hasFinishedSoulmass = currentAnim != "Attack3003";
            Thread.Sleep(10);
            waitCounter++;
            if (waitCounter > maxWaitTime) {
                MessageBox.Show("Soulmass animation didn't start in time", "Operation Failed");
                return;
            }
        }

        int previousPhase = _currentPhaseLookUp[phaseBeforeSoulmass];

        ForceAnimation(_phases[previousPhase].AnimationId);
    }
    
    
     private bool _isSoulmassHookInstalled;
    private long _codecaveStart = 0x143B536D0;
    public void EnableEndlessSoulmass()
    {
        if (!_isSoulmassHookInstalled)
        {
            var origin = 0x140E30719;
            
            byte[] soulmassBytes =
            {
                0x48, 0x81, 0xFB, 0x30, 0x67, 0x12, 0x00, // cmp    rbx,0x126730
                0x0F, 0x85, 0x00, 0x00, 0x00, 0x00, // jne    d <_main+0xd>
                0x48, 0x01, 0xD3, // add    rbx,rdx
                0xC7, 0x43, 0x08, 0x00, 0x00, 0x80, 0xBF, // mov    DWORD PTR [rbx+0x8],0xbf800000
                0xE9, 0x00, 0x00, 0x00, 0x00, // jmp    1c <_main+0x1c>
                0x48, 0x01, 0xD3, // add    rbx,rdx
                0x48, 0x89, 0x6C, 0x24, 0x28, // mov    QWORD PTR [rsp+0x28],rbp
                0xE9, 0x00, 0x00, 0x00, 0x00 // jmp    29 <_main+0x29>
            };

            byte[] bytes = BitConverter.GetBytes(15); //Not soul mass, skip
            Array.Copy(bytes, 0, soulmassBytes, 9, 4);
            bytes = BitConverter.GetBytes(3); // Skip original add
            Array.Copy(bytes, 0, soulmassBytes, 24, 4);
            bytes = BitConverter.GetBytes((int)(origin + 8 - (_codecaveStart + 41))); //Jmp origin
            Array.Copy(bytes, 0, soulmassBytes, 37, 4);

            _ds3Process.WriteBytes((IntPtr)_codecaveStart, soulmassBytes);

            byte[] jmpBytes = { 0xE9, 0x00, 0x00, 0x00, 0x00, 0x90, 0x90, 0x90 };
            bytes = BitConverter.GetBytes((int)(_codecaveStart - (origin + 5)));
            Array.Copy(bytes, 0, jmpBytes, 1, 4);
            _ds3Process.WriteBytes((IntPtr)origin, jmpBytes);
            _isSoulmassHookInstalled = true;
        }
        else
        {
            byte[] enableBytes = { 0x00, 0x00, 0x80, 0xBF };
            _ds3Process.WriteBytes((IntPtr)_codecaveStart + 0x13, enableBytes);
        }
    }

    public void DisableEndlessSoulmass()
    {
        byte[] disableBytes = { 0x00, 0x00, 0x20, 0x42 };
        _ds3Process.WriteBytes((IntPtr)_codecaveStart + 0x13, disableBytes);
    }

}
