fwrules
=======

Builds `iptables` firewall rules from strongly typed policies.

The goal of the project is to define a network topology as a set of policies
that define the allowable communications so that firewall policies can be
generated from the topology.  Some ideas to make use of this:

* Generate lists of firewall rules in various formats
    - `iptables` on Linux
    - `ipfw` on FreeBSD
    - `easyrule` on pfSense
    - `netsh advfirewall` on Windows
* Request rules that are specific to a specific set of IP's to get the rules for a specific machine
* Generate a Kubernetes network policy
* Generate a set of NSX Edge firewall rules
* CLI to read and validate a policy from an external file - not embedded F# source - then generate above rules


Initial To Do
-------------

* Make modules with formatters for the different outputs - right now the types have `Label` fields that really only format for `iptables`.
* Set up CI
    - nupkg for library project
    - would be nice as a standalone .exe where possible (mkbundle)

Example output:
```
iptables -A INPUT -p tcp -s 66.23.49.0/24 -d 66.23.49.121:80 -j ACCEPT -m comment --comment "corporate to load balancers: http"
iptables -A INPUT -p tcp -s 66.23.52.0/24 -d 66.23.49.121:80 -j ACCEPT -m comment --comment "corporate to load balancers: http"
iptables -A INPUT -p tcp -s 66.23.49.0/24 -d 66.23.49.121:443 -j ACCEPT -m comment --comment "corporate to load balancers: https"
iptables -A INPUT -p tcp -s 66.23.52.0/24 -d 66.23.49.121:443 -j ACCEPT -m comment --comment "corporate to load balancers: https"
iptables -A INPUT -p tcp -s 66.23.49.121 -d 192.168.0.101:80 -j ACCEPT -m comment --comment "lb to web servers: http"
iptables -A INPUT -p tcp -s 66.23.49.121 -d 192.168.0.102:80 -j ACCEPT -m comment --comment "lb to web servers: http"
iptables -A INPUT -p tcp -s 66.23.49.121 -d 192.168.0.103:80 -j ACCEPT -m comment --comment "lb to web servers: http"
iptables -A INPUT -p tcp -s 66.23.49.121 -d 192.168.0.101:443 -j ACCEPT -m comment --comment "lb to web servers: https"
iptables -A INPUT -p tcp -s 66.23.49.121 -d 192.168.0.102:443 -j ACCEPT -m comment --comment "lb to web servers: https"
iptables -A INPUT -p tcp -s 66.23.49.121 -d 192.168.0.103:443 -j ACCEPT -m comment --comment "lb to web servers: https"
iptables -A INPUT -p tcp -s 192.168.100.2 -d 66.23.49.121:22 -j ACCEPT -m comment --comment "jump boxes to servers: ssh"
iptables -A INPUT -p tcp -s 192.168.100.2 -d 192.168.0.101:22 -j ACCEPT -m comment --comment "jump boxes to servers: ssh"
iptables -A INPUT -p tcp -s 192.168.100.2 -d 192.168.0.102:22 -j ACCEPT -m comment --comment "jump boxes to servers: ssh"
iptables -A INPUT -p tcp -s 192.168.100.2 -d 192.168.0.103:22 -j ACCEPT -m comment --comment "jump boxes to servers: ssh"
iptables -A INPUT -p tcp -s 192.168.100.2 -d 192.168.5.20:22 -j ACCEPT -m comment --comment "jump boxes to servers: ssh"
iptables -A INPUT -p tcp -s 192.168.100.2 -d 192.168.5.21:22 -j ACCEPT -m comment --comment "jump boxes to servers: ssh"
iptables -A INPUT -p tcp -s 192.168.0.101 -d 192.168.5.20:5432 -j ACCEPT -m comment --comment "web to database servers: pgsql"
iptables -A INPUT -p tcp -s 192.168.0.101 -d 192.168.5.21:5432 -j ACCEPT -m comment --comment "web to database servers: pgsql"
iptables -A INPUT -p tcp -s 192.168.0.102 -d 192.168.5.20:5432 -j ACCEPT -m comment --comment "web to database servers: pgsql"
iptables -A INPUT -p tcp -s 192.168.0.102 -d 192.168.5.21:5432 -j ACCEPT -m comment --comment "web to database servers: pgsql"
iptables -A INPUT -p tcp -s 192.168.0.103 -d 192.168.5.20:5432 -j ACCEPT -m comment --comment "web to database servers: pgsql"
iptables -A INPUT -p tcp -s 192.168.0.103 -d 192.168.5.21:5432 -j ACCEPT -m comment --comment "web to database servers: pgsql"
iptables -A INPUT -p all -s 0.0.0.0/32 -d 0.0.0.0 -j REJECT -m comment --comment "deny anything else"
```