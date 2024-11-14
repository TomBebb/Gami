using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Input;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Silk.NET.SDL;

namespace Gami.BigPicture.Inputs;

public sealed class InputManager : IDisposable
{
    private readonly Sdl _sdl = Sdl.GetApi();
    private bool _isDisposed;


    private InputManager()
    {
        _sdl.Init(Sdl.InitGamecontroller | Sdl.InitJoystick);
        Task.Run(async () =>
        {
            var quit = false;
            var ev = new Event();
            while (!quit && !_isDisposed)
            {
                while (_sdl.PollEvent(ref ev) != 0)
                    ProcessEvent(ev, ref quit);
                await Task.Delay(2);
            }
        });
    }

    public static InputManager Instance { get; private set; } = new();

    [Reactive]
    public ImmutableHashSet<MappedInputType> ActiveInputs { get; private set; } =
        ImmutableHashSet<MappedInputType>.Empty;

    public void Dispose()
    {
        _isDisposed = true;
    }

    public event Action<MappedInputType> OnPressed;

    public event Action<MappedInputType> OnReleased;

    private static MappedInputType? MapButton(GameControllerButton button)
    {
        return button switch
        {
            GameControllerButton.A => MappedInputType.Confirm,
            GameControllerButton.B => MappedInputType.Back,
            GameControllerButton.DpadUp => MappedInputType.Up,
            GameControllerButton.DpadDown => MappedInputType.Down,
            GameControllerButton.DpadLeft => MappedInputType.Left,
            GameControllerButton.DpadRight => MappedInputType.Right,
            GameControllerButton.Guide => MappedInputType.MainMenu,
            _ => null
        };
    }

    private static MappedInputType? MapKey(Key key)
    {
        return key switch
        {
            Key.Enter => MappedInputType.Confirm,
            Key.Back => MappedInputType.Back,
            Key.Up => MappedInputType.Up,
            Key.Down => MappedInputType.Down,
            Key.Left => MappedInputType.Left,
            Key.Right => MappedInputType.Right,
            Key.Escape => MappedInputType.MainMenu,
            _ => null
        };
    }

    private void ProcessPressed(MappedInputType inputType)
    {
        if (ActiveInputs.Contains(inputType)) return;
        ActiveInputs = ActiveInputs.Add(inputType);
        OnPressed?.Invoke(inputType);
    }

    private void ProcessReleased(MappedInputType inputType)
    {
        if (!ActiveInputs.Contains(inputType)) return;
        ActiveInputs = ActiveInputs.Remove(inputType);
        OnReleased?.Invoke(inputType);
    }

    private void ProcessEvent(Event ev, ref bool quit)
    {
        Log.Information("Processing event {Event}", (EventType)ev.Type);
        switch ((EventType)ev.Type)
        {
            case EventType.Quit:
                quit = true;
                break;
            case EventType.Controllerdeviceadded:
                var num = _sdl.NumJoysticks();
                for (var i = 0; i < num; i++)
                    if (_sdl.IsGameController(i) == SdlBool.True)
                        unsafe
                        {
                            var gc = _sdl.GameControllerOpen(i);

                            Log.Information("Game controller opened: {Name}",
                                Marshal.PtrToStringAnsi((IntPtr)_sdl.GameControllerName(gc)));
                        }

                break;

            case EventType.Controllerbuttondown:
            {
                var mapped = MapButton((GameControllerButton)ev.Cbutton.Button);
                if (mapped.HasValue)
                    ProcessPressed(mapped.Value);
                break;
            }
            case EventType.Controllerbuttonup:
            {
                var mapped = MapButton((GameControllerButton)ev.Cbutton.Button);
                if (mapped.HasValue)
                    ProcessReleased(mapped.Value);
                break;
            }
        }
    }

    public void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var mapped = MapKey(e.Key);
        if (!mapped.HasValue) return;
        ProcessPressed(mapped.Value);
    }

    public void OnKeyUp(object? sender, KeyEventArgs e)
    {
        var mapped = MapKey(e.Key);
        if (!mapped.HasValue) return;
        ProcessReleased(mapped.Value);
    }
}