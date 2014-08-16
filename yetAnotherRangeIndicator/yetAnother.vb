Imports LeagueSharp
Imports System.Drawing
Imports System.Windows.Input

Public Class yetAnotherRangeIndicator

    'Script Information
    Shared versionNumber As String = "1.0.0"

    Overloads Shared Sub Main(ByVal arg() As String)
        AddHandler Drawing.OnDraw, AddressOf Drawing_OnDraw
        AddHandler Game.OnGameUpdate, AddressOf Game_OnTick

        'Script Information
        Console.Write(Environment.NewLine & "Started yetAnotherRangeIndicator" & ", Version: " & versionNumber)
        Game.PrintChat("yetAnotherRangeIndicator" & ", Version: " & versionNumber)

    End Sub

    Public Shared Sub Drawing_OnDraw(ByVal args As EventArgs)
		'On Draw - Drawing Happens Here
        If System.Windows.Input.Keyboard.IsKeyToggled(Key.Numpad1) Then
            drawOnEnemy()
        End If
    End Sub

    Public Shared Sub Game_OnTick(ByVal args As EventArgs)
        'On Tick - Maths go here
        
    End Sub

    '-------------------------------------------------------------------------------------------------------------------------------------------------------------'
    'Actual Methods
	'-------------------------------------------------------------------------------------------------------------------------------------------------------------
    Public Shared Sub drawOnEnemy()
        For Each target As Obj_AI_Hero In ObjectManager.Get(Of Obj_AI_Hero)()
            If target.IsValid And target.IsEnemy And target.IsDead = False And target.IsVisible Then
                Drawing.DrawCircle(target.Position, target.AttackRange, System.Drawing.Color.AliceBlue)
            End If
            If target.Health - (DamageLib.getDmg(ObjectManager.Player, SpellType.Q) + (2 * DamageLib.getDmg(ObjectManager.Player, SpellType.AD))) <= 0 Then
				Drawing.DrawText(target.Position.X, target.Position.Y, Color.DarkRed, "Enemy Ready")
            End If
        Next
	End Sub

	'Add more methods and functions

End Class
