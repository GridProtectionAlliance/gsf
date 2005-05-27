'***********************************************************************
'  Config.vb - PDCstream Configuration File Reader
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports System.IO
Imports System.Text
Imports System.Reflection.Assembly
Imports System.Security.Principal
Imports System.Threading
Imports TVA.Shared.String
Imports TVA.Shared.Common
Imports TVA.Interop.Windows

Namespace EE.Phasor.PDCstream

    ' Note that there is typically only one instance of this class created used by any number of different threads and a request
    ' can be made at anytime to "reload" the config file, so make sure all publically accessible methods in the class make proper
    ' use of the internal reader-writer lock
    Public Class ConfigFile

        Private m_readWriteLock As ReaderWriterLock
        Private m_iniFile As IniFile
        Private m_defaultPhasorV As PhasorDefinition
        Private m_defaultPhasorI As PhasorDefinition
        Private m_defaultFrequency As FrequencyDefinition
        Private m_sampleRate As Integer
        Private m_pmuTable As Hashtable
        Private m_orderedIDList As ArrayList
        Private m_orderedPMUList As ArrayList

        Public Event ConfigFileReloaded()

        Public Sub New(ByVal configFileName As String)

            m_iniFile = New IniFile(configFileName)
            m_readWriteLock = New ReaderWriterLock
            Refresh()

        End Sub

        Public Sub Refresh()

            ' The only time we need a write lock is when we reload the config file...
            m_readWriteLock.AcquireWriterLock(-1)

            Try
                With m_iniFile
                    If File.Exists(.IniFileName) Then
                        Dim newPMU As PMUDefinition
                        Dim x, phasorCount As Integer

                        m_pmuTable = New Hashtable(CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default)
                        m_defaultPhasorV = New PhasorDefinition(Me, 0, .KeyValue("DEFAULT", "PhasorV", "V,4500.0,0.0060573,0,0,500,Default 500kV"))
                        m_defaultPhasorI = New PhasorDefinition(Me, 0, .KeyValue("DEFAULT", "PhasorI", "I,600.00,0.000040382,0,1,1.0,Default Current"))
                        m_defaultFrequency = New FrequencyDefinition(Me, .KeyValue("DEFAULT", "Frequency", "F,1000,60,1000,0,0,Frequency"))
                        m_sampleRate = CInt(.KeyValue("CONFIG", "SampleRate", "30"))

                        ' Load phasor data for each section in config file...
                        For Each section As String In .SectionNames()
                            If Len(section) > 0 Then
                                ' Make sure this is not a special section
                                If String.Compare(section, "DEFAULT", True) <> 0 And String.Compare(section, "CONFIG", True) <> 0 Then
                                    ' Create new PMU entry structure from config file settings...
                                    phasorCount = CInt(.KeyValue(section, "NumberPhasors", "0"))

                                    newPMU = New PMUDefinition(phasorCount)

                                    newPMU.ID = section
                                    newPMU.Name = .KeyValue(section, "Name", section)
                                    newPMU.Index = CInt(.KeyValue(section, "PMU", m_pmuTable.Count))

                                    For x = 0 To phasorCount - 1
                                        newPMU.Phasors(x) = New PhasorDefinition(Me, x + 1, .KeyValue(section, "Phasor" & (x + 1)))
                                    Next

                                    newPMU.Frequency = New FrequencyDefinition(Me, .KeyValue(section, "Frequency"))
                                    newPMU.SampleRate = m_sampleRate

                                    m_pmuTable.Add(section, newPMU)
                                End If
                            End If
                        Next

                        ' Now that all the PMU definitions have been loaded, we reassign their index values to match their physical position
                        ' in the ordered PMU list - end users setting up the config file may accidentally use the same PMU index (ID) twice,
                        ' or not define one at all, so this makes the index values unique and usable (a good index here makes PMU data cells
                        ' directly accesible from their rows by using this index alone)
                        m_orderedIDList = New ArrayList(m_pmuTable.Keys)
                        m_orderedIDList.Sort()

                        m_orderedPMUList = New ArrayList(m_pmuTable.Values)
                        m_orderedPMUList.Sort()

                        For x = 0 To PMUCount - 1
                            PMU(x).Index = x
                        Next

                        m_orderedPMUList.Sort()
                    Else
                        Throw New InvalidOperationException("PDC config file """ & .IniFileName & """ does not exist.")
                    End If
                End With
            Catch
                Throw
            Finally
                m_readWriteLock.ReleaseWriterLock()
            End Try

            ' In case other classes want to know, we send out a notification that the config file has been reloaded (make sure
            ' you do this after the write lock has been released to avoid possible dead-lock situations)
            RaiseEvent ConfigFileReloaded()

        End Sub

        Public ReadOnly Property ConfigFileName() As String
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    Return m_iniFile.IniFileName
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

        Public ReadOnly Property DefaultPhasorV() As PhasorDefinition
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    Return m_defaultPhasorV
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

        Public ReadOnly Property DefaultPhasorI() As PhasorDefinition
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    Return m_defaultPhasorI
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

        Public ReadOnly Property DefaultFrequency() As FrequencyDefinition
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    Return m_defaultFrequency
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

        Public ReadOnly Property SampleRate() As Integer
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    Return m_sampleRate
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

        Default Public ReadOnly Property PMU(ByVal ID As String) As PMUDefinition
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    Return m_pmuTable(ID)
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

        Default Public ReadOnly Property PMU(ByVal index As Integer) As PMUDefinition
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    Return DirectCast(m_orderedPMUList(index), PMUDefinition)
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

        Public ReadOnly Property IDList() As ArrayList
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    Return m_orderedIDList
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

        Public ReadOnly Property PMUCount() As Integer
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    Return m_pmuTable.Count
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

        Public ReadOnly Property OutputContent() As String
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    With New StringBuilder
                        .Append("; File - " & m_iniFile.IniFileName & vbCrLf)
                        .Append("; Auto-generated on " & Now() & " by TVA DatAWare PDC" & vbCrLf)
                        .Append(";    Assembly: " & GetShortAssemblyName(GetExecutingAssembly) & vbCrLf)
                        .Append(";    Compiled: " & File.GetLastWriteTime(GetExecutingAssembly.Location) & vbCrLf)
                        .Append(";" & vbCrLf)
                        .Append(";" & vbCrLf)
                        .Append("; Format:" & vbCrLf)
                        .Append(";   Each Column in data file is given a bracketed identifier, numbered in the order it" & vbCrLf)
                        .Append(";   appears in the data file, and identified by data type ( PMU, PDC, or other)" & vbCrLf)
                        .Append(";     PMU designates column data format from a single PMU" & vbCrLf)
                        .Append(";     PDC designates column data format from another PDC which is somewhat different from a single PMU" & vbCrLf)
                        .Append(";   Default gives default values for a processing algorithm in case quantities are omitted" & vbCrLf)
                        .Append(";   Name= gives the overall station name for print labels" & vbCrLf)
                        .Append(";   NumberPhasors= :  for PMU data, gives the number of phasors contained in column" & vbCrLf)
                        .Append(";                     for PDC data, gives the number of PMUs data included in the column" & vbCrLf)
                        .Append(";                     Note - for PDC data, there will be 2 phasors & 1 freq per PMU" & vbCrLf)
                        .Append(";   Quantities within the column are listed by PhasorI=, Frequency=, etc" & vbCrLf)
                        .Append(";   Each quantity has 7 comma separated fields followed by an optional comment" & vbCrLf)
                        .Append(";" & vbCrLf)
                        .Append(";   Phasor entry format:  Type, Ratio, Cal Factor, Offset, Shunt, VoltageRef/Class, Label  ;Comments" & vbCrLf)
                        .Append(";    Type:       Type of measurement, V=voltage, I=current, N=don't care, single ASCII character" & vbCrLf)
                        .Append(";    Ratio:      PT/CT ratio N:1 where N is a floating point number" & vbCrLf)
                        .Append(";    Cal Factor: Conversion factor between integer in file and secondary volts, floating point" & vbCrLf)
                        .Append(";    Offset:     Phase Offset to correct for phase angle measurement errors or differences, floating point" & vbCrLf)
                        .Append(";    Shunt:      Current- shunt resistence in ohms, or the equivalent ratio for aux CTs, floating point" & vbCrLf)
                        .Append(";                Voltage- empty, not used" & vbCrLf)
                        .Append(";    VoltageRef: Current- phasor number (1-10) of voltage phasor to use for power calculation, integer" & vbCrLf)
                        .Append(";                Voltage- voltage class, standard l-l voltages, 500, 230, 115, etc, integer" & vbCrLf)
                        .Append(";    Label:      Phasor quantity label for print label, text" & vbCrLf)
                        .Append(";    Comments:   All text after the semicolon on a line are optional comments not for processing" & vbCrLf)
                        .Append(";" & vbCrLf)
                        .Append(";   Voltage Magnitude = MAG(Real,Imaginary) * CalFactor  * PTR    (line-neutral)" & vbCrLf)
                        .Append(";   Current Magnitude = MAG(Real,Imaginary)  * CalFactor * CTR / Shunt   (phase current)" & vbCrLf)
                        .Append(";   Phase Angle = ATAN(Imaginary/Real) + Phase Offset   (usually degrees)" & vbCrLf)
                        .Append(";     Note: Usually phase Offset is 0, but is sometimes required for comparing measurements" & vbCrLf)
                        .Append(";           from different systems or through transformer banks" & vbCrLf)
                        .Append(";" & vbCrLf)
                        .Append(";   Frequency entry format:  scale, offset, dF/dt scale, dF/dt offset, dummy, label  ;Comments" & vbCrLf)
                        .Append(";   Frequency = Number / scale + offset" & vbCrLf)
                        .Append(";   dF/dt = Number / (dF/dt scale) + (dF/dt offset)" & vbCrLf)
                        .Append(";" & vbCrLf)
                        .Append(";" & vbCrLf)

                        .Append("[DEFAULT]" & vbCrLf)
                        .Append("PhasorV=" & PhasorDefinition.ConfigFileFormat(DefaultPhasorV) & vbCrLf)
                        .Append("PhasorI=" & PhasorDefinition.ConfigFileFormat(DefaultPhasorI) & vbCrLf)
                        .Append("Frequency=" & FrequencyDefinition.ConfigFileFormat(DefaultFrequency) & vbCrLf)
                        .Append(vbCrLf)

                        .Append("[CONFIG]" & vbCrLf)
                        .Append("SampleRate=" & m_sampleRate & vbCrLf)
                        .Append("NumberOfPMUs=" & PMUCount & vbCrLf)
                        .Append(vbCrLf)

                        For x As Integer = 0 To PMUCount - 1
                            .Append("[" & Me(x).ID & "]" & vbCrLf)
                            .Append("Name=" & Me(x).Name & vbCrLf)
                            .Append("PMU=" & x & vbCrLf)
                            .Append("NumberPhasors=" & Me(x).Phasors.Length & vbCrLf)
                            For y As Integer = 0 To Me(x).Phasors.Length - 1
                                .Append("Phasor" & (y + 1) & "=" & PhasorDefinition.ConfigFileFormat(Me(x).Phasors(y)) & vbCrLf)
                            Next
                            .Append("Frequency=" & FrequencyDefinition.ConfigFileFormat(Me(x).Frequency) & vbCrLf)
                            .Append(vbCrLf)
                        Next

                        Return .ToString()
                    End With
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

    End Class

End Namespace