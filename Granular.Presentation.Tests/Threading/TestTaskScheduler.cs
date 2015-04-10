﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Granular.Collections;
using System.Windows;

namespace Granular.Presentation.Tests.Threading
{
    public class TestTaskScheduler : ITaskScheduler
    {
        private class CancellableAction
        {
            private Action action;
            private bool isCancelled;

            public CancellableAction(Action action)
            {
                this.action = action;
            }

            public void Cancel()
            {
                isCancelled = true;
            }

            public void Invoke()
            {
                if (!isCancelled)
                {
                    action();
                }
            }
        }

        public TimeSpan CurrentTime { get; private set; }

        private PriorityQueue<TimeSpan, CancellableAction> queue;

        public TestTaskScheduler()
        {
            this.queue = new PriorityQueue<TimeSpan, CancellableAction>();
        }

        public IDisposable ScheduleTask(TimeSpan timeSpan, Action action)
        {
            CancellableAction cancellableAction = new CancellableAction(action);
            queue.Enqueue(CurrentTime + timeSpan, cancellableAction);
            return new Disposable(() => cancellableAction.Cancel());
        }

        public void AdvanceBy(TimeSpan timeSpan)
        {
            if (timeSpan < TimeSpan.Zero)
            {
                throw new Granular.Exception("Time must be positive");
            }

            AdvanceTo(CurrentTime + timeSpan);
        }

        public void AdvanceTo(TimeSpan time)
        {
            if (time < CurrentTime)
            {
                throw new Granular.Exception("Time must larger than CurrentTime");
            }

            while (queue.Count > 0 && queue.First().Key <= time)
            {
                CurrentTime = queue.First().Key;
                DequeueDueOperations();
            }

            CurrentTime = time;
        }

        private void DequeueDueOperations()
        {
            while (queue.Count > 0 && queue.First().Key <= CurrentTime)
            {
                queue.Dequeue().Invoke();
            }
        }
    }
}
