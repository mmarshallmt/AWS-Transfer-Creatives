Imports System.Threading

Class ConsoleSpinner
    Private increment As Boolean = True, [loop] As Boolean = False
    Private counter As Integer = 0
    Private delay As Integer
    Private sequence As String()
    Private active As Boolean
    Private ReadOnly thread As Thread

    Public Sub New(Optional sSequence As String = "dots", Optional iDelay As Integer = 80, Optional bLoop As Boolean = False)
        delay = iDelay
        If sSequence = "dots" Then
            sequence = New String() {".   ", "..  ", "... ", "....", ".....", "......"}
            [loop] = True
        ElseIf sSequence = "slashes" Then
            sequence = New String() {"/", "-", "\", "|"}
        ElseIf sSequence = "circles" Then
            sequence = New String() {".", "o", "0", "o"}
        ElseIf sSequence = "crosses" Then
            sequence = New String() {"+", "x"}
        ElseIf sSequence = "arrows" Then
            sequence = New String() {"V", "<", "^", ">"}
        End If
        thread = New Thread(AddressOf Spin)
    End Sub

    Public Sub Start()
        active = True
        If Not Thread.IsAlive Then
            Thread.Start()
        End If
    End Sub

    Public Sub [Stop]()
        active = False
        Draw(" "c)
    End Sub

    Private Sub Spin()
        While active
            Turn()
            Thread.Sleep(delay)
        End While
    End Sub
    Private Sub Draw(c As Char)
        Console.SetCursorPosition(2, 2)
        'Console.ForegroundColor = ConsoleColor.Red
        Console.Write(c)
    End Sub
    Public Sub Turn()
        If [loop] Then
            If counter >= sequence.Length - 1 Then
                increment = False
            End If
            If counter <= 0 Then
                increment = True
            End If

            If increment Then
                counter += 1
            ElseIf Not increment Then
                counter -= 1
            End If
        Else
            counter += 1

            If counter >= sequence.Length Then
                counter = 0
            End If
        End If

        Console.Write(sequence(counter))
        Console.SetCursorPosition(Console.CursorLeft - sequence(counter).Length, Console.CursorTop)

        System.Threading.Thread.Sleep(delay)
    End Sub
End Class