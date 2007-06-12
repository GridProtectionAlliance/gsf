Public Interface IBinaryDataConsumer

    Function Initialize(ByVal binaryImage As Byte()) As Integer

    Function Initialize(ByVal binaryImage As Byte(), ByVal startIndex As Integer) As Integer

End Interface
