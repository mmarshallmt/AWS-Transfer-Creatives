Imports Amazon.S3
Imports Amazon
Imports Amazon.S3.Model
Imports System.IO
Imports System.Collections.Specialized
Imports System.Configuration
Public Class awsProcess

    Private BUCKET_NAME As String = AWSTransfer.My.MySettings.Default.BucketName
    Private Const S3_KEY As String = "s3_key"
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
    Private Property DefaultAWSPath As String = AWSTransfer.My.MySettings.Default.DefaultAWSPath
    Private Property IsConverted As Boolean
    Dim HasThumbnail As Boolean = False
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
        IsConverted = False
    End Sub

    Public Function GetS3Client() As AmazonS3Client

        Dim AWSAccessKey = AWSTransfer.My.MySettings.Default.AWSAccessKey
        Dim AWSSecretKey = AWSTransfer.My.MySettings.Default.AWSSecretKey
        Dim s3Client As AmazonS3Client = New AmazonS3Client(AWSAccessKey, AWSSecretKey, Amazon.RegionEndpoint.USEast1)

        Return s3Client
    End Function

    Private Function UrlAwsFormat(ByVal url As String) As String
        Return url.Replace("\", "/")
    End Function

    Private Function PathURLFormat(ByVal url As String) As String
        Return url.Replace("/", "\")
    End Function

    Public Sub CreateNewFolder(client As AmazonS3Client, Path As String)
        Dim S3_KEY As [String] = DefaultAWSPath + Path
        Dim request As New PutObjectRequest()
        request.BucketName = BUCKET_NAME
        request.Key = S3_KEY
        request.ContentBody = "CreativeAssets"
        client.PutObject(request)
    End Sub

    Public Sub CreateNewFileInFolder(client As AmazonS3Client)

        Dim S3_KEY As [String] = "Demo Create Folder/" + "Demo Create File.txt"
        Dim request As New PutObjectRequest()
        request.BucketName = BUCKET_NAME
        request.Key = S3_KEY
        request.ContentBody = "This is body of S3 object."
        client.PutObject(request)
    End Sub

    Public Sub DeleteFile(Client As AmazonS3Client, toPath As String)
        Dim request As New DeleteObjectRequest()
        request.BucketName = BUCKET_NAME
        request.Key = DefaultAWSPath + toPath
        Client.DeleteObject(request)
    End Sub

    Public Function UploadFile(client As AmazonS3Client, FromPath As String, ToPath As String) As Boolean
        Try
            Dim contentType As String
            Dim objTaggList As List(Of Tag)

            objTaggList = New List(Of Tag)() From {New Tag() With {
            .Key = "cft-createdby QC UPLOADER",
            .Value = "cft-createdate " + Now().Date
        }}
            If HasThumbnail = False Then
                contentType = GetContentType(Path.GetExtension(FromPath))
            End If


            'S3_KEY is name of file we want upload
            Dim request As New PutObjectRequest()
            request.BucketName = BUCKET_NAME
            request.Key = DefaultAWSPath + ToPath
            request.ContentType = contentType
            request.CannedACL = S3CannedACL.Private
            request.FilePath = FromPath
            request.StorageClass = "Standard"
            request.TagSet = objTaggList

            'UPload Images 
            client.PutObject(request)

            IsConverted = True
            'Process to update Logg
            If HasThumbnail = False Then
                Aws_Creative_Key = request.Key
                file_Format = Path.GetExtension(FromPath).Replace(".", "")
                MimeType = contentType
                ViewType = GetViewType(Path.GetExtension(FromPath.ToLower))

            Else
                Aws_Thumbnail_Key = request.Key
            End If
            Return True
        Catch ex As Exception
            Console.WriteLine("AWS Issue with file {0}", FromPath)
            Return False
        End Try

    End Function

    Public Function FolderExistCheck(Path As String) As Boolean
        Using s3Client As AmazonS3Client = GetS3Client()
            Try

                Dim findFolderRequest As New ListObjectsRequest()
                findFolderRequest.BucketName = BUCKET_NAME
                'findFolderRequest.Delimiter = "/";
                findFolderRequest.Prefix = DefaultAWSPath + Path
                Dim findFolderResponse As ListObjectsResponse = s3Client.ListObjects(findFolderRequest)
                Dim commonPrefixes As List(Of [String]) = findFolderResponse.CommonPrefixes
                Dim folderExists As [Boolean] = commonPrefixes.Any()
                Return True
            Catch e As AmazonS3Exception
                Return False
                Console.WriteLine("Folder existence check has failed.")
                Console.WriteLine("Amazon error code: {0}", If(String.IsNullOrEmpty(e.ErrorCode), "None", e.ErrorCode))
                Console.WriteLine("Exception message: {0}", e.Message)
            End Try
        End Using
    End Function

    Public Function FileExistCheck(Path As String) As Boolean
        Dim SKey As String = DefaultAWSPath + Path
        Using client As AmazonS3Client = GetS3Client()
            Dim request As New ListObjectsRequest()
            request.BucketName = BUCKET_NAME
            request.Prefix = SKey
            Dim response As ListObjectsResponse = client.ListObjects(request)

            For Each o As S3Object In response.S3Objects
                    If o.Key = SKey Then
                        Return True
                    End If
                Next
            Return False
        End Using
    End Function

    Public Function TransferProcess(MediaType As String, AdID As Integer, CreativeDetailID As Integer, OccurrenceID As Integer, CreativeSignature As String, RemoteCreativeFilePath As String, RemoteCreativeThumbFilePath As String, FileType As String) As Boolean
        Dim FilePath, toPath, toPathThumb As String
        Dim RemotePath As String = AWSTransfer.My.MySettings.Default.RemotePath
        FilePath = RemoteCreativeFilePath
        toPath = UrlAwsFormat(RemoteCreativeFilePath.Replace(RemotePath.ToLower(), ""))

        If MediaType = "TV" Then
            If FileType <> "AVI" Then

                If String.IsNullOrEmpty(RemoteCreativeFilePath) = False Then
                    UploadAwsTV(False, FilePath, CreativeSignature)

                End If
                If String.IsNullOrEmpty(RemoteCreativeThumbFilePath) = False Then
                    UploadAwsTV(True, RemoteCreativeThumbFilePath, CreativeSignature)
                End If
            Else
                RemotePath = RemotePath.Replace("CreativeAssets\", "")

                Dim tempThumbPth As String = RemoteCreativeThumbFilePath.Replace(RemotePath.ToLower, "")
                toPathThumb = UrlAwsFormat(tempThumbPth)
                toPath = UrlAwsFormat(RemoteCreativeFilePath.Replace(RemotePath.ToLower(), ""))
                If String.IsNullOrEmpty(RemoteCreativeFilePath) = False Then
                    UploadAwsOnlineDisplay(False, "/" + toPath, FilePath)
                End If
                If String.IsNullOrEmpty(RemoteCreativeThumbFilePath) = False Then
                    UploadAwsOnlineDisplay(True, "/" + toPathThumb.ToUpper(), RemoteCreativeThumbFilePath)
                End If

            End If


        ElseIf MediaType = "RAD" Or MediaType = "PUB" Then

                If String.IsNullOrEmpty(RemoteCreativeFilePath) = False Then
                    UploadAwsRadioPrint(False, toPath, FilePath)

                End If
                toPathThumb = UrlAwsFormat(RemoteCreativeThumbFilePath.Replace(RemotePath.ToLower(), ""))
                If String.IsNullOrEmpty(RemoteCreativeThumbFilePath) = False Then
                    If toPathThumb.Contains("mid") = True Then
                        toPathThumb = toPathThumb.Replace("mid", "thumb")
                        'ElseIf toPathThumb.Contains("print_mid") = True Then
                        '    toPathThumb = toPathThumb.Replace("print_mid", "print_thumb")
                    End If
                    UploadAwsRadioPrint(True, toPathThumb, RemoteCreativeThumbFilePath)
                End If


            ElseIf MediaType = "OND" Then
                Dim tempThumbPth As String = RemoteCreativeThumbFilePath.Replace(RemotePath.ToLower, "")
                toPathThumb = UrlAwsFormat(tempThumbPth)

            If String.IsNullOrEmpty(RemoteCreativeFilePath) = False Then
                UploadAwsOnlineDisplay(False, toPath, FilePath)
                End If
            If String.IsNullOrEmpty(RemoteCreativeThumbFilePath) = False Then
                If toPathThumb.Contains("mid") = True Then
                    toPathThumb = toPathThumb.Replace("mid", "thumb")
                End If
                UploadAwsOnlineDisplay(True, toPathThumb.ToUpper(), RemoteCreativeThumbFilePath)
            End If



        Else


                If String.IsNullOrEmpty(RemoteCreativeThumbFilePath) = False Then
                toPathThumb = UrlAwsFormat(RemoteCreativeThumbFilePath).Substring(26)
            End If

            If FolderExistCheck(UrlAwsFormat(FilePath.Replace(RemotePath.ToLower(), ""))) = True Then
                'UploadFile(GetS3Client(), FilePath, toPath)
                If String.IsNullOrEmpty(RemoteCreativeThumbFilePath) = False Then
                    'UploadFile(GetS3Client(), RemoteCreativeThumbFilePath, toPathThumb)
                End If
            Else
                CreateNewFolder(GetS3Client(), FilePath)
                'UploadFile(GetS3Client(), RemoteCreativeThumbFilePath, toPathThumb)
            End If
        End If
        Return True
    End Function
    Private Sub UploadAwsRadioPrint(hasThumb As Boolean, toPath As String, FilePath As String)
        Dim RADFolderPath As String
        RADFolderPath = "/" + UrlAwsFormat(Path.GetDirectoryName(toPath)) + "/"
        If FolderExistCheck(RADFolderPath) = False Then
            CreateNewFolder(GetS3Client(), RADFolderPath)
        End If

        If FileExistCheck("/" + toPath) = False Then
            HasThumbnail = hasThumb
            If UploadFile(GetS3Client(), FilePath, "/" + toPath) = False Then
                IsConverted = False
            End If
        Else
            IsConverted = True
            HasThumbnail = hasThumb
            UploadFileData(FilePath, "/" + toPath)
        End If

    End Sub

    Private Sub UploadAwsOnlineDisplay(hasThumb As Boolean, toPath As String, FilePath As String)
        Dim ONDFolderPath As String
        ONDFolderPath = UrlAwsFormat(Path.GetDirectoryName(toPath)) + "/"
        If FolderExistCheck(ONDFolderPath.ToUpper()) = False Then
            CreateNewFolder(GetS3Client(), ONDFolderPath.ToUpper())
        End If

        If FileExistCheck(toPath) = False Then
            HasThumbnail = hasThumb
            UploadFile(GetS3Client(), FilePath, toPath.ToUpper())
        Else
            IsConverted = True
            HasThumbnail = hasThumb
            UploadFileData(FilePath, "/" + toPath)
        End If
        If Path.GetExtension(toPath) = ".MP4" Then
            Dim RemoveFile As String = Path.ChangeExtension(toPath.ToUpper(), "MPG")
            DeleteFile(GetS3Client(), RemoveFile)
        End If
    End Sub

    Private Sub UploadAwsTV(hasThumb As Boolean, FilePath As String, CreativeSignature As String)
        Dim TVFolderPath As String = ""
        Dim ExistPath As String = ""
        TVFolderPath = "/TV/" + CreativeSignature + "/" + GetTVFolder(Path.GetExtension(FilePath.ToLower)) + "/"
        ExistPath = UrlAwsFormat(Path.GetDirectoryName(TVFolderPath) + "/")
        If FolderExistCheck(ExistPath) = False Then

            CreateNewFolder(GetS3Client(), ExistPath)
        End If
        Dim NewFileName = Path.GetFileName(FilePath)
        TVFolderPath = TVFolderPath + NewFileName

        If FileExistCheck(TVFolderPath) = False Then
            HasThumbnail = hasThumb
            If UploadFile(GetS3Client(), FilePath, TVFolderPath) = False Then
                IsConverted = False
            End If
        Else
            IsConverted = True
            HasThumbnail = hasThumb
            UploadFileData(FilePath, TVFolderPath)
        End If

        If Path.GetExtension(TVFolderPath) = ".MP4" Then
            Dim RemoveFile As String = Path.ChangeExtension(TVFolderPath.ToUpper(), "MPG")
            DeleteFile(GetS3Client(), RemoveFile)
        End If
    End Sub

    Public Shared Function GetContentType(fileExtension As String) As String
        Dim mimeTypes = New Dictionary(Of [String], [String])() From {
            {".bmp", "image/bmp"},
            {".gif", "image/gif"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".png", "image/png"},
            {".tif", "image/tiff"},
            {".tiff", "image/tiff"},
            {".mp3", "audio/mpeg"},
            {".wma", "audio/wma"},
            {".wav", "audio/wav"},
            {".wmv", "audio/wmv"},
            {".swf", "application/x-shockwave-flash"},
            {".avi", "video/avi"},
            {".mp4", "video/mp4"},
            {".mpeg", "video/mpeg"},
            {".mpg", "video/mpeg"},
            {".qt", "video/quicktime"}
        }

        ' if the file type is not recognized, return "application/octet-stream" so the browser will simply download it
        Return If(mimeTypes.ContainsKey(fileExtension.ToLower()), mimeTypes(fileExtension.ToLower()), "application/octet-stream")
    End Function

    Public Shared Function GetViewType(fileExtension As String) As String
        Dim mimeTypes = New Dictionary(Of [String], [String])() From {
            {".bmp", "Static"},
            {".gif", "Static"},
            {".jpeg", "Static"},
            {".jpg", "Static"},
            {".png", "Static"},
            {".mp3", "Streaming"},
            {".wma", "Streaming"},
            {".wav", "Streaming"},
            {".wmv", "Streaming"},
            {".swf", "Streaming"},
            {".avi", "Streaming"},
            {".mp4", "Streaming"},
            {".mpeg", "Streaming"},
            {".mpg", "Streaming"},
            {".qt", "Streaming"}
        }

        Return If(mimeTypes.ContainsKey(fileExtension), mimeTypes(fileExtension), "application/octet-stream")
    End Function

    Public Function Tranfered() As Boolean
        Return IsConverted
    End Function

    Public Function GetTVFolder(fileExtension) As String
        Dim folderValue As String
        Select Case fileExtension
            Case ".mp3"
                folderValue = "MP3"
            Case ".jpg"
                folderValue = "JPG"
            Case ".wav"
                folderValue = "WAV"
            Case ".mpg"
                folderValue = "HQ"
            Case ".mp4"
                folderValue = "HQ"
        End Select
        Return folderValue
    End Function

    Public Sub UploadFileData(FromPath As String, ToPath As String)

        Dim contentType As String

        contentType = GetContentType(Path.GetExtension(FromPath))

        Dim awsKey = DefaultAWSPath + ToPath

        IsConverted = True
        'Process to update Logg
        file_Format = Path.GetExtension(FromPath).Replace(".", "")
        MimeType = contentType
        If HasThumbnail = False Then
            Aws_Creative_Key = awsKey
        Else
            Aws_Thumbnail_Key = awsKey
        End If
        ViewType = GetViewType(Path.GetExtension(FromPath.ToLower))

    End Sub

    Public Function DownloadFile(ByVal fileName As String) As Boolean
        Using s3Client As AmazonS3Client = GetS3Client()
            Dim request As GetObjectRequest = New GetObjectRequest With {.BucketName = BUCKET_NAME, .Key = fileName}
            Using response As GetObjectResponse = s3Client.GetObject(request)
                Dim dest As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName)
                If Not File.Exists(dest) Then
                    response.WriteResponseStreamToFile(dest)
                End If
            End Using
        End Using

        Return True
    End Function



End Class
