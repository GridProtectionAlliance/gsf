import json

class Record:
    startRegister = 0
    endRegister = 1
    numberOfRegisters = 2
    
    description = 'L1 to Earth'
    recordType = 'Single'
    address = ''
    dataValue = ''
    units = ''
    
    def __init__(self, startRegister, numberOfRegisters, description, recordType = None, units = ''):
        startRegister += 7000
        self.startRegister = startRegister
        self.numberOfRegisters = numberOfRegisters
        self.endRegister = startRegister + numberOfRegisters - 1
        self.units = units
        
        self.description = description
        self.recordType = recordType
        
    
    def ToRecords(self):
        # Add Records
        returnList = []
        for record in range(self.numberOfRegisters):
            tempDictionary = {}
            
            tempDictionary['recordType'] = 3
            tempDictionary['address'] = str(self.startRegister + record)
            tempDictionary['description'] = self.description
            tempDictionary['dataValue'] = ''
            
            returnList += [ tempDictionary ]
        
        # add derived value record    
        if self.numberOfRegisters > 1:           
            
            # add suffix to sub records                
            for i, record in enumerate(returnList):
                if self.numberOfRegisters == 2:
                    if i == 0:
                        suffix = 'Low'
                    else:
                        suffix = 'High'
                else:
                    suffix = str(i + 1)
                record['description'] += ' ' + suffix
            
            # add derived value
            derivedValue = {}
            
            addresses = '(HR' + str(self.startRegister)
            for address in range(self.startRegister + 1, self.endRegister + 1):
                addresses += ',HR' + str(address)
            addresses += ')'
                
            
            derivedValue['recordType'] = 4
            derivedValue['address'] = self.recordType + addresses
            derivedValue['description'] = self.description
            if self.units != None:
                derivedValue['description'] +=  ' (' + self.units + ')'
            derivedValue['dataValue'] = ''
            
            returnList += [ derivedValue ]
            
        else:
            # Add Unit to record
            if self.units != None:
                returnList[0]['description'] += ' ('+ self.units + ')'
        
        return returnList
'''
L1-E -> RMS Volts ***************
Voltage THD -> % ****************
Current TDD -> % ****************
Current Basis for TDD -> RMS Amps*******
Current -> RMS Amps *************
Power -> Watts ******************
Apparent Power -> VA ************
True Power Factor -> None *******
Energy -> Wh
Apparent Energy -> VAh
Volt-Amps Reactive -> VAR ********
Voltage, Current Unbalance -> % ****
Temperature -> Celsius ***********
Humidity -> %RH ******************
CO2 Rate -> Grams per Hour *******
CO2 Accumulated -> Grams *********
Angle -> Degrees *****************
GPS Latitude -> degrees *********
GPS Longitude -> degrees ********
User Counter -> Counts **********
Frequency -> Hertz **************
'''
def GetUnitType(field):
    units = None
    field = field.lower()
    
    if 'frequency' in field:
        units = 'Hertz'
        
    elif 'temperature' in field:
        units = 'Celsius'
        
    elif 'humidity' in field:
        units = '%RH'   
    
    elif 'co2' in field:
        if 'rate' in field:
            units = 'Grams per Hour'
        else:
            units = 'Grams'
    
    elif 'angle' in field or 'latitude' in field or 'longitude' in field:
        units = 'degrees'
        
    elif 'tdd' in field or 'thd' in field:
        if 'current' in field:
            units = 'RMS Amps'
        else:
            units = '%'
            
    elif 'unbalance' in field:
        units = '%'
    
    elif 'volt-amps reactive' in field:
        units = 'VAR'
            
    elif 'current' in field and 'date' not in field:
        units = 'RMS Amps'
    
    elif 'volt' in field or ' to ' in field:
        if 'multiplier' in field:
            units = None
        else:
            units = 'RMS Volts'
    
    elif 'power' in field and 'date' not in field:
        if 'true' in field or 'configuration' in field:
            units = None
        elif 'apparent' in field:
            units = 'VA'
        else:
            units = 'Watts'

    elif 'energy' in field and 'date' not in field:
        if 'apparent' in field:
            units = 'VAh'
        else:
            units = 'Wh'
        
    else:
        units = None

    return units

def ListToSequence(inputList, startRegister):
    currentRegister = startRegister
    registersPerField = 0
    returnList = []
    typeList = [ None, None, 'Single', None, 'Int64' ]
    
    for i, field in enumerate(inputList):
        if type(field) is int:
            registersPerField = field
        else:
            units = GetUnitType(field)
            returnList += [ Record(currentRegister, registersPerField, field, typeList[registersPerField], units) ]
            currentRegister += registersPerField
        
    return returnList
    
    
    

def MakeAllSequences():
    fileHandler = open('PQube Mappings.json', mode='w')
    
    fieldsForSequence1 = [ 
        2, 
        'L1 To Earth', 'L2 To Earth', 'L3 To Earth', 'Neutral To Earth', 'L1 To Neutral', 
        'L2 To Neutral', 'L3 To Neutral', 'L1 To L2', 'L2 To L3', 'L3 To L1', # 19
               
        'Analog1 To Earth', 'Analog2 To Earth', 'Analog1 To Analog2', 'Frequency', 'L1 Current', 
        'L2 Current', 'L3 Current', 'Neutral Current', 'Power', 'Apparent Power', # 39
               
        'Digital Input', 'Peak Current(1 Cycle)', 'Peak Current(1 Minute', 'Peak Current(N Minutes)', 'Peak Power(1 Cycle)', 
        'Peak Power(1 Minute)', 'Peak Power(N Minutes)', 'Peak VA(1 Cycle)', 'Peak VA(1 Minute)', 'Peak VA(N Minutes)', # 59
        
        'Energy', 'Apparent Energy', 'Voltage THD', 'Current TDD', 'ANSI Voltage Unbalance', 
        'ANSI Current Unbalance', 'L1 Flicker (Instantaneous)', 'L1 Flicker (10 Minutes)', 'L1 Flicker (2 Hours)',  # 77
               
        1, 
        'New Event Recordings', 'New Trend Recordings', # 79
               
        2,
        'Volt-Amps Reactive', 'True Power Factor', 'Temperature Probe 1', 'Humidity Probe 1', 'Temperature Probe 2', 
        'Humidity Probe 2', 'CO2 Rate', 'CO2 Accumulated', 'Earth Current', 'L1 To Neutral Voltage Fundamental Magnitude' ] # 99
    
    fieldsForSequence2 = [ 
        2, 
        'L1 To Neutral Voltage Fundamental Angle', 'L2 To Neutral Voltage Fundamental Magnitude', 'L2 To Neutral Voltage Fundamental Angle','L3 To Neutral Voltage Fundamental Magnitude', 'L3 To Neutral Voltage Fundamental Angle',
        'L1 Current Fundamental Magnitude', 'L1 Current Fundamental Angle', 'L2 Current Fundamental Magnitude', 'L2 Current Fundamental Angle', 'L3 Current Fundamental Magnitude', # 119
                 
        'L3 Current Fundamental Angle', # 121
                           
        1, 
        'Peak Current "Since" Date (Year)', 'Peak Current "Since" Date (Month)', 'Peak Current "Since" Date (Day)', # 124
        'Peak Power "Since" Date (Year)', 'Peak Power "Since" Date (Month)', 'Peak Power "Since" Date (Day)', 'Peak VA "Since" Date (Year)', 'Peak VA "Since" Date (Month)', # 129
                                           
        'Peak VA "Since" Date (Day)', 'Energy "Since" Date (Year)', 'Energy "Since" Date (Month)', 'Energy "Since" Date (Day)', 'PQube Clock-Calendar (Year)', 
        'PQube Clock-Calendar (Month)', 'PQube Clock-Calendar (Day)', 'PQube Clock-Calendar (Hour)', 'PQube Clock-Calendar (Minute)', 'PQube Clock-Calendar (Second)', # 139
        
        2,
        'PQube Offset from UTC', 'Harmonic L1 To Neutral Volts', 'Harmonic L2 To Neutral Volts', 'Harmonic L3 To Neutral Volts', 'Harmonic L1 Current', 
        'Harmonic L2 Current', 'Harmonic L3 Current', 'Harmonic L1 To Neutral Voltage Angle', 'Harmonic L2 To Neutral Voltage Angle', 'Harmonic L3 To Neutral Voltage Angle', # 159
                                           
        'Harmonic L1 Current Angle', 'Harmonic L2 Current Angle', 'Harmonic L3 Current Angle',  # 165
        
        1, 
        'Harmonic Order of Interest', 'GPS Status',  # 167
        
        2, 
        'GPS Latitude', 'GPS Longitude', # 171
        
        1, 
        'GPS Number of Sattelites', 'Unused',  # 173
        
        2, 
        'IEC or GB Unbalance -V-', 'IEC or GB Unbalance -A-', 'IEC or GB Unbalance -V0-', # 179
        'IEC or GB Unbalance -A0-', 'DC Power', 'DC Energy', # 185
        
        4,
        'User Counter', # 189
        
        1,
        'Trigger Snapshot', 'Reset Peak Measurements', # 191 WO
        
        2,
        'L1 Voltage THD', 'L2 Voltage THD', 'L3 Voltage THD', 'L1 Current TDD' ] # 199
    
    fieldsForSequence3 = [
        2,
        'L2 Current TDD', 'L3 Current TDD', 'L1 Power', 'L2 Power', 'L3 Power', 
        'L1 Apparent Power ', 'L2 Apparent Power ', 'L3 Apparent Power ', 'L1 Volt-Amps Reactive', 'L2 Volt-Amps Reactive', # 219
                                           
        'L3 Volt-Amps Reactive', 'L1 True Power Factor', 'L2 True Power Factor', 'L3 True Power Factor', 'L2 Flicker (Instananeous)', 
        'L3 Flicker (Instananeous)', 'L2 Flicker (10 Minutes)', 'L3 Flicker (10 Minutes)', 'L2 Flicker (2 Hours)', 'L3 Flicker (2 Hours)', # 239
                                           
        'VAR Hours Accumulated',  # 241
        
        1,
        'DC Energy "Since" Date (Year)', 'DC Energy "Since" Date (Month)', 'DC Energy "Since" Date (Day)',  # 244
        
        'Reset Energy Acculuators', 'Reset Analog Energy Accumulators' ] # 246 WO
    
    fieldsForSequence4 = [
        1,
        'PQube Firmware Major Revision', 'PQube Firmware Minor Revision', 'PQube Firmware Bug Fix Revision', 'PQube Firmware Build Number', # 1003
        
        2, 
        'Nominal Line To Neutral Voltage', 'Nominal Line To Line Voltage', 'Nominal Frequency', 
        'PT (Potential Transformer) Ratio', 'CT (Current Transformer) Ratio', 'PQube Serial Number', 'AN1 (Analog 1 To Earth) Multiplier', 'AN2 (Analog 2 To Earth) Multiplier', # 1019
                                             
        'Current Basis For TDD',  # 1021
        
        1, 
        'Power Configuration', 'Ground Point', 'N-Minutes' ] # 1024

    # Initialize Return Dictionary
    returnDictionary = { 'sequences': [ {}, {}, {}, {} ] }
    for count, sequence in enumerate(returnDictionary['sequences'], start=1):
        sequence['type'] = 0
        sequence['name'] = 'PQube Register Mappings ' + str(count)
        sequence['records'] = []
    
    returnDictionary['sequences'][3]['name'] = 'PQube Static Information'
    
    # Fill First Sequece
    returnDictionary['sequences'][0]['records'] += ListToSequence(fieldsForSequence1, 0)
    
    # Fill Second Sequence
    returnDictionary['sequences'][1]['records'] += ListToSequence(fieldsForSequence2, 100)

        
    # Fill Third Sequence
    returnDictionary['sequences'][2]['records'] += ListToSequence(fieldsForSequence3, 200)

    # Fill Fourth Sequence
    returnDictionary['sequences'][3]['records'] += ListToSequence(fieldsForSequence4, 1000)
        
    # Convert list[record object] to list[records]
    for sequence in returnDictionary['sequences']:
        tempRecordList = []
        
        for record in sequence['records']:
            tempRecordList += record.ToRecords()
        
        sequence['records'] = tempRecordList
        
    json.dump(returnDictionary, fileHandler, indent=4)
    return returnDictionary