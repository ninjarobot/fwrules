namespace FwRules

open System
open System.Net
open Domain
open Formatters.IpTables

module Policy =

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
