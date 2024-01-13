/// <reference path="Sdk.Soap.vsdoc.js" />
/// <reference path="jquery-1.11.1.js" />
/// <reference path="Sdk.Soap.vsdoc.js" />
/// <reference path="Xrm.Page.js" />
/// <reference path="../../../../Yagasoft.Libraries/Yagasoft.Libraries.Common/Scripts/ys_CommonGeneric.js" />
/// <reference path="../../../../Yagasoft.Libraries/Yagasoft.Libraries.Common/CrmSchemaJs.js" />

var $ = window.$ || parent.$;

function RunEngine(formContext)
{
    BuildAnchoredExecutionContext(formContext);

    SetFieldSubmitMode(Sdk.CustomJobEngine.StatusReason, SubmitMode.Always);
    SetFieldValue(Sdk.CustomJobEngine.HourlyTriggered, false, true);
    SetFieldValue(Sdk.CustomJobEngine.StatusReason, Sdk.CustomJobEngine.StatusReasonEnum.Running, true);
    SaveForm();
}

function StopEngine(formContext)
{
    BuildAnchoredExecutionContext(formContext);

	SetFieldSubmitMode(Sdk.CustomJobEngine.StatusReason, SubmitMode.Always);
    SetFieldValue(Sdk.CustomJobEngine.StatusReason, Sdk.CustomJobEngine.StatusReasonEnum.Stopped, true);
	SaveForm();
}

function IsRunEnabled(formContext)
{
    return formContext.getAttribute(Sdk.CustomJobEngine.StatusReason).getValue() === 753240000;
}

function IsStopEnabled(formContext)
{
	return formContext.getAttribute(Sdk.CustomJobEngine.StatusReason).getValue() === 1;
}
