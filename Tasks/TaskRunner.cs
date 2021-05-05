using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sayer.Tasks
{
    /// <summary>
    /// Provides the ability run an arbitrary number of tasks, but with a limit on the number of these tasks that can be
    /// concurrently running at any one moment.
    /// </summary>
    public class TaskRunner<T>
    {
        /// <summary>
        /// Constructs a TaskRunner
        /// </summary>
        /// <param name="maxConcurrent">
        /// The maximum number of tasks to run concurrently
        /// </param>
        public TaskRunner(int maxConcurrent)
        {
            if (maxConcurrent < 1)
            {
                throw new ArgumentException("maxConcurrent must be > 0", nameof(maxConcurrent));
            }

            MaxConcurrent = maxConcurrent;
            _tasks = new List<Task<T>>(maxConcurrent);
        }

        /// <summary>
        /// The maximum number of tasks allowed to be run concurrently by this task runner.
        /// </summary>
        public int MaxConcurrent { get; }

        /// <summary>
        /// Adds a task.
        /// </summary>
        /// <param name="task">
        /// If this task does significant synchronous work before doing anything asynchronous,
        /// consider wrapping the task in Task.Run to take better advantage of concurrency.
        ///
        /// The task must be either running or scheduled to run. (It's uncommon to create a task in any other state.)
        /// </param>
        /// <returns>
        /// Returns a task that will complete when there are less than the maximum number of concurrent tasks currently executing.
        /// Be sure to wait for the returned task to complete before invoking any more method calls upon this TaskRunner instance.
        /// Not doing so is not thread-safe.
        ///
        /// This method will throw an exception if, after waiting for less than the maximum number of concurrent tasks to be currently
        /// executing, the task that completes failed. 
        /// </returns>
        public async Task Add(Task<T> task)
        {
            // Usage for this class only makes sense when tasks are added sequentially. Thread-safety with regards
            // to the _tasks member variable thus is not a concern.
            _tasks.Add(task);

            // It is a mis-usage of this class not to wait upon the returned task.
            if (_tasks.Count == MaxConcurrent)
            {
                Task<T> completed = await Task<T>.WhenAny(_tasks).ConfigureAwait(false);
                _tasks.Remove(completed);

                // If the completed task failed, the line below will cause the exception to get thrown (WhenAny does not throw)
                await completed.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Adds a task.
        /// </summary>
        /// <param name="taskFunc">
        /// If this does significant synchronous work before doing anything asynchronous,
        /// it is recommended that you instead wrap the procedure in Task.Run to take better advantage of concurrency.
        ///
        /// The returned task must be either running or scheduled to run. (It's uncommon to create a task in any other state.)
        /// </param>
        /// <returns>
        /// Returns a task that will complete when there are less than the maximum number of concurrent tasks currently executing.
        /// Be sure to wait for the returned task to complete before invoking any more method calls upon this TaskRunner instance.
        /// Not doing so is not thread-safe.
        ///
        /// This method will throw an exception if, after waiting for less than the maximum number of concurrent tasks to be currently
        /// executing, the task that completes failed. 
        /// </returns>
        public Task Add(Func<Task<T>> taskFunc) => Add(taskFunc());

        /// <summary>
        /// Waits for all tasks passed to the Add() method to complete. It is important to call
        /// this method (and wait upon it) after all tasks have been added.
        /// </summary>
        /// <returns></returns>
        public Task<T[]> WhenAll() => Task.WhenAll(_tasks);

        public Task<Task<T>> WhenAny() => Task.WhenAny(_tasks);

        private readonly List<Task<T>> _tasks;
    }

    /// <summary>
    /// Provides the ability run an arbitrary number of tasks, but with a limit on the number of these tasks that can be
    /// concurrently running at any one moment. This is a more lightweight alternative to TaskLimit/CPULimit.
    /// </summary>
    public class TaskRunner
    {
        /// <summary>
        /// Waits for all of the provided enumerable tasks to complete, limiting the number of concurrent tasks to a specified value.
        /// This static method serves as a wrapper around using the TaskRunner class.
        /// </summary>
        /// <param name="tasks">
        /// The tasks to enumerate. Note that if the tasks have all been created already (for example, added to a List),
        /// this method will not be able to enforce a maximum number of concurrent tasks. Proper usage is to to make this
        /// enumerable be lazy-evaluated (for example, via yield return statements).
        ///
        /// If a task returned from this enumerable does significant synchronous work before doing anything asynchronous,
        /// it is recommended that you wrap that task in Task.Run to take better advantage of concurrency.
        ///
        /// Any task returned from this enumerable must be either running or scheduled to run. (It's very uncommon to create
        /// a task in any other state.)
        /// </param>
        /// <param name="maxConcurrent">
        /// The maximum number of tasks to run concurrently
        /// </param>
        /// <returns></returns>
        public static async Task WhenAll(IEnumerable<Task> tasks, int maxConcurrent)
        {
            var taskRunner = new TaskRunner(maxConcurrent);
            foreach (Task task in tasks)
            {
                await taskRunner.Add(task).ConfigureAwait(false);
            }

            await taskRunner.WhenAll().ConfigureAwait(false);
        }

        /// <summary>
        /// Constructs a TaskRunner
        /// </summary>
        /// <param name="maxConcurrent">
        /// The maximum number of tasks to run concurrently
        /// </param>
        public TaskRunner(int maxConcurrent)
        {
            if (maxConcurrent < 1)
            {
                throw new ArgumentException("maxConcurrent must be > 0", nameof(maxConcurrent));
            }

            MaxConcurrent = maxConcurrent;
            _tasks = new List<Task>(maxConcurrent);
        }

        /// <summary>
        /// The maximum number of tasks allowed to be run concurrently by this task runner.
        /// </summary>
        public int MaxConcurrent { get; }

        /// <summary>
        /// Adds a task.
        /// </summary>
        /// <param name="task">
        /// If this task does significant synchronous work before doing anything asynchronous,
        /// it is recommended that you instead wrap the task in Task.Run to take better advantage of concurrency.
        ///
        /// The task must be either running or scheduled to run. (It's uncommon to create a task in any other state.)
        /// </param>
        /// <returns>
        /// Returns a task that will complete when there are less than the maximum number of concurrent tasks currently executing.
        /// Be sure to wait for the returned task to complete before invoking any more method calls upon this TaskRunner instance.
        /// Not doing so is not thread-safe.
        ///
        /// This method will throw an exception if, after waiting for less than the maximum number of concurrent tasks to be currently
        /// executing, the task that completes failed. 
        /// </returns>
        public async Task Add(Task task)
        {
            // Usage for this class only makes sense when tasks are added sequentially. Thread-safety with regards
            // to the _tasks member variable thus is not a concern.
            _tasks.Add(task);

            // It is a mis-usage of this class not to wait upon the returned task.
            if (_tasks.Count == MaxConcurrent)
            {
                Task completed = await Task.WhenAny(_tasks).ConfigureAwait(false);
                _tasks.Remove(completed);

                // If the completed task failed, the line below will cause the exception to get thrown (WhenAny does not throw)
                await completed.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Adds a task.
        /// </summary>
        /// <param name="taskFunc">
        /// If this does significant synchronous work before doing anything asynchronous,
        /// it is recommended that you instead wrap the procedure in Task.Run to take better advantage of concurrency.
        ///
        /// The returned task must be either running or scheduled to run. (It's uncommon to create a task in any other state.)
        /// </param>
        /// <returns>
        /// Returns a task that will complete when there are less than the maximum number of concurrent tasks currently executing.
        /// Be sure to wait for the returned task to complete before invoking any more method calls upon this TaskRunner instance.
        /// Not doing so is not thread-safe.
        ///
        /// This method will throw an exception if, after waiting for less than the maximum number of concurrent tasks to be currently
        /// executing, the task that completes failed. 
        /// </returns>
        public Task Add(Func<Task> taskFunc) => Add(taskFunc());

        /// <summary>
        /// Waits for all tasks passed to the Add() method to complete. It is important to call
        /// this method (and wait upon it) after all tasks have been added.
        /// </summary>
        /// <returns></returns>
        public Task WhenAll() => Task.WhenAll(_tasks);

        public Task<Task> WhenAny() => Task.WhenAny(_tasks);

        private readonly List<Task> _tasks;
    }
}
