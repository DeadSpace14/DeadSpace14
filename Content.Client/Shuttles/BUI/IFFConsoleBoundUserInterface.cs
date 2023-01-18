using Content.Client.Shuttles.UI;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Events;
using JetBrains.Annotations;
using Robust.Client.GameObjects;

namespace Content.Client.Shuttles.BUI;

[UsedImplicitly]
public sealed class IFFConsoleBoundUserInterface : BoundUserInterface
{
    private IFFConsoleWindow? _window;

    public IFFConsoleBoundUserInterface(ClientUserInterfaceComponent owner, Enum uiKey) : base(owner, uiKey)
    {

    }

    protected override void Open()
    {
        base.Open();
        _window = new IFFConsoleWindow();
        _window.OnClose += Close;
        _window.ShowIFF += SendIFFMessage;
        _window.ShowVessel += SendVesselMessage;
        _window.OpenCenteredLeft();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not IFFConsoleBoundUserInterfaceState bState)
            return;

        _window?.UpdateState(bState);
    }

    private void SendIFFMessage(bool obj)
    {
        SendMessage(new IFFShowIFFMessage()
        {
            Show = obj,
        });
    }

    private void SendVesselMessage(bool obj)
    {
        SendMessage(new IFFShowVesselMessage()
        {
            Show = obj,
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _window?.Close();
            _window = null;
        }
    }
}
