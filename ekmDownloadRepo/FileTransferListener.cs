using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using com.ansys.ingress.client.api;
using com.ansys.ingress.connector.model;
using com.ansys.ingress.connector.model.spec;

namespace ekmDownloadRepo
{
    internal class FileTransferListener : TransferEventListener
    {

        private bool active;

        private TransferEventError error;
        /// <summary>
        /// Indicates the transfer was canceled.
        /// </summary>
        /// <param name="eventObj">the event</param>
        public void TransferCanceled(TransferEvent eventObj)
        {
            active = false;
        }
        /// <summary>
        /// Indicates the transfer completed.
        /// </summary>
        /// <param name="eventObj">the event</param>
        public void TransferCompleted(TransferEvent eventObj)
        {
            active = false;
        }
        /// <summary>
        /// Indicates the transfer completed.
        /// </summary>
        /// <param name="eventObj">the event</param>
        public void TransferCached(TransferEvent eventObj)
        {
            active = false;
        }
        /// <summary>
        /// Indicates the transfer failed.
        /// </summary>
        /// <param name="eventObj">the event</param>
        public void TransferFailed(TransferEvent eventObj)
        {
            active = false;
            error = eventObj.GetError();
        }
        /// <summary>
        /// Displays progress information for a transfer.
        /// </summary>
        /// <param name="eventObj">the event</param>
        public void TransferProgress(TransferEvent eventObj)
        {
            //Console.WriteLine(">>>> Progress: " + eventObj.GetProgress());
        }
        /// <summary>
        /// Indicates a transfer was started.
        /// </summary>
        /// <param name="eventObj">the event</param>
        public void TransferStarted(TransferEvent eventObj)
        {
            active = true;
        }
        /// <summary>
        /// Indicates metadata extraction was started on the server.
        /// </summary>
        /// <param name="eventObj">the event</param>
        public void MetadataExtractionStarted(TransferEvent eventObj)
        {
        }
        /// <summary>
        /// Indicates metadata extraction ended on the server.
        /// </summary>
        /// <param name="eventObj">the event</param>
        public void MetadataExtractionEnded(TransferEvent eventObj)
        {
        }
        /// <summary>
        /// Indicates a file was downloaded from the server.
        /// </summary>
        /// <param name="eventObj">the event</param>
        public void FileDownloaded(TransferEvent eventObj)
        {
        }
        /// <summary>
        /// Waits for the transfer to complete.
        /// </summary>
        public void WaitForTransferToEnd()
        {
            do
            {
                Thread.Sleep(1000);
            } while (active);
        }
        /// <summary>
        /// Determines if the transfer failed or was successful.
        /// </summary>
        /// <returns>Returns true if the transfer failed.</returns>
        public bool IsFailure()
        {
            return error != null && (error.GetException() != null ||
                error.GetFailures().Length > 0);
        }
        /// <summary>
        /// Gets any error information if the transfer failed.
        /// </summary>
        /// <returns>Returns the error information.</returns>
        public TransferEventError GetError()
        {
            return error;
        }
    }
}
