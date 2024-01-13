/// <reference path="Sdk.Soap.vsdoc.js" />
/// <reference path="jquery-1.11.1.js" />
/// <reference path="Sdk.Soap.vsdoc.js" />
/// <reference path="ys_CommonGeneric.js" />

var $ = window.$ || parent.$;

function SetToDraft(formContext)
{
    BuildAnchoredExecutionContext(formContext);

    SetFieldValue(Sdk.CustomJob.StatusReason, Sdk.CustomJob.StatusReasonEnum.Draft, true);
	SaveForm();
}

function EnqueueJob(formContext)
{
    BuildAnchoredExecutionContext(formContext);

    SetFieldValue(Sdk.CustomJob.StatusReason, Sdk.CustomJob.StatusReasonEnum.Waiting, true);
	SaveForm();
}

function CancelJob(formContext)
{
    BuildAnchoredExecutionContext(formContext);

	if (window.confirm('Are you sure you want to cancel this job?'))
	{
		ShowBusyIndicator('Cancelling job ...', 'jobCancel');

		$.ajax({
				type: "PATCH",
				contentType: "application/json; charset=utf-8",
				datatype: "json",
				url: Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v8.0/ys_customjobs(" + GetRecordId(true) + ")",
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

function IsSetToDraftEnabled(formContext)
{
    BuildAnchoredExecutionContext(formContext);

	return GetAllowedStatuses().indexOf('753240006') >= 0 && GetFieldValue('statecode') === 0
		&& formContext.ui.getFormType() !== 1;
}

function IsEnqueueJobEnabled(formContext)
{
    BuildAnchoredExecutionContext(formContext);

	return GetAllowedStatuses().indexOf('753240005') >= 0 && GetFieldValue('statecode') === 0
		&& formContext.ui.getFormType() !== 1;
}

function IsCancelJobEnabled(formContext)
{
    BuildAnchoredExecutionContext(formContext);

	return GetFieldValue('statecode') === 0 && formContext.ui.getFormType() !== 1;
}

function GetAllowedStatuses()
{
	var options = $.map(AnchoredExecutionContext.getFormContext().getAttribute('statuscode').getOptions(),
		function(e)
		{
			return e.value + '';
		});

	var selectedIndex = options
		.indexOf(AnchoredExecutionContext.getFormContext().getAttribute('statuscode').getSelectedOption().value + '');

	if (selectedIndex >= 0)
	{
		options.splice(selectedIndex, 1);
	}

	return options;
}

function ClearLogs(formContext)
{
    BuildAnchoredExecutionContext(formContext);

	ShowBusyIndicator("Clearing logs ...", 'logClearance');
	
	$.ajax({
			type: "POST",
			contentType: "application/json; charset=utf-8",
			datatype: "json",
			url: Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v8.1/" +
				"ys_customjobs(" + GetRecordId(true) + ")" +
				"/Microsoft.Dynamics.CRM.ys_CustomJobClearLogs",
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
