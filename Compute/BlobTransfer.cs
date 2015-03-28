using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Inedo.BuildMasterExtensions.Azure
{
    /// <summary>
    /// Class to allow for easy async Azure Blob upload and download functions with 
    /// progress change notifications for every 5% downloaded.
    /// </summary>
    /// <remarks>
    /// This class was adapted from: http://blogs.msdn.com/b/kwill/archive/2013/03/06/asynchronous-parallel-block-blob-transfers-with-progress-change-notification-2-0.aspx
    /// Since it is marked as an example or sample, it is released under the terms of the Microsoft Limited Public License.
    /// </remarks>
    internal sealed class BlobTransfer
    {
        public event AsyncCompletedEventHandler TransferCompleted;
        public event EventHandler<BlobTransferProgressChangedEventArgs> TransferProgressChanged;

        public TransferTypeEnum TransferType;

        private ICancellableAsyncResult asyncresult;
        private bool working = false;
        private object workingLock = new object();
        private AsyncOperation asyncOp;
        private Queue<long> timeQueue = new Queue<long>(200);
        private Queue<long> bytesQueue = new Queue<long>(200);
        private DateTime updateTime = DateTime.Now;
        private string fileName;
        private ICloudBlob blob;
        private int lastProgress = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobTransfer"/> class.
        /// </summary>
        /// <param name="blob">The BLOB.</param>
        public BlobTransfer(ICloudBlob blob)
        {
            this.blob = blob;
        }

        /// <summary>
        /// Uploads a file to an Azure blob asynchronously.
        /// </summary>
        /// <param name="localFile">The local file.</param>
        /// <exception cref="System.Exception">BlobTransfer already initiated. Create new BlobTransfer object to initiate a new file transfer.</exception>
        public ICancellableAsyncResult UploadBlobAsync(string localFile)
        {
            lock (workingLock)
            {
                if (!working)
                    working = true;
                else
                    throw new Exception("BlobTransfer already initiated. Create new BlobTransfer object to initiate a new file transfer.");
            }

            // Attempt to open the file first so that we throw an exception before getting into the async work
            using (var fstemp = new FileStream(localFile, FileMode.Open, FileAccess.Read)) { }

            // Create an async op in order to raise the events back to the client on the correct thread.
            asyncOp = AsyncOperationManager.CreateOperation(this.blob);

            TransferType = TransferTypeEnum.Upload;
            fileName = localFile;

            var file = new FileInfo(fileName);
            long fileSize = file.Length;

            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            ProgressStream pstream = new ProgressStream(fs);
            pstream.ProgressChanged += pstream_ProgressChanged;
            pstream.SetLength(fileSize);
            this.blob.ServiceClient.ParallelOperationThreadCount = 10;
            asyncresult = this.blob.BeginUploadFromStream(pstream, BlobTransferCompletedCallback, new BlobTransferAsyncState(this.blob, pstream));
            return asyncresult;
        }

        /// <summary>
        /// Downloads a blob to a local file asynchronously.
        /// </summary>
        /// <param name="localFile">The local file.</param>
        /// <exception cref="System.Exception">BlobTransfer already initiated. Create new BlobTransfer object to initiate a new file transfer.</exception>
        public ICancellableAsyncResult DownloadBlobAsync(string localFile)
        {
            lock (workingLock)
            {
                if (!working)
                    working = true;
                else
                    throw new Exception("BlobTransfer already initiated. Create new BlobTransfer object to initiate a new file transfer.");
            }

            // Create an async op in order to raise the events back to the client on the correct thread.
            asyncOp = AsyncOperationManager.CreateOperation(blob);

            TransferType = TransferTypeEnum.Download;
            fileName = localFile;

            this.blob.FetchAttributes();

            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            ProgressStream pstream = new ProgressStream(fs);
            pstream.ProgressChanged += pstream_ProgressChanged;
            pstream.SetLength(this.blob.Properties.Length);
            this.blob.ServiceClient.ParallelOperationThreadCount = 10;
            asyncresult = this.blob.BeginDownloadToStream(pstream, BlobTransferCompletedCallback, new BlobTransferAsyncState(this.blob, pstream));
            return asyncresult;
        }

        private void pstream_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BlobTransferProgressChangedEventArgs eArgs = null;
            int progress = (int)((double)e.BytesRead / e.TotalLength * 100);

            // raise the progress changed event on the asyncop thread
            eArgs = new BlobTransferProgressChangedEventArgs(e.BytesRead, e.TotalLength, progress, CalculateSpeed(e.BytesRead), null);
            asyncOp.Post(delegate(object e2) { OnTaskProgressChanged((BlobTransferProgressChangedEventArgs)e2); }, eArgs);
        }

        private void BlobTransferCompletedCallback(IAsyncResult result)
        {
            BlobTransferAsyncState state = (BlobTransferAsyncState)result.AsyncState;
            ICloudBlob blob = state.Blob;
            ProgressStream stream = (ProgressStream)state.Stream;

            try
            {
                stream.Close();

                // End the operation.
                if (TransferType == TransferTypeEnum.Download)
                    blob.EndDownloadToStream(result);
                else if (TransferType == TransferTypeEnum.Upload)
                    blob.EndUploadFromStream(result);

                // Operation completed normally, raise the completed event
                AsyncCompletedEventArgs completedArgs = new AsyncCompletedEventArgs(null, false, null);
                asyncOp.PostOperationCompleted(delegate(object e) { OnTaskCompleted((AsyncCompletedEventArgs)e); }, completedArgs);
            }
            catch (StorageException ex)
            {
                if (!state.Cancelled)
                {
                    throw (ex);
                }

                // Operation was cancelled, raise the event with the cancelled flag = true
                AsyncCompletedEventArgs completedArgs = new AsyncCompletedEventArgs(null, true, null);
                asyncOp.PostOperationCompleted(delegate(object e) { OnTaskCompleted((AsyncCompletedEventArgs)e); }, completedArgs);
            }
        }

        public void CancelAsync()
        {
            ((BlobTransferAsyncState)asyncresult.AsyncState).Cancelled = true;
            asyncresult.Cancel();
        }

        private void OnTaskCompleted(AsyncCompletedEventArgs e)
        {
            if (TransferCompleted != null)
                TransferCompleted(this, e);
        }

        private void OnTaskProgressChanged(BlobTransferProgressChangedEventArgs e)
        {
            // only fire if the % change is a multiple of 5% or has surpassed the next 5%
            if (((e.ProgressPercentage % 5 == 0) && e.ProgressPercentage > lastProgress)
                || e.ProgressPercentage > lastProgress + 5)
            {
                lastProgress = e.ProgressPercentage;

                if (TransferProgressChanged != null)
                    TransferProgressChanged(this, e);
            }
        }

        // Keep the last 200 progress change notifications and use them to calculate the average speed over that duration. 
        private double CalculateSpeed(long BytesSent)
        {
            double speed = 0;

            if (timeQueue.Count >= 200)
            {
                timeQueue.Dequeue();
                bytesQueue.Dequeue();
            }

            timeQueue.Enqueue(System.DateTime.Now.Ticks);
            bytesQueue.Enqueue(BytesSent);

            if (timeQueue.Count > 2)
            {
                updateTime = System.DateTime.Now;
                speed = (bytesQueue.Max() - bytesQueue.Min()) / TimeSpan.FromTicks(timeQueue.Max() - timeQueue.Min()).TotalSeconds;
            }

            return speed;
        }

        // A modified version of the ProgressStream from http://blogs.msdn.com/b/paolos/archive/2010/05/25/large-message-transfer-with-wcf-adapters-part-1.aspx
        // This class allows progress changed events to be raised from the blob upload/download.
        private class ProgressStream : Stream
        {
            #region Private Fields
            private Stream stream;
            private long bytesTransferred;
            private long totalLength;
            #endregion

            #region Public Handler
            public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
            #endregion

            #region Public Constructor
            public ProgressStream(Stream file)
            {
                this.stream = file;
                this.totalLength = file.Length;
                this.bytesTransferred = 0;
            }
            #endregion

            #region Public Properties
            public override bool CanRead
            {
                get
                {
                    return this.stream.CanRead;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return this.stream.CanSeek;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return this.stream.CanWrite;
                }
            }

            public override void Flush()
            {
                this.stream.Flush();
            }

            public override void Close()
            {
                this.stream.Close();
            }

            public override long Length
            {
                get
                {
                    return this.stream.Length;
                }
            }

            public override long Position
            {
                get
                {
                    return this.stream.Position;
                }
                set
                {
                    this.stream.Position = value;
                }
            }
            #endregion

            #region Public Methods
            public override int Read(byte[] buffer, int offset, int count)
            {
                int result = stream.Read(buffer, offset, count);
                bytesTransferred += result;
                if (ProgressChanged != null)
                {
                    try
                    {
                        OnProgressChanged(new ProgressChangedEventArgs(bytesTransferred, totalLength));
                    }
                    catch (Exception)
                    {
                        ProgressChanged = null;
                    }
                }
                return result;
            }

            protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
            {
                if (ProgressChanged != null)
                    ProgressChanged(this, e);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return this.stream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                totalLength = value;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                this.stream.Write(buffer, offset, count);
                bytesTransferred += count;
                {
                    try
                    {
                        OnProgressChanged(new ProgressChangedEventArgs(bytesTransferred, totalLength));
                    }
                    catch (Exception)
                    {
                        ProgressChanged = null;
                    }
                }
            }

            protected override void Dispose(bool disposing)
            {
                stream.Dispose();
                base.Dispose(disposing);
            }

            #endregion
        }

        private class BlobTransferAsyncState
        {
            public ICloudBlob Blob;
            public Stream Stream;
            public DateTime Started;
            public bool Cancelled;

            public BlobTransferAsyncState(ICloudBlob blob, Stream stream)
                : this(blob, stream, DateTime.Now)
            { }

            public BlobTransferAsyncState(ICloudBlob blob, Stream stream, DateTime started)
            {
                Blob = blob;
                Stream = stream;
                Started = started;
                Cancelled = false;
            }
        }

        private class ProgressChangedEventArgs : EventArgs
        {
            #region Private Fields
            private long bytesRead;
            private long totalLength;
            #endregion

            #region Public Constructor
            public ProgressChangedEventArgs(long bytesRead, long totalLength)
            {
                this.bytesRead = bytesRead;
                this.totalLength = totalLength;
            }
            #endregion

            #region Public properties

            public long BytesRead
            {
                get
                {
                    return this.bytesRead;
                }
                set
                {
                    this.bytesRead = value;
                }
            }

            public long TotalLength
            {
                get
                {
                    return this.totalLength;
                }
                set
                {
                    this.totalLength = value;
                }
            }
            #endregion
        }

        public enum TransferTypeEnum
        {
            Download,
            Upload
        }

        public class BlobTransferProgressChangedEventArgs : System.ComponentModel.ProgressChangedEventArgs
        {
            private long m_BytesSent = 0;
            private long m_TotalBytesToSend = 0;
            private double m_Speed = 0;

            public long BytesSent
            {
                get { return m_BytesSent; }
            }

            public long TotalBytesToSend
            {
                get { return m_TotalBytesToSend; }
            }

            public double Speed
            {
                get { return m_Speed; }
            }

            public TimeSpan TimeRemaining
            {
                get
                {
                    TimeSpan time = new TimeSpan(0, 0, (int)((TotalBytesToSend - m_BytesSent) / (m_Speed == 0 ? 1 : m_Speed)));
                    return time;
                }
            }

            public BlobTransferProgressChangedEventArgs(long BytesSent, long TotalBytesToSend, int progressPercentage, double Speed, object userState)
                : base(progressPercentage, userState)
            {
                m_BytesSent = BytesSent;
                m_TotalBytesToSend = TotalBytesToSend;
                m_Speed = Speed;
            }
        }
    }
}
