using System.Diagnostics;

namespace Client.Game
{
    public class GameLoop
    {
        private readonly Form _form;
        private CancellationTokenSource? _cts;
        private Task? _loopTask;

        public GameLoop(Form form)
        {
            _form = form;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _loopTask = Task.Run(() => LoopAsync(_cts.Token));
        }

        private async Task LoopAsync(CancellationToken token)
        {
            const int frameTime = 24;
            var sw = new Stopwatch();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    sw.Restart();

                    if (!_form.IsDisposed && _form.Created)
                        _form.Invalidate();

                    int delay = frameTime - (int)sw.ElapsedMilliseconds;
                    if (delay > 0)
                        await Task.Delay(delay, token);
                }
            }
            catch (TaskCanceledException) { }
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts = null;
        }
    }
}
