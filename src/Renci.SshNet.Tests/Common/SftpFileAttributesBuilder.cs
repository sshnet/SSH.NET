using System;
using System.Collections.Generic;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Common
{
    public class SftpFileAttributesBuilder
    {
        private DateTime? _lastAccessTime;
        private DateTime? _lastWriteTime;
        private long? _size;
        private int? _userId;
        private int? _groupId;
        private uint? _permissions;
        private IDictionary<string, string> _extensions;

        public SftpFileAttributesBuilder()
        {
            _extensions = new Dictionary<string, string>();
        }

        public SftpFileAttributesBuilder WithLastAccessTime(DateTime lastAccessTime)
        {
            _lastAccessTime = lastAccessTime;
            return this;
        }

        public SftpFileAttributesBuilder WithLastWriteTime(DateTime lastWriteTime)
        {
            _lastWriteTime = lastWriteTime;
            return this;
        }

        public SftpFileAttributesBuilder WithSize(long size)
        {
            _size = size;
            return this;
        }

        public SftpFileAttributesBuilder WithUserId(int userId)
        {
            _userId = userId;
            return this;
        }

        public SftpFileAttributesBuilder WithGroupId(int groupId)
        {
            _groupId = groupId;
            return this;
        }

        public SftpFileAttributesBuilder WithPermissions(uint permissions)
        {
            _permissions = permissions;
            return this;
        }

        public SftpFileAttributesBuilder WithExtension(string name, string value)
        {
            _extensions.Add(name, value);
            return this;
        }

        public SftpFileAttributes Build()
        {
            if (_lastAccessTime == null)
                throw new ArgumentException();
            if (_lastWriteTime == null)
                throw new ArgumentException();
            if (_size == null)
                throw new ArgumentException();
            if (_userId == null)
                throw new ArgumentException();
            if (_groupId == null)
                throw new ArgumentException();
            if (_permissions == null)
                throw new ArgumentException();

            return new SftpFileAttributes(_lastAccessTime.Value,
                                          _lastWriteTime.Value,
                                          _size.Value,
                                          _userId.Value,
                                          _groupId.Value,
                                          _permissions.Value,
                                          _extensions);
        }
    }
}
