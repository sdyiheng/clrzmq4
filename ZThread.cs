﻿namespace ZeroMQ
{
	using System;
	using System.Threading;
	using System.Collections.Generic;

	public abstract class ZThread : IZThread
	{
		protected CancellationTokenSource _cancellor;

		protected Thread _thread;

		protected bool _disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="ZThread"/> class.
		/// </summary>
		protected ZThread()
		{ }

		/// <summary>
		/// Finalizes an instance of the <see cref="ZThread"/> class.
		/// </summary>
		~ZThread()
		{
			Dispose(false);
		}

		/// <summary>
		/// Gets a value indicating whether the device loop is running.
		/// </summary>
		public bool IsCancellationRequested
		{
			get { return _cancellor.IsCancellationRequested; }
		}

		/// <summary>
		/// Start the device in the current thread.
		/// </summary>
		/// <exception cref="ObjectDisposedException">The <see cref="ZThread"/> has already been disposed.</exception>
		public virtual IZThread Start(CancellationTokenSource cancellor)
		{
			EnsureNotDisposed();

			_cancellor = cancellor;

			if (_thread == null)
			{
				_thread = new Thread(Run);
			}
			_thread.Start();

			return this;
		}

		/// <summary>
		/// Blocks the calling thread until the device terminates.
		/// </summary>
		public virtual void Join()
		{
			EnsureNotDisposed();

			_thread.Join();
		}

		/// <summary>
		/// Blocks the calling thread until the device terminates.
		/// </summary>
		public virtual bool Join(int ms)
		{
			EnsureNotDisposed();

			return _thread.Join(ms);
		}

		/// <summary>
		/// Blocks the calling thread until the device terminates or the specified time elapses.
		/// </summary>
		/// <param name="timeout">
		/// A <see cref="TimeSpan"/> set to the amount of time to wait for the device to terminate.
		/// </param>
		/// <returns>
		/// true if the device terminated; false if the device has not terminated after
		/// the amount of time specified by <paramref name="timeout"/> has elapsed.
		/// </returns>
		public virtual bool Join(TimeSpan timeout)
		{
			EnsureNotDisposed();

			return _thread.Join(timeout);
		}

		/// <summary>
		/// Stop the device in such a way that it can be restarted.
		/// </summary>
		public virtual IZThread Stop()
		{
			EnsureNotDisposed();

			_cancellor.Cancel();

			return this;
		}

		/// <summary>
		/// Stop the device and safely terminate the underlying sockets.
		/// </summary>
		public virtual IZThread Close()
		{
			EnsureNotDisposed();

			if (_thread.IsAlive)
			{
				_thread.Abort();
			}

			return this;
		}

		/// <summary>
		/// Releases all resources used by the current instance, including the frontend and backend sockets.
		/// </summary>
		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected abstract void Run();

		/// <summary>
		/// Stops the device and releases the underlying sockets. Optionally disposes of managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				if (_cancellor != null && !_cancellor.IsCancellationRequested)
				{
					_thread.Abort();
					_cancellor = null;
				}
			}

			_disposed = true;
		}

		protected void EnsureNotDisposed()
		{
			if (_disposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
		}
	}
}