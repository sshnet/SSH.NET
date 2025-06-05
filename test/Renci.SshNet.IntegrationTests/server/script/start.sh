#!/bin/sh
/usr/sbin/syslog-ng

# allow us to make changes to /etc/hosts; we need this for the port forwarding tests
chmod 777 /etc/hosts

# start PAM-enabled ssh daemon as we also want keyboard-interactive authentication to work
/usr/sbin/sshd.pam

tail -f < /var/log/auth.log
