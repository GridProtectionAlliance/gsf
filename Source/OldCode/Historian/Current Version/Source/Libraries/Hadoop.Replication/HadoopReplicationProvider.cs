//*******************************************************************************************************
//  HadoopReplicationProvider.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC § 105.  All Other Rights Reserved.
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  11/12/2009 - Pinal C. Patel
//       Generated original version of source code.
//  11/18/2009 - Pinal C. Patel
//       Localized the use of replication log file to ReplicateArchive() method.
//  12/04/2009 - Pinal C. Patel
//       Added more information to replication log file that can be useful for troubleshooting.
//  12/22/2009 - Pinal C. Patel
//       Added try...catch block to the code that performs FTP file delete when an exception is 
//       encountered in ReplicateArchive() method.
//  12/23/2009 - Pinal C. Patel
//       Added HashRequestAttempts and HashRequestWaitTime properties for better control over HDFS file 
//       hash request process.
//  12/24/2009 - Pinal C. Patel
//       Added error handling around the hash request process due to the flaky nature of FTP server when
//       dealing with large files.
//  03/03/2010 - Pinal C. Patel
//       Modified to include files in sub directories of the root directory to be included in the 
//       replication process.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using TVA;
using TVA.Collections;
using TVA.Configuration;
using TVA.Historian.Replication;
using TVA.IO;
using TVA.IO.Checksums;
using TVA.Net.Ftp;
using TVA.Units;

namespace Hadoop.Replication
{
    /// <summary>
    /// Represents a provider of replication for the <see cref="TVA.Historian.IArchive"/> to Hadoop using FTP channel.
    /// </summary>
    public class HadoopReplicationProvider : ReplicationProviderBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="BytesPerCrc32"/> property.
        /// </summary>
        public const int DefaultBytesPerCrc32 = 512;

        /// <summary>
        /// Specifies the default value for the <see cref="HdfsBlockSize"/> property.
        /// </summary>
        public const int DefaultHdfsBlockSize = 64;

        /// <summary>
        /// Specifies the default value for the <see cref="ApplyBufferPadding"/> property.
        /// </summary>
        public const bool DefaultApplyBufferPadding = true;

        /// <summary>
        /// Specifies the default value for the <see cref="HashRequestAttempts"/> property.
        /// </summary>
        public const int DefaultHashRequestAttempts = 3;

        /// <summary>
        /// Specifies the default value for the <see cref="HashRequestWaitTime"/> property.
        /// </summary>
        public const int DefaultHashRequestWaitTime = 3000;

        /// <summary>
        /// Name of the file where replication history information is to be serialized.
        /// </summary>
        private const string ReplicationLogFile = "HadoopReplicationLog.xml";

        // Fields
        private int m_bytesPerCrc32;
        private int m_hdfsBlockSize;
        private bool m_applyBufferPadding;
        private int m_hashRequestAttempts;
        private int m_hashRequestWaitTime;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="HadoopReplicationProvider"/> class.
        /// </summary>
        public HadoopReplicationProvider()
            : base()
        {
            m_bytesPerCrc32 = DefaultBytesPerCrc32;
            m_hdfsBlockSize = DefaultHdfsBlockSize;
            m_applyBufferPadding = DefaultApplyBufferPadding;
            m_hashRequestAttempts = DefaultHashRequestAttempts;
            m_hashRequestWaitTime = DefaultHashRequestWaitTime;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the number of bytes at which HDFS is configured to compute a CRC32.
        /// </summary>
        /// <exception cref="ArgumentException">Value being assigned is zero or negative.</exception>
        public int BytesPerCrc32
        {
            get
            {
                return m_bytesPerCrc32;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value must be at least 1.");

                m_bytesPerCrc32 = value;
            }
        }

        /// <summary>
        /// Gets or sets the size, in MB, of the data blocks for HDFS where the file resides.
        /// </summary>
        /// <exception cref="ArgumentException">Value being assigned is zero or negative.</exception>
        public int HdfsBlockSize
        {
            get
            {
                return m_hdfsBlockSize;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Value must be at least 1.");

                m_hdfsBlockSize = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether the buffer used for computing file hash is to be padded with null bytes for replicating HDFS hashing bug.
        /// </summary>
        public bool ApplyBufferPadding
        {
            get
            {
                return m_applyBufferPadding;
            }
            set
            {
                m_applyBufferPadding = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of requests to be made to the FTP server for HDFS file hash.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is negative or zero.</exception>
        public int HashRequestAttempts
        {
            get
            {
                return m_hashRequestAttempts;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value must be positive");

                m_hashRequestAttempts = value;
            }
        }

        /// <summary>
        /// Gets or set the time, in milliseconds, to wait between requests to the FTP server for HDFS file hash.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is negative or zero.</exception>
        public int HashRequestWaitTime
        {
            get
            {
                return m_hashRequestWaitTime;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value must be positive");

                m_hashRequestWaitTime = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Saves <see cref="HadoopReplicationProvider"/> settings to the config file if the <see cref="ReplicationProviderBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void SaveSettings()
        {
            base.SaveSettings();
            if (PersistSettings)
            {
                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings["BytesPerCrc32", true].Update(m_bytesPerCrc32);
                settings["HdfsBlockSize", true].Update(m_hdfsBlockSize);
                settings["ApplyBufferPadding", true].Update(m_applyBufferPadding);
                settings["HashRequestAttempts", true].Update(m_hashRequestAttempts);
                settings["HashRequestWaitTime", true].Update(m_hashRequestWaitTime);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved <see cref="HadoopReplicationProvider"/> settings from the config file if the <see cref="ReplicationProviderBase.PersistSettings"/> property is set to true.
        /// </summary>  
        public override void LoadSettings()
        {
            base.LoadSettings();
            if (PersistSettings)
            {
                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings.Add("BytesPerCrc32", m_bytesPerCrc32, "Number of bytes at which HDFS is configured to compute a CRC32.");
                settings.Add("HdfsBlockSize", m_hdfsBlockSize, "Size (in MB) of the data blocks for HDFS where the file resides.");
                settings.Add("ApplyBufferPadding", m_applyBufferPadding, "True if the buffer used for computing file hash is to be padded with null bytes for replicating HDFS hashing bug, otherwise False.");
                settings.Add("HashRequestAttempts", m_hashRequestAttempts, "Maximum number of requests to be made to the FTP server for HDFS file hash.");
                settings.Add("HashRequestWaitTime", m_hashRequestWaitTime, "Time (in milliseconds) to wait between requests to the FTP server for HDFS file hash.");
                BytesPerCrc32 = settings["BytesPerCrc32"].ValueAs(m_bytesPerCrc32);
                HdfsBlockSize = settings["HdfsBlockSize"].ValueAs(m_hdfsBlockSize);
                ApplyBufferPadding = settings["ApplyBufferPadding"].ValueAs(m_applyBufferPadding);
                HashRequestAttempts = settings["HashRequestAttempts"].ValueAs(m_hashRequestAttempts);
                HashRequestWaitTime = settings["HashRequestWaitTime"].ValueAs(m_hashRequestWaitTime);
            }
        }

        /// <summary>
        /// Replicates the <see cref="TVA.Historian.IArchive"/>.
        /// </summary>
        protected override void ReplicateArchive()
        {
            // Parse FTP client information.
            Uri replicaUri = new Uri(ReplicaLocation);
            string[] credentials = replicaUri.UserInfo.Split(':');

            // Ensure credentials are supplied.
            if (credentials.Length != 2)
                throw new ArgumentException("FTP credentials are missing in ReplicaLocation.");

            // Create FTP client for uploading.
            FtpClient ftpClient = new FtpClient();
            ftpClient.Server = replicaUri.Host;
            ftpClient.Port = replicaUri.Port;

            // Initialize the replication log.
            DataTable replicationLog = new DataTable("ReplicationRecord");
            replicationLog.Columns.Add("DateTime");
            replicationLog.Columns.Add("FileName");
            replicationLog.Columns.Add("FileHash");
            replicationLog.Columns.Add("FileSync");
            replicationLog.Columns.Add("TransferTime");
            replicationLog.Columns.Add("TransferRate");
            replicationLog.Columns.Add("ServerRequests");
            replicationLog.Columns.Add("ServerResponse");
            if (File.Exists(FilePath.GetAbsolutePath(ReplicationLogFile)))
                replicationLog.ReadXml(FilePath.GetAbsolutePath(ReplicationLogFile));

            try
            {
                // Connect FTP client to server.
                ftpClient.Connect(credentials[0], credentials[1]);
                ftpClient.SetCurrentDirectory(replicaUri.LocalPath);

                // Create list of files to be replicated.
                List<string> files = new List<string>(Directory.GetFiles(ArchiveLocation, "*_to_*.d", SearchOption.AllDirectories));
                files.Sort();

                // Process all the files in the list.
                foreach (string file in files)
                {
                    bool uploading = false;
                    string justFileName = FilePath.GetFileName(file);
                    try
                    {
                        // Continue to "ping" FTP server so that it knows we are alive and well
                        ftpClient.ControlChannel.Command("NOOP");

                        // Compute HDFS file hash.
                        byte[] localHash = ComputeHdfsFileHash(file, m_bytesPerCrc32, m_hdfsBlockSize, m_applyBufferPadding);

                        // Check if file is to be uploaded.
                        int requests;
                        double transferStartTime, transferTotalTime;
                        DataRow record = null;
                        DataRow[] filter = replicationLog.Select(string.Format("FileName ='{0}'", justFileName));
                        if (filter.Length == 0 ||
                            filter[0]["FileSync"].ToString() == "Fail" ||
                            localHash.CompareTo(ByteEncoding.Hexadecimal.GetBytes(filter[0]["FileHash"].ToString())) != 0)
                        {
                            // Upload file to HDFS since:
                            // 1) File has not been replicated previously.
                            // OR
                            // 2) File has been replicated in the past, but its content has changed since then.
                            uploading = true;
                            transferStartTime = Common.SystemTimer;
                            ftpClient.CurrentDirectory.PutFile(file);
                            transferTotalTime = Common.SystemTimer - transferStartTime;

                            // Request file hash from HDFS.
                            for (requests = 1; requests <= m_hashRequestAttempts; requests++)
                            {
                                try
                                {
                                    // Wait before request.
                                    Thread.Sleep(m_hashRequestWaitTime);
                                    // Request file hash.
                                    ftpClient.ControlChannel.Command(string.Format("HDFSCHKSM {0}{1}", ftpClient.CurrentDirectory.FullPath, justFileName));
                                    // Exit when successful.
                                    if (ftpClient.ControlChannel.LastResponse.Code == 200)
                                        break;
                                }
                                catch
                                {
                                    // Apache MINA FTP server acts funny with updoad & hash check of large files.
                                    try
                                    {
                                        ftpClient.Close();
                                    }
                                    catch { }
                                    try
                                    {
                                        ftpClient.Connect(credentials[0], credentials[1]);
                                        ftpClient.SetCurrentDirectory(replicaUri.LocalPath);
                                    }
                                    catch { }
                                }
                            }

                            // Initialize replication log entry.
                            if (filter.Length > 0)
                            {
                                record = filter[0];
                            }
                            else
                            {
                                record = replicationLog.NewRow();
                                replicationLog.Rows.Add(record);
                            }

                            // Update replication log entry.
                            record["DateTime"] = DateTime.UtcNow;
                            record["FileName"] = justFileName;
                            record["FileHash"] = ByteEncoding.Hexadecimal.GetString(localHash);
                            record["TransferTime"] = transferTotalTime.ToString("0.000");
                            record["TransferRate"] = ((new FileInfo(file).Length / SI2.Kilo) / transferTotalTime).ToString("0.00");
                            record["ServerRequests"] = requests < m_hashRequestAttempts ? requests : m_hashRequestAttempts;
                            record["ServerResponse"] = ftpClient.ControlChannel.LastResponse.Message.RemoveCrLfs();

                            // Compare local and HDFS hash.
                            if (ftpClient.ControlChannel.LastResponse.Code == 200 &&
                                localHash.CompareTo(ByteEncoding.Hexadecimal.GetBytes(ftpClient.ControlChannel.LastResponse.Message.RemoveCrLfs().Split(':')[1])) == 0)
                            {
                                // File uploaded and hashes match.
                                record["FileSync"] = "Pass";
                                OnReplicationProgress(new ProcessProgress<int>("ReplicateArchive", justFileName, 1, 1));

                                // Write XML log thus far
                                replicationLog.WriteXml(FilePath.GetAbsolutePath(ReplicationLogFile));
                            }
                            else
                            {
                                // Hashes are different - possible causes:
                                // 1) Local file got modified after hash was computed locally.
                                // OR
                                // 2) Local and remote hashing algorithms are not the same.
                                record["FileSync"] = "Fail";
                                throw new InvalidDataException("File hash mismatch");
                            }
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        // Delete file from FTP site if an exception is encountered when processing the file.
                        try
                        {
                            if (uploading && ftpClient.IsConnected)
                                ftpClient.CurrentDirectory.RemoveFile(justFileName);
                        }
                        catch { }
                        // Re-throw the encountered exception.
                        throw;
                    }
                    catch (Exception ex)
                    {
                        // Delete file from FTP site if an exception is encountered when processing the file.
                        try
                        {
                            if (uploading && ftpClient.IsConnected)
                                ftpClient.CurrentDirectory.RemoveFile(justFileName);
                        }
                        catch { }
                        // Notify about the encountered exception.
                        OnReplicationException(ex);
                    }
                }
            }
            finally
            {
                ftpClient.Dispose();
                replicationLog.WriteXml(FilePath.GetAbsolutePath(ReplicationLogFile));
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Computes a MD5 hash of the file content using the algorithm used by HDFS.
        /// </summary>
        /// <param name="fileName">Name of the file for which the hash is to be computed.</param>
        /// <param name="bytesPerCrc32">Number of bytes at which HDFS is configured to compute a CRC32.</param>
        /// <param name="hdfsBlockSize">Size (in MB) of the data blocks for HDFS where the file resides.</param>
        /// <param name="applyBufferPadding">true if the buffer used for computing file hash is to be padded with null bytes for replicating HDFS hashing bug, otherwise false.</param>
        /// <returns>An <see cref="Array"/> of <see cref="byte"/>s containing the file hash.</returns>
        public static byte[] ComputeHdfsFileHash(string fileName, int bytesPerCrc32, int hdfsBlockSize, bool applyBufferPadding)
        {
            int bytesRead = 0;
            int blockCount = 0;
            byte[] blockCRC = null;
            byte[] blockMD5 = null;
            byte[] fileHash = null;
            FileStream fileStream = null;
            List<byte> blockCRCs = new List<byte>();
            List<byte> blockMD5s = new List<byte>();
            byte[] readBuffer = new byte[bytesPerCrc32];
            MD5CryptoServiceProvider hasher = new MD5CryptoServiceProvider();
            try
            {
                // Open file whose hash is to be computed.
                fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                // Calculate the number of HDFS blocks used by the file on HDFS.
                blockCount = (int)Math.Ceiling((double)fileStream.Length / (double)(hdfsBlockSize * SI2.Mega));

                // For each HDFS block used by the file on HDFS:
                // 1) Compute CRC32s at every "bytesPerCrc32" bytes (default is 512 bytes) and store it in a buffer.
                // 2) Compute block MD5 hash by computing a MD5 hash of the buffer that contains the block CRC32s.
                for (int i = 1; i <= blockCount; i++)
                {
                    // Clear existing data from CRC32 buffer.
                    blockCRCs.Clear();
                    while ((bytesRead = fileStream.Read(readBuffer, 0, readBuffer.Length)) != 0)
                    {
                        // Read "bytesPerCrc32" bytes from the file and compute CRC32.
                        blockCRC = EndianOrder.BigEndian.GetBytes(readBuffer.Crc32Checksum(0, bytesRead));
                        // Add big-endian byte array of the computed CRC32 to CRC32 buffer.
                        blockCRCs.AddRange(blockCRC);
                        // Stop reading and compute block MD5 hash when a HDFS block data has been processed.
                        if (fileStream.Position >= i * hdfsBlockSize * SI2.Mega)
                            break;
                    }

                    // Compute block MD5 hash from the buffer containg block CRC32s.
                    blockMD5 = hasher.ComputeHash(blockCRCs.ToArray());

                    // Store the computed block MD5 hash in a buffer that will be used for computing the file hash.
                    if (applyBufferPadding)
                    {
                        // Apply padding - this replicates a bug in HDFS file hashing algorithm.
                        if (blockMD5s.Count == 0)
                            // Initialize the buffer to 32 bytes.
                            blockMD5s.AddRange(new byte[32]);
                        else if (blockMD5s.Count < blockMD5.Length * i)
                            // Extend the buffer twice its current size.
                            blockMD5s.AddRange(new byte[(blockMD5s.Count * 2) - blockMD5s.Count]);

                        blockMD5s.UpdateRange((i - 1) * blockMD5.Length, blockMD5);
                    }
                    else
                    {
                        // Don't apply padding - this will compute the correct HDFS file hash as per its design.
                        blockMD5s.AddRange(blockMD5);
                    }
                }

                // Compute the final file hash from the buffer that contains block MD5 hashes.
                fileHash = hasher.ComputeHash(blockMD5s.ToArray());
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Dispose();
            }

            return fileHash;
        }

        #endregion
    }
}
