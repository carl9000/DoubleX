﻿namespace UTH.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using UTH.Infrastructure.Resource;
    using UTH.Infrastructure.Resource.Culture;
    using UTH.Infrastructure.Utility;

    /// <summary>
    /// 工作单元
    /// </summary>
    public abstract class UnitOfWorkBase : IUnitOfWork
    {
        private bool _isBeginCalledBefore;
        private bool _isCompleteCalledBefore;
        private bool _succeed;
        private Exception _exception;
        private IUnitOfWork _outer;

        /// <summary>
        /// 单元标识
        /// </summary>
        public string Id { get; private set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 数据上下文
        /// </summary>
        public abstract dynamic Context { get;  set; }
        
        /// <summary>
        /// 配置信息
        /// </summary>
        public UnitOfWorkOptions Options { get; private set; }

        /// <summary>
        /// 是否释放
        /// </summary>
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// 完成事件
        /// </summary>
        public event EventHandler Completed;

        /// <summary>
        /// 失败事件
        /// </summary>
        public event EventHandler<UnitOfWorkFailedEventArgs> Failed;

        /// <summary>
        /// 释放事件
        /// </summary>
        public event EventHandler Disposed;

        public virtual void Begin(UnitOfWorkOptions options)
        {
            options.IsNull();
            Options = options; //TODO: Do not set options like that, instead make a copy?
            PreventMultipleBegin();
            BeginUow();
        }

        public abstract void SaveChanges();
        public abstract Task SaveChangesAsync();

        public void Complete()
        {
            PreventMultipleComplete();
            try
            {
                CompleteUow();
                _succeed = true;
                OnCompleted();
            }
            catch (Exception ex)
            {
                _exception = ex;
                throw;
            }
        }
        public async Task CompleteAsync()
        {
            PreventMultipleComplete();
            try
            {
                await CompleteUowAsync();
                _succeed = true;
                OnCompleted();
            }
            catch (Exception ex)
            {
                _exception = ex;
                throw;
            }
        }

        public void Dispose()
        {
            if (!_isBeginCalledBefore || IsDisposed)
            {
                return;
            }

            IsDisposed = true;

            if (!_succeed)
            {
                OnFailed(_exception);
            }

            DisposeUow();
            OnDisposed();
        }

        protected virtual void BeginUow()
        {

        }
        protected abstract void CompleteUow();
        protected abstract Task CompleteUowAsync();
        protected abstract void DisposeUow();

        protected virtual void OnCompleted()
        {
            Completed.InvokeSafely(this);
        }
        protected virtual void OnFailed(Exception exception)
        {
            Failed.InvokeSafely(this, new UnitOfWorkFailedEventArgs(exception));
        }
        protected virtual void OnDisposed()
        {
            Disposed.InvokeSafely(this);
        }

        private void PreventMultipleBegin()
        {
            if (_isBeginCalledBefore)
            {
                throw new DbxException(EnumCode.提示消息, "当前工作单元已启动，不能多次调用Begin 方法");
            }

            _isBeginCalledBefore = true;
        }
        private void PreventMultipleComplete()
        {
            if (_isCompleteCalledBefore)
            {
                throw new DbxException(EnumCode.提示消息, "Complete is called before!");
            }

            _isCompleteCalledBefore = true;
        }

        public IUnitOfWork GetOuter()
        {
            return _outer;
        }
        public void SetOuter(IUnitOfWork outer)
        {
            _outer = outer;
        }
    }
}