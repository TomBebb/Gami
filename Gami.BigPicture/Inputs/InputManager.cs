using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;
using Serilog;
using Silk.NET.SDL;

namespace Gami.BigPicture.Inputs;

public sealed class InputManager : IDisposable
{
    private readonly Sdl _sdl = Sdl.GetApi();
    private bool _isDisposed;

    public bool DidConfirm;


    public InputManager()
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

    [Reactive]
    public ImmutableHashSet<MappedInputType> ActiveInputs { get; private set; } =
        ImmutableHashSet<MappedInputType>.Empty;

    public void Dispose()
    {
        _isDisposed = true;
    }

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

    public void ProcessEvent(Event ev, ref bool quit)
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
                if (mapped.HasValue) ActiveInputs = ActiveInputs.Add(mapped.Value);
                Log.Debug("Button down: {Ty}; Pressed: {Pressed}; Mapped: {Mapped}", (GameControllerButton)ev.Cbutton.Button,
                    ActiveInputs, mapped);
                if (mapped == MappedInputType.Confirm)
                    DidConfirm = true;
                break;
            }
            case EventType.Controllerbuttonup:
            {
                var mapped = MapButton((GameControllerButton)ev.Cbutton.Button);
                if (mapped.HasValue) ActiveInputs = ActiveInputs.Remove(mapped.Value);
                Log.Debug("Button up: {Ty}; Pressed: {Pressed}", (GameControllerButton)ev.Cbutton.Button,
                    ActiveInputs);
                break;
            }
        }
    }
}