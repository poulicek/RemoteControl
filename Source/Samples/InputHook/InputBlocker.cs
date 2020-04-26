using System;
using InputHook;

namespace InputHookWin
{
    public class InputBlocker : IDisposable
    {
        private KeyCombination controlKey;

        public event Action InputBlocked;
        public event Action<bool> BlockingStateChanged;

        public bool IsBlocking { get { return HooksManager.BlockInput; } }

        public KeyCombination ControlKey { get { return this.controlKey; } set { HooksManager.SetHooks(this.controlKey = value); } }

        public InputBlocker(KeyCombination controlKey)
        {
            HooksManager.InputBlocked += () => this.InputBlocked?.Invoke();
            HooksManager.KeyCombinationTriggered += this.onControlKeyTriggered;

            this.ControlKey = controlKey;
        }

        public void StartBlocking()
        {
            var state = HooksManager.BlockInput;
            HooksManager.SetHooks(this.controlKey, true);
            if (state != true)
                this.BlockingStateChanged?.Invoke(HooksManager.BlockInput);
        }


        public void StopBlocking()
        {
            var state = HooksManager.BlockInput;
            HooksManager.SetHooks(this.controlKey, false);
            if (state != false)
                this.BlockingStateChanged?.Invoke(HooksManager.BlockInput);
        }

        private void onControlKeyTriggered()
        {
            if (HooksManager.BlockInput)
                this.StopBlocking();
            else
                this.StartBlocking();
        }

        public void Dispose()
        {
            this.StopBlocking();
        }
    }
}
