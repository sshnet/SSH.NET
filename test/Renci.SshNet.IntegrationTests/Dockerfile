FROM alpine:latest

COPY --chown=root:root server/ssh /etc/ssh/
COPY --chown=root:root server/script /opt/sshnet
COPY user/sshnet /home/sshnet/.ssh

RUN apk update && apk upgrade --no-cache
RUN apk add --no-cache syslog-ng
    # install and configure sshd
RUN apk add --no-cache openssh
    # install openssh-server-pam to allow for keyboard-interactive authentication
RUN apk add --no-cache openssh-server-pam
RUN dos2unix /etc/ssh/*
RUN chmod 400 /etc/ssh/ssh*key
RUN sed -i 's/#PasswordAuthentication yes/PasswordAuthentication yes/' /etc/ssh/sshd_config
RUN sed -i 's/#LogLevel\s*INFO/LogLevel DEBUG3/' /etc/ssh/sshd_config
    # Set the default RSA key
RUN echo 'HostKey /etc/ssh/ssh_host_rsa_key' >> /etc/ssh/sshd_config
RUN chmod 646 /etc/ssh/sshd_config
    # install and configure sudo
RUN apk add --no-cache sudo
RUN addgroup sudo
    # allow root to run any command
RUN echo 'root ALL=(ALL) ALL' > /etc/sudoers
    # allow everyone in the 'sudo' group to run any command without a password
RUN echo '%sudo ALL=(ALL) NOPASSWD:ALL' >> /etc/sudoers
    # add user to run most of the integration tests
RUN adduser -D sshnet
RUN passwd -u sshnet
RUN echo 'sshnet:ssh4ever' | chpasswd
RUN dos2unix /home/sshnet/.ssh/*
RUN chown -R sshnet:sshnet /home/sshnet
RUN chmod -R 700 /home/sshnet/.ssh
RUN chmod -R 644 /home/sshnet/.ssh/authorized_keys
    # add user to administer container (update configs, restart sshd)
RUN adduser -D sshnetadm
RUN passwd -u sshnetadm
RUN echo 'sshnetadm:ssh4ever' | chpasswd
RUN addgroup sshnetadm sudo
RUN dos2unix /opt/sshnet/*
    # install shadow package; we use chage command in this package to expire/unexpire password of the sshnet user
RUN apk add --no-cache shadow
    # allow us to use telnet command; we use this in the remote port forwarding tests
RUN apk --no-cache add busybox-extras
    # install full-fledged ps command
RUN apk add --no-cache procps

EXPOSE 22 22

ENTRYPOINT ["/opt/sshnet/start.sh"]
