' 06-01-06

Public Enum TransportProtocol As Integer
    Tcp
    Udp
    Serial
    File
End Enum

Public Enum CRCCheckType As Integer
    None
    CRC16
    CRC32
    CRC_CCITT
End Enum