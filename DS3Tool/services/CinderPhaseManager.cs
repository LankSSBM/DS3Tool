using DS3Tool;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Windows;

internal class CinderPhaseManager : IDisposable

{
    private const string CINDER_ENEMY_ID = "c5280_0000";
    private const int CodeCavePointerOffset = 0x1914670;
    private const int AnimationPointerChainOffset1 = 0x1F90;
    private const int AnimationPointerChainOffset2 = 0x58;
    private const int AnimationFinalOffset = 0x20;
    private const int LuaStateOffset = 0x58;
    private const int LuaNumbersBaseOffset = 0x320;
    private const int LuaCounterOffset = 0x6C4;


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
        ResetLuaNumbers();

 
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
        var luaPtr = _ds3Process.ReadInt64(new IntPtr(targetPtr) + LuaStateOffset);
        if (luaPtr != 0)
        {
            var luaBase = _ds3Process.ReadInt64(new IntPtr(luaPtr) + LuaNumbersBaseOffset);
            if (luaBase != 0)
            {
                var currentValue = _ds3Process.ReadFloat(new IntPtr(luaBase) + LuaCounterOffset);
                _ds3Process.WriteFloat(new IntPtr(luaBase) + LuaCounterOffset, 0);

                if (currentValue > 50)
                {
                    var phase = _phases[_currentPhase];
                    ForceAnimation(phase.AnimationId);
                    ResetLuaNumbers();
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

    private void ResetLuaNumbers()
    {
        SetLuaNumber(0, 0);
        SetLuaNumber(1, 0);
        SetLuaNumber(2, 0);
    }

    private void SetLuaNumber(int numberIndex, float value)
    {
        var codeCavePointer = _ds3Process.ReadInt64(_ds3Process.ds3Base + CodeCavePointerOffset);
        var luaStatePointer = _ds3Process.ReadInt64(new IntPtr(codeCavePointer) + LuaStateOffset);
        var luaBaseAddress = _ds3Process.ReadInt64(new IntPtr(luaStatePointer) + LuaNumbersBaseOffset);
        var finalAddr = new IntPtr(luaBaseAddress) + 0x6BC + (4 * numberIndex);
        _ds3Process.WriteFloat(finalAddr, value);
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
        return targetId == CINDER_ENEMY_ID;
    }

}
