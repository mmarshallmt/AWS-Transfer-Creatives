Imports System.IO

Module Module1
    Private objMCAPDataAccess As New DataProcess()
    Private objConvertFile As New ConvertVideoFiles()
    Sub Main()
        Call StartConverting()
    End Sub
    Private Function StartConverting()
        Dim dtGetAws As New DataTable()

        Dim CreativeDetailID As Integer, MediaType As String
        Dim RemoteCreativeFilePath As String
        Dim RemoteCreativeThumbFilePath As String


        Dim RemotePath As String = AwsConvertMp4.My.MySettings.Default.RemotePath

        dtGetAws = objMCAPDataAccess.GetFilesToConvertToAws()
        Dim i As Integer = 0

        For Each dr As DataRow In dtGetAws.Rows


            MediaType = dr("MediaType")
            CreativeDetailID = dr("CreativeDetailID")
            If IsDBNull(dr("FileLocation")) = False Then
                RemoteCreativeFilePath = dr("FileLocation").tolower

                If RemoteCreativeFilePath.Contains("tv\testcreatives") = True And RemoteCreativeFilePath.Contains("\\192.168.3.126\canada_assets\creativeassets\") = False Then
                    RemoteCreativeFilePath = RemotePath + RemoteCreativeFilePath
                ElseIf RemoteCreativeFilePath.Contains("\\napfileserver3.comptrk.com") = True Then
                    RemoteCreativeFilePath = RemoteCreativeFilePath.Replace(RemotePath.ToLower, "")
                ElseIf RemoteCreativeFilePath.Contains("\\10.0.100.71\tv_cap2mcq") = True Then
                    RemoteCreativeFilePath = RemoteCreativeFilePath.Replace(RemotePath.ToLower, "")
                End If

                If IfFileExist(RemoteCreativeFilePath) = True Then
                    If Path.GetExtension(RemoteCreativeFilePath).ToLower = ".mpg" Then
                        objConvertFile.FileToConvert = RemoteCreativeFilePath
                        If IfFileExist(Path.ChangeExtension(RemoteCreativeFilePath, ".MP4")) = False Then
                            If objConvertFile.StartConversion() = True Then

                                RemoteCreativeFilePath = Path.ChangeExtension(RemoteCreativeFilePath, ".MP4")
                                If objMCAPDataAccess.UpdateConvertedData(MediaType, CreativeDetailID, RemoteCreativeFilePath) = True Then
                                    Console.WriteLine("Data Updated for {0} Successfully", RemoteCreativeFilePath)
                                End If
                            End If
                        Else
                            RemoteCreativeFilePath = Path.ChangeExtension(RemoteCreativeFilePath, ".MP4")
                            If objMCAPDataAccess.UpdateConvertedData(MediaType, CreativeDetailID, RemoteCreativeFilePath) = True Then
                                Console.WriteLine("Data Updated for {0} Successfully", RemoteCreativeFilePath)
                            End If
                        End If
                    Else
                        objMCAPDataAccess.UpdateFileNotFound(MediaType, CreativeDetailID)
                    End If
                Else
                    ' objMCAPDataAccess.UpdateFileNotFound(MediaType, CreativeDetailID)
                    Console.WriteLine(" {0} Failed", RemoteCreativeFilePath)
                End If
            Else
                RemoteCreativeFilePath = ""

            End If
        Next
    End Function
    Private Function IfFileExist(ByVal _fileToCheck As String) As Boolean
        Dim fileExist As Boolean = False
        If File.Exists(_fileToCheck) = True Then
            fileExist = True
        End If
        Return fileExist
    End Function

End Module
