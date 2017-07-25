import json

class Field:
    startRegister = 0
    endRegister = 1
    numberOfRegisters = 2
    
    description = 'Default Description'
    recordType = 'Default Record Type'
    address = 'Default Address'
    dataValue = 'Default Data Value'
    units = 'Default Units'
    
    def __init__(self, startRegister, numberOfRegisters, description, recordType = None, units = ''):
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
                    suffix = '[' + str(i + 1) + ']'
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

def MakeInitialDictionary(numberOfSequences):
    sequences = { 'sequences': [] }
    
    for i in range(numberOfSequences):
        sequences['sequences'] += [ {} ]

    return sequences

def WriteSequence(title, sequenceList):
    fileHandler = open(title + '.json', mode='w')
    result = MakeInitialDictionary(len(sequenceList))
    
    for count, sequence in enumerate(result['sequences'], start = 1):
        sequence['type'] = 0
        sequence['name'] = title + ' ' + str(count)
        sequence['records'] = []
        
    for i, sequence in enumerate(sequenceList):
        tempList = []
        for field in sequence:
            tempList += field.ToRecords()
        result['sequences'][i]['records'] = tempList
    
    json.dump(result, fileHandler, indent = 4)       
    return result

def ListToSequence(inputList):
    registersPerField = 0
    returnList = []
    typeList = [ None, None, 'Single', None, 'Int64' ]    

    for i, element in enumerate(inputList):
        if type(element) is int:
            currentRegister = element
        else:
            currentList = []            
            for j, field in enumerate(element):
                if type(field) is int:
                    registersPerField = field
                elif field == 'Unused':
                    currentRegister += registersPerField
                    returnList += [ currentList ]
                    currentList = []
                else:
                    units = None
                    if '[int]' in field:
                        fieldType = 'Int32'
                        field = field.replace('[int]', '')
                    elif '[uint]' in field:
                        fieldType = 'Uint32'
                        field = field.replace('[uint]', '')
                    elif registersPerField < 5:
                        fieldType = typeList[registersPerField]
                    else:
                        fieldType = 'String'
                    currentList += [ Field(currentRegister, registersPerField, field, fieldType, units) ]
                    currentRegister += registersPerField
            
            if len(currentList) > 0:
                returnList += [ currentList ]
                
    return returnList

            
'''
Sequence List: 
General Information
Time 
TransOpto Alarm 1 Settings 
TransOpto Alarm 2 Settings
TransOpto Alarm 3 Settings
TransOpto Alarm 4 Settings
Peripheral Device Scheduler Settings
Digital Input 1 Alarm 1, 2, and 3 Settings
Digital Input 2 Alarm 1, 2, and 3 Settings
Oil Source C Ratio 6 Settings
Oil Source C Ratio 6 Settings
Digital Input 3 Alarm 1, 2, and 3 Settings
Analog Input 1 Alarm 1, 2, and 3 Settings
Analog Input 2 Alarm 1, 2, and 3 Settings
Analog Input 3 Alarm 1, 2, and 3 Settings
Analog Input 4 Alarm 1, 2, and 3 Settings
Analog Input 5 Alarm 1, 2, and 3 Settings
Analog Input 6 Alarm 1, 2, and 3 Settings
PreSens O2 Sensor 1 and 2 Configuration Settings
Oil Source A Settings
Oil Source A Last Record (Float)
Oil Source A Last Record (Int)
Oil Source A Alarm 1 Settings
Oil Source A Alarm 2 Settings
Oil Source A Alarm 3 Settings
Oil Source A Alarm 4 Settings
Oil Source A Alarm 5 Settings
Oil Source A Alarm 6 Settings
Oil Source A Relative Saturation Alarm Settings
Oil Source B Settings
Oil Source B Last Record (Float)
Oil Source B Last Record (Int)
Oil Source B Alarm 1 Settings
Oil Source B Alarm 2 Settings
Oil Source B Alarm 3 Settings
Oil Source B Alarm 4 Settings
Oil Source B Alarm 5 Settings
Oil Source B Alarm 6 Settings
Oil Source B Relative Saturation Alarm Settings
Oil Source C Settings
Oil Source C Last Record (Float)
Oil Source C Last Record (Int)
Oil Source C Alarm 1 Settings
Oil Source C Alarm 2 Settings
Oil Source C Alarm 3 Settings
Oil Source C Alarm 4 Settings
Oil Source C Alarm 5 Settings
Oil Source C Alarm 6 Settings
Oil Source C Relative Saturation Alarm Settings
Oil Source A Ratio 1 Settings
Oil Source A Ratio 2 Settings
Oil Source A Ratio 3 Settings
Oil Source A Ratio 4 Settings
Oil Source A Ratio 5 Settings
Oil Source B Ratio 1 Settings
Oil Source B Ratio 2 Settings
Oil Source B Ratio 3 Settings
Oil Source B Ratio 4 Settings
Oil Source B Ratio 5 Settings
Oil Source C Ratio 1 Settings
Oil Source C Ratio 2 Settings
Oil Source C Ratio 3 Settings
Oil Source C Ratio 4 Settings
Oil Source C Ratio 5 Settings
Analog Input 1 Last Record
Analog Input 2 Last Record
Analog Input 3 Last Record
Analog Input 4 Last Record
Analog Input 5 Last Record
Analog Input 6 Last Record
TransOpto Last Record
Digital Input 1 Last Record
Digital Input 2 Last Record
Digital Input 3 Last Record
'''
def MakeAllSequences():
    baseRegister = -1
    
    mappings = {
        'General Information': [ 1000, GetInformation()  ], 
        'Time': [ 1197, GetTime() ], 
        'TransOpto Alarm 1 Settings': [ 2100, GetAlarm(1) ], 
        'TransOpto Alarm 2 Settings': [ 2200, GetAlarm(2) ], 
        'TransOpto Alarm 3 Settings': [ 2300, GetAlarm(3) ], 
        'TransOpto Alarm 4 Settings': [ 2400, GetAlarm(4) ], 
        'Peripheral Device Scheduler Settings':  [ 2655, GetPeripheralDeviceSchedulerSettings() ], 
        'Digital Input 1 Alarm 1, 2, and 3 Settings': [ 2800, GetDigitalInputAlarmSettings(1, 1), 2820, GetDigitalInputAlarmSettings(1, 2), 2840, GetDigitalInputAlarmSettings(1, 3) ], 
        'Digital Input 2 Alarm 1, 2, and 3 Settings': [ 2860, GetDigitalInputAlarmSettings(2, 1), 2880,  GetDigitalInputAlarmSettings(2, 2), 2900, GetDigitalInputAlarmSettings(2, 3) ], 
        'Digital Input 3 Alarm 1, 2, and 3 Settings': [ 2920, GetDigitalInputAlarmSettings(3, 1), 2940, GetDigitalInputAlarmSettings(3, 2), 2960, GetDigitalInputAlarmSettings(3, 3) ], 
        'Analog Input 1 Alarm 1, 2, and 3 Settings': [ 3040, GetAnalogInputAlarmSettings(1, 1), 3060, GetAnalogInputAlarmSettings(1, 2), 3080, GetAnalogInputAlarmSettings(1, 3) ], 
        'Analog Input 2 Alarm 1, 2, and 3 Settings': [ 3140, GetAnalogInputAlarmSettings(2, 1), 3160, GetAnalogInputAlarmSettings(2, 2), 3180, GetAnalogInputAlarmSettings(2, 3) ],
        'Analog Input 3 Alarm 1, 2, and 3 Settings': [ 3240, GetAnalogInputAlarmSettings(3, 1), 3260, GetAnalogInputAlarmSettings(3, 2), 3280, GetAnalogInputAlarmSettings(3, 3) ],
        'Analog Input 4 Alarm 1, 2, and 3 Settings': [ 3340, GetAnalogInputAlarmSettings(4, 1), 3360, GetAnalogInputAlarmSettings(4, 2), 3380, GetAnalogInputAlarmSettings(4, 3) ],
        'Analog Input 5 Alarm 1, 2, and 3 Settings': [ 3440, GetAnalogInputAlarmSettings(5, 1), 3460, GetAnalogInputAlarmSettings(5, 2), 3480, GetAnalogInputAlarmSettings(5, 3) ],
        'Analog Input 6 Alarm 1, 2, and 3 Settings': [ 3540, GetAnalogInputAlarmSettings(6, 1), 3560, GetAnalogInputAlarmSettings(6, 2), 3580, GetAnalogInputAlarmSettings(6, 3) ],
        'PreSens O2 Sensor 1 and 2 Configuration Settings': [ 3700, GetO2SensorConfigurationSettings(1), 3740, GetO2SensorConfigurationSettings(2) ], 
        'Oil Source A Settings': [ 4000, GetOilSourceSettings('A') ], 
        'Oil Source A Last Record (Float)': [ 4098, GetFloatOilSourceLastRecord('A') ], 
        'Oil Source A Last Record (Int)': [ 4198, GetIntOilSourceLastRecord('A') ], 
        'Oil Source A Alarm 1 Settings': [ 4300, GetOilSourceAlarmSettings('A', 1) ], 
        'Oil Source A Alarm 2 Settings': [ 4400, GetOilSourceAlarmSettings('A', 2) ], 
        'Oil Source A Alarm 3 Settings': [ 4500, GetOilSourceAlarmSettings('A', 3) ], 
        'Oil Source A Alarm 4 Settings': [ 4600, GetOilSourceAlarmSettings('A', 4) ], 
        'Oil Source A Alarm 5 Settings': [ 4700, GetOilSourceAlarmSettings('A', 5) ], 
        'Oil Source A Alarm 6 Settings': [ 4800, GetOilSourceAlarmSettings('A', 6) ], 
        'Oil Source A Relative Saturation Alarm Settings': [ 4902, GetOilSourceRelativeSaturationAlarmSettings('A') ], 
        'Oil Source B Settings': [ 5000, GetOilSourceSettings('B') ], 
        'Oil Source B Last Record (Float)': [ 5098, GetFloatOilSourceLastRecord('B') ], 
        'Oil Source B Last Record (Int)': [ 5198, GetIntOilSourceLastRecord('B') ], 
        'Oil Source B Alarm 1 Settings': [ 5300, GetOilSourceAlarmSettings('B', 1) ], 
        'Oil Source B Alarm 2 Settings': [ 5400, GetOilSourceAlarmSettings('B', 2) ], 
        'Oil Source B Alarm 3 Settings': [ 5500, GetOilSourceAlarmSettings('B', 3) ], 
        'Oil Source B Alarm 4 Settings': [ 5600, GetOilSourceAlarmSettings('B', 4) ], 
        'Oil Source B Alarm 5 Settings': [ 5700, GetOilSourceAlarmSettings('B', 5) ], 
        'Oil Source B Alarm 6 Settings': [ 5800, GetOilSourceAlarmSettings('B', 6) ], 
        'Oil Source B Relative Saturation Alarm Settings': [ 5902, GetOilSourceRelativeSaturationAlarmSettings('B') ], 
        'Oil Source C Settings': [ 6000, GetOilSourceSettings('C') ], 
        'Oil Source C Last Record (Float)': [ 6098, GetFloatOilSourceLastRecord('C') ], 
        'Oil Source C Last Record (Int)': [ 6198, GetIntOilSourceLastRecord('C') ], 
        'Oil Source C Alarm 1 Settings': [ 6300, GetOilSourceAlarmSettings('C', 1) ], 
        'Oil Source C Alarm 2 Settings': [ 6400, GetOilSourceAlarmSettings('C', 2) ], 
        'Oil Source C Alarm 3 Settings': [ 6500, GetOilSourceAlarmSettings('C', 3) ], 
        'Oil Source C Alarm 4 Settings': [ 6600, GetOilSourceAlarmSettings('C', 4) ], 
        'Oil Source C Alarm 5 Settings': [ 6700, GetOilSourceAlarmSettings('C', 5) ], 
        'Oil Source C Alarm 6 Settings': [ 6800, GetOilSourceAlarmSettings('C', 6) ], 
        'Oil Source C Relative Saturation Alarm Settings': [ 6902, GetOilSourceRelativeSaturationAlarmSettings('C') ], 
        'Oil Source A Ratio 1 Settings': [ 7100, GetOilSourceRatioSettings('A', 1) ], 
        'Oil Source A Ratio 2 Settings': [ 7120, GetOilSourceRatioSettings('A', 2) ], 
        'Oil Source A Ratio 3 Settings': [ 7140, GetOilSourceRatioSettings('A', 3) ], 
        'Oil Source A Ratio 4 Settings': [ 7160, GetOilSourceRatioSettings('A', 4) ], 
        'Oil Source A Ratio 5 Settings': [ 7180, GetOilSourceRatioSettings('A', 5) ], 
        'Oil Source B Ratio 1 Settings': [ 7300, GetOilSourceRatioSettings('B', 1) ], 
        'Oil Source B Ratio 2 Settings': [ 7320, GetOilSourceRatioSettings('B', 2) ], 
        'Oil Source B Ratio 3 Settings': [ 7340, GetOilSourceRatioSettings('B', 3) ], 
        'Oil Source B Ratio 4 Settings': [ 7360, GetOilSourceRatioSettings('B', 4) ], 
        'Oil Source B Ratio 5 Settings': [ 7380, GetOilSourceRatioSettings('B', 5) ], 
        'Oil Source C Ratio 1 Settings': [ 7500, GetOilSourceRatioSettings('C', 1) ], 
        'Oil Source C Ratio 2 Settings': [ 7520, GetOilSourceRatioSettings('C', 2) ], 
        'Oil Source C Ratio 3 Settings': [ 7540, GetOilSourceRatioSettings('C', 3) ], 
        'Oil Source C Ratio 4 Settings': [ 7560, GetOilSourceRatioSettings('C', 4) ], 
        'Oil Source C Ratio 5 Settings': [ 7580, GetOilSourceRatioSettings('C', 5) ], 
        'Analog Input 1 Last Record': [ 7998, GetAnalogInputLastRecord(1) ], 
        'Analog Input 2 Last Record': [ 8050, GetAnalogInputLastRecord(2) ], 
        'Analog Input 3 Last Record': [ 8100, GetAnalogInputLastRecord(3) ], 
        'Analog Input 4 Last Record': [ 8150, GetAnalogInputLastRecord(4) ], 
        'Analog Input 5 Last Record': [ 8200, GetAnalogInputLastRecord(5) ], 
        'Analog Input 6 Last Record': [ 8250, GetAnalogInputLastRecord(6) ], 
        'TransOpto Last Record': [ 8298, GetTransOptoLastRecord() ], 
        'Digital Input 1 Last Record': [ 8348, GetDigitalInputLastRecord(1) ], 
        'Digital Input 2 Last Record': [ 8398, GetDigitalInputLastRecord(2) ], 
        'Digital Input 3 Last Record': [ 8448, GetDigitalInputLastRecord(3) ]
    }
    
    tempDictionary = {}
    
    for key, value in mappings.items():
        for i, element in enumerate(value):
            if type(element) is int:
                mappings[key][i] = mappings[key][i] + baseRegister
    
    for key, value in mappings.items():
        tempDictionary[key] = ListToSequence(value)
    
    mappings = tempDictionary
    
    for key, value in mappings.items():
        WriteSequence(key, value)
    
    return mappings

def GetInformation():
    return [ 
        16,
        'Device ID', # 1015
        
        8,
        'Serial Number', # 1023
        
        1,
        'Firmware Version', 'Unused', 
        'Number of Oil Sources Available', 'TRANSFIX Class', 'Chosen User Language', 
        
        2, 
        'TRANSIFX Firmware Verion[int]',  # 1030
        
        12,
        'Unused', # 1042
        
        1,
        'Measurement Counter', 'Unused', 'Heartbeat', 'Normal State', # 1046
        
        4, 
        'Unused', # 1050
        
        1,
        'Measuring / Standby', 'Scheduler Enable', # 1052
        
        7,
        'Unused', # 1059
        
        1,
        'Caution Indicator', 'Alarm Indicator', 'Service Indicator', 'Relay 2', 'Relay 3', 
        'Relay 4', 'Relay 5', 'Relay 6', 'Relay 7', 'Alarm Reflash Enable' # 1069        
        ]

def GetTime():
    return[
        2,
        'Time (UTC) in UNIX format[uint]', 
        
        1, 
        'Unused', # 1199
        
        1,
        'UTC Clock: Years', 'UTC Clock: Months, Days', 'UTC Clock: Hours, Minutes', 'UTC Clock: Seconds, Day of Week', # 1203
        
        62,
        'Unused', # 1265
        
        1,
        'Local Time: Years', 'Local Time: Months, Days', 'Local Time: Hours, Minutes', 'Local Time: Seconds, Day of Week' # 1269
        ]

def GetAlarm(alarm):
    return[
        2,
        'TransOpto Channel 1 Min Limit of Value', 'TransOpto Channel 2 Min Limit of Value', 'TransOpto Channel 3 Min Limit of Value', 'TransOpto Channel 4 Min Limit of Value', 
        'TransOpto Channel 5 Min Limit of Value', 'TransOpto Channel 6 Min Limit of Value', 'TransOpto Channel 7 Min Limit of Value', 'TransOpto Channel 8 Min Limit of Value',  # 2115
        
        'TransOpto Channel 1 Max Limit of Value', 'TransOpto Channel 2 Max Limit of Value', 'TransOpto Channel 3 Max Limit of Value', 'TransOpto Channel 4 Max Limit of Value', 
        'TransOpto Channel 5 Max Limit of Value', 'TransOpto Channel 6 Max Limit of Value', 'TransOpto Channel 7 Max Limit of Value', 'TransOpto Channel 8 Max Limit of Value', # 2131
        
        'TransOpto Channel 1 Max Positive Limit of RoC', 'TransOpto Channel 2 Max Positive Limit of RoC', 'TransOpto Channel 3 Max Positive Limit of RoC', 'TransOpto Channel 4 Max Positive Limit of RoC',
        'TransOpto Channel 5 Max Positive Limit of RoC', 'TransOpto Channel 6 Max Positive Limit of RoC', 'TransOpto Channel 7 Max Positive Limit of RoC', 'TransOpto Channel 8 Max Positive Limit of RoC', # 2147
        
        'Spare', 'Spare', 'Spare', 'Spare', 'Spare', 'Spare', 'Spare', 'Spare', # 2163
        
        1, 
        'TransOpto Alarm ' + str(alarm) + ' Output', 'TransOpto Time Window for Alarm ' + str(alarm) + ' ROC Calculation (Hours)', 'TransOpto Minimum Number of Samples for Alarm ' + str(alarm) + ' ROC Calculation' # 2166 Check About Derived Values with masks for 2164        
        ]

def GetPeripheralDeviceSchedulerSettings():
    return [
        1,
        'Measurement Scheduling Mode for Peripheral Devices', 'Measurement Schedule For Peripheral Devices in Normal Mode', 'Measurement Schedule For Peripheral Devices in Caution Mode', 
        'Measurement Schedule For Peripheral Devices in Alarm Mode', 'Next Measurement: Years (GMT Time)', 'Next Measurement: Month, Days (GMT Time)', 'Next Measurement: Hours, Minutes (GMT Time)' # 2661
        ]

def GetDigitalInputAlarmSettings(digitalInput, alarm):
    return [
        2,
        'Digital Input ' + str(digitalInput) + ' Alarm ' + str(alarm) + ' Max Level of Transition Count' # 2961 
        ]

def GetAnalogInputAlarmSettings(analogInput, alarm):
    return [
        2,
        'Analog Input ' + str(analogInput) + ' Alarm ' + str(alarm) + ' Min Limit of Value', 'Analog Input ' + str(analogInput) + ' Alarm ' + str(alarm) + 'Max Limit of Value',
        'Analog Input ' + str(analogInput) + ' Alarm ' + str(alarm) + 'Max Positive Limit of RoC', 'Spare', 
        
        1, 
        'Analog Input ' + str(analogInput) + ' Alarm ' + str(alarm) + 'Output Alerts', 'Analog Input ' + str(analogInput) + ' Alarm ' + str(alarm) + ' Time Window (hours) for ROC Calculation', 
        'Analog Input ' + str(analogInput) + ' Alarm ' + str(alarm) + ' Min Number of Samples for ROC Calculation'#3590
        ]

def GetO2SensorConfigurationSettings(sensor):
    return [
        1,
        'O2 Sensor ' + str(sensor) + ' Enable', 'O2 Sensor ' + str(sensor) + ' Modbus Address' # 3741
        ]

def GetOilSourceSettings(source):
    return [
        8,
        'Oil Source ' + source + ' ID String',
        
        1, 
        'Measurement Scheduling Mode for Oil Source ' + source, 'Measurement Schedule for Oil Source ' + source + ' in Normal Mode', 'Measurement Schedule for Oil Source ' + source + ' in Caution Mode', 
        'Measurement Schedule for Oil Source ' + source + ' in Alarm Mode', 'Enables/Disables the Oil Source, Used in MultiTrans Only', 'Oil Source ' + source + ' Scheduler Control', 
        
        6, 
        'Unused', 
        
        1, 
        'Hydrogen Conc. Calibration Gain in %', 'Hydrogen Conc. Calibration Offset in PPM', 'Carbon Dioxide Conc. Calibration Gain in %', 'Carbon Dioxide Conc. Calibration Offset in PPM', 
        'Carbon Monoxide Conc. Calibration Gain in %', 'Carbon Monoxide Conc. Calibration Offset in PPM', 'Ethylene Conc. Calibration Gain in %', 'Ethylene Conc. Calibration Offset in PPM', 
        'Ethane Conc. Calibration Gain in %', 'Ethane Conc. Calibration Offset in PPM', 'Methane Conc. Calibration Gain in %', 'Methane Conc. Calibration Offset in PPM', 
        'Acetylene Conc. Calibration Gain in %', 'Acetylene Conc. Calibration Offset in PPM', 'Water Conc. Calibration Gain in %', 'Water Conc. Calibration Offset in PPM', 
        'Oxygen Conc. Calibration Gain in %', 'Oxygen Conc. Calibration Offset in PPM', 'Nitrogen Conc. Calibration Gain in %', 'Nitrogen Conc. Calibration Offset in PPM', 
        
        10,
        'Unused',
        
        1, 
        'Next Measurement (UTC/Local): Years', 'Next Measurement (UTC/Local): Month, Days', 'Next Measurement (UTC/Local): Hours, Minutes'
        ]
def GetFloatOilSourceLastRecord(source):
    returnList = [ 2, 
                   'Record Date and Time(UTC) in UNIX Format[uint]', 
                   
                   1, 
                   'Record Number', 'Record Oil Source', 'Record Date (UTC/Local): Years', 'Record Date (UTC/Local): Months, Days', 
                   'Record Time (UTC/Local): Hours, Minutes', 'PGA Firmware Version', 'Host Firmware Version', 'Gas Conc. Level Alarm Status', 'Gas ROC Alarm Status', 'Transfix Status', 'Measurement Flags', 'PGA State (If Error Occurred)', 
                   
                   2, 
                   'PGA Error Codes', 
                   
                   1, 
                   'Measurement Duration (Seconds)', 'Ratio Alarm Status', 
                   
                   2 ]
    source = 'Oil Source ' + source
    concentrations = [ 'Hydrogen', 'Carbon Dioxide', 'Carbon Monoxide', 'Ethylene', 'Ethane', 'Methane', 'Acetylene', 'Water', 'Oxygen', 'Total Disolved Combustible Gas', 'Nitrogen', 'Total Dissolved Gas' ]
    otherMeasurements = [ 'Oil Pressure (kPa)', 'Oil Temperature (C)', 'Ambient Temperature (C)', 'Normalisation Temperature (C)' ]
    finalMeasurements = [ 'Spare Register', 'ESHL (Estimated Safe Handling Limit) (%)', 'Relative Saturation (%)', 'TDH (Total Dissolved Hydrocarbons (PPM)' ]
    
    for concentration in concentrations:
        returnList += [ source + ' ' + concentration + ' Conc. (PPM)' ]
    
    for measurement in otherMeasurements:
        returnList += [ source + ' ' + measurement ]
        
    for i in range(1, 7):
        returnList += [ source + ' Analog Input ' + str(i) ]
        if i == 1:
            returnList[-1] += ' (Transformer Load)'
        
    for measurement in finalMeasurements:
        returnList += [ source + ' ' + measurement ]
        
    return returnList

def GetIntOilSourceLastRecord(source):
    returnList = [ 2, 'Record Date and Time (UTC) Unix Format[int]', 1 ]
    source = 'Oil Source ' + source
    
    fields1 = [ 'Record Number', 'Record Oil Source', 'Record Date (UTC/Local): Years', 'Record Date (UTC/Local): Monts, Days', 'Record Date (UTC/Local): Hours, Minutes', 
                     'PGA Firmware Version', 'Host Firmware Version', 'Gas Conc. Level Alarm Status', 'Gas ROC Alarm Status', 'Transfix Status', 
                     'Measurement Flags', 'PGA State (If Error Occured)' ]
    
    fields2 = [ 'Oil Pressure (.1 kPa)', 'Oil Temperature (.1 C)', 'Ambient Temperature (.1 C)', 
                     'Normalisation Temperature (C)', 'Analogue Input 1 (Transformer Load)', 'Analogue Input 2', 'Analogue Input 3', 'Analogue Input 4', 
                     'Analogue Input 5', 'Analogue Input 6', 'Spare Registers', 'ESHL (Estimated Safe Handling Limit) (%)', 'Relative Saturation (.1 %)', 
                     'TDH (Total Dissolved Hydrocarbons) (PPM)', 'Total Dissolved Gas Conc. (10 PPM)', 'Nitrogen Conc. (10 PPM)', 'TDG (Total Dissolved Gas (10 PPM)'
                     ]
    
    gases = [ 'Hydrogen', 'Carbon Dioxide', 'Carbon Monoxide', 'Ethylene', 'Ethane', 'Methane', 'Acetylene', 'Water', 'Oxygen', 'Total Disolved Combustible Gas', 'Nitrogen', 'TDG (Total Dissolved Gas)' ]
    
    for field in fields1:
        returnList += [ source + ' ' + field ]
    
    returnList += [ 2, 'PGA Error Codes', 1, 'Measurement Duration', 'Ratio Alarm Status' ]
    
    for gas in gases:
        returnList += [ source + ' ' + gas + ' Conc. (PPM)' ] 
    
    for field in fields2:
        returnList += [ source + ' ' + field ]
    
    return returnList

def GetOilSourceAlarmSettings(source, alarm):
    returnList = [ 2 ]
    
    source = 'Oil Source: ' + source + ' Alarm: ' + str(alarm)
    
    gases = [ 'Hydrogen', 'Carbon Dioxide', 'Carbon Monoxide' , 'Ethylene', 'Ethane', 
              'Methane', 'Acetylene', 'Water', 'Oxygen',  'Total Dissolved Combustible Gas', 
              'Nitrogen' ]
    
    finalFields = [ 'Output', 'Time Window (in hours) for ROC Calculation', 'Minimum Number of Samples for ROC Calculation' ]
    
    for gas in gases:
        returnList += [ source + ' ' +  gas + ' Conc. Limit' ]
    
    returnList += [ 18, 'Spare Register', 2 ]
    
    for gas in gases:
        returnList += [ source + ' ' + gas + ' ROC Limit' ]
    
    returnList += [ 18, 'Unused', 1 ]
    
    for field in finalFields:
        returnList += [ source + ' ' + field ]    
    
    return returnList

def GetOilSourceRelativeSaturationAlarmSettings(source):
    return [ 2, 'Oil Source ' + source + ' Relative Saturation Upper Limit in Percent', 1, 'Oil Source ' + source + ' Relative Saturation Alarm Output' ]

def GetOilSourceRatioSettings(source, ratio):
    source = 'Oil Source ' + source + ' Ratio ' + str(ratio)
    returnList = [ 8, source + ' Name', 
                   1, source + ' Numerator', source + ' Denominator',
                   
                   2, 
                   source + ' Upper Limit', source + ' Lower Limit', 
                   
                   1, 
                   source + ' Enable', source + ' Alarm Output', 
                   
                   2, 
                   source + ' Value From Last Measurement', 
                   
                   1, 
                   source + ' Output Flags From Last Measurement' ]
    
    return returnList

def GetAnalogInputLastRecord(analogInput):
    source = 'Analog Input ' + str(analogInput) + ' '
    
    returnList = [ 2, 
                   source + 'Record Date and Time (UTC) in Unix Format[int]', 
                   
                   1, 
                   source + 'Record Number', source + 'Oil Source', source + 'Record Date (UTC/Local) Year', source + 'Record Date (UTC/Local) Month, Day', 
                   source + 'Record Time (UTC/Local) Hours, Minutes', source + 'Input Number', source + 'Type of Analog Input', 
                   
                   5, 
                   source + 'Name', 
                   
                   2, 
                   source + 'Units', 
                   
                   1,
                   source + 'Alarm Status (Reason for Alarm)', source + 'Alarm Output', 
                   
                   2, 
                   source + 'PGA Value (Float)', 
                   
                   1, 
                   source + 'PGA Value (Int)', 
                   
                   2, 
                   'Spare'                   
                   ]
    
    return returnList

def GetTransOptoLastRecord():
    return[
        2, 
        'Record Date and Time (UTC) UNIX Format[uint]', 
        
        1, 
        'Record Number', 'Record Oil Source', 'Record Date (UTC/Local): Years', 'Record Date (UTC/Local): Months, Days', 'Record Time (UTC/Local): Hours, Minutes', 
        'Spare', 'Spare', 'TransOpto Units', 'TransOpto Calibration Type', 'Alarm Status HI Word', 'Alarm Status LO Word', 'TransOpto Alarm Output Flags From Last Measurement', 
        'TransOpto Channel 1 Current Value', 'TransOpto Channel 2 Current Value', 'TransOpto Channel 3 Current Value', 'TransOpto Channel 4 Current Value', 'TransOpto Channel 5 Current Value', 
        'TransOpto Channel 6 Current Value', 'TransOpto Channel 7 Current Value', 'TransOpto Channel 8 Current Value', 'Spare' ]

def GetDigitalInputLastRecord(digitalInput):
    source = 'Digital Input ' + str(digitalInput)
    return[
        2,
        'Record Date and Time (UTC) UNIX Format[uint]',
        
        1, 
        'Record Number', 'Record Oil Source', 'Record Date (UTC/Local): Year', 'Record Date (UTC/Local): Month, Day', 'Record Time (UTC/Local): Hours, Minutes', 
        'Input Number', source + ' Type', 
        
        5, 
        source + ' Name', 
        
        1, 
        source + ' Result Status', source + ' Output Flags', source + ' Value (ON/OFF)', 
        
        2, 
        'Number of ' + source + ' Transition Since Last ' + source + ' Enable', 'Spare', 'Spare' ]

MakeAllSequences()