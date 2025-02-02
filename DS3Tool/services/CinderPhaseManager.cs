using DS3Tool;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

internal class CinderPhaseManager : IDisposable

{
    private readonly DS3Process _ds3Process;
    private const int CODE_CAVE_PTR_LOC = 0x1914670;
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
        _monitorCancellation?.Cancel();
        try
        {
            _monitoringTask?.Wait();
        }
        catch (AggregateException) { }

        _monitorCancellation?.Dispose();
        _monitorCancellation = null;
        _monitoringTask = null;
        _isLocked = false;
      
    }


    private async Task MonitorAndResetCounter(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {

                if (_currentPhase == 4) 
                {
                    await Task.Delay(1000, token);
                    continue;
                }

                var targetPtr = _ds3Process.ReadInt64(_ds3Process.ds3Base + CODE_CAVE_PTR_LOC);
                if (targetPtr != 0)
                {
                    var luaPtr = _ds3Process.ReadInt64(new IntPtr(targetPtr) + 0x58);
                    if (luaPtr != 0)
                    {
                        var luaBase = _ds3Process.ReadInt64(new IntPtr(luaPtr) + 0x320);
                        if (luaBase != 0)
                        {
                            var currentValue = _ds3Process.ReadFloat(new IntPtr(luaBase) + 0x6C4);
                            _ds3Process.WriteFloat(new IntPtr(luaBase) + 0x6C4, 0);

                            if (currentValue > 50)
                            {
                                var phase = _phases[_currentPhase];
                                ForceAnimation(phase.AnimationId);
                                ResetLuaNumbers();
                            }
                        }
                    }
                }
                await Task.Delay(1000, token);
            }
            catch (Exception ex)
            {
           
                await Task.Delay(1000, token);
            }
        }
    }

    private void ForceAnimation(int animationId)
    {
        var storedPtr = _ds3Process.ReadInt64(_ds3Process.ds3Base + CODE_CAVE_PTR_LOC);
        if (storedPtr == 0)
            throw new InvalidOperationException("No target selected!");

        var basePtr = new IntPtr(storedPtr);
        var ptr1 = _ds3Process.ReadInt64(basePtr + 0x1F90);
        var ptr2 = _ds3Process.ReadInt64(new IntPtr(ptr1) + 0x58);
        var finalAddr = new IntPtr(ptr2) + 0x20;
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
        var storedPtr = _ds3Process.ReadInt64(_ds3Process.ds3Base + CODE_CAVE_PTR_LOC);
        var ptr1 = _ds3Process.ReadInt64(new IntPtr(storedPtr) + 0x58);
        var baseAddr = _ds3Process.ReadInt64(new IntPtr(ptr1) + 0x320);
        var finalAddr = new IntPtr(baseAddr) + 0x6BC + (4 * numberIndex);
        _ds3Process.WriteFloat(finalAddr, value);
    }

    public void Dispose()
    {
        StopPhaseLock();
    }
}
