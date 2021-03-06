﻿using System;
using System.Threading;

namespace Reloaded.Mod.Launcher.Utility
{
    /// <summary>
    /// A utility class that allows for the setting of <see cref="Action"/>s to execute code at certain intervals.
    /// It uses a clock (timer). When the timer elapses the current action is executed and set to null.
    /// </summary>
    public class CancellableExecuteActionTimer : IDisposable
    {
        /// <summary>
        /// The current action to execute.
        /// </summary>
        private Action<CancellationToken> _actionToExecute;

        /// <summary>
        /// The timer used to execute the ticks.
        /// </summary>
        private Timer _timer;

        /// <summary>
        /// The token source for the token used to cancel the executed events.
        /// </summary>
        private CancellationTokenSource _tokenSource;

        private object _lock = new object();
        private TimeSpan _currentTickDuration;

        /// <summary>
        /// Creates an <see cref="CancellableExecuteActionTimer"/> given the length of time between ticks.
        /// </summary>
        /// <param name="delayMilliseconds">Amount of milliseconds between individual ticks.</param>
        public CancellableExecuteActionTimer(int delayMilliseconds)
        {
            _currentTickDuration = TimeSpan.FromMilliseconds(delayMilliseconds);
            _timer = new Timer(Tick, null, TimeSpan.Zero, _currentTickDuration);
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sets the action to be executed on next tick.
        /// </summary>
        public void SetAction(Action<CancellationToken> action)
        {
            _tokenSource.Cancel();
            _actionToExecute = action;
        }

        /// <summary>
        /// Sets the action to be executed on next tick and resets the timer.
        /// </summary>
        public void SetActionAndReset(Action<CancellationToken> action)
        {
            _tokenSource.Cancel();
            _timer.Change(_currentTickDuration, _currentTickDuration);
            _actionToExecute = action;
        }

        /// <summary>
        /// Sets the time taken between each successive tick.
        /// </summary>
        public void SetTickDuration(TimeSpan timeSpan)
        {
            _currentTickDuration = timeSpan;
            _timer.Change(TimeSpan.Zero, timeSpan);
        }

        /* Tick Code */
        private void Tick(object state)
        {
            lock (_lock)
            {
                _tokenSource = new CancellationTokenSource();
                _actionToExecute?.Invoke(_tokenSource.Token);
                _actionToExecute = null;
            }
        }
    }
}
