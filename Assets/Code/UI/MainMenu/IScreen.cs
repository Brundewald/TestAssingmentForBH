using System;

namespace Code.UI
{
    public interface IScreen
    {
        event Action<bool> SwitchScreen;

        void ChangeScreenState(bool state);
    }
}