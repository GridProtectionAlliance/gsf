//
// Copyright (c) TVA, 2004. All rights reserved.
//
// James Ritchie Carroll - June 2004
//

function OnFinish(selProj, selObj)
{
    var oldSuppressUIValue = true;
    try
    {
        oldSuppressUIValue = dte.SuppressUI;
        var bSilent = wizard.FindSymbol("SILENT_WIZARD");
        dte.SuppressUI = bSilent;

      	var project = null;
      	var dtm = new Date();
      	var strCodeLibPath = "\\\\opdat\\opdat\\CtrlSys\\CodeLibrary\\Builds\\Release\\Current";
      	var strProjectName = wizard.FindSymbol("PROJECT_NAME");
      	var strProjectID = ValidIdentifier(strProjectName);
        var strProjectPath = wizard.FindSymbol("PROJECT_PATH");
        var strTemplatePath = wizard.FindSymbol("TEMPLATES_PATH");
        var strTemplateFile = strTemplatePath + "\\TVAService.vbproj";
        var strRootProjectPath = strProjectPath;
        var strRootTemplatePath = strTemplatePath;
        var strRawGuid;
        var item;
        var editor;

        // Create an instance of the TVA wizard tools object
        var wto = new ActiveXObject("TVA.WizTools");
            
	// Create a file system object
        var fso = new ActiveXObject("Scripting.FileSystemObject");
        var fsoFiles = null;
        var strFileName = "";
        var strFileExt = "";
        var i = 0;

	// Create the base service project
	project = CreateVSProject(strProjectName, ".vbproj", strProjectPath, strTemplateFile);
        if (project)
        {
        	project.Properties("RootNamespace").Value = strProjectID;        	
		strProjectName = project.Name;  //In case it got changed

		// We add the the project name as one of our key template symbols
		wizard.AddSymbol("PROJECT_NAME", strProjectName);
		wizard.AddSymbol("PROJECT_ID", strProjectID);
		wizard.AddSymbol("CAP_PROJECT_ID", strProjectID.toUpperCase());
		wizard.AddSymbol("GEN_TIME", dtm.toString());
		wizard.AddSymbol("USER_LOGINID", wto.CurrentUserID);
		wizard.AddSymbol("USER_NAME", wto.CurrentUserFullName);
		wizard.AddSymbol("CURR_DATE", (dtm.getMonth() + 1) + "/" + dtm.getDate() + "/" + dtm.getYear());
		wizard.AddSymbol("CURR_YEAR", dtm.getYear().toString());
		wizard.AddSymbol("DEV_NAME", wto.CurrentUserFullName + ", " + wto.CurrentUserProperty("title") + " [" + wto.CurrentUserProperty("company") + "]");
		wizard.AddSymbol("DEV_OFFICE", wto.CurrentUserProperty("physicalDeliveryOfficeName") + " - " + wto.CurrentUserProperty("department") + ", " + wto.CurrentUserProperty("l") + ", " + wto.CurrentUserProperty("st") + " - " + wto.CurrentUserProperty("streetAddress"));
		wizard.AddSymbol("DEV_PHONE", wto.CurrentUserProperty("telephoneNumber"));
		wizard.AddSymbol("DEV_EMAIL", wto.CurrentUserProperty("mail"));
		
		strRawGuid = wizard.CreateGuid();
		wizard.AddSymbol("GUID_HOSTAUTHKEY", wizard.FormatGuid(strRawGuid, 0));

		// Add content files to project
		fso.CopyFile(strTemplatePath + "\\StatusLog.template", strProjectPath + "\\StatusLog.template");
		AddContentFileToVSProject(project.ProjectItems, strProjectPath + "\\StatusLog.template");

		// Add misc files to project
		fso.CopyFile(strTemplatePath + "\\TVAService.ico", strProjectPath + "\\TVAService.ico");
		AddMiscFileToVSProject(project.ProjectItems, strProjectPath + "\\TVAService.ico");
		
		// Copy TVA.Remoting.Host.exe as content
		fso.CopyFile(strCodeLibPath + "\\TVA.Remoting.Host.exe", strProjectPath +  "\\TVA.Remoting.Host.exe");
		AddContentFileToVSProject(project.ProjectItems, strProjectPath +  "\\TVA.Remoting.Host.exe");

		// Make sure needed TVA code library assemblies get added as references
		AddProjectReference(project, strCodeLibPath + "\\TVA.Shared.dll");
		AddProjectReference(project, strCodeLibPath + "\\TVA.Config.dll");
		AddProjectReference(project, strCodeLibPath + "\\TVA.Database.dll");
		AddProjectReference(project, strCodeLibPath + "\\TVA.ESO.Ssam.dll");
		AddProjectReference(project, strCodeLibPath + "\\TVA.Remoting.dll");
		AddProjectReference(project, strCodeLibPath + "\\TVA.Services.dll");

		strRawGuid = wizard.CreateGuid();
		wizard.AddSymbol("GUID_ASSEMBLY", wizard.FormatGuid(strRawGuid, 0));

		strTemplateFile = strTemplatePath + "\\app.config"; 
		item = AddFileToVSProject("app.config", project, project.ProjectItems, strTemplateFile, false);

		strTemplateFile = strTemplatePath + "\\AssemblyInfo.vb"; 
		item = AddFileToVSProject("AssemblyInfo.vb", project, project.ProjectItems, strTemplateFile, false);
		if (item) item.Properties("SubType").Value = "Code";

		strTemplateFile = strTemplatePath + "\\PrimaryServiceProcess.vb"; 
		item = AddFileToVSProject("PrimaryServiceProcess.vb", project, project.ProjectItems, strTemplateFile, false);
		if (item)
		{
			item.Properties("SubType").Value = "Code";
			editor = item.Open(vsViewKindCode);
			editor.Visible = true;
		}

		strTemplateFile = strTemplatePath + "\\ProjectInstaller.vb"; 
		item = AddFileToVSProject("ProjectInstaller.vb", project, project.ProjectItems, strTemplateFile, false);
		if (item) item.Properties("SubType").Value = "Component";

		strTemplateFile = strTemplatePath + "\\UserService.vb"; 
		item = AddFileToVSProject(strProjectName + ".vb", project, project.ProjectItems, strTemplateFile, false);
		if (item)
		{
			item.Properties("SubType").Value = "Component";
			project.Properties("StartupObject").Value = project.Properties("RootNamespace").Value + "." + strProjectID;
			
			// Add resource file for service
			AddResourceFileToVSProject(item.ProjectItems, strProjectName + ".resx", strTemplatePath + "\\UserService.resx");
		}

		project.Save();
        }
        
        // We keep this solution open so we can add other projects
        wizard.RemoveSymbol("CLOSE_SOLUTION");
        project = null;
        strProjectPath = strRootProjectPath;
        strTemplatePath = strRootTemplatePath;

	// Create the console monitor project
	strProjectName = "ConsoleMonitor";
	strTemplatePath += "\\ConsoleMonitor";
	strProjectPath += "\\ConsoleMonitor";
        strTemplateFile = strTemplatePath + "\\ConsoleMonitor.vbproj";

        project = CreateVSProject(strProjectName, ".vbproj", strProjectPath, strTemplateFile);

        if (project)
        {
		strProjectName = project.Name;  //In case it got changed

		// Add misc files to project
		fso.CopyFile(strTemplatePath + "\\TVAConsoleMonitor.ico", strProjectPath + "\\TVAConsoleMonitor.ico");
		AddMiscFileToVSProject(project.ProjectItems, strProjectPath + "\\TVAConsoleMonitor.ico");

		// Make sure needed TVA code library assemblies get added as references
		AddProjectReference(project, strCodeLibPath + "\\TVA.Shared.dll");
		AddProjectReference(project, strCodeLibPath + "\\TVA.Config.dll");
		AddProjectReference(project, strCodeLibPath + "\\TVA.Remoting.dll");
		AddProjectReference(project, strCodeLibPath + "\\TVA.Services.dll");

		// We have to create a new GUID for this project
		wizard.RemoveSymbol("GUID_ASSEMBLY");
		strRawGuid = wizard.CreateGuid();
		wizard.AddSymbol("GUID_ASSEMBLY", wizard.FormatGuid(strRawGuid, 0));

		strTemplateFile = strTemplatePath + "\\AssemblyInfo.vb"; 
		item = AddFileToVSProject("AssemblyInfo.vb", project, project.ProjectItems, strTemplateFile, false);
		if (item) item.Properties("SubType").Value = "Code";

		strTemplateFile = strTemplatePath + "\\ConsoleMonitor.vb"; 
		item = AddFileToVSProject("ConsoleMonitor.vb", project, project.ProjectItems, strTemplateFile, false);
		if (item)
		{
			item.Properties("SubType").Value = "Code";
			project.Properties("StartupObject").Value = project.Properties("RootNamespace").Value + ".ConsoleMonitor";
		}

		project.Save();
        }
        
        // We keep this solution open so we can add other projects
        wizard.RemoveSymbol("CLOSE_SOLUTION");
        project = null;
        strProjectPath = strRootProjectPath;
        strTemplatePath = strRootTemplatePath;

	// Create the GUI application based remote monitor project
	strProjectName = "RemoteMonitor";
	strTemplatePath += "\\RemoteMonitor";
	strProjectPath += "\\RemoteMonitor";
        strTemplateFile = strTemplatePath + "\\RemoteMonitor.vbproj";
        project = CreateVSProject(strProjectName, ".vbproj", strProjectPath, strTemplateFile);
        if (project)
        {
		strProjectName = project.Name;  //In case it got changed

		// Add misc files to project
		fso.CopyFile(strTemplatePath + "\\TVARemoteMonitor.ico", strProjectPath + "\\TVARemoteMonitor.ico");
		AddMiscFileToVSProject(project.ProjectItems, strProjectPath + "\\TVARemoteMonitor.ico");

		// Make sure needed TVA code library assemblies get added as references
		AddProjectReference(project, strCodeLibPath + "\\TVA.Shared.dll");
		AddProjectReference(project, strCodeLibPath + "\\TVA.Config.dll");
		AddProjectReference(project, strCodeLibPath + "\\TVA.Forms.dll");
		AddProjectReference(project, strCodeLibPath + "\\TVA.Remoting.dll");
		AddProjectReference(project, strCodeLibPath + "\\TVA.Services.dll");

		// We have to create a new GUID for this project
		wizard.RemoveSymbol("GUID_ASSEMBLY");
		strRawGuid = wizard.CreateGuid();
		wizard.AddSymbol("GUID_ASSEMBLY", wizard.FormatGuid(strRawGuid, 0));

		strTemplateFile = strTemplatePath + "\\AssemblyInfo.vb"; 
		item = AddFileToVSProject("AssemblyInfo.vb", project, project.ProjectItems, strTemplateFile, false);
		if (item) item.Properties("SubType").Value = "Code";

		strTemplateFile = strTemplatePath + "\\RemoteMonitor.vb"; 
		item = AddFileToVSProject("RemoteMonitor.vb", project, project.ProjectItems, strTemplateFile, false);
		if (item)
		{
			item.Properties("SubType").Value = "Form";
			project.Properties("StartupObject").Value = project.Properties("RootNamespace").Value + ".RemoteMonitor";
			
			// Add resource file for form
			AddResourceFileToVSProject(item.ProjectItems, "RemoteMonitor.resx", strTemplatePath + "\\RemoteMonitor.resx");
		}

		project.Save();
        }
                        
        return 0;
    }
    catch(e)
    {   
        switch(e.number)
        {
        case -2147024816 /* FILE_ALREADY_EXISTS */ :
            return -2147213313;

        default:
            SetErrorInfo(e);
            return e.number;
        }
    }
    finally
    {
        dte.SuppressUI = oldSuppressUIValue;
    }
}

function JustFileName(filePath)
{
	while(filePath.indexOf("\\") > -1 || filePath.indexOf("/") > -1 || filePath.indexOf(":") > -1)
		filePath = filePath.substr(1);
		
	return filePath;
}

function JustPath(filePath)
{
	if (filePath.length)
		return filePath.substring(0, filePath.length - JustFileName(filePath).length - 1);
	else
		return "";
}

function GetReferenceManager(oProj)
{
	var VSProject = oProj.Object;
	var refManager = VSProject.References;
	return refManager;
}

function AddProjectReference(oProj, sRef)
{
    try
    {
        var refManager = GetReferenceManager(oProj);
        refManager.Add(sRef);
    }
    catch(e)
    {
        wizard.ReportError("Error adding reference to new TVA service: " + e.description);
    }
}

function AddResourceFileToVSProject(selObj, strFileName, strTemplateFile)
{
	var item = AddFileToVSProject(strFileName, selObj.parent, selObj.parent.ProjectItems, strTemplateFile, false);
	
	if (item) item.Properties.Item("BuildAction").Value = 3;	// prjBuildActionEmbeddedResource
		
	return item;
}

function AddContentFileToVSProject(selObj, strFile)
{
	var item = selObj.parent.ProjectItems.AddFromFile(strFile);
	
	if (item) item.Properties.Item("BuildAction").Value = 2;	// prjBuildActionContent
		
	return item;
}

function AddMiscFileToVSProject(selObj, strFile)
{
	var item = selObj.parent.ProjectItems.AddFromFile(strFile);
	
	if (item) item.Properties.Item("BuildAction").Value = 0;  // prjBuildActionNone
		
	return item;
}

function ValidIdentifier(strName)
{
    var nLen = strName.length;
    var strLegalName = "";
    var cChar = strName.charAt(0);
    
    switch(cChar)
    {
        case "0":
        case "1":
        case "2":
        case "3":
        case "4":
        case "5":
        case "6":
        case "7":
        case "8":
        case "9":
            strLegalName += "_";
            break;
    }
    
    for (nCntr = 0; nCntr < nLen; nCntr++)
    {
        cChar = strName.charAt(nCntr);
        switch(cChar)
        {
            case " ":
            case "~":
            case "&":
            case "'":
            case "#":
            case "!":
            case "@":
            case "$":
            case "%":
            case "^":
            case "(":
            case ")":
            case "-":
            case "+":
            case "=":
            case "{":
            case "}":
            case "[":
            case "]":
            case ";":
            case ",":
            case "`":
            case ".":
                // We remove illegal characters
                break;
            default:
                strLegalName += cChar;
                break;
        }
    }
    return strLegalName;
}
