FROM alpine:3.22

COPY --chown=root:root server/ssh /etc/ssh/
COPY --chown=root:root server/script /opt/sshnet
COPY user/sshnet /home/sshnet/.ssh

RUN apk update && apk upgrade --no-cache && \
    apk add --no-cache syslog-ng && \
    # install and configure sshd
    apk add --no-cache openssh && \
    # install openssh-server-pam to allow for keyboard-interactive authentication
    apk add --no-cache openssh-server-pam && \
    # must not use * for dos2unix parameter otherwise it tries to process folders too and fails
    dos2unix /etc/ssh/ssh*key && \
    chmod 400 /etc/ssh/ssh*key && \
    sed -i 's/#PasswordAuthentication yes/PasswordAuthentication yes/' /etc/ssh/sshd_config && \
    sed -i 's/#LogLevel\s*INFO/LogLevel DEBUG3/' /etc/ssh/sshd_config && \
    # Set the default RSA key
    echo 'HostKey /etc/ssh/ssh_host_rsa_key' >> /etc/ssh/sshd_config && \
    echo 'TrustedUserCAKeys /etc/ssh/user-ca.pub' >> /etc/ssh/sshd_config && \
    chmod 646 /etc/ssh/sshd_config && \
    # install and configure sudo
    apk add --no-cache sudo && \
    addgroup sudo && \
    # allow root to run any command
    echo 'root ALL=(ALL) ALL' > /etc/sudoers && \
    # allow everyone in the 'sudo' group to run any command without a password
    echo '%sudo ALL=(ALL) NOPASSWD:ALL' >> /etc/sudoers && \
    # add user to run most of the integration tests
    adduser -D sshnet && \
    passwd -u sshnet && \
    echo 'sshnet:ssh4ever' | chpasswd && \
    # must not use * for dos2unix parameter otherwise it tries to process folders too and fails
    dos2unix /home/sshnet/.ssh/*_key* && \
    chown -R sshnet:sshnet /home/sshnet && \
    chmod -R 700 /home/sshnet/.ssh && \
    chmod -R 644 /home/sshnet/.ssh/authorized_keys && \
    # add user to administer container (update configs, restart sshd)
    adduser -D sshnetadm && \
    passwd -u sshnetadm && \
    echo 'sshnetadm:ssh4ever' | chpasswd && \
    addgroup sshnetadm sudo && \
    dos2unix /opt/sshnet/* && \
    # install shadow package; we use chage command in this package to expire/unexpire password of the sshnet user
    apk add --no-cache shadow && \
    # allow us to use telnet command; we use this in the remote port forwarding tests
    apk --no-cache add busybox-extras && \
    # install full-fledged ps command
    apk add --no-cache procps

EXPOSE 22 22

ENTRYPOINT ["/opt/sshnet/start.sh"]
