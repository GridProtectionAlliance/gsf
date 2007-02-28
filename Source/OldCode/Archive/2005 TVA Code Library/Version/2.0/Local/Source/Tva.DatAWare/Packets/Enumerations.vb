'''' <summary>
'''' Specifies the action to be taken on the packet.
'''' </summary>
'Public Enum PacketActionType
'    ''' <summary>
'    ''' The packet is to be saved and no reply to the sender is required.
'    ''' </summary>
'    SaveOnly
'    ''' <summary>
'    ''' A reply is to be sent to the sender and the packet is not to be saved anywhere.
'    ''' </summary>
'    ReplyOnly
'    ''' <summary>
'    ''' The packet is to be saved and a reply is to be sent to the sender.
'    ''' </summary>
'    SaveAndReply
'End Enum

'''' <summary>
'''' Specifies where the packet is to be saved.
'''' </summary>
'Public Enum PacketSaveLocation
'    ''' <summary>
'    ''' The packet is not to be saved anywhere.
'    ''' </summary>
'    None
'    ''' <summary>
'    ''' The packet is to be saved to the archive file.
'    ''' </summary>
'    ArchiveFile
'    ''' <summary>
'    ''' The packet is to be saved to the metadata file.
'    ''' </summary>
'    MetadataFile
'End Enum