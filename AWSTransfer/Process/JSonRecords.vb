Public Class JSonRecords
    Public Property Creative_id As Integer
    Public Property Aws_Creative_Key As String
    Public Property Aws_Thumbnail_Key As String
    Public Property Creative_Domain_Name As String
    Public Property Creative_Domain_Code As String
    Public Property file_Format As String
    Public Property MimeType As String
    Public Property ViewType As String
    Public Property Player_Mode As String
    Public Property OrderID As Integer

    Private Sub init()
        Creative_id = 0
        Aws_Creative_Key = ""
        Aws_Thumbnail_Key = ""
        Creative_Domain_Code = ""
        Creative_Domain_Name = ""
        file_Format = ""
        MimeType = ""
        ViewType = ""
        Player_Mode = ""
        OrderID = 0

    End Sub

End Class
