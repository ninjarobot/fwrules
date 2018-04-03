fwrules
=======

Builds `iptables` firewall rules from strongly typed policies.

Example output:
```
iptables -A INPUT -p tcp -s 66.23.49.0/24 -d 66.23.49.121 -dport 80 ACCEPT --comment "corporate to load balancers: http"
iptables -A INPUT -p tcp -s 66.23.52.0/24 -d 66.23.49.121 -dport 80 ACCEPT --comment "corporate to load balancers: http"
iptables -A INPUT -p tcp -s 66.23.49.0/24 -d 66.23.49.121 -dport 443 ACCEPT --comment "corporate to load balancers: https"
iptables -A INPUT -p tcp -s 66.23.52.0/24 -d 66.23.49.121 -dport 443 ACCEPT --comment "corporate to load balancers: https"
iptables -A INPUT -p tcp -s 66.23.49.121 -d 192.168.0.101 -dport 80 ACCEPT --comment "lb to web servers: http"
iptables -A INPUT -p tcp -s 66.23.49.121 -d 192.168.0.102 -dport 80 ACCEPT --comment "lb to web servers: http"
iptables -A INPUT -p tcp -s 66.23.49.121 -d 192.168.0.103 -dport 80 ACCEPT --comment "lb to web servers: http"
iptables -A INPUT -p tcp -s 66.23.49.121 -d 192.168.0.101 -dport 443 ACCEPT --comment "lb to web servers: https"
iptables -A INPUT -p tcp -s 66.23.49.121 -d 192.168.0.102 -dport 443 ACCEPT --comment "lb to web servers: https"
iptables -A INPUT -p tcp -s 66.23.49.121 -d 192.168.0.103 -dport 443 ACCEPT --comment "lb to web servers: https"
iptables -A INPUT -p tcp -s 192.168.100.2 -d 66.23.49.121 -dport 22 ACCEPT --comment "jump boxes to servers: ssh"
iptables -A INPUT -p tcp -s 192.168.100.2 -d 192.168.0.101 -dport 22 ACCEPT --comment "jump boxes to servers: ssh"
iptables -A INPUT -p tcp -s 192.168.100.2 -d 192.168.0.102 -dport 22 ACCEPT --comment "jump boxes to servers: ssh"
iptables -A INPUT -p tcp -s 192.168.100.2 -d 192.168.0.103 -dport 22 ACCEPT --comment "jump boxes to servers: ssh"
iptables -A INPUT -p tcp -s 192.168.100.2 -d 192.168.5.20 -dport 22 ACCEPT --comment "jump boxes to servers: ssh"
iptables -A INPUT -p tcp -s 192.168.100.2 -d 192.168.5.21 -dport 22 ACCEPT --comment "jump boxes to servers: ssh"
iptables -A INPUT -p tcp -s 192.168.0.101 -d 192.168.5.20 -dport 5432 ACCEPT --comment "web to database servers: pgsql"
iptables -A INPUT -p tcp -s 192.168.0.101 -d 192.168.5.21 -dport 5432 ACCEPT --comment "web to database servers: pgsql"
iptables -A INPUT -p tcp -s 192.168.0.102 -d 192.168.5.20 -dport 5432 ACCEPT --comment "web to database servers: pgsql"
iptables -A INPUT -p tcp -s 192.168.0.102 -d 192.168.5.21 -dport 5432 ACCEPT --comment "web to database servers: pgsql"
iptables -A INPUT -p tcp -s 192.168.0.103 -d 192.168.5.20 -dport 5432 ACCEPT --comment "web to database servers: pgsql"
iptables -A INPUT -p tcp -s 192.168.0.103 -d 192.168.5.21 -dport 5432 ACCEPT --comment "web to database servers: pgsql"
iptables -A INPUT -p all -s 0.0.0.0/32 -d 0.0.0.0 -dport * REJECT --comment "deny anything else"
```