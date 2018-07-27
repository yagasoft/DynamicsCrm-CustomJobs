/// <reference path="Sdk.Soap.vsdoc.js" />
/// <reference path="jquery-1.11.1.js" />
/// <reference path="Sdk.Soap.vsdoc.js" />
/// <reference path="Xrm.Page.js" />
/// <reference path="../../../../LinkDev.Libraries/LinkDev.Libraries.Common/Scripts/ldv_CommonGeneric.js" />
/// <reference path="../../../../LinkDev.Libraries/LinkDev.Libraries.Common/CrmSchemaJs.js" />

var $ = window.$ || parent.$;

function StopEngine()
{
	SetFieldSubmitMode(Sdk.CustomJobEngine.StatusReason, SubmitMode.Always);
    SetFieldValue(Sdk.CustomJobEngine.StatusReason, Sdk.CustomJobEngine.StatusReasonEnum.Stopped, true);
	SaveForm();
}

function RunEngine()
{
	SetFieldSubmitMode(Sdk.CustomJobEngine.StatusReason, SubmitMode.Always);
    SetFieldValue(Sdk.CustomJobEngine.HourlyTriggered, false, true);
    SetFieldValue(Sdk.CustomJobEngine.StatusReason, Sdk.CustomJobEngine.StatusReasonEnum.Running, true);
	SaveForm();
}

function IsStopEnabled()
{
	return Xrm.Page.getAttribute(Sdk.CustomJobEngine.StatusReason).getValue() === 1;
}

function IsRunEnabled()
{
	return Xrm.Page.getAttribute(Sdk.CustomJobEngine.StatusReason).getValue() === 753240000;
}
