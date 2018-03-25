/*
 * AFileBaseオブジェクトからStream型への受け渡しラッパー
 */
using System;
using System.IO;
using System.Text;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util {
    /// <summary>
    /// </summary>
    public class FileBaseStream : Stream {
        AFileBase filebase;
        public FileBaseStream(AFileBase file) {
            filebase = file;
        }

        public override bool CanRead {
            get { return true; }
        }

        public override bool CanSeek {
            get {return true;}
        }

        public override bool CanWrite {
            get { return false;}
        }

        public override long Length {
            get { return filebase.GetSize(); }
        }

        public override long Position {
            get {
                return filebase.Tell();
            }
            set{
                filebase.Seek((int)value, true);
            }
        }

        public override void Close() {
            // LogUtil.Debug("close");
            filebase.Dispose();
            base.Close();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            // LogUtil.Debug("seek, offset=", offset, ", origin=", origin);

            switch (origin) {
            case SeekOrigin.Current:
                return filebase.Seek((int)offset, false);
            case SeekOrigin.Begin:
                return filebase.Seek((int)offset, true);
            default:
                throw new NotSupportedException("unsuported");
            }
        }

        public override int Read(byte[] buffer, int offset, int count) {
            // LogUtil.Debug("read. offset:", offset, ", count=", count);
            int length;

            if (offset == 0 && buffer.Length > count) {
                length = filebase.Read(ref buffer, count);

            } else {
                var maxLength = buffer.Length - offset;
                if (maxLength < count) count = maxLength;
                var buff = new byte[count];
                
                length = filebase.Read(ref buff, count);
                if (length > 0) {
                    Array.Copy(buff, 0, buffer, offset, length);
                }
            }
////            if (length > 0 && pos == filebase.Tell()) {
////                filebase.Seek(pos+length, true);
////            }
//            LogUtil.Debug("length:", length, ", pos=",  filebase.Tell());
            return length;
        }

        public override int ReadByte() {
            var array = new byte[1];
            return Read(array, 0, 1) == 0 ? -1 : (int)array[0];
            // LogUtil.Debug("readByte=", ret);
        }

        public override void Flush() {
            // LogUtil.Debug("flush");
        }

        public override void SetLength(long value) {
            throw new NotSupportedException("unsupported");
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotSupportedException("unsupported");
        }
    }
}