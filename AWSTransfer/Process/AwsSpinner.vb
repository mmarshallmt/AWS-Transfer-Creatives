Public NotInheritable Class AwsSpinner
    Private Shared lazy As New Lazy(Of AwsSpinner)(Function() New AwsSpinner())

    Public Shared Sub Reset()
        lazy = New Lazy(Of AwsSpinner)(Function() New AwsSpinner())
    End Sub

    Public Shared ReadOnly Property Instance() As AwsSpinner
        Get
            Return lazy.Value
        End Get
    End Property

    Private ReadOnly _consoleX As Integer
    Private ReadOnly _consoleY As Integer
    Private ReadOnly _frames As Char() = {"|"c, "/"c, "-"c, "\"c}
    Private _current As Integer

    Private Sub New()
        _current = 0
        _consoleX = Console.CursorLeft
        _consoleY = Console.CursorTop
    End Sub

    Public Sub Update()
        Console.Write(_frames(_current))
        Console.SetCursorPosition(_consoleX, _consoleY)

        If System.Threading.Interlocked.Increment(_current) >= _frames.Length Then
            _current = 0
        End If
    End Sub
End Class
