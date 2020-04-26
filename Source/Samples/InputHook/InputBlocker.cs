using InputHook;

namespace InputHookWin
{
    public class InputBlocker
    {
        private KeyCombination controlKey;

        public bool IsBlocking { get { return HooksManager.InputBlocked; } }

        public KeyCombination ControlKey { get { return this.controlKey; } set { HooksManager.SetHooks(this.controlKey = value); } }

        public InputBlocker(KeyCombination controlKey)
        {
            HooksManager.KeyCombinationTriggered += this.onControlKeyTriggered;
            this.ControlKey = controlKey;
        }

        public void StartBlocking()
        {
            HooksManager.SetHooks(this.controlKey, true);
        }


        public void StopBlocking()
        {
            HooksManager.SetHooks(this.controlKey, false);
        }

        private void onControlKeyTriggered()
        {
            if (HooksManager.InputBlocked)
                this.StopBlocking();
            else
                this.StartBlocking();
        }
    }
}
