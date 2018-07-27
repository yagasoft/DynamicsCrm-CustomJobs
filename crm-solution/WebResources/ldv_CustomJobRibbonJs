/// <reference path="Sdk.Soap.vsdoc.js" />
/// <reference path="jquery-1.11.1.js" />
/// <reference path="Sdk.Soap.vsdoc.js" />
/// <reference path="Xrm.Page.js" />
/// <reference path="../../../../LinkDev.Libraries/LinkDev.Libraries.Common/Scripts/ldv_CommonGeneric.js" />
/// <reference path="../../../../LinkDev.Libraries/LinkDev.Libraries.Common/CrmSchemaJs.js" />

var $ = window.$ || parent.$;

function SetToDraft()
{
    SetFieldValue(Sdk.CustomJob.StatusReason, Sdk.CustomJob.StatusReasonEnum.Draft, true);
	SaveForm();
}

function EnqueueJob()
{
	SetFieldValue(Sdk.CustomJob.RecurrenceUpdatedTrigger, new Date().toString(), true);
    SetFieldValue(Sdk.CustomJob.StatusReason, Sdk.CustomJob.StatusReasonEnum.Waiting, true);
	SaveForm();
}

function CancelJob()
{
	if (window.confirm('Are you sure you want to cancel this job?'))
	{
		ShowBusyIndicator('Cancelling job ...', 'jobCancel');

		$.ajax({
				type: "PATCH",
				contentType: "application/json; charset=utf-8",
				datatype: "json",
				url: Xrm.Page.context.getClientUrl() + "/api/data/v8.0/ldv_customjobs(" + GetRecordId(true) + ")",
				data: JSON.stringify({ statecode: 1, statuscode: 753240008 }),
				beforeSend: function(xmlHttpRequest)
				{
					xmlHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
					xmlHttpRequest.setRequestHeader("OData-Version", "4.0");
					xmlHttpRequest.setRequestHeader("Accept", "application/json");
				},
				async: true,
				success: function(data, textStatus, xhr)
				{
					HideBusyIndicator('jobCancel');
					RefreshOnNotDirty();
				},
				error: function(xhr, textStatus, errorThrown)
				{
					console.error(xhr);
					console.error(textStatus + ": " + errorThrown);
					alert(textStatus + ": " + errorThrown);
					HideBusyIndicator('jobCancel');
				}
			});
	}
}

function IsSetToDraftEnabled()
{
	return GetAllowedStatuses().indexOf('753240006') >= 0 && GetFieldValue('statecode') === 0
		&& Xrm.Page.ui.getFormType() !== 1;
}

function IsEnqueueJobEnabled()
{
	return GetAllowedStatuses().indexOf('753240005') >= 0 && GetFieldValue('statecode') === 0
		&& Xrm.Page.ui.getFormType() !== 1;
}

function IsCancelJobEnabled()
{
	return GetFieldValue('statecode') === 0 && Xrm.Page.ui.getFormType() !== 1;
}

function GetAllowedStatuses()
{
	var options = $.map(Xrm.Page.getAttribute('statuscode').getOptions(),
		function(e)
		{
			return e.value + '';
		});

	var selectedIndex = options
		.indexOf(Xrm.Page.getAttribute('statuscode').getSelectedOption().value + '');

	if (selectedIndex >= 0)
	{
		options.splice(selectedIndex, 1);
	}

	return options;
}

function ClearLogs()
{
	ShowBusyIndicator("Clearing logs ...", 'logClearance');
	
	$.ajax({
			type: "POST",
			contentType: "application/json; charset=utf-8",
			datatype: "json",
			url: Xrm.Page.context.getClientUrl() + "/api/data/v8.1/" +
				"ldv_customjobs(" + GetRecordId(true) + ")" +
				"/Microsoft.Dynamics.CRM.ldv_CustomJobClearLogs",
			beforeSend: function(xmlHttpRequest)
			{
				xmlHttpRequest.setRequestHeader("OData-MaxVersion", "4.0");
				xmlHttpRequest.setRequestHeader("OData-Version", "4.0");
				xmlHttpRequest.setRequestHeader("Accept", "application/json");
			},
			async: true,
			success: function(data, textStatus, xhr)
			{
				HideBusyIndicator('logClearance');
				RefreshOnNotDirty();
			},
			error: function(xhr, textStatus, errorThrown)
			{
				console.error(xhr);
				alert(textStatus + ": " + errorThrown + ": " + xhr.responseJSON.error.message);
				HideBusyIndicator('logClearance');
			}
		});
}
