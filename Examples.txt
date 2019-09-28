<RecordSelectionFormula> - search inner text


<Tables>
	<Table
		<Fields>
			<Table FormulaForm="{PATIENT.PAT_NAME}">


<RecordSelectionFormula> (   (   (  {CLARITY_TDL.DETAIL_TYPE} in [60, 61]  AND  {CLARITY_TDL.POST_DATE} = {?Post Date}  )   AND  {CLARITY_TDL.DEBIT_GL_NUM} = "14241140"  )   AND  {CLARITY_TDL.SERV_AREA_ID} = 2  )  </RecordSelectionFormula>

<Command>SELECT 
DISTINCT d.PROV_ID, a.PROC_ID
FROM Clarity..CLARITY_TDL_AGE a 
INNER JOIN Clarity..CLARITY_EAP c on a.PROC_ID = c.PROC_ID
INNER JOIN Clarity..CLARITY_SER d on a.BILLING_PROVIDER_ID = d.PROV_ID

WHERE 
a.DEBIT_GL_NUM = '14241140' AND
a.POST_DATE = {?Post Date} AND
c.RPT_GRP_SIX = 1 AND
d.RPT_GRP_FIVE=1
</Command>







