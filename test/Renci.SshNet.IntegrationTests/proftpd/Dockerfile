# ProFTPD with mod_sftp, used by ProFtpdRekeyTests to reproduce
# https://github.com/sshnet/SSH.NET/issues/1764 (alpine:3.24 ships ProFTPD 1.3.9c).
FROM alpine:3.24
RUN apk add --no-cache proftpd proftpd-mod_sftp openssh-keygen
RUN ssh-keygen -t rsa -b 3072 -N "" -m PEM -f /etc/proftpd/host_rsa
RUN chmod 400 /etc/proftpd/host_rsa
RUN adduser -D sshnet
RUN echo 'sshnet:ssh4ever' | chpasswd
COPY proftpd/proftpd.conf /etc/proftpd/proftpd.conf
EXPOSE 22
CMD ["proftpd", "--nodaemon", "--config", "/etc/proftpd/proftpd.conf"]
