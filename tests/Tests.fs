module Tests

open System
open Xunit
open FwRules.Domain
open FwRules.Policy
open FwRules.Formatters.IpTables

let http = 
    Services ("http",
        [
            Service("http", Tcp, Port (80us))
            Service("https", Tcp, Port (443us))
        ])
let ssh = Service ("ssh", Tcp, Port (22us))
let imap =
    Services ("imap", 
        [
            Service ("imap", Tcp, Port (143us))
            Service ("imap", Udp, Port (143us))
            Service ("imaps", Tcp, Port (993us))
            Service ("imaps", Udp, Port (9933us))
        ])

let loadBalancer = Host (IP("66.23.49.121"))
let www1 = Host (IP("192.168.0.101"))
let www2 = Host (IP("192.168.0.102"))
let www3 = Host (IP("192.168.0.103"))
let webservers = [www1; www2; www3]
let pg1 = Host (IP("192.168.5.20"))
let pg2 = Host (IP("192.168.5.21"))
let dbservers = [pg1; pg2]
let jumpBox = Host (IP("192.168.100.2"))
let corporateNetwork1 = Network (IP("66.23.49.0"), 24)
let corporateNetwork2 = Network (IP("66.23.52.0"), 24)

let lbPolicy = {
    Name = "corporate to load balancers"
    Service = http
    Source = [ corporateNetwork1; corporateNetwork2 ]
    Destination = [ loadBalancer ]
    Operation = Accept
}

let webPolicy = {
    Name = "lb to web servers"
    Service = http
    Source = [ loadBalancer ]
    Destination = webservers
    Operation = Accept
}
let jumpBoxPolicy = {
    Name = "jump boxes to servers"
    Service = ssh
    Source = [ jumpBox ]
    Destination = List.concat [[loadBalancer]; webservers; dbservers]
    Operation = Accept
}
let dbPolicy = {
    Name = "web to database servers"
    Service = Service ("pgsql", Tcp, Port (5432us))
    Source = webservers
    Destination = dbservers
    Operation = Accept
}
let denyAllPolicy = {
    Name = "deny all"
    Service = Service ("Anything", Tcp, Range (0us, 65535us))
    Source = []
    Destination = []
    Operation = Reject
}

[<Fact>]
let ``Proof of concept test - generate IP tables rules`` () =

    let buildIptablesRules (p:Policy) =
        let rules = p |> policyToRules None
        rules |> List.map (fun (rule:Rule) ->
            sprintf """iptables -A INPUT -p %s -s %s -d %s:%s -j %s -m comment --comment "%s: %s" """ rule.Protocol rule.Source rule.Destination rule.Port rule.Operation rule.PolicyName rule.RuleName
        )

    let buildIptablesPolicies (policies:Policy list) =
        let ipTables = 
            policies
            |> List.collect buildIptablesRules
        ipTables @ [ (sprintf """iptables -A INPUT -p %s -s %s -d %s -j REJECT -m comment --comment "deny anything else" """ All.Label "0.0.0.0/32" "0.0.0.0") ]

    let iptablesRules = [lbPolicy; webPolicy; jumpBoxPolicy; dbPolicy] |> buildIptablesPolicies
    let expectedRules = System.IO.File.ReadAllLines("SampleIptables.txt")
    iptablesRules |> List.iteri (fun idx rule -> Assert.Equal (expectedRules.[idx].Trim(), rule.Trim()))

[<Fact>]
let ``Generate rules for a specific host`` () =

    let buildRules (p:Policy) =
        let rules = p |> policyToRules (Some (IP ("192.168.0.102")))
        rules |> List.map (fun (rule:Rule) ->
            let chain = 
                if rule.Destination = "192.168.0.102" then "INPUT"
                else "OUTPUT"
            sprintf """iptables -A %s -p %s -s %s -d %s:%s -j %s -m comment --comment "%s: %s" """ chain rule.Protocol rule.Source rule.Destination rule.Port rule.Operation rule.PolicyName rule.RuleName
        )
    let iptablesRules = [lbPolicy; webPolicy; jumpBoxPolicy; dbPolicy] |> List.collect buildRules
    let expectedRules = System.IO.File.ReadAllLines("FilteredIptables.txt")
    iptablesRules |> List.iteri (fun idx rule -> Assert.Equal (expectedRules.[idx].Trim(), rule.Trim()))
