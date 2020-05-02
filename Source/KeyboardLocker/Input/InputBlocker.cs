using System;

namespace KeyboardLocker.Input
{
    public class InputBlocker : IDisposable
    {
        public event Action ScreenOffRequested;
        public event Action<bool> BlockingStateChanged;

        public bool IsBlocking { get { return HooksManager.BlockInput; } }

        public ActionKey BlockingKey { get; private set; }
        public ActionKey UnblockingKey { get; private set; }

        public InputBlocker(ActionKey blockingKey, ActionKey unblockingKey)
        {
            this.BlockingKey = blockingKey;
            this.BlockingKey.Pressed += this.StartBlocking;
            this.BlockingKey.Released += this.TurnScreenOff;
            this.BlockingKey.LongPress += this.onBlockingKeyLongPress;

            this.UnblockingKey = unblockingKey;
            this.UnblockingKey.Pressed += this.StopBlocking;

            this.setBlockingState(false);
            
        }

        private void onBlockingKeyLongPress()
        {
            this.ScreenOffRequested?.Invoke();
        }

        private void setBlockingState(bool blockInput)
        {
            HooksManager.SetHooks(blockInput, this.BlockingKey, this.UnblockingKey);
        }

        public bool TurnScreenOff()
        {
            if (this.BlockingKey?.WasLongPressed != true)
                return false;

            this.StartBlocking();

#if !DEBUG
            SystemControls.TurnOffScreen();
#endif
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
