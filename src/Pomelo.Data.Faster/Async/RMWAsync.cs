﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pomelo.Data.Faster
{
    public partial class FasterKV<Key, Value> : FasterBase, IFasterKV<Key, Value>
    {
        internal struct RmwAsyncOperation<Input, Output, Context> : IUpdateAsyncOperation<Input, Output, Context, RmwAsyncResult<Input, Output, Context>>
        {
            AsyncIOContext<Key, Value> diskRequest;

            internal RmwAsyncOperation(AsyncIOContext<Key, Value> diskRequest) => this.diskRequest = diskRequest;

            /// <inheritdoc/>
            public RmwAsyncResult<Input, Output, Context> CreateResult(Status status, Output output) => new(status, output);

            /// <inheritdoc/>
            public Status DoFastOperation(FasterKV<Key, Value> fasterKV, ref PendingContext<Input, Output, Context> pendingContext, IFasterSession<Key, Value, Input, Output, Context> fasterSession,
                                            FasterExecutionContext<Input, Output, Context> currentCtx, bool asyncOp, out CompletionEvent flushEvent, out Output output)
            {
                flushEvent = fasterKV.hlog.FlushEvent;
                Status status = !this.diskRequest.IsDefault()
                    ? fasterKV.InternalCompletePendingRequestFromContext(currentCtx, currentCtx, fasterSession, this.diskRequest, ref pendingContext, asyncOp, out AsyncIOContext<Key, Value> newDiskRequest)
                    : fasterKV.CallInternalRMW(fasterSession, currentCtx, ref pendingContext, ref pendingContext.key.Get(), ref pendingContext.input.Get(), ref pendingContext.output, pendingContext.userContext,
                                                     pendingContext.serialNum, asyncOp, out flushEvent, out newDiskRequest);
                output = pendingContext.output;

                if (status == Status.PENDING && !newDiskRequest.IsDefault())
                {
                    flushEvent = default;
                    this.diskRequest = newDiskRequest;
                }
                return status;
            }

            /// <inheritdoc/>
            public ValueTask<RmwAsyncResult<Input, Output, Context>> DoSlowOperation(FasterKV<Key, Value> fasterKV, IFasterSession<Key, Value, Input, Output, Context> fasterSession,
                                            FasterExecutionContext<Input, Output, Context> currentCtx, PendingContext<Input, Output, Context> pendingContext, CompletionEvent flushEvent, CancellationToken token)
                => SlowRmwAsync(fasterKV, fasterSession, currentCtx, pendingContext, diskRequest, flushEvent, token);

            /// <inheritdoc/>
            public bool CompletePendingIO(IFasterSession<Key, Value, Input, Output, Context> fasterSession)
            {
                if (this.diskRequest.IsDefault())
                    return false;

                // CompletePending() may encounter OperationStatus.ALLOCATE_FAILED; if so, we don't have a more current flushEvent to pass back.
                fasterSession.CompletePendingWithOutputs(out var completedOutputs, wait: true, spinWaitForCommit: false);
                var status = completedOutputs.Next() ? completedOutputs.Current.Status : Status.ERROR;
                completedOutputs.Dispose();
                return status != Status.PENDING;
            }

            /// <inheritdoc/>
            public void DecrementPending(FasterExecutionContext<Input, Output, Context> currentCtx, ref PendingContext<Input, Output, Context> pendingContext)
            {
                currentCtx.ioPendingRequests.Remove(pendingContext.id);
                currentCtx.asyncPendingCount--;
                currentCtx.pendingReads.Remove();
            }
        }

        /// <summary>
        /// State storage for the completion of an async RMW, or the result if the RMW was completed synchronously
        /// </summary>
        public struct RmwAsyncResult<Input, TOutput, Context>
        {
            internal readonly UpdateAsyncInternal<Input, TOutput, Context, RmwAsyncOperation<Input, TOutput, Context>, RmwAsyncResult<Input, TOutput, Context>> updateAsyncInternal;

            /// <summary>Current status of the RMW operation</summary>
            public Status Status { get; }

            /// <summary>Output of the RMW operation if current status is not <see cref="Status.PENDING"/></summary>
            public TOutput Output { get; }

            internal RmwAsyncResult(Status status, TOutput output)
            {
                this.Status = status;
                this.Output = output;
                this.updateAsyncInternal = default;
            }

            internal RmwAsyncResult(FasterKV<Key, Value> fasterKV, IFasterSession<Key, Value, Input, TOutput, Context> fasterSession,
                FasterExecutionContext<Input, TOutput, Context> currentCtx, PendingContext<Input, TOutput, Context> pendingContext,
                AsyncIOContext<Key, Value> diskRequest, ExceptionDispatchInfo exceptionDispatchInfo)
            {
                Status = Status.PENDING;
                this.Output = default;
                updateAsyncInternal = new UpdateAsyncInternal<Input, TOutput, Context, RmwAsyncOperation<Input, TOutput, Context>, RmwAsyncResult<Input, TOutput, Context>>(
                                        fasterKV, fasterSession, currentCtx, pendingContext, exceptionDispatchInfo, new RmwAsyncOperation<Input, TOutput, Context>(diskRequest));
            }

            /// <summary>Complete the RMW operation, issuing additional (rare) I/O asynchronously if needed. It is usually preferable to use Complete() instead of this.</summary>
            /// <returns>ValueTask for RMW result. User needs to await again if result status is <see cref="Status.PENDING"/>.</returns>
            public ValueTask<RmwAsyncResult<Input, TOutput, Context>> CompleteAsync(CancellationToken token = default) 
                => this.Status != Status.PENDING
                    ? new ValueTask<RmwAsyncResult<Input, TOutput, Context>>(new RmwAsyncResult<Input, TOutput, Context>(this.Status, this.Output))
                    : updateAsyncInternal.CompleteAsync(token);

            /// <summary>Complete the RMW operation, issuing additional (rare) I/O synchronously if needed.</summary>
            /// <returns>Status of RMW operation</returns>
            public (Status status, TOutput output) Complete()
            {
                if (this.Status != Status.PENDING)
                    return (this.Status, this.Output);
                var rmwAsyncResult = updateAsyncInternal.Complete();
                return (rmwAsyncResult.Status, rmwAsyncResult.Output);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ValueTask<RmwAsyncResult<Input, Output, Context>> RmwAsync<Input, Output, Context>(IFasterSession<Key, Value, Input, Output, Context> fasterSession,
            FasterExecutionContext<Input, Output, Context> currentCtx, ref Key key, ref Input input, Context context, long serialNo, CancellationToken token = default)
        {
            var pcontext = default(PendingContext<Input, Output, Context>);
            pcontext.IsAsync = true;
            return RmwAsync(fasterSession, currentCtx, ref pcontext, ref key, ref input, context, serialNo, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ValueTask<RmwAsyncResult<Input, Output, Context>> RmwAsync<Input, Output, Context>(IFasterSession<Key, Value, Input, Output, Context> fasterSession,
            FasterExecutionContext<Input, Output, Context> currentCtx, ref PendingContext<Input, Output, Context> pcontext, ref Key key, ref Input input, Context context, long serialNo, CancellationToken token)
        {
            var diskRequest = default(AsyncIOContext<Key, Value>);
            CompletionEvent flushEvent;

            fasterSession.UnsafeResumeThread();
            try
            {
                Output output = default;
                var status = CallInternalRMW(fasterSession, currentCtx, ref pcontext, ref key, ref input, ref output, context, serialNo, asyncOp: true, out flushEvent, out diskRequest);
                if (status != Status.PENDING)
                    return new ValueTask<RmwAsyncResult<Input, Output, Context>>(new RmwAsyncResult<Input, Output, Context>(status, output));
            }
            finally
            {
                Debug.Assert(serialNo >= currentCtx.serialNum, "Operation serial numbers must be non-decreasing");
                currentCtx.serialNum = serialNo;
                fasterSession.UnsafeSuspendThread();
            }

            return SlowRmwAsync(this, fasterSession, currentCtx, pcontext, diskRequest, flushEvent, token);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Status CallInternalRMW<Input, Output, Context>(IFasterSession<Key, Value, Input, Output, Context> fasterSession,
            FasterExecutionContext<Input, Output, Context> currentCtx, ref PendingContext<Input, Output, Context> pcontext, ref Key key, ref Input input, ref Output output, Context context, long serialNo,
            bool asyncOp, out CompletionEvent flushEvent, out AsyncIOContext<Key, Value> diskRequest)
        {
            diskRequest = default;
            OperationStatus internalStatus;
            do
            {
                flushEvent = hlog.FlushEvent;
                internalStatus = InternalRMW(ref key, ref input, ref output, ref context, ref pcontext, fasterSession, currentCtx, serialNo);
            } while (internalStatus == OperationStatus.RETRY_NOW || internalStatus == OperationStatus.RETRY_LATER);

            if (internalStatus == OperationStatus.SUCCESS || internalStatus == OperationStatus.NOTFOUND)
                return (Status)internalStatus;
            if (internalStatus == OperationStatus.ALLOCATE_FAILED)
                return Status.PENDING;    // This plus diskRequest.IsDefault() means allocate failed

            flushEvent = default;
            return HandleOperationStatus(currentCtx, currentCtx, ref pcontext, fasterSession, internalStatus, asyncOp, out diskRequest);
        }

        private static async ValueTask<RmwAsyncResult<Input, Output, Context>> SlowRmwAsync<Input, Output, Context>(
            FasterKV<Key, Value> @this, IFasterSession<Key, Value, Input, Output, Context> fasterSession,
            FasterExecutionContext<Input, Output, Context> currentCtx, PendingContext<Input, Output, Context> pcontext,
            AsyncIOContext<Key, Value> diskRequest, CompletionEvent flushEvent, CancellationToken token = default)
        {
            currentCtx.asyncPendingCount++;
            currentCtx.pendingReads.Add();

            ExceptionDispatchInfo exceptionDispatchInfo = default;
            try
            {
                token.ThrowIfCancellationRequested();

                if (@this.epoch.ThisInstanceProtected())
                    throw new NotSupportedException("Async operations not supported over protected epoch");

                // If we are here because of flushEvent, then _diskRequest is default--there is no pending disk operation.
                if (diskRequest.IsDefault())
                    await flushEvent.WaitAsync(token).ConfigureAwait(false);
                else
                {
                    using (token.Register(() => diskRequest.asyncOperation.TrySetCanceled()))
                        diskRequest = await diskRequest.asyncOperation.Task.WithCancellationAsync(token).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                exceptionDispatchInfo = ExceptionDispatchInfo.Capture(e);
            }

            return new RmwAsyncResult<Input, Output, Context>(@this, fasterSession, currentCtx, pcontext, diskRequest, exceptionDispatchInfo);
        }
    }
}
