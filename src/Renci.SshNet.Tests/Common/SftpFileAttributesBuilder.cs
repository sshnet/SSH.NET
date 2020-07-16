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
        private readonly IDictionary<string, string> _extensions;

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
                _lastAccessTime = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            else if (_lastAccessTime.Value.Kind != DateTimeKind.Utc)
                _lastAccessTime = _lastAccessTime.Value.ToUniversalTime();

            if (_lastWriteTime == null)
                _lastWriteTime = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            else if (_lastWriteTime.Value.Kind != DateTimeKind.Utc)
                _lastWriteTime = _lastWriteTime.Value.ToUniversalTime();

            if (_size == null)
                _size = 0;
            if (_userId == null)
                _userId = 0;
            if (_groupId == null)
                _groupId = 0;
            if (_permissions == null)
                _permissions = 0;

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
