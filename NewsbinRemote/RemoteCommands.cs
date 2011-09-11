using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Globalization;
using System.Security.Cryptography;
using System.Diagnostics;


namespace NewsbinRemote
{
    class RemoteCommand
    {
       private StringBuilder    m_sCommandBuffer;
       private StringBuilder    m_sReplyString;
       int m_nReplyCode;
	
        private Socket          m_Socket;
        private BinaryReader    m_Reader;
        private BinaryWriter    m_Writer;
        private Stream          m_SocketStream;




        public String      GetReplyString()
        {
            return(m_sReplyString.ToString());
        }
	
	   /***
	    * Initiates control connections and gets initial reply, determining
	    * if the client is allowed to post to the server.  Initializes
	    * {@link #_reader_} and {@link #_writer_} to wrap
	    * {@link SocketClient#_input_} and {@link SocketClient#_output_}.
	    ***/
	   public int ConnectServer(String	sServer,int nPort)
	   {
           m_sCommandBuffer = new StringBuilder();
           m_sReplyString = new StringBuilder(); 


            m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            System.Net.IPAddress ipAdd = System.Net.IPAddress.Parse(sServer);

            System.Net.IPEndPoint remoteEP = new IPEndPoint(ipAdd, nPort);

            m_Socket.Connect(remoteEP);

            StringBuilder sResponse = new StringBuilder();

            m_SocketStream = new NetworkStream(m_Socket);
            m_Reader = new BinaryReader(m_SocketStream);
            m_Writer = new BinaryWriter(m_SocketStream);
//            m_Writer.AutoFlush = true; // enable automatic flushing

            return(ReadResponse());

	   }

	   public int SendCommand(StringBuilder sCommand)
	   {
           sCommand.Append("\r\n");
           byte[] AsciiBytes = Encoding.ASCII.GetBytes(sCommand.ToString());

//            char[] asciiChars = new char[Encoding.ASCII.GetCharCount(AsciiBytes, 0, AsciiBytes.Length)];
//            Encoding.ASCII.GetChars(AsciiBytes, 0, AsciiBytes.Length, asciiChars, 0);
//            string asciiString = new string(asciiChars);

           m_Writer.Write(AsciiBytes, 0, AsciiBytes.Length);
		   return(0);
	   }


       public int ReadResponse()
       {
           m_sReplyString.Remove(0, m_sReplyString.Length);

           byte cur = (byte)m_Reader.ReadByte();
           while (cur != '\n')
           {
               if (cur != '\r')
               {
                   m_sReplyString.Append(((char)cur).ToString());
               }
               cur = m_Reader.ReadByte();
           }
           if (m_sReplyString.Length < 3)
           {
               return (-1);
           }

           // In case we run into an anomaly we don't want fatal index exceptions
           // to be thrown.
           m_nReplyCode = Int32.Parse(m_sReplyString.ToString().Substring(0, 3));
           return (m_nReplyCode);
       }

       public int Auth(String sKey, String sPass)
       {
           StringBuilder sKeyBuilder = new StringBuilder();
           sKeyBuilder.Append(sKey);
           sKeyBuilder.Append('|');
           sKeyBuilder.Append(sPass);

           using (MD5 md5Hash = MD5.Create())
           {
               byte[] AsciiData = Encoding.ASCII.GetBytes(sKeyBuilder.ToString());

               byte[] data = md5Hash.ComputeHash(AsciiData, 0, AsciiData.Length);


               StringBuilder sBuilder = new StringBuilder();

               sBuilder.Append("AUTH ");
               for (int i = 0; i < data.Length; i++)
               {
                   sBuilder.Append(data[i].ToString("X2"));
               }
               SendCommand(sBuilder);
           }

           ReadResponse();
           Debug.Assert(m_nReplyCode < 299);
           if (m_nReplyCode > 299)
           {
           }
           return (m_nReplyCode);
       }
	   

	   public int LoadGroup(int nMinRange,int nMaxRange)
	   {
		   m_sCommandBuffer.Remove(0,m_sCommandBuffer.Length);
		   m_sCommandBuffer.Append("group load ");
		   m_sCommandBuffer.Append(nMinRange);
		   m_sCommandBuffer.Append(" ");
		   m_sCommandBuffer.Append(nMaxRange);
		   SendCommand(m_sCommandBuffer);
		   
		   ReadResponse();
		   Debug.Assert(m_nReplyCode < 299);
		   if( m_nReplyCode > 299 )
		   {
			   // Command error - now hwhat?
			   
		   }
		   return(m_nReplyCode);
	   }
	   
	   public int SearchLocal(int nMinRange,int nMaxRange,String sSearch)
	   {
		   m_sCommandBuffer.Remove(0,m_sCommandBuffer.Length);
		   m_sCommandBuffer.Append("search local ");
		   m_sCommandBuffer.Append(nMinRange);
		   m_sCommandBuffer.Append(" ");
		   m_sCommandBuffer.Append(nMaxRange);
		   m_sCommandBuffer.Append(" \"");
		   m_sCommandBuffer.Append(sSearch);
		   m_sCommandBuffer.Append("\"");
		   
		   SendCommand(m_sCommandBuffer);
		   
		   ReadResponse();
		   Debug.Assert(m_nReplyCode < 299);
		   if( m_nReplyCode > 299 )
		   {
			   // Command error - now hwhat?
			   
		   }
		   return(m_nReplyCode);
	   }
       public int SearchInternet(int nMinRange, int nMaxRange, String sSearch)
       {
           m_sCommandBuffer.Remove(0, m_sCommandBuffer.Length);
           m_sCommandBuffer.Append("search internet ");
           m_sCommandBuffer.Append(nMinRange);
           m_sCommandBuffer.Append(" ");
           m_sCommandBuffer.Append(nMaxRange);
           m_sCommandBuffer.Append(" \"");
           m_sCommandBuffer.Append(sSearch);
           m_sCommandBuffer.Append("\"");

           SendCommand(m_sCommandBuffer);

           ReadResponse();
           Debug.Assert(m_nReplyCode < 299);
           if (m_nReplyCode > 299)
           {
               // Command error - now hwhat?

           }
           return (m_nReplyCode);
       }

       public int LoadedCount()
       {
           m_sCommandBuffer.Remove(0, m_sCommandBuffer.Length);
           m_sCommandBuffer.Append("Loaded Count");
           SendCommand(m_sCommandBuffer);

           ReadResponse();
           Debug.Assert(m_nReplyCode < 299);
           if (m_nReplyCode > 299)
           {
               // Command error - now hwhat?
               return (-1);
           }
           string[] vResults = m_sReplyString.ToString().Split(' ');
           Debug.Assert(vResults.Length >= 2);
           if (vResults.Length < 2)
           {
               // Command error - now hwhat?
               return (-1);
           }
           int nCount = int.Parse(vResults[1]);

           return (nCount);
       }

       public int LoadedList(long nMinIndex, long nMaxIndex)
       {
           m_sCommandBuffer.Remove(0, m_sCommandBuffer.Length);
           m_sCommandBuffer.Append("Loaded List ");
           m_sCommandBuffer.Append(nMinIndex);
           m_sCommandBuffer.Append(" ");
           m_sCommandBuffer.Append(nMaxIndex);
           SendCommand(m_sCommandBuffer);

           ReadResponse();
           Debug.Assert(m_nReplyCode < 299);
           if (m_nReplyCode > 299)
           {
               // Command error - now hwhat?
               return (-1);
           }
           return (m_nReplyCode);
       }

        //
        // No support for adding paths yet
        //
       public int LoadedDownload(long nIndex)
       {
           m_sCommandBuffer.Remove(0, m_sCommandBuffer.Length);
           m_sCommandBuffer.Append("Loaded Download ");
           m_sCommandBuffer.Append(nIndex);
           SendCommand(m_sCommandBuffer);

           ReadResponse();
           Debug.Assert(m_nReplyCode < 299);
           if (m_nReplyCode > 299)
           {
               // Command error - now hwhat?
               return (-1);
           }
           return (m_nReplyCode);
       }


        
       public int ReadListItem(ref StringBuilder Line)
       {
           byte cur = (byte)m_Reader.ReadByte();

           //
           // Handle empty lines by reading the next one
           //
           do
           {
               Line.Remove(0, Line.Length);
               while (cur != '\n')
               {
                   if (cur != '\r')
                   {
                       Line.Append(((char)cur).ToString());
                   }
                   cur = m_Reader.ReadByte();
               }
           } while (0 == Line.Length);

           if (1 == Line.Length)
           {
               if (Line[0] == '.')
               {
                   return (0);
               }
           }

           return (Line.Length);
       }


       public int FilterAddGroup(String sGroup)
       {
           m_sCommandBuffer.Remove(0, m_sCommandBuffer.Length);
           m_sCommandBuffer.Append("filter addgroup \"");
           m_sCommandBuffer.Append(sGroup);
           m_sCommandBuffer.Append("\"");
           SendCommand(m_sCommandBuffer);

           ReadResponse();
           Debug.Assert(m_nReplyCode < 299);
           if (m_nReplyCode > 299)
           {
               // Command error - now hwhat?

           }
           return (m_nReplyCode);
       }

       public int FilterClearGroups()
       {
           m_sCommandBuffer.Remove(0, m_sCommandBuffer.Length);
           m_sCommandBuffer.Append("filter cleargroups");
           SendCommand(m_sCommandBuffer);

           ReadResponse();
           Debug.Assert(m_nReplyCode < 299);
           if (m_nReplyCode > 299)
           {
               // Command error - now hwhat?

           }
           return (m_nReplyCode);
       }


       public int FilterSetAge(long nAge)
       {
           m_sCommandBuffer.Remove(0, m_sCommandBuffer.Length);
           m_sCommandBuffer.Append("filter setage ");
           m_sCommandBuffer.Append(nAge);
           SendCommand(m_sCommandBuffer);

           ReadResponse();
           Debug.Assert(m_nReplyCode < 299);
           if (m_nReplyCode > 299)
           {
               // Command error - now hwhat?

           }
           return (m_nReplyCode);
       }

       public int FilterSetMinSize(long nMinSize)
       {
           m_sCommandBuffer.Remove(0, m_sCommandBuffer.Length);
           m_sCommandBuffer.Append("filter setminsize ");
           m_sCommandBuffer.Append(nMinSize);
           SendCommand(m_sCommandBuffer);

           ReadResponse();
           Debug.Assert(m_nReplyCode < 299);
           if (m_nReplyCode > 299)
           {
               // Command error - now hwhat?

           }
           return (m_nReplyCode);
       }

       public int FilterSetMaxSize(long nMinSize)
       {
           m_sCommandBuffer.Remove(0, m_sCommandBuffer.Length);
           m_sCommandBuffer.Append("filter setmaxsize ");
           m_sCommandBuffer.Append(nMinSize);
           SendCommand(m_sCommandBuffer);

           ReadResponse();
           Debug.Assert(m_nReplyCode < 299);
           if (m_nReplyCode > 299)
           {
               // Command error - now hwhat?

           }
           return (m_nReplyCode);
       }

       public int FilterSetTextFilter(String sFilter)
       {
           m_sCommandBuffer.Remove(0, m_sCommandBuffer.Length);
           m_sCommandBuffer.Append("filter setminsize \"");
           m_sCommandBuffer.Append(sFilter);
           m_sCommandBuffer.Append("\"");
           SendCommand(m_sCommandBuffer);

           ReadResponse();
           Debug.Assert(m_nReplyCode < 299);
           if (m_nReplyCode > 299)
           {
               // Command error - now hwhat?

           }
           return (m_nReplyCode);
       }



       public int FileCount()
       {
           m_sCommandBuffer.Remove(0, m_sCommandBuffer.Length);
           m_sCommandBuffer.Append("files count");
           SendCommand(m_sCommandBuffer);

           ReadResponse();
           Debug.Assert(m_nReplyCode < 299);
           if (m_nReplyCode > 299)
           {
               // Command error - now hwhat?
               return (m_nReplyCode);
           }

           string[] vResults = m_sReplyString.ToString().Split(' ');
           Debug.Assert(vResults.Length >= 3);
           if (vResults.Length < 2)
           {
               return (-1);
           }
           int nLength = int.Parse(vResults[1]);

           //
           // Response contains the filename and the file size
           //
           return (nLength);
       }

       public int FileList(int nMinIndex, int nMaxIndex)
       {
           m_sCommandBuffer.Remove(0, m_sCommandBuffer.Length);
           m_sCommandBuffer.Append("files list ");
           m_sCommandBuffer.Append(nMinIndex);
           m_sCommandBuffer.Append(" ");
           m_sCommandBuffer.Append(nMaxIndex);
           SendCommand(m_sCommandBuffer);

           ReadResponse();
           Debug.Assert(m_nReplyCode < 299);
           if (m_nReplyCode > 299)
           {
               // Command error - now hwhat?
               return (m_nReplyCode);
           }
           // Response contains the filename and the file size
           //
           return (m_nReplyCode);
       }

       public int DownloadCount()
       {
           m_sCommandBuffer.Remove(0, m_sCommandBuffer.Length);
           m_sCommandBuffer.Append("downloads count");
           SendCommand(m_sCommandBuffer);

           ReadResponse();
           Debug.Assert(m_nReplyCode < 299);
           if (m_nReplyCode > 299)
           {
               // Command error - now hwhat?
               return (m_nReplyCode);
           }

           string[] vResults = m_sReplyString.ToString().Split(' ');
           Debug.Assert(vResults.Length >= 3);
           if (vResults.Length < 2)
           {
               return (-1);
           }
           int nLength = int.Parse(vResults[1]);

           //
           // Response contains the filename and the file size
           //
           return (nLength);
       }


       public int DownloadList(long nMinIndex, long nMaxIndex)
       {
           m_sCommandBuffer.Remove(0, m_sCommandBuffer.Length);
           m_sCommandBuffer.Append("downloads List ");
           m_sCommandBuffer.Append(nMinIndex);
           m_sCommandBuffer.Append(" ");
           m_sCommandBuffer.Append(nMaxIndex);
           SendCommand(m_sCommandBuffer);

           ReadResponse();
           Debug.Assert(m_nReplyCode < 299);
           if (m_nReplyCode > 299)
           {
               // Command error - now hwhat?
               return (-1);
           }
           return (m_nReplyCode);
       }



       public int NZBUpload(String sFilename)
       {
           FileInfo f = new FileInfo(sFilename);
           int nLength = (int)f.Length;

           m_sCommandBuffer.Remove(0, m_sCommandBuffer.Length);
           m_sCommandBuffer.Append("upload nzbbinary \"");
           m_sCommandBuffer.Append(System.IO.Path.GetFileName(sFilename));
           m_sCommandBuffer.Append("\" ");
           m_sCommandBuffer.Append(nLength);
           SendCommand(m_sCommandBuffer);

           ReadResponse();
           Debug.Assert(m_nReplyCode == 300);
           if (m_nReplyCode != 300)
           {
               // Command error - now hwhat?
               return (-1);
           }


           using (BinaryReader Reader = new BinaryReader(File.Open(sFilename, FileMode.Open)))
           {
               byte[] buffer = new byte[64 * 1024];

               int nOffset = 0;
               do
               {
                   //
                   // Prevent it from consuming the 200 reply from newsbin. Make
                   // sure it ONLY reads file sized amounts of data
                   //
                   int nLengthToRead = Math.Min(buffer.Length, nLength);
                   int nChunk = Reader.Read(buffer, 0, nLengthToRead);
                   if (nChunk <= 0)
                   {
                       break;
                   }

                   m_Writer.Write(buffer, 0, nChunk);
                   m_Writer.Flush();

                   nOffset += nChunk;
                   nLength -= nChunk;
                   if (nLength <= 0)
                   {
                       break;
                   }
               }
               while (true);
           }
           ReadResponse();
           Debug.Assert(m_nReplyCode < 299);
           if (m_nReplyCode > 299)
           {
               // Command error - now hwhat?
               return (m_nReplyCode);
           }
           return (m_nReplyCode);
       }


       public int NZBUploadAutoload(String sFilename)
       {
           FileInfo f = new FileInfo(sFilename);
           int nLength = (int)f.Length;

           m_sCommandBuffer.Remove(0, m_sCommandBuffer.Length);
           m_sCommandBuffer.Append("upload nzbautoload \"");
           m_sCommandBuffer.Append(System.IO.Path.GetFileName(sFilename));
           m_sCommandBuffer.Append("\" ");
           m_sCommandBuffer.Append(nLength);
           SendCommand(m_sCommandBuffer);

           ReadResponse();
           Debug.Assert(m_nReplyCode == 300);
           if (m_nReplyCode != 300)
           {
               // Command error - now hwhat?
               return (-1);
           }


           using (BinaryReader Reader = new BinaryReader(File.Open(sFilename, FileMode.Open)))
           {
               byte[] buffer = new byte[64 * 1024];

               int nOffset = 0;
               do
               {
                   //
                   // Prevent it from consuming the 200 reply from newsbin. Make
                   // sure it ONLY reads file sized amounts of data
                   //
                   int nLengthToRead = Math.Min(buffer.Length, nLength);
                   int nChunk = Reader.Read(buffer, 0, nLengthToRead);
                   if (nChunk <= 0)
                   {
                       break;
                   }

                   m_Writer.Write(buffer, 0, nChunk);
                   m_Writer.Flush();

                   nOffset += nChunk;
                   nLength -= nChunk;
                   if (nLength <= 0)
                   {
                       break;
                   }
               }
               while (true);
           }
           ReadResponse();
           Debug.Assert(m_nReplyCode < 299);
           if (m_nReplyCode > 299)
           {
               // Command error - now hwhat?
               return (m_nReplyCode);
           }
           return (m_nReplyCode);
       }




       public int FileRetrieve(int nIndex, String sPath)
       {
           m_sCommandBuffer.Remove(0, m_sCommandBuffer.Length);
           m_sCommandBuffer.Append("files download ");
           m_sCommandBuffer.Append(nIndex);
           SendCommand(m_sCommandBuffer);

           ReadResponse();
           Debug.Assert(m_nReplyCode < 299);
           if (m_nReplyCode > 299)
           {
               // Command error - now hwhat?
               return (m_nReplyCode);
           }

           string[] vResults = m_sReplyString.ToString().Split(' ');
           Debug.Assert(vResults.Length >= 3);
           if (vResults.Length < 3)
           {
               return (-1);
           }

           StringBuilder sFilename = new StringBuilder();
           sFilename.Append(sPath);
           sFilename.Append(vResults[1].Substring(1));
           sFilename.Remove(sFilename.Length - 1, 1);
           int nLength = int.Parse(vResults[2]);

           m_sCommandBuffer.Remove(0, m_sCommandBuffer.Length);
           m_sCommandBuffer.Append("300 Go!");
           SendCommand(m_sCommandBuffer);

           File.Delete(sFilename.ToString());
           using (BinaryWriter Writer = new BinaryWriter(File.Open(sFilename.ToString(), FileMode.CreateNew)))
           {
               byte[] buffer = new byte[64 * 1024];

               int nOffset = 0;
               do
               {
                   //
                   // Prevent it from consuming the 200 reply from newsbin. Make
                   // sure it ONLY reads file sized amounts of data
                   //
                   int nLengthToRead = Math.Min(buffer.Length, nLength);

                   int nChunk = m_SocketStream.Read(buffer, 0, nLengthToRead);
                   if (nChunk <= 0)
                   {
                       break;
                   }
                   Writer.Write(buffer, 0, nChunk);
                   Writer.Flush();

                   nOffset += nChunk;
                   nLength -= nChunk;
                   if (nLength <= 0)
                   {
                       break;
                   }
               }
               while (true);
           }

           ReadResponse();
           Debug.Assert(m_nReplyCode < 299);
           if (m_nReplyCode > 299)
           {
               // Command error - now hwhat?
               return (m_nReplyCode);
           }

           //
           // Response contains the filename and the file size
           //
           return (0);
       }
    }
}
