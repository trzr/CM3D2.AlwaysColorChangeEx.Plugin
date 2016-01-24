/*
 * AFileBaseオブジェクトからStream型への受け渡しラッパー
 */
using System;
using System.IO;

namespace CM3D2.AlwaysColorChangeEx.Plugin.Util
{
    /// <summary>
    /// Description of FileBaseStream.
    /// </summary>
    public class FileBaseStream : Stream
    {
        AFileBase filebase;
        public FileBaseStream(AFileBase file) {
            this.filebase = file;
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
            filebase.Dispose();
            base.Close();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            if (origin == SeekOrigin.Current) {
                return filebase.Seek((int)offset, false);
            } else if (origin == SeekOrigin.Begin) {
                return filebase.Seek((int)offset, true);
            } else {
                throw new NotSupportedException("unsuported");
            }
        }

        public override int Read( byte[] buffer, int offset, int count) {
            var buff = new byte[count];
            int length = filebase.Read(ref buff, count);
            Array.Copy(buff, 0, buffer, offset, length);
            return length;
        }

        public override int ReadByte() {
            var array = new byte[1];
            return this.Read(array, 0, 1) == 0 ? -1 : (int)array[0];
        }

        public override void Flush() {
            // do nothing
        }

        public override void SetLength(long value) {
            throw new NotSupportedException("unsupported");
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotSupportedException("unsupported");
        }
    }
}
