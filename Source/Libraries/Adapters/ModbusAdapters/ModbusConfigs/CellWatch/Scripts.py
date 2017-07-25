import json

# Helper Function Only
def MakeLabel(name, startRegister):
    returnDictionary = {}
    
    returnDictionary['type'] = 0
    returnDictionary['name'] = name
    returnDictionary['records'] = []
    
    # Add individual Label registers
    for record in range(32):
        tempDictionary = {}
        
        tempDictionary['recordType'] = 3
        tempDictionary['address'] = str(startRegister + record)
        tempDictionary['description'] = name + ' Label Bytes ' + format(record*2, '02d') + '-' + format(record*2 + 1, '02d')
        tempDictionary['dataValue'] = ''
        
        returnDictionary['records'] += [tempDictionary]
    
    # Add Derived Label    
    returnDictionary['records'] += [{}]
    
    addressString = 'String(HR' + str(startRegister)
    for address in range(startRegister + 1, startRegister + 32):
        addressString += ',HR' + str(address)
    addressString += ')'
    
    returnDictionary['records'][-1]['recordType'] = 4
    returnDictionary['records'][-1]['address'] = addressString
    returnDictionary['records'][-1]['description'] = name + ' Label'
    returnDictionary['records'][-1]['dataValue'] = ''
    
    
    return returnDictionary

def MakeCategory(name, startRegister, fields, addNameToFields = True):
    fileHandler = open(name + '.json', mode='w')
    
    startRegister -=1
    tempDictionary = {'sequences': [ {} ] }
    
    tempDictionary['sequences'][0]['type'] = 0
    tempDictionary['sequences'][0]['name'] = name
    tempDictionary['sequences'][0]['records'] = []
    label = MakeLabel(name, startRegister)
    tempDictionary['sequences'][0] = label
    
    for i in range(len(fields)):
        fieldDictionary = {}
        
        fieldDictionary['recordType'] = 3
        fieldDictionary['address'] = str(i + startRegister + 32)
        if addNameToFields:
            fieldDictionary['description'] = name + fields[i]
        else:
            fieldDictionary['description'] = fields[i]
            
        fieldDictionary['dataValue'] = ''

        tempDictionary['sequences'][0]['records'] += [ fieldDictionary ]
        
    json.dump(tempDictionary, fileHandler, indent=4)
    return tempDictionary

def MakePhysicalCT():
    name = 'Physical CT'
    fileHandler = open(name + '.json', mode = 'w')
    
    startRegister = 80
    
    tempDictionary = { 'sequences': [ {} ] }
    
    tempDictionary['sequences'][0]['type'] = 0
    tempDictionary['sequences'][0]['name'] = name
    tempDictionary['sequences'][0]['records'] = []
    
    for record in range(4):
        # Add LSW
        tempDictionary['sequences'][0]['records'] += [ {} ]
        tempDictionary['sequences'][0]['records'][record*2]['recordType'] = 3
        tempDictionary['sequences'][0]['records'][record*2]['dataValue'] = ''
        tempDictionary['sequences'][0]['records'][record*2]['address'] = str(startRegister + record*2)
        tempDictionary['sequences'][0]['records'][record*2]['description'] = name + ' ' + str(record + 1) + ' LSW'
        
        # Add MSW
        tempDictionary['sequences'][0]['records'] += [ {} ]
        tempDictionary['sequences'][0]['records'][record*2 + 1]['recordType'] = 3
        tempDictionary['sequences'][0]['records'][record*2 + 1]['dataValue'] = ''
        tempDictionary['sequences'][0]['records'][record*2 + 1]['address'] = str(startRegister + record*2 + 1)
        tempDictionary['sequences'][0]['records'][record*2 + 1]['description'] = name + ' ' + str(record + 1) + ' MSW'
        
    # Add Derived Values
    for record in range(4):
        tempDictionary['sequences'][0]['records'] += [ {} ]
        tempDictionary['sequences'][0]['records'][-1]['recordType'] = 4
        tempDictionary['sequences'][0]['records'][-1]['dataValue'] = ''
        tempDictionary['sequences'][0]['records'][-1]['address'] = 'Int32(HR' + str(startRegister + record*2) + ',HR' + str(startRegister + record*2 + 1) + ')'
        tempDictionary['sequences'][0]['records'][-1]['description'] = name + ' ' + str(record + 1)        

    json.dump(tempDictionary, fileHandler, indent = 4)
    return tempDictionary

def Make256Sequence(name, startRegister):
    name = 'String ' + name
    fileHandler = open(name + '.json', mode='w')
    
    startRegister -= 1
    registersPerSequence = 100
    endRegister = startRegister + 255
    totalRegisters = 256
    
    tempDictionary = {'sequences': [{}, {}, {} ]}
    
    for sequence in range(0, (totalRegisters // registersPerSequence) + 1):
        tempDictionary['sequences'][sequence]['type'] = 0
        tempDictionary['sequences'][sequence]['name'] = name + ' ' + str(sequence)
        tempDictionary['sequences'][sequence]['records'] = []
        
        overallRecordNumber = sequence*registersPerSequence
        
        for record in range(overallRecordNumber, min(totalRegisters, overallRecordNumber + registersPerSequence)):
            tempDictionary['sequences'][sequence]['records'] += [{}]
            tempDictionary['sequences'][sequence]['records'][record % registersPerSequence]['recordType'] = 3
            tempDictionary['sequences'][sequence]['records'][record % registersPerSequence]['address'] = str(startRegister + record)
            tempDictionary['sequences'][sequence]['records'][record % registersPerSequence]['description'] = name + ' Bytes ' + format(record*2, '03d') + '-' + format(record*2 + 1, '03d')
            tempDictionary['sequences'][sequence]['records'][record % registersPerSequence]['dataValue'] = ''
                       
    json.dump(tempDictionary, fileHandler, indent=4)
    return tempDictionary

def ThermalRunawayChannelDurationPairs():
    name = 'String Thermal Runaway Channel Duration'
    fileHandler = open(name + '.json', mode='w')
    
    startRegister = 49201
    registersPerSequence = 100
    totalRegisters = 512
    
    tempDictionary = {'sequences': [ {}, {}, {}, {}, {}, {} ] }
    
    for sequence in range(0, (totalRegisters // registersPerSequence) + 1):
        tempDictionary['sequences'][sequence]['type'] = 0
        tempDictionary['sequences'][sequence]['name'] = name + ' ' + str(sequence)
        tempDictionary['sequences'][sequence]['records'] = []
        
        overallRecordNumber = sequence*registersPerSequence
        
        for record in range(overallRecordNumber, min(totalRegisters, overallRecordNumber + registersPerSequence), 2):
            # Insert LSW
            tempDictionary['sequences'][sequence]['records'] += [{}]
            tempDictionary['sequences'][sequence]['records'][record % registersPerSequence]['recordType'] = 3
            tempDictionary['sequences'][sequence]['records'][record % registersPerSequence]['address'] = str(startRegister + record)
            tempDictionary['sequences'][sequence]['records'][record % registersPerSequence]['description'] = name + ' ' + format(record, '03d') + ' LSW'
            tempDictionary['sequences'][sequence]['records'][record % registersPerSequence]['dataValue'] = ''        
            
            # Insert MSW
            tempDictionary['sequences'][sequence]['records'] += [{}]
            tempDictionary['sequences'][sequence]['records'][(record + 1) % registersPerSequence]['recordType'] = 3
            tempDictionary['sequences'][sequence]['records'][(record + 1) % registersPerSequence]['address'] = str(startRegister + record + 1)
            tempDictionary['sequences'][sequence]['records'][(record + 1) % registersPerSequence]['description'] = name + ' ' + format(record + 1, '03d') + ' MSW'
            tempDictionary['sequences'][sequence]['records'][(record + 1) % registersPerSequence]['dataValue'] = ''        
            
        # Insert Derived Values
        for record in range(overallRecordNumber, min(totalRegisters, overallRecordNumber + registersPerSequence), 2):
            address = record
            
            tempDictionary['sequences'][sequence]['records'] += [{}]
            tempDictionary['sequences'][sequence]['records'][-1]['recordType'] = 4
            tempDictionary['sequences'][sequence]['records'][-1]['address'] = 'Int32(HR' + str(startRegister + record) + ',HR' + str(startRegister + record + 1) + ')'
            tempDictionary['sequences'][sequence]['records'][-1]['description'] = name + ' ' + format(record // 2, '03d')
            tempDictionary['sequences'][sequence]['records'][-1]['dataValue'] = ''         
    
    json.dump(tempDictionary, fileHandler, indent=4)
    return tempDictionary

def MakeAllSequences():
    systemSummaryFields = [ ' Number of Batteries Configured', ' Number of Current Probes Configured', ' Number of DCMs (Total Actual Detected)',  
                            ' Number of Channels (Total Actual Detected)', ' Last Scan Year', ' Last Scan Month', ' Last Scan Day', ' Last Scan Hour',  
                            ' Last Scan Minute', ' Last Scan Second', ' Alarm Status (LSW)', ' Scan Status (LSW)', ' Scan Status (MSW)', ' Scanner Software Version', 
                            ' Modbus Software Version', ' Scan Control Flags ', ' Alarm Relay Control Bits (LSW)', ' Alarm Relay State', ' Ohmic Scan Schedule Hour', 
                            ' Ohmic Frequency (Days)', ' Selected Battery Number "B"', ' Selected String Number "S"', ' TOD Year', ' TOD Month', ' TOD Day',  
                            ' TOD Hour', ' TOD Minute', ' TOD Second', ' Alarm Status (MSW)', ' Alarm Relay Control Bits (MSW)' ]
    batteryFields = [ ' String Count', ' Status (LSW)', ' Status (MSW)' ]
    stringInformationFields = [ ' Number of DCMs Configured', ' Channels Count Detected ', ' Input Bit Count Configured', ' Logical Current Probe Configured', ' Temperature Probe Count Configured',  
                                ' Voltage (calculated)', ' Average Voltage (calculated)', ' Number of Averaged Measurements', ' Voltage Status', ' Voltage High Limit', ' Voltage Low Limit',  
                                ' Voltage Hysteresis', ' Voltage Thermal Limit', ' Voltage Limit Low Discharge', ' Ohmic Value (calculated)', ' Average Ohmic Value (calculated)', 
                                ' Number of Averaged Measurements', ' Ohmic Value Status', ' Ohmic Value High Limit', ' Ohmic Value Low Limit', ' Ohmic Value Hysteresis', ' Status (LSW)', 
                                ' Ripple Voltage', ' Ripple Average', ' Ripple Average Count', ' Ripple Status', ' Ripple Limit', ' Ripple Hysteresis', ' Thermal Runaway Status', 
                                ' Thermal Runaway Year', ' Thermal Runaway Month', ' Thermal Runaway Day', ' Thermal Runaway Hour', ' Thermal Runaway Minute', ' Thermal Runaway Second', 
                                ' Thermal Runaway Duration', ' Thermal Runaway Time-To-Cutoff (LSW)', ' Thermal Runaway Time-To-Cutoff (MSW)', ' Thermal Runaway Hold Delay (LSW)', 
                                ' Thermal Runaway Hold Delay (MSW)', ' Status (MSW)' ]
    stringLogicalCurrentFields = [ ' Current Value (LSW)', ' Current Value (MSW)', ' Average (non-Discharge, LSW)', ' Average (non-Discharge, MSW)', ' Average Count', ' Current Status', ' Discharge Peak (LSW)', 
                                   ' Discharge Peak (MSW)', ' Charge Peak (LSW)', ' Charge Peak (MSW)', ' Discharge Start Year', ' Discharge Start Month', ' Discharge Start Day', ' Discharge Start Hour', 
                                   ' Discharge Start Minute', ' Discharge Start Second', ' Discharge Duration (LSW)', ' Discharge Duration (MSW)', ' Bulk Charge Start Year', ' Bulk Charge Start Month', 
                                   ' Bulk Charge Start Day', ' Bulk Charge Start Hour', ' Bulk Charge Start Minute', ' Bulk Charge Start Second', ' Bulk Charge Duration (LSW)', ' Bulk Charge Duration (MSW)',
                                   ' Discharge Limit (LSW)', ' Discharge Limit (MSW)', ' Charge Limit (LSW)', ' Charge Limit (MSW)', ' Hysteresis' ]
    inputBitFields = [' State', ' Alarm', ' Limit (On/Off)']
    temperatureProbeFields = [ ' Value', ' Average', ' Average Count', ' Status', ' High Limit', ' Low Limit', ' Hysteresis', ' Thermal Runaway Limit' ]    
    
    
    name = 'System Summary'
    MakeCategory(name, 1, systemSummaryFields, False)
    
    name = 'String'
    MakeCategory(name, 513, stringInformationFields, True)
    
    MakePhysicalCT()
    
    name = 'String Logical Current'
    MakeCategory(name, 641, stringLogicalCurrentFields, False)
    
    # Make Battery, String Input Bit and String Temperature Probe sequences
    for i in range(4):
        name = 'Battery ' + str(i + 1)
        MakeCategory(name, 129 + i*48,  batteryFields, True)
        
        name = 'String Input Bit ' + str(i + 1)
        MakeCategory(name, 737 + i*48, inputBitFields, True)
        
        name = 'String Temperature Probe ' + str(i + 1)
        MakeCategory(name, 929 + i*48, temperatureProbeFields, True)
    
    
    Make256Sequence('Temperature (DCM) Value', 1265)
    Make256Sequence('Temperature (DCM) Average', 1521)
    Make256Sequence('Temperature (DCM) Average Count', 1777)
    Make256Sequence('Temperature (DCM) Status', 2033)
    Make256Sequence('Temperature (DCM) High Limit', 2289)
    Make256Sequence('Temperature (DCM) Low Limit', 2545)
    Make256Sequence('Temperature (DCM) Hysteresis', 2801)
    Make256Sequence('Temperature (DCM) Thermal Runaway Limit', 3057)
    
    Make256Sequence('Voltage Value', 3313)
    Make256Sequence('Voltage Average', 3569)
    Make256Sequence('Voltage Average Count', 3825)
    Make256Sequence('Voltage Status', 4081)
    Make256Sequence('Voltage High Limit', 4337)
    Make256Sequence('Voltage Low Limit', 4593)
    Make256Sequence('Voltage Hysteresis', 4849)
    Make256Sequence('Voltage Thermal Runaway Limit', 5105)
    Make256Sequence('Voltage Low Discharge Limit', 5361)
    
    Make256Sequence('Ohmic Value', 5617)
    Make256Sequence('Ohmic Value Average', 5873)
    Make256Sequence('Ohmic Value Average Counts', 6129)
    Make256Sequence('Ohmic Value Status', 6385)
    Make256Sequence('Ohmic Value High Limit', 6641)
    Make256Sequence('Ohmic Value Low Limit', 6897)
    Make256Sequence('Ohmic Value Hysteresis', 7153)
    Make256Sequence('Ohmic Value Last Measurement Year', 7409)
    Make256Sequence('Ohmic Value Last Measurement Month', 7665)
    Make256Sequence('Ohmic Value Last Measurement Day', 7921)
    Make256Sequence('Ohmic Value Last Measurement Hour', 8177)
    Make256Sequence('Ohmic Value Last Measurement Minute', 8433)
    Make256Sequence('Ohmic Value Last Measurement Second', 8689)
    
    Make256Sequence('Thermal Runaway Channel Status', 8945)
    ThermalRunawayChannelDurationPairs()
    Make256Sequence('Thermal Runaway Channel Timestamp, Year', 9713)
    Make256Sequence('Thermal Runaway Channel Timestamp, Month', 9969)
    Make256Sequence('Thermal Runaway Channel Timestamp, Day', 10225)
    Make256Sequence('Thermal Runaway Channel Timestamp, Hour', 10481)
    Make256Sequence('Thermal Runaway Channel Timestamp, Minute', 10737)
    Make256Sequence('Thermal Runaway Channel Timestamp, Second', 10993)
    
    Make256Sequence('FED Electrolyte Level Status', 13297)
    Make256Sequence('FED Electrolyte Alarm Enabled', 13553)
    
    
###################################Deprecated Things############################################################################


def MakeStringInputBits(bitNumber, startRegister):
    name = 'String Input Bit ' + str(bitNumber)
    
    #fileHandler = open('String InputBit' + str(bitNumber) + '.json', mode='w')
    
    tempDictionary = {'sequences': [ {} ] }
    
    tempDictionary['sequences'][0]['type'] = 0
    tempDictionary['sequences'][0]['name'] = name
    tempDictionary['sequences'][0]['records'] = []
    label = MakeLabel(name, startRegister)
    tempDictionary['sequences'][0] = label
    
    inputBitFields = [' State', ' Alarm', ' Limit (On/Off)']
    
    for i in range(len(inputBitFields)):
        fieldDictionary = {}
        
        fieldDictionary['recordType'] = 3
        fieldDictionary['address'] = str(i + startRegister + 32)
        fieldDictionary['Description'] = name + inputBitFields[i]
        fieldDictionary['dataValue'] = ''

        tempDictionary['sequences'][0]['records'] += [ fieldDictionary ]
        
    return tempDictionary
        
def MakeStringTemperatureProbes(probeNumber, startRegister):
    name = 'String Temperature Probe ' + str(probeNumber)
    
    #FileHandler...
    
    tempDictionary = {'sequences': [ {} ] }
    
    tempDictionary['sequences'][0]['type'] = 0
    tempDictionary['sequences'][0]['name'] = name
    tempDictionary['sequences'][0]['records'] = []
    label = MakeLabel(name, startRegister)
    tempDictionary['sequences'][0] = label    
    
    temperatureProbeBitFields = [ ' Value', ' Average', ' Average Count', ' Status', ' High Limit', ' Low Limit', ' Hysteresis', ' Thermal Runaway Limit' ]
    
def GetBatteryLabelAndStatus(): 
    BLAS = json.load(sourceObject, parse_int = int)
    
    stringStart = 40273
    stringEnd = 40304
    
    labelAddress = 'String(HR' + str(stringStart)
    
    for address in range(stringStart + 1, stringEnd + 1):
        labelAddress += ',HR' + str(address)
    
    labelAddress += ')'
    
    for someVar in BLAS['sequences'][0]['records']:
        someVar['description'] = someVar['description'].replace('Battery1', 'Battery 4')
        if (someVar['recordType'] == 3):
            someVar['address'] = str(int(someVar['address']) + 144)
        else:
            someVar['address'] = labelAddress
    
    
    print(BLAS['sequences'][0]['records'][0])
    print(BLAS['sequences'][0]['records'][31])
    print(BLAS['sequences'][0]['records'][34])
    print(BLAS['sequences'][0]['records'][35])
    
    json.dump(BLAS, sourceObject, indent = 4)
    
    sourceObject.close()