namespace FwRules
open Domain

module Formatters =
    module IpTables =
        type Endpoint
            with
                member x.Label =
                    match x with
                    | Host (address) -> address.ToString ()
                    | Network (address, subnetNumber) -> sprintf "%O/%i" address subnetNumber

        type Protocol
            with
                member x.Label =
                    match x with
                    | All -> "all"
                    | Tcp -> "tcp"
                    | Udp -> "udp"
                    | Icmp -> "icmp"

        type Operation
            with
                member x.Label =
                    match x with
                    | Accept -> "ACCEPT"
                    | Reject -> "REJECT"
                    | Drop -> "DROP"
    
        type Port
            with
                member x.Label =
                    match x with
                    | Port p -> sprintf "%i" p
                    | Range (startPort, endPort) -> sprintf "%i:%i" startPort endPort
