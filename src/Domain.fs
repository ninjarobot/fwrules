namespace FwRules

open System.Net

module Domain =

    type Endpoint =
        | Host of IPAddress
        | Network of IPAddress * SubnetNumber:int

    type Protocol =
        | All
        | Tcp
        | Udp
        | Icmp

    type Operation =
        | Accept
        | Reject
        | Drop

    type Port =
        | Port of uint16
        | Range of Start:uint16 * End:uint16

    type Service =
        | Service of Name:string * Protocol * Port
        | Services of Name:string * Service list
