'    Copyright (C) 2007 TibiaTek Development Team
'
'    This file is part of TibiaTek Bot.
'
'    TibiaTek Bot is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.
'
'    TibiaTek Bot is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.
'
'    You should have received a copy of the GNU General Public License
'    along with TibiaTek Bot. If not, see http://www.gnu.org/licenses/gpl.txt
'    or write to the Free Software Foundation, 59 Temple Place - Suite 330,
'    Boston, MA 02111-1307, USA.

Imports System.Math, System.Xml, Scripting

Public Module WalkerModule

    Public Class Walker
		Public Coordinates As ITibia.LocationDefinition
        Public Type As WaypointType
        Public Info As String
        Public IsReady As Boolean

        Public Enum WaypointType
            Walk = 0
            StairsOrHole = 1
            Rope = 2
            Ladder = 3
            Say = 4
            Wait = 5
            Sewer = 6
            Shovel = 7
        End Enum

        Public Enum Directions
            Left = 1
            Down = 2
            Right = 3
            Up = 4
        End Enum


        Public Function MoveChar() As Boolean
            Try
                Dim BL As New BattleList
                Dim TD As New TileData
                BL.Reset()
                TD.Reset()
				BL.JumpToEntity(IBattlelist.SpecialEntity.Myself)
                TD.JumpToTile(TileData.SpecialTile.Myself)
                Dim StatusText As String = ""
                Dim StatusTimer As Integer = 0
                Kernel.Client.ReadMemory(Consts.ptrStatusMessage, StatusText)
                Kernel.Client.ReadMemory(Consts.ptrStatusMessageTimer, StatusTimer, 4)
                Kernel.Client.ReadMemory(Consts.ptrCoordX, Kernel.CharacterLoc.X, 4)
                Kernel.Client.ReadMemory(Consts.ptrCoordY, Kernel.CharacterLoc.Y, 4)
                Kernel.Client.ReadMemory(Consts.ptrCoordZ, Kernel.CharacterLoc.Z, 1)
                Dim SP As New ServerPacketBuilder(Kernel.Proxy)
                If StatusText = "There is no way." And StatusTimer <> 0 Then
                    'BL.JumpToEntity(SpecialEntity.Myself)
                    If BL.IsWalking = False Then
                        Kernel.Client.WriteMemory(Consts.ptrGoToX, BL.GetLocation.X, 2)
                        Kernel.Client.WriteMemory(Consts.ptrGoToY, BL.GetLocation.Y, 2)
                        Kernel.Client.WriteMemory(Consts.ptrGoToZ, BL.GetLocation.Z, 1)
                        BL.IsWalking = True
                        IsReady = False
                        Return False
                    End If
                End If

                Select Case Type
                    Case WaypointType.Walk
                        If Kernel.CharacterLoc.X = Coordinates.X AndAlso Kernel.CharacterLoc.Y = Coordinates.Y AndAlso Kernel.CharacterLoc.Z = Coordinates.Z Then
                            IsReady = True
                            Return True
                        Else
                            If BL.IsWalking = False Then
                                Kernel.Client.WriteMemory(Consts.ptrGoToX, CInt(Coordinates.X), 2)
                                Kernel.Client.WriteMemory(Consts.ptrGoToY, CInt(Coordinates.Y), 2)
                                Kernel.Client.WriteMemory(Consts.ptrGoToZ, CInt(Coordinates.Z), 1)
                                BL.IsWalking = True
                                IsReady = False
                                Return False
                            End If
                        End If
                    Case WaypointType.Ladder
                        If Kernel.CharacterLoc.Z <> Coordinates.Z Then
                            IsReady = True
                            Return True
                        End If
                        If Kernel.CharacterLoc.X = Coordinates.X AndAlso Kernel.CharacterLoc.Y = Coordinates.Y AndAlso Kernel.CharacterLoc.Z = Coordinates.Z Then
                            'TD.JumpToTile(TileData.SpecialTile.Myself)
                            TD.Get_TileInfo()
                            If TD.Count = 0 Then
                                Kernel.ConsoleError("Theres no objects in the tile you're standing.")
                                IsReady = False
                                Return False
                            End If
                            For i As Integer = 0 To TD.Count - 1 'CHECK THIS
                                If Kernel.Client.Items.GetItemKind(TD.ObjectId(i)) = IItems.ItemKind.UsableTeleport Then
                                    SP.UseObject(TD.ObjectId(i), Coordinates)
                                    'Core.Proxy.SendPacketToServer(UseObject(TD.ObjectId(i), Coordinates))
                                    IsReady = False
                                    Return False
                                End If
                            Next
                            Kernel.ConsoleWrite("Couldn't find Ladders from the tile you are standing.")
                            IsReady = False
                            Return False
                        Else
                            If BL.IsWalking = False Then
                                Kernel.Client.WriteMemory(Consts.ptrGoToX, CInt(Coordinates.X), 2)
                                Kernel.Client.WriteMemory(Consts.ptrGoToY, CInt(Coordinates.Y), 2)
                                Kernel.Client.WriteMemory(Consts.ptrGoToZ, CInt(Coordinates.Z), 1)
                                BL.IsWalking = True
                                IsReady = False
                                Return False
                            End If
                        End If

                    Case WaypointType.Rope
                        If Kernel.CharacterLoc.X = Coordinates.X AndAlso Kernel.CharacterLoc.Y = Coordinates.Y AndAlso Kernel.CharacterLoc.Z = Coordinates.Z Then
                            'TD.JumpToTile(TileData.SpecialTile.Myself)
                            BL.JumpToEntity(IBattlelist.SpecialEntity.Myself)
                            Dim Container As New Container
                            'Dim Rope As ContainerItemDefinition
                            Dim RopeId As UShort = Kernel.Client.Items.GetItemID("Rope")
                            Dim ServerPacket As New ServerPacketBuilder(Kernel.Proxy)
                            ServerPacket.UseObjectWithObjectOnGround(RopeId, BL.GetLocation, TD.TileId)
                            ServerPacket.Send()
                            System.Threading.Thread.Sleep(2000)
                            If Kernel.CharacterLoc.Z <> Coordinates.Z Then
                                IsReady = True
                                Return True
                            End If
                        ElseIf Kernel.CharacterLoc.Z <> Coordinates.Z Then
                            IsReady = True
                            Return True
                        ElseIf BL.IsWalking = False Then
                            Kernel.Client.WriteMemory(Consts.ptrGoToX, Coordinates.X, 2)
                            Kernel.Client.WriteMemory(Consts.ptrGoToY, Coordinates.Y, 2)
                            Kernel.Client.WriteMemory(Consts.ptrGoToZ, Coordinates.Z, 1)
                            BL.IsWalking = True
                            IsReady = False
                            Return False
                        End If
                    Case WaypointType.StairsOrHole
                        If Kernel.CharacterLoc.Z <> Coordinates.Z Then
                            IsReady = True
                            Return True
                        Else
                            If BL.IsWalking = False Then
                                Kernel.Client.WriteMemory(Consts.ptrGoToX, Coordinates.X, 2)
                                Kernel.Client.WriteMemory(Consts.ptrGoToY, Coordinates.Y, 2)
                                Kernel.Client.WriteMemory(Consts.ptrGoToZ, Coordinates.Z, 1)
                                BL.IsWalking = True
                                IsReady = False
                                Return False
                            End If
                        End If
                    Case WaypointType.Say
                        If Kernel.CharacterLoc.X = Coordinates.X AndAlso Kernel.CharacterLoc.Y = Coordinates.Y AndAlso Kernel.CharacterLoc.Z = Coordinates.Z Then
                            Dim CM As New ChatMessageDefinition
                            System.Threading.Thread.Sleep(1000)
                            CM.MessageType = ITibia.MessageType.Default
                            CM.DefaultMessageType = ITibia.DefaultMessageType.Normal
                            CM.Prioritize = True
                            CM.Message = Info
                            Kernel.ChatMessageQueueList.Add(CM)
                            System.Threading.Thread.Sleep(1000)
                            'Core.Proxy.SendPacketToServer(Speak())
                            IsReady = True
                            Return True
                        Else
                            If BL.IsWalking = False Then
                                Kernel.Client.WriteMemory(Consts.ptrGoToX, Coordinates.X, 2)
                                Kernel.Client.WriteMemory(Consts.ptrGoToY, Coordinates.Y, 2)
                                Kernel.Client.WriteMemory(Consts.ptrGoToZ, Coordinates.Z, 1)
                                BL.IsWalking = True
                                IsReady = False
                                Return False
                            End If
                        End If
                    Case WaypointType.Wait
                        If Kernel.WalkerWaitUntil < Date.Now Then
                            IsReady = True
                            Kernel.WalkerFirstTime = True
                            Return True
                        Else
                            IsReady = False
                            Kernel.WalkerFirstTime = False
                            Return False
                        End If
                    Case WaypointType.Sewer
                        If Kernel.CharacterLoc.X = Coordinates.X AndAlso Kernel.CharacterLoc.Y = Coordinates.Y AndAlso Kernel.CharacterLoc.Z = Coordinates.Z Then
                            'Let's get Sewer's id and position
                            TD.Get_TileInfo()
                            Dim SewerId As Integer = 0
                            Dim SewerStackPos As Integer = 0
                            For i As Integer = 0 To TD.Count
                                If Kernel.Client.Items.GetItemKind(TD.ObjectId(i)) = IItems.ItemKind.UsableTeleport2 Then
                                    Dim ServerPacket As New ServerPacketBuilder(Kernel.Proxy)
                                    ServerPacket.UseObject(TD.ObjectId(i), Kernel.CharacterLoc)
                                    'Core.Proxy.SendPacketToServer(UseObject(TD.ObjectId(i), Core.CharacterLoc))
                                    If Kernel.CharacterLoc.Z <> Coordinates.Z Then
                                        IsReady = True
                                        Return True
                                    End If
                                End If
                            Next
                        Else
                            If BL.IsWalking = False Then
                                Kernel.Client.WriteMemory(Consts.ptrGoToX, Coordinates.X, 2)
                                Kernel.Client.WriteMemory(Consts.ptrGoToY, Coordinates.Y, 2)
                                Kernel.Client.WriteMemory(Consts.ptrGoToZ, Coordinates.Z, 1)
                                BL.IsWalking = True
                                IsReady = False
                                Return False
                            End If
                        End If
                    Case WaypointType.Shovel
                        If Kernel.CharacterLoc.X = Coordinates.X AndAlso Kernel.CharacterLoc.Y = Coordinates.Y AndAlso Kernel.CharacterLoc.Z = Coordinates.Z Then
                            Dim HoleLoc As New ITibia.LocationDefinition
                            'Finding hole location
                            HoleLoc = Kernel.CharacterLoc
                            Select Case CType(Info, Directions)
                                Case Directions.Left
                                    HoleLoc.X -= 1
                                Case Directions.Right
                                    HoleLoc.X += 1
                                Case Directions.Up
                                    HoleLoc.Y -= 1
                                Case Directions.Down
                                    HoleLoc.Y += 1
                            End Select
                            'Finding Shovel
                            Dim Shovel As Scripting.IContainer.ContainerItemDefinition
                            If (New Container).FindItem(Shovel, Kernel.Client.Items.GetItemID("Shovel")) = False Then
                                If (New Container).FindItem(Shovel, Kernel.Client.Items.GetItemID("Light Shovel")) = False Then
                                    Kernel.ConsoleError("Unable to find shovel. Stopping for 10 seconds.")
                                    System.Threading.Thread.Sleep(10000)
                                    IsReady = False
                                    Return False
                                End If
                            End If
                            Dim ServerPacket As New ServerPacketBuilder(Kernel.Proxy)
                            ServerPacket.UseObjectWithObjectOnGround(Shovel.ID, HoleLoc)
                            ServerPacket.Send()
                            'Core.Proxy.SendPacketToServer(UseObjectWithObjectOnGround(Shovel.ID, HoleLoc))
                            System.Threading.Thread.Sleep(1000)
                            Kernel.Client.WriteMemory(Consts.ptrGoToX, HoleLoc.X, 2)
                            Kernel.Client.WriteMemory(Consts.ptrGoToY, HoleLoc.Y, 2)
                            Kernel.Client.WriteMemory(Consts.ptrGoToZ, HoleLoc.Z, 1)
                            System.Threading.Thread.Sleep(2000)
                            BL.IsWalking = True
                            If Kernel.CharacterLoc.Z <> HoleLoc.Z Then
                                IsReady = True
                                Return True
                            End If
                        Else
                            If BL.IsWalking = False Then
                                Kernel.Client.WriteMemory(Consts.ptrGoToX, Coordinates.X, 2)
                                Kernel.Client.WriteMemory(Consts.ptrGoToY, Coordinates.Y, 2)
                                Kernel.Client.WriteMemory(Consts.ptrGoToZ, Coordinates.Z, 1)
                                BL.IsWalking = True
                                IsReady = False
                                Return False
                            End If
                        End If
                End Select
            Catch Ex As Exception
                MessageBox.Show("TargetSite: " & Ex.TargetSite.Name & vbCrLf & "Message: " & Ex.Message & vbCrLf & "Source: " & Ex.Source & vbCrLf & "Stack Trace: " & Ex.StackTrace & vbCrLf & vbCrLf & "Please report this error to the developers, be sure to take a screenshot of this message box.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End
            End Try
        End Function

        Public Shared Function CheckDistance() As Boolean
            Try
                Dim BL As New BattleList
                Dim PrevWp As New ITibia.LocationDefinition

                If Kernel.Walker_Waypoints.Count = 0 Then Return True
                PrevWp = Kernel.Walker_Waypoints(Kernel.Walker_Waypoints.Count - 1).Coordinates

                BL.JumpToEntity(IBattlelist.SpecialEntity.Myself)
                If BL.GetDistanceFromLocation(PrevWp) > Consts.WaypointMaxDistance Then
                    Kernel.ConsoleError("The waypoint is too far.")
                    Return False
                Else
                    Return True
                End If
            Catch Ex As Exception
                MessageBox.Show("TargetSite: " & Ex.TargetSite.Name & vbCrLf & "Message: " & Ex.Message & vbCrLf & "Source: " & Ex.Source & vbCrLf & "Stack Trace: " & Ex.StackTrace & vbCrLf & vbCrLf & "Please report this error to the developers, be sure to take a screenshot of this message box.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End
            End Try
        End Function
    End Class

    Public Sub Save(ByVal Path As String)
        Try
            Dim Document As New XmlDocument
            Dim xmlWayPoints As XmlElement = Document.CreateElement("Waypoints")
            For Each WayPoint As Walker In Kernel.Walker_Waypoints
                Dim xmlWayPoint As XmlElement = Document.CreateElement("WayPoint")
                Dim xmlPosX As XmlAttribute = Document.CreateAttribute("PosX")
                xmlPosX.InnerText = WayPoint.Coordinates.X
                Dim xmlPosY As XmlAttribute = Document.CreateAttribute("PosY")
                xmlPosY.InnerText = WayPoint.Coordinates.Y
                Dim xmlPosZ As XmlAttribute = Document.CreateAttribute("PosZ")
                xmlPosZ.InnerText = WayPoint.Coordinates.Z
                Dim xmlType As XmlAttribute = Document.CreateAttribute("Type")
                xmlType.InnerText = WayPoint.Type
                Dim xmlInfo As XmlAttribute = Document.CreateAttribute("Info")
                xmlInfo.InnerText = WayPoint.Info

                xmlWayPoint.Attributes.Append(xmlPosX)
                xmlWayPoint.Attributes.Append(xmlPosY)
                xmlWayPoint.Attributes.Append(xmlPosZ)
                xmlWayPoint.Attributes.Append(xmlType)
                xmlWayPoint.Attributes.Append(xmlInfo)
                xmlWayPoints.AppendChild(xmlWayPoint)
            Next
            Dim Declaration As XmlDeclaration = Document.CreateXmlDeclaration("1.0", "", "")
            Document.AppendChild(Declaration)
            Document.AppendChild(xmlWayPoints)
            Document.Save(Path)
        Catch Ex As Exception
            MessageBox.Show("TargetSite: " & Ex.TargetSite.Name & vbCrLf & "Message: " & Ex.Message & vbCrLf & "Source: " & Ex.Source & vbCrLf & "Stack Trace: " & Ex.StackTrace & vbCrLf & vbCrLf & "Please report this error to the developers, be sure to take a screenshot of this message box.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End
        End Try
    End Sub

    Public Sub Load(ByVal Path As String)
        Dim Document As New XmlDocument
        Try
            Document.Load(Path)
            Dim TempStr As String = ""

            Kernel.Walker_Waypoints.Clear()

            For Each Element As XmlElement In Document.Item("Waypoints")
                Dim Walker_Char As New Walker
                TempStr = Element.GetAttribute("PosX")
                If Not String.IsNullOrEmpty(TempStr) Then Walker_Char.Coordinates.X = CInt(TempStr)
                TempStr = Element.GetAttribute("PosY")
                If Not String.IsNullOrEmpty(TempStr) Then Walker_Char.Coordinates.Y = CInt(TempStr)
                TempStr = Element.GetAttribute("PosZ")
                If Not String.IsNullOrEmpty(TempStr) Then Walker_Char.Coordinates.Z = CInt(TempStr)
                TempStr = Element.GetAttribute("Type")
                If Not String.IsNullOrEmpty(TempStr) Then Walker_Char.Type = CInt(TempStr)
                TempStr = Element.GetAttribute("Info")
                If Not String.IsNullOrEmpty(TempStr) Then Walker_Char.Info = TempStr
                Kernel.Walker_Waypoints.Add(Walker_Char)
            Next
            UpdateList()
        Catch
            MessageBox.Show("Unable to load waypoints.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Kernel.Walker_Waypoints.Clear()
            UpdateList()
        End Try
    End Sub
    Public Sub UpdateList()
        Try
            Kernel.CavebotForm.Waypointslst.Items.Clear()
            If Kernel.Walker_Waypoints.Count = 0 Then Exit Sub
            Dim Character As Walker
            Dim WPType As String
            For Each Character In Kernel.Walker_Waypoints
                Select Case Character.Type
                    Case Walker.WaypointType.Ladder
                        WPType = "L"
                    Case Walker.WaypointType.Rope
                        WPType = "R"
                    Case Walker.WaypointType.StairsOrHole
                        WPType = "S/H"
                    Case Walker.WaypointType.Walk
                        WPType = "W"
                    Case Walker.WaypointType.Say
                        WPType = "S"
                    Case Walker.WaypointType.Wait
                        WPType = "WT"
                    Case Walker.WaypointType.Sewer
                        WPType = "SE"
                    Case Walker.WaypointType.Shovel
                        WPType = "SH"
                    Case Else
                        WPType = "NotFound"
                End Select

                If Character.Type = Walker.WaypointType.Wait Then
                    Kernel.CavebotForm.Waypointslst.Items.Add(WPType & ": Wait: " & Character.Info)

                Else

                    Kernel.CavebotForm.Waypointslst.Items.Add(WPType & ":" & Character.Coordinates.X _
            & ":" & Character.Coordinates.Y _
            & ":" & Character.Coordinates.Z & " " & Character.Info)
                End If
            Next
        Catch Ex As Exception
            MessageBox.Show("TargetSite: " & Ex.TargetSite.Name & vbCrLf & "Message: " & Ex.Message & vbCrLf & "Source: " & Ex.Source & vbCrLf & "Stack Trace: " & Ex.StackTrace & vbCrLf & vbCrLf & "Please report this error to the developers, be sure to take a screenshot of this message box.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End
        End Try
    End Sub

End Module