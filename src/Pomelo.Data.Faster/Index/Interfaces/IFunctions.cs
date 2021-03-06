// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace Pomelo.Data.Faster
{
    /// <summary>
    /// Callback functions to FASTER
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    /// <typeparam name="Value"></typeparam>
    /// <typeparam name="Input"></typeparam>
    /// <typeparam name="Output"></typeparam>
    /// <typeparam name="Context"></typeparam>
    public interface IFunctions<Key, Value, Input, Output, Context>
    {
        /// <summary>
        /// Read completion
        /// </summary>
        /// <param name="key">The key for this record</param>
        /// <param name="input">The user input that was used in the read operation</param>
        /// <param name="output">The result of the read operation; if this is a struct, then it will be a temporary and should be copied to <paramref name="ctx"/></param>
        /// <param name="ctx">The application context passed through the pending operation</param>
        /// <param name="status">The result of the pending operation</param>
        void ReadCompletionCallback(ref Key key, ref Input input, ref Output output, Context ctx, Status status);

        /// <summary>
        /// Upsert completion
        /// </summary>
        /// <param name="key">The key for this record</param>
        /// <param name="value">The value passed to Upsert</param>
        /// <param name="ctx">The application context passed through the pending operation</param>
        void UpsertCompletionCallback(ref Key key, ref Value value, Context ctx);

        /// <summary>
        /// RMW completion
        /// </summary>
        /// <param name="key">The key for this record</param>
        /// <param name="input">The user input that was used to perform the modification</param>
        /// <param name="output">The result of the RMW operation; if this is a struct, then it will be a temporary and should be copied to <paramref name="ctx"/></param>
        /// <param name="ctx">The application context passed through the pending operation</param>
        /// <param name="status">The result of the pending operation</param>
        void RMWCompletionCallback(ref Key key, ref Input input, ref Output output, Context ctx, Status status);

        /// <summary>
        /// Delete completion
        /// </summary>
        /// <param name="key">The key for this record</param>
        /// <param name="ctx">The application context passed through the pending operation</param>
        void DeleteCompletionCallback(ref Key key, Context ctx);

        /// <summary>
        /// Checkpoint completion callback (called per client session)
        /// </summary>
        /// <param name="sessionId">Session ID reporting persistence</param>
        /// <param name="commitPoint">Commit point descriptor</param>
        void CheckpointCompletionCallback(string sessionId, CommitPoint commitPoint);

        /// <summary>
        /// Initial update for RMW (insert at the tail of the log).
        /// </summary>
        /// <param name="key">The key for this record</param>
        /// <param name="input">The user input to be used for computing the updated <paramref name="value"/></param>
        /// <param name="value">The destination to be updated; because this is an insert, there is no previous value there.</param>
        /// <param name="output">The location where the result of the <paramref name="input"/> operation on <paramref name="value"/> is to be copied</param>
        void InitialUpdater(ref Key key, ref Input input, ref Value value, ref Output output);

        /// <summary>
        /// Whether we need to invoke copy-update for RMW
        /// </summary>
        /// <param name="key">The key for this record</param>
        /// <param name="input">The user input to be used for computing the updated value</param>
        /// <param name="oldValue">The existing value that would be copied.</param>
        /// <param name="output">The location where the result of the <paramref name="input"/> operation on <paramref name="oldValue"/> is to be copied</param>
        bool NeedCopyUpdate(ref Key key, ref Input input, ref Value oldValue, ref Output output)
#if NETSTANDARD2_1 || NET
            => true
#endif
            ;

        /// <summary>
        /// Copy-update for RMW (RCU to the tail of the log)
        /// </summary>
        /// <param name="key">The key for this record</param>
        /// <param name="input">The user input to be used for computing <paramref name="newValue"/> from <paramref name="oldValue"/></param>
        /// <param name="oldValue">The previous value to be copied/updated</param>
        /// <param name="newValue">The destination to be updated; because this is an copy to a new location, there is no previous value there.</param>
        /// <param name="output">The location where <paramref name="newValue"/> is to be copied</param>
        void CopyUpdater(ref Key key, ref Input input, ref Value oldValue, ref Value newValue, ref Output output);

        /// <summary>
        /// In-place update for RMW
        /// </summary>
        /// <param name="key">The key for this record</param>
        /// <param name="input">The user input to be used for computing the updated <paramref name="value"/></param>
        /// <param name="value">The destination to be updated; because this is an in-place update, there is a previous value there.</param>
        /// <param name="output">The location where the result of the <paramref name="input"/> operation on <paramref name="value"/> is to be copied</param>
        bool InPlaceUpdater(ref Key key, ref Input input, ref Value value, ref Output output);

        /// <summary>
        /// Non-concurrent reader. 
        /// </summary>
        /// <param name="key">The key for the record to be read</param>
        /// <param name="input">The user input for computing <paramref name="dst"/> from <paramref name="value"/></param>
        /// <param name="value">The value for the record being read</param>
        /// <param name="dst">The location where <paramref name="value"/> is to be copied</param>
        void SingleReader(ref Key key, ref Input input, ref Value value, ref Output dst);

        /// <summary>
        /// Conncurrent reader
        /// </summary>
        /// <param name="key">The key for the record to be read</param>
        /// <param name="input">The user input for computing <paramref name="dst"/> from <paramref name="value"/></param>
        /// <param name="value">The value for the record being read</param>
        /// <param name="dst">The location where <paramref name="value"/> is to be copied</param>
        void ConcurrentReader(ref Key key, ref Input input, ref Value value, ref Output dst);

        /// <summary>
        /// Non-concurrent writer; called on an Upsert that does not find the key so does an insert or finds the key's record in the immutable region so does a read/copy/update (RCU),
        /// or when copying reads fetched from disk to either read cache or tail of log.
        /// </summary>
        /// <param name="key">The key for this record</param>
        /// <param name="src">The previous value to be copied/updated</param>
        /// <param name="dst">The destination to be updated; because this is an copy to a new location, there is no previous value there.</param>
        void SingleWriter(ref Key key, ref Value src, ref Value dst);

        /// <summary>
        /// Concurrent writer; called on an Upsert that finds the record in the mutable range.
        /// </summary>
        /// <param name="key">The key for the record to be written</param>
        /// <param name="src">The value to be copied to <paramref name="dst"/></param>
        /// <param name="dst">The location where <paramref name="src"/> is to be copied; because this method is called only for in-place updates, there is a previous value there.</param>
        bool ConcurrentWriter(ref Key key, ref Value src, ref Value dst);

        /// <summary>
        /// Whether this Functions instance supports locking. Iff so, FASTER will call <see cref="Lock(ref RecordInfo, ref Key, ref Value, LockType, ref long)"/> 
        /// and <see cref="Unlock(ref RecordInfo, ref Key, ref Value, LockType, long)"/>.
        /// </summary>
        bool SupportsLocking { get; }

        /// <summary>
        /// User-provided lock call, defaulting to no-op. A default exclusive implementation is available via <see cref="RecordInfo.SpinLock()"/>.
        /// See also <see cref="IntExclusiveLocker"/> to use two bits of an existing int value.
        /// </summary>
        /// <param name="recordInfo">The header for the current record</param>
        /// <param name="key">The key for the current record</param>
        /// <param name="value">The value for the current record</param>
        /// <param name="lockType">The type of lock being taken</param>
        /// <param name="lockContext">Context-specific information; will be passed to <see cref="Unlock(ref RecordInfo, ref Key, ref Value, LockType, long)"/></param>
        /// <remarks>
        /// This is called only for records guaranteed to be in the mutable range.
        /// </remarks>
        void Lock(ref RecordInfo recordInfo, ref Key key, ref Value value, LockType lockType, ref long lockContext);

        /// <summary>
        /// User-provided unlock call, defaulting to no-op. A default exclusive implementation is available via <see cref="RecordInfo.Unlock()"/>.
        /// See also <see cref="IntExclusiveLocker"/> to use two bits of an existing int value.
        /// </summary>
        /// <param name="recordInfo">The header for the current record</param>
        /// <param name="key">The key for the current record</param>
        /// <param name="value">The value for the current record</param>
        /// <param name="lockType">The type of lock being released, as passed to <see cref="Lock(ref RecordInfo, ref Key, ref Value, LockType, ref long)"/></param>
        /// <param name="lockContext">The context returned from <see cref="Lock(ref RecordInfo, ref Key, ref Value, LockType, ref long)"/></param>
        /// <remarks>
        /// This is called only for records guaranteed to be in the mutable range.
        /// </remarks>
        /// <returns>
        /// True if no inconsistencies detected. Otherwise, the lock and user's callback are reissued.
        /// Currently this is handled only for <see cref="ConcurrentReader(ref Key, ref Input, ref Value, ref Output)"/>.
        /// </returns>
        bool Unlock(ref RecordInfo recordInfo, ref Key key, ref Value value, LockType lockType, long lockContext);
    }

    /// <summary>
    /// Callback functions to FASTER (two-param version)
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    /// <typeparam name="Value"></typeparam>
    public interface IFunctions<Key, Value> : IFunctions<Key, Value, Value, Value, Empty>
    {
    }

    /// <summary>
    /// Callback functions to FASTER (two-param version with context)
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    /// <typeparam name="Value"></typeparam>
    /// <typeparam name="Context"></typeparam>
    public interface IFunctions<Key, Value, Context> : IFunctions<Key, Value, Value, Value, Context>
    {
    }
}