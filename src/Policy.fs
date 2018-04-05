namespace FwRules

open System
open System.Net

module Policy =

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

    let policyToRules (filter:IPAddress option) (p:Policy) =
        let rec getServiceRules (service:Service) =
            match service with
            | Service (serviceName, protocol, port) ->
                p.Source |> List.map (fun src -> 
                    p.Destination
                    |> List.filter (fun dest ->
                        match filter with
                        | None -> true
                        | Some filterIp ->
                            match src, dest with
                            | Host srcIp, Host destIp when srcIp = filterIp || destIp = filterIp -> true
                            | Host srcIp, Network _ when srcIp = filterIp -> true
                            | Network _, Host destIp when destIp = filterIp -> true
                            | Network _, Network _ -> true
                            | _ -> false
                    )
                    |> List.map (fun dest ->
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
