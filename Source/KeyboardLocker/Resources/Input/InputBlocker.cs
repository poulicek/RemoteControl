using System;

namespace KeyboardLocker.Input
{
    public class InputBlocker : IDisposable
    {
        public event Action<bool> BlockingStateChanged;

        public bool IsBlocking { get { return HooksManager.BlockInput; } }

        public KeyCombination BlockingKey { get; private set; }
        public KeyCombination UnblockingKey { get; private set; }
        public KeyCombination ScreenOffKey { get; private set; }

        public InputBlocker(KeyCombination blockingKey, KeyCombination unblockingKey, KeyCombination screenOffKey)
        {
            this.BlockingKey = blockingKey;
            this.BlockingKey.Pressed += this.StartBlocking;

            this.UnblockingKey = unblockingKey;
            this.UnblockingKey.Pressed += this.StopBlocking;

            this.ScreenOffKey = screenOffKey;
            this.ScreenOffKey.Pressed += this.TurnScreenOff;

            this.setBlockingState(false);
            
        }

        private void setBlockingState(bool blockInput)
        {
            HooksManager.SetHooks(blockInput, this.BlockingKey, this.UnblockingKey, this.ScreenOffKey);
        }

        public bool TurnScreenOff()
        {
            if (!this.StopBlocking())
                return false;

            SystemControls.TurnOffScreen();
            return true;
        }

        public bool StartBlocking()
        {
            if (HooksManager.BlockInput)
                return false;

            this.setBlockingState(true);
            this.BlockingStateChanged?.Invoke(HooksManager.BlockInput);
            return true;
        }


        public bool StopBlocking()
        {
            if (!HooksManager.BlockInput)
                return false;

            this.setBlockingState(false);
            this.BlockingStateChanged?.Invoke(HooksManager.BlockInput);
            return true;
        }

        public void Dispose()
        {
            this.StopBlocking();
        }
    }
}
