open System
open System.Net

type Endpoint =
    | Host of IPAddress
    | Network of IPAddress * SubnetNumber:int
    with
        member x.Label =
            match x with
            | Host (address) -> address.ToString ()
            | Network (address, subnetNumber) -> sprintf "%O/%i" address subnetNumber

type Protocol =
    | All
    | Tcp
    | Udp
    | Icmp
    with
        member x.Label =
            match x with
            | All -> "all"
            | Tcp -> "tcp"
            | Udp -> "udp"
            | Icmp -> "icmp"

type Operation =
    | Accept
    | Reject
    | Drop
    with
        member x.Label =
            match x with
            | Accept -> "ACCEPT"
            | Reject -> "REJECT"
            | Drop -> "DROP"

type Port =
    | Port of uint16
    | Range of Start:uint16 * End:uint16
    with
        member x.Label =
            match x with
            | Port p -> sprintf "%i" p
            | Range (startPort, endPort) -> sprintf "%i:%i" startPort endPort

type Service =
    | Service of Name:string * Protocol * Port
    | Services of Name:string * Service list

type Policy = {
    Name: string
    Service: Service
    Source: Endpoint list
    Destination : Endpoint list
    Operation : Operation
}

type Rule = {
    Operation : string
    Protocol : string
    Source: string
    Destination: string
    Port: string
    RuleName: string // The rule name
    PolicyName: string // The policy name
}

let policyToRules (p:Policy) =
    let rec getServiceRules (service:Service) =
        match service with
        | Service (serviceName, protocol, port) ->
            p.Source |> List.map (fun src -> 
                p.Destination |> List.map (fun dest ->
                    {
                        Operation = p.Operation.Label
                        Protocol = protocol.Label
                        Source = src.Label
                        Destination = dest.Label
                        Port = port.Label
                        RuleName = serviceName
                        PolicyName = p.Name
                    }
                )
            )
        | Services (serviceName, services) ->
            services |> List.map getServiceRules
        |> List.concat // Flatten rules into a single list
    p.Service |> getServiceRules

let IP = IPAddress.Parse

[<EntryPoint>]
let main argv =
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

    let printIptablesRules (p:Policy) =
        let rules = p |> policyToRules
        rules |> List.iter (fun (rule:Rule) ->
            printfn """iptables -A INPUT -p %s -s %s -d %s -dport %s %s --comment "%s: %s" """ rule.Protocol rule.Source rule.Destination rule.Port rule.Operation rule.PolicyName rule.RuleName
        )

    let printPolicies (policies:Policy list) =
        policies |> List.iter printIptablesRules
        printfn """iptables -A INPUT -p %s -s %s -d %s -dport %s REJECT --comment "deny anything else" """ All.Label "0.0.0.0/32" "0.0.0.0" "*"

    [lbPolicy; webPolicy; jumpBoxPolicy; dbPolicy] |> printPolicies
    
    0 // return an integer exit code
