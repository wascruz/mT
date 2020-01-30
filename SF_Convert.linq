<Query Kind="Program">
  <Output>DataGrids</Output>
  <NuGetReference>Snowflake.Data</NuGetReference>
  <Namespace>Snowflake.Data.Client</Namespace>
</Query>

void Main()
{
	string[] SelectCmds = { "--WITH USERTABLE(USER_ID, LOGIN_ID, FIRST_NAME, MIDDLE_NAME, LAST_NAME, LOCATION_ID, INSTITUTION_ID) AS(SELECT USER_ID, LOGIN_ID, FIRST_NAME, MIDDLE_NAME, LAST_NAME, LOCATION_ID, INSTITUTION_ID FROM CPCE.USER), CASEALL AS(SELECT DISTINCT(C.CASE_KEY) AS 'DISTINCT_CASE_KEY', C.* FROM CPCE.CASE C LEFT OUTER JOIN CPCE.CASE_APPOINTMENT AS CA ON C.CASE_KEY = CA.CASE_KEY WHERE(C.BUSINESS_GROUP_ID IN(@BUSINESS_GROUP_ID0)) FETCH FIRST 20000 ROWS ONLY), SARSTRFILEDDATE_NULLCHECK(CASE_KEY, SAREFILEDDATE, SUSPICIOUS_ACTIVITY_IDENTIFIED_DATE, ROWNUMBER) AS(SELECT C.CASE_KEY, (CASE WHEN CD.SAR_STR_FILED_DATE IS NULL THEN CASE WHEN CR.EFILED_DATE IS NULL THEN CR.CREATE_TIMESTAMP ELSE CR.EFILED_DATE END ELSE CD.SAR_STR_FILED_DATE END) AS SAREFILEDDATE, CD.SUSPICIOUS_ACTIVITY_IDENTIFIED_DATE, ROW_NUMBER() OVER(PARTITION BY C.CASE_KEY ORDER BY(CASE WHEN CD.SAR_STR_FILED_DATE IS NULL THEN CASE WHEN CR.EFILED_DATE IS NULL THEN CR.CREATE_TIMESTAMP ELSE CR.EFILED_DATE END ELSE CD.SAR_STR_FILED_DATE END)) AS ROWNUMBER  FROM CASEALL AS C LEFT OUTER JOIN CPCE.CASE_DETAILS AS CD ON C.CASE_KEY = CD.CASE_KEY LEFT OUTER JOIN CPCE.CASE_REPORT AS CR ON CR.CASE_KEY = CD.CASE_KEY ) ,  SAREFILEDDATE(CASE_KEY, REPORTCLOCK, SAREFILEDDATE) AS(SELECT CASE_KEY, CASE WHEN SAREFILEDDATE IS NULL THEN(DAYS(CURRENT_TIMESTAMP) - (CASE WHEN SUSPICIOUS_ACTIVITY_IDENTIFIED_DATE IS NULL THEN 0 ELSE DAYS(SUSPICIOUS_ACTIVITY_IDENTIFIED_DATE) END)) + 1  ELSE(DAYS(SAREFILEDDATE) - (CASE WHEN SUSPICIOUS_ACTIVITY_IDENTIFIED_DATE IS NULL THEN 0 ELSE DAYS(SUSPICIOUS_ACTIVITY_IDENTIFIED_DATE) END)) +1 END AS REPORTCLOCK, SAREFILEDDATE FROM SARSTRFILEDDATE_NULLCHECK WHERE ROWNUMBER = 1) , PDSPRODUCT(CASEKEY, PRODUCT) AS(SELECT CPI.CASE_KEY, LISTAGG(PIM.PRODUCT_NAME, ',') WITHIN GROUP(ORDER BY CPI.CASE_KEY ASC) AS PRODUCTS FROM CPCE.CASE_PRODUCT_IMPACTED CPI LEFT JOIN CASEALL C ON C.CASE_KEY = CPI.CASE_KEY LEFT JOIN CPCE.PRODUCT_IMPACTED_MASTER PIM ON CPI.PRODUCT_IMPACTED_KEY = PIM.PRODUCT_IMPACTED_KEY GROUP BY CPI.CASE_KEY) SELECT DISTINCT C.CASE_KEY as 'Case Key',LTRIM(RTRIM(C.CASE_ID)) as 'Case ID', BG.BUSINESS_GROUP_NAME as 'BusinessGroup',CASE WHEN ST.SUBJECT_TYPE_NAME = 'PCP' THEN(SELECT SUBJECT_KEY FROM CPCE.CASE_SUBJECT WHERE CASE_KEY = C.CASE_KEY FETCH FIRST 1 ROWS ONLY) WHEN S.SUBJECT_ATTRIBUTE IS NOT NULL THEN(SELECT SUBJECT_KEY FROM CPCE.CASE_SUBJECT WHERE CASE_KEY = C.CASE_KEY FETCH FIRST 1 ROWS ONLY) WHEN S.SUBJECT_KEY IS NULL THEN 0 ELSE S.SUBJECT_KEY END as 'Subject Key', CASE WHEN ST.SUBJECT_TYPE_NAME = 'PCP' THEN S.SUBJECT_ATTRIBUTE WHEN ST.IS_HASH_TYPE = 'Y' THEN CPCE.DECRYPT(S.SUBJECT_ATTRIBUTE)WHEN S.SUBJECT_ATTRIBUTE IS NOT NULL THEN S.SUBJECT_ATTRIBUTE ELSE S.SUBJECT_ID END as 'Subject ID', CASE WHEN ST.SUBJECT_TYPE_NAME = 'PCP' THEN(SELECT S1.SUBJECT_NAME FROM CPCE.SUBJECT S1 LEFT OUTER JOIN CPCE.CASE_SUBJECT CS1 ON S1.SUBJECT_KEY = CS1.SUBJECT_KEY  WHERE CS1.CASE_KEY = C.CASE_KEY  FETCH FIRST 1 ROWS ONLY) ELSE S.SUBJECT_NAME END  as 'Subject Name', CASE WHEN  C.TXN_US_AMOUNT = 0.0 THEN '' ELSE VARCHAR(VARCHAR_FORMAT((C.TXN_US_AMOUNT),'999,999,999,990.99'),30) End as 'Txn US Amount',C.TXN_COUNT as 'Txn Count',C.INV_GRP_ID as 'INV GRPId',INV.INV_GRP_NAME as  'Investigative Group',CDD.FRAUD_FILE_NUMBER as 'Fraud File#',CDD.NEW_MTCN as 'New MTCN',CDD.MTCN as 'Triggering MTCN',((CASE WHEN CDE.BRAND_NAME = 'VG' THEN 'VIGO' ELSE CDE.BRAND_NAME END) || '-' || CDE.PRODUCT) as 'Transaction Brand',CASE WHEN CL.CASE_KEY IS NOT NULL AND BG.BUSINESS_GROUP_NAME = 'GFCC' THEN 'Related Case' WHEN CL.CASE_KEY IS NULL AND BG.BUSINESS_GROUP_NAME = 'GFCC' THEN 'Anchor Case' ELSE NULL END as 'Case Relationship',S.SUBJECT_TYPE_ID as 'Subject Type', S.SUBJECT_CATEGORY_DESCRIPTION as 'Subject Description', STS.STATUS_DESCRIPTION as 'Status', CL.QA_REVIEW_LEVEL as 'Case QA Level Description',SSTS.SUB_STATUS_DESCRIPTION as 'Sub Status', CD.SUB_STATUS_TIMESTAMP as 'Sub Status Date', SREF.REFERRAL_SOURCE_DESCRIPTION as 'Source of Referral', ISRC.INTR_SOURCE_DESCRIPTION as 'Interdiction Source', CASE WHEN C.BUSINESS_GROUP_ID = 'GFO' AND(C.CASE_STATUS_ID = 9 OR C.CASE_STATUS_ID = 10) THEN '' WHEN C.BUSINESS_GROUP_ID = 'GDD' THEN QRUSER.FIRST_NAME ELSE AUSER.FIRST_NAME END as 'Analyst First Name', CASE WHEN C.BUSINESS_GROUP_ID = 'GFO' AND(C.CASE_STATUS_ID = 9 OR C.CASE_STATUS_ID = 10) THEN '' WHEN C.BUSINESS_GROUP_ID = 'GDD' THEN QRUSER.LAST_NAME ELSE AUSER.LAST_NAME END as 'Analyst Last Name', CTRY.ISO_COUNTRY_CODE_2 as 'Country', CD.STATE_CODE as 'State', CD.PROCESSING_STAGE as 'Processing Stage', CD.BO_AVAILABLE_DATE as 'BO Available Date',(select First_Name || ' ' || Last_Name from cpce.User where User_Id = CD.Foco_User_Id) as 'FOCO User Name',C.CASE_STATUS_ID as 'Case Status', (Select IS_TIBCO_SUCCESS from CPCE.TIBCO_REQUEST Where Case_Key = C.CASE_KEY ORDER BY STATUS_TIMESTAMP DESC FETCH FIRST 1 ROW ONLY) as 'Txn Decision', (Select IS_CALL_BACK_QUEUE_HIT from CPCE.TIBCO_REQUEST Where Case_Key = C.CASE_KEY ORDER BY STATUS_TIMESTAMP DESC FETCH FIRST 1 ROW ONLY) as 'Is Call Back Hit', CD.CASE_ACTION as 'Case Action', CASE WHEN C.BUSINESS_GROUP_ID = 'KYC' THEN CaseMetric.CASE_ATTR_ID15_VAL ELSE CaseMetric.CASE_ATTR_ID1_VAL END as 'FO/L1 Age(DD:HH:MM)',CASE WHEN C.BUSINESS_GROUP_ID = 'KYC' THEN CaseMetric.CASE_ATTR_ID16_VAL ELSE CaseMetric.CASE_ATTR_ID2_VAL END as 'BO/L2 Age(DD:HH:MM)',CASE WHEN C.BUSINESS_GROUP_ID = 'KYC' THEN CaseMetric.CASE_ATTR_ID17_VAL ELSE CaseMetric.CASE_ATTR_ID3_VAL END as 'ES/L3 Age(DD:HH:MM)',C.CLASSIFICATION as 'Case Classification', S.NETWORK_ID as 'Network ID', S.NETWORK_NAME as 'Network Name', C.CREATE_TIMESTAMP as 'Start Date', C.CREATE_USER_ID as 'Create User ID',C.MODIFIED_USER_ID as 'Modified User Id',C.MODIFIED_TIMESTAMP as 'Modified Timestamp',C.ETDD as 'ETDD',C.BUSINESS_GROUP_ID as 'Business Group Id',C.CASE_TYPE as 'Case Type',C.ALERT_KEY as 'Alert Key',C.ALERT_ROLLUP_STOPPED as 'Alert Rollup Stopped',C.BATCH_CYCLE_ID as 'Batch Cycle Id',C.QUEUE_ID as 'Queue Id',Qu.Queue_Name as 'Queue Name',C.RPT_GRP_ID as 'RPT GRPId',C.SIDE as 'Side',C.CASE_WEIGHT as 'Case Weight',C.LAST_WORKED_SESSION_ID as 'Last Worked Session Id',C.LAST_WORKED_USER_ID as 'Last Worked User Id',C.ASSIGNED_BY_USER_ID as 'Assigned by User Id',C.ASSIGNED_TO_USER_ID as 'Assigned to User Id',C.CASE_STATUS_TIMESTAMP as 'Case Status Timestamp',C.PREVIOUS_CASE_STATUS_ID as 'Previous Case Status Id',C.QA_INDICATOR_ID as 'QA Indicator Id',C.QA_INDICATOR_TIMESTAMP as 'QA Indicator Timestamp',C.PREVIOUS_QA_INDICATOR_ID as 'Previous QA Indicator Id',C.QA_USER_ID as 'QA User Id',(QUSER.FIRST_NAME || '  ' || QUSER.LAST_NAME) as 'QA Analyst Name',(CASE  WHEN QR.IS_CASE_SELECTED_FOR_QA = 'Y' THEN 'Yes' ELSE 'No' END) as 'QA Selection Flag', SSTS1.SUB_STATUS_DESCRIPTION as 'QA Sub Status', QA_STAT.NAME as 'QA Status', (CUSER.FIRST_NAME || '  ' || CUSER.LAST_NAME) as 'Created By',C.SUBMIT_TO_SAME_QA as 'Submit to Same QA',C.OLDEST_TXN_SEND_DATE as 'Oldest Txn Send Date',CASE WHEN C.BUSINESS_GROUP_ID = 'KYC' AND(INV.INV_GRP_NAME LIKE 'KYC CDD%' OR INV.INV_GRP_NAME LIKE 'KYC ECDD%' OR INV.INV_GRP_NAME LIKE 'KYC IDV%') THEN(C.DAYS_PENDED / 24) || ' Days ' || MOD(C.DAYS_PENDED, 24) || ' hrs '  ELSE VARCHAR(C.DAYS_PENDED) END as 'Days Pended',C.PENDED_TIMESTAMP as 'Pended Timestamp',C.DUE_DATE as 'Due Date',C.PRESENTATION_THRESHHOLD as 'PendedtoSystemPresentationDate', (CASE WHEN C.BUSINESS_GROUP_ID = 'R&R' AND C.CASE_TYPE = 'S' AND C.FREEZE_TIMESTAMP IS NULL THEN C.CREATE_TIMESTAMP + INV.PRESENTATION_THRESHHOLD DAYS ELSE C.PRESENTATION_ELIGIBLE END) as 'PE Date',CD.CASE_QA_LEVEL_ID as 'Case QA Level Id',I.INSTITUTION_NAME as 'Institution Name',(SELECT CA.APPOINMENT_STATUS FROM CPCE.CASE_APPOINTMENT CA WHERE CA.CASE_KEY = C.CASE_KEY ORDER BY CA.MODIFIED_TIMESTAMP desc FETCH FIRST 1 ROWS ONLY) as 'Appointment Status' ,(CASE When BG.BUSINESS_GROUP_ID = 'FIU' OR BG.BUSINESS_GROUP_ID = 'CS' OR BG.BUSINESS_GROUP_ID = 'FRM' THEN CD.Sar_Report_Count ELSE  C.NUMBER_OF_REPORTS END) as 'Number of Reports',CMA.IWATCH_DESCRIPTION as 'Agent Business',CMO.IWATCH_DESCRIPTION as 'Ownership Type',agtloc.cot_code as 'COT Code', agtloc.RISK_GRADE as 'COT Risk Level',A.ZIPCODE_POSTALCODE as 'Zip Code' ,C.CASE_HAS_BEEN_COMPLETE as 'Case has been delete',C.REPORT_DELETED_ON_COMPLETE as 'Report Deleted On Complete',C.REFERRED_ORIGINAL_RPT_GRP_ID as 'Referred Original RPT GRP Id',C.REFERRED_ORIGINAL_CASE_KEY as 'Referred Original Case Key',C.REFERRED_ORIGINAL_SIDE as 'Referred Original Side',C.REFERRED_BY_USER_ID as 'Referred By User ID',C.REFERRED_COUNTRY_CODE as 'Referred Country Code',CD.CLASSIFICATION as 'CLASSIFICATION', CD.SUB_STATUS_ID as 'Sub Status ID', CD.REFERRAL_SOURCE_ID as 'Referral Source ID', CD.REFERRAL_INFORMATION as 'Referral Information', RT.RISK_TYPE_DESCRIPTION as 'Risk Type', (CASE WHEN BG.BUSINESS_GROUP_NAME = 'FRM' THEN '' ELSE CD.RISK_LEVEL_CODE END) AS 'Risk Level', CD.REGION_ID as 'Region ID', CD.COUNTRY_KEY as 'Country Key', CD.LAW_ENFORCEMENT_AGENCY_ID as 'Law Enforcement ID', CD.REPORT_TRANSACTIONS as 'Report Transactions', CD.COUNTRY_KEY as 'Country Key', CD.LAW_ENFORCEMENT_AGENCY_ID as 'Law Enforcement ID', CD.REPORT_TRANSACTIONS as 'Report Transactions', CD.REPORT_AMOUNT as 'Report Amount', SN.REPORTCLOCK as 'ReportClock',CD.SUSPICIOUS_ACTIVITY_IDENTIFIED_DATE as 'SUSPICIOUS ACTIVITY IDENTIFIED DATE',CD.QAA as 'Questionable Agent Activity', CD.QCA as 'Questionable Consumer Activity', CD.FRAUD as 'Fraud', CD.AGENT_COMPLICITY as 'Agent Complicity', CD.SURFING as 'Surfing', CD.FLIPPING as 'Flipping', CD.AGENT_SUSPENDED as 'Agent Suspended', CD.FRAUD_COUNT as 'Fraud Count', CD.FRAUD_AMOUNT as 'Fraud Amount', CD.FRAUD_COUNT_CONFIRMED as 'Confirmed Fraud Count', CD.FRAUD_AMOUNT_CONFIRMED as 'Confirmed Fraud Amount', CD.FRAUD_COUNT_POTENTIAL as 'Potential Fraud Count', CD.FRAUD_AMOUNT_POTENTIAL as 'Potential Fraud Amount', CD.CASE_DISPOSITION as 'Case Disposition', CD.PROCESSING_STAGE as 'Case Level', (CASE WHEN BG.BUSINESS_GROUP_NAME = 'FRM' THEN CASE CD.RISK_LEVEL_CODE WHEN 'L - 1' THEN '' WHEN 'L - 2' THEN '' WHEN 'L - 3' THEN '' WHEN 'L - 4' THEN '' ELSE RL.RISK_LEVEL_DESCRIPTION END  WHEN BG.BUSINESS_GROUP_NAME = 'GMI' THEN '' ELSE RL.RISK_LEVEL_DESCRIPTION END) AS 'Recommendation', RL2.RISK_LEVEL_DESCRIPTION as 'Final Resolution', CD.RESOLUTION_OTHER as 'Other Actions', CD.RECOMMENDATION_DATE as 'Recommendation Date', CD.RESOLUTION_FINAL_DATE as 'Final Resolution Date', CD.RESOLUTION_OTHER_DATE as 'Other Actions Date', CTOB.ISO_COUNTRY_CODE_2 as 'Outbound Corridor', CTIB.ISO_COUNTRY_CODE_2 as 'Inbound Corridor', CSPR.DESCRIPTION as 'Case Priority', CPFR.DESCRIPTION as 'Final Risk Level', CPFR.DESCRIPTION as 'Initial Risk Level', CD.DATA_INTEGRITY as 'Data Integrity', CD.QCA_PERCENT_OF_TOTAL as 'Questionable Consumer Activity-Percent of Total', CD.QAA_PERCENT_OF_TOTAL as 'Questionable Agent Activity-Percent of Total', CD.FRAUD_PERCENT_OF_TOTAL as 'Fraud Percent', CD.DATAINTEGRITY_PERCENT_OF_TOTAL as 'Data Integrity Percent', CASE WHEN CD.SAR_STR_FILED_DATE IS NULL THEN CASE WHEN CRRREPORT.EFILED_DATE IS NULL THEN CRXREPORT.CREATE_TIMESTAMP ELSE CRRREPORT.EFILED_DATE END ELSE CD.SAR_STR_FILED_DATE END as 'Report Filed Date',CD.INTERDICTION_COUNT as 'Interdiction Count', CD.C314B as '314B', CD.CONSUMER_INTERVIEW as 'Consumer Interview', (CASE WHEN CD.SWB = 'Y' THEN 'Y' ELSE 'N' END) as 'SWB', CD.ESCALATE_FOR_FURTHER_INVESTIGATION as 'Escalated To T2', CD.RESTRICTED_CASE as 'RestrictedCase', (CASE C.CASE_STATUS_ID WHEN 9 THEN C.CASE_STATUS_TIMESTAMP WHEN 10 THEN C.CASE_STATUS_TIMESTAMP END) as 'End Date',C.QA_PERCENT_SELECTED as 'QA Percent Selected',STSQA.STATUS_DESCRIPTION as 'QA Indicator Status',C.QA_RANDOM_VALUE as 'QA Random Value', (Case when(select count(CPCE.CASE_AGENT_TERMINATION.CASE_KEY) from CPCE.CASE_AGENT_TERMINATION where CPCE.CASE_AGENT_TERMINATION.CASE_KEY = C.CASE_KEY and CPCE.CASE_AGENT_TERMINATION.IS_AGENT_TERMINATED = 'Y') > 0 THEN 'Yes' ELSE 'No' END) as 'Agent Termination', (CASE  WHEN S.DIRECT_DEPOSIT = 'Y' THEN 'Yes' ELSE 'No' END) as 'Direct Deposit', CD.REFERRAL_REASON_CODE as 'Reason Code', CD.REFERRAL_INFORMATION as 'Referral Info/ID', CD.REFERRAL_SOURCE_ID as 'Source of Referral', (CASE  WHEN CD.REFERRED_TO_LAW_ENFORCEMENT = 'Y' THEN 'Yes' ELSE 'No' END) as 'Referred to Law Enforcement', C.CASE_STATUS_TIMESTAMP as 'Status Date', CD.INTERDICTION_GROUP_ID as 'Group Code', CD.INTERDICTION_STATUS_CODE as 'Interdiction Status Code', LEA.LAW_ENFORCEMENT_AGENCY_NAME as 'Law Enforcement Agency', CSPR.DESCRIPTION as 'Initial Risk Level', C.PRIORITY_SCORE as 'Priority Score', C.RISK_SCORE as 'Risk Score', C.AGE_WEIGHT as 'Age Weight', CD.SAR_REPORT_COUNT as 'SAR Report Count', CD.SAR_REPORT_COMMENTS as 'SAR Report Comments', CD.REPORT_TRANSACTIONS as 'Report Transactions', CD.REPORT_AMOUNT as 'Report Amount', CASE WHEN CD.SIGNIFICANT_REPORT = 'Y' THEN 'Yes' ELSE 'No' END as 'Significant Report', CD.DIGITAL as 'Digital', CD.LICENSE as 'License', CD.APPROVAL_LEVEL as 'Approval Level', CD.RISK_LEVEL_CODE as 'Risk Assessment', CD.GATE1_APPROVED_DATE as 'Gate1 Approved Date', CD.GATE2_APPROVED_DATE as 'Gate2 Approved Date', CD.GATE3_APPROVED_DATE as 'Gate3 Approved Date', CD.GATE4_APPROVED_DATE as 'Gate4 Approved Date', CD.GATE5_APPROVED_DATE as 'Gate5 Approved Date', PD.PRODUCT as 'Products Impacted', S.KEEP_OPEN_REQUEST as 'Keep Open Request', S.KEEP_OPEN_START_DATE as 'Keep Open Start Date', S.KEEP_OPEN_END_DATE as 'Keep Open End Date', CD.AGENT_REFERRED_DATE as 'Referral Date' ,  CASE WHEN INV.INV_GRP_NAME = 'KYC US' THEN CAT.DUE_DATE WHEN SLD.NEW_DUE_DATE IS NOT NULL THEN SLD.NEW_DUE_DATE ELSE CAT.DUE_DATE END AS 'SLA Due Date',  CASE WHEN INV.INV_GRP_NAME = 'KYC US' THEN(DAYS(CAT.DUE_DATE) - DAYS(CURRENT_TIMESTAMP))  WHEN CAT.DUE_DATE IS NULL THEN NULL WHEN((C.CASE_STATUS_ID IN (9,10)) AND(DAYS(CAT.DUE_DATE) - DAYS(C.CASE_STATUS_TIMESTAMP) >= 0)) THEN NULL  WHEN SLD.NEW_DUE_DATE IS NOT NULL THEN(DAYS(SLD.NEW_DUE_DATE) - DAYS(CURRENT_TIMESTAMP))  ELSE(DAYS(CAT.DUE_DATE) - DAYS(CURRENT_TIMESTAMP)) END AS 'SLA Countdown' , CASE WHEN CS.CDD_STATUS IS NOT NULL AND RTRIM(CS.CDD_STATUS) = 'Y' THEN 'CDD PASSED' WHEN CS.CDD_STATUS = 'N' THEN 'CDD FAILED' WHEN CS.CDD_STATUS = 'P' THEN 'CDD PENDING' WHEN CS.CDD_STATUS = 'B' THEN 'NOT SET' ELSE CS.CDD_STATUS END as 'CDD', CASE WHEN CS.ECDD_STATUS IS NOT NULL AND TRIM(CS.ECDD_STATUS) = 'Y' THEN 'ECDD PASSED' WHEN CS.ECDD_STATUS = 'N' THEN 'ECDD FAILED' WHEN CS.ECDD_STATUS = 'P' THEN 'ECDD PENDING' WHEN CS.ECDD_STATUS = 'B' THEN 'NOT SET' ELSE CS.ECDD_STATUS END as 'ECDD', CASE WHEN CS.IDV_STATUS = 'Y' THEN 'IDVV PASSED' WHEN CS.IDV_STATUS = 'N' THEN 'IDVV FAILED' WHEN CS.IDV_STATUS = 'P' THEN 'IDVV PENDING' WHEN CS.IDV_STATUS = 'B' THEN 'NOT SET' ELSE CS.IDV_STATUS END as 'IDV', CASE WHEN CS.ADVV_STATUS = 'Y' THEN 'ADVV PASSED' WHEN CS.ADVV_STATUS = 'N' THEN 'ADVV FAILED' WHEN CS.ADVV_STATUS = 'P' THEN 'ADVV PENDING' WHEN CS.ADVV_STATUS = 'B' THEN 'NOT SET' ELSE CS.ADVV_STATUS END as 'ADVV', CASE WHEN CS.EV_STATUS = 'Y' THEN 'EV PASSED' WHEN CS.EV_STATUS = 'N' THEN 'EV FAILED' WHEN CS.EV_STATUS = 'P' THEN 'EV PENDING' WHEN CS.EV_STATUS = 'B' THEN 'NOT SET' ELSE CS.EV_STATUS END as 'EV', (SELECT CASE WHEN STATUS = 'A' THEN 'SOF PASSED'  WHEN STATUS = 'D' THEN 'SOF FAILED'  WHEN STATUS = 'B' THEN 'NOT SET' ELSE '' END FROM CPCE.CASE_DOCUMENTS_REFERENCES WHERE CASE_KEY = C.CASE_KEY AND DOCUMENT_TYPE = 'SF' ORDER BY CASE WHEN STATUS = 'A' THEN 0 WHEN STATUS = 'D' THEN 1 WHEN STATUS = 'B' THEN 3 ELSE 4 END ASC FETCH FIRST 1 ROWS ONLY) AS 'SOF', (SELECT CASE WHEN STATUS = 'A' THEN 'POT PASSED'  WHEN STATUS = 'D' THEN 'POT FAILED'  WHEN STATUS = 'B' THEN 'NOT SET' ELSE '' END FROM CPCE.CASE_DOCUMENTS_REFERENCES WHERE CASE_KEY = C.CASE_KEY AND DOCUMENT_TYPE = 'PT' ORDER BY CASE WHEN STATUS = 'A' THEN 0 WHEN STATUS = 'D' THEN 1 WHEN STATUS = 'B' THEN 3 ELSE 4 END ASC FETCH FIRST 1 ROWS ONLY) AS 'POT', (SELECT CASE WHEN STATUS = 'A' THEN 'RLT PASSED'  WHEN STATUS = 'D' THEN 'RLT FAILED'  WHEN STATUS = 'B' THEN 'NOT SET' ELSE '' END FROM CPCE.CASE_DOCUMENTS_REFERENCES WHERE CASE_KEY = C.CASE_KEY AND DOCUMENT_TYPE = 'RC' ORDER BY CASE WHEN STATUS = 'A' THEN 0 WHEN STATUS = 'D' THEN 1 WHEN STATUS = 'B' THEN 3 ELSE 4 END ASC FETCH FIRST 1 ROWS ONLY) AS 'RLT', (SELECT CASE WHEN STATUS = 'A' THEN 'EOE PASSED'  WHEN STATUS = 'D' THEN 'EOE FAILED'  WHEN STATUS = 'B' THEN 'NOT SET' ELSE '' END FROM CPCE.CASE_DOCUMENTS_REFERENCES WHERE CASE_KEY = C.CASE_KEY AND DOCUMENT_TYPE = 'WS' ORDER BY CASE WHEN STATUS = 'A' THEN 0 WHEN STATUS = 'D' THEN 1 WHEN STATUS = 'B' THEN 3 ELSE 4 END ASC FETCH FIRST 1 ROWS ONLY)  AS 'EOE', CASE WHEN CD.IS_LITE_CASE = 'Y' THEN 'Yes' WHEN CD.IS_LITE_CASE = 'N' THEN 'No' ELSE '' END as 'Is Lite Case', CD.NEW_IWATCH_RULES as 'New iWatch Rules', CD.NEW_RTRA_RULES as 'New RTRA Rules', CD.IWATCH_RULES_COUNT as 'iWatch Rules Count', CD.RTRA_RULES_COUNT as 'RTRA Rules Count', CD.ANTICIPATED_LAUNCH_DATE as 'Anticipated Launch Date' , (SELECT COUNT(*) FROM CPCE.APPLIED_CASE_SUBJECTS WHERE CPCE.APPLIED_CASE_SUBJECTS.CASE_KEY = C.CASE_KEY) as 'Applied Subject ID Count' , C.FREQUENCY_INDICATOR as 'Frequency Indicator' , GST.ATTRIBUTE_VALUE as 'Generic Subject Category' , CD.EXTERNAL_REFERENCE_ID AS 'External Reference Id', CD.REQUEST_RECEIPT_DATE AS 'Request Receipt Date' , CD.AGENTSCREENING_REQUEST_ID as 'Lookback Request Id' FROM CASEALL AS C LEFT OUTER JOIN USERTABLE AS AUSER ON C.ASSIGNED_TO_USER_ID = AUSER.USER_ID LEFT OUTER JOIN USERTABLE AS CUSER ON C.CREATE_USER_ID = CUSER.USER_ID LEFT OUTER JOIN USERTABLE AS QUSER ON C.QA_USER_ID = QUSER.USER_ID LEFT OUTER JOIN CPCE.CASE_DETAILS AS CD ON C.CASE_KEY = CD.CASE_KEY LEFT OUTER JOIN CPCE.CASE AS CCHILDRNR ON CCHILDRNR.REFERRED_ORIGINAL_CASE_KEY = CD.CASE_KEY AND CCHILDRNR.BUSINESS_GROUP_ID = 'R&R' LEFT OUTER JOIN CPCE.CASE_SUBJECT AS CS ON C.CASE_KEY = CS.CASE_KEY LEFT OUTER JOIN CPCE.SUBJECT AS S ON CS.SUBJECT_KEY = S.SUBJECT_KEY LEFT OUTER JOIN CPCE.INSTITUTION AS I ON I.INSTITUTION_ID = CD.INSTITUTION_ID LEFT OUTER JOIN CPCE.CASE_QA_LEVEL AS CL ON CL.CASE_QA_LEVEL_ID = CD.CASE_QA_LEVEL_ID LEFT OUTER JOIN CPCE.SUBJECT_TYPE ST ON S.Subject_Type_Id = ST.Subject_Type_Id LEFT OUTER JOIN SAREFILEDDATE AS SN ON C.CASE_KEY = SN.CASE_KEY LEFT OUTER JOIN CPCE.CASE_REPORT AS CRRREPORT ON CRRREPORT.CASE_KEY = CCHILDRNR.CASE_KEY AND CRRREPORT.DISCLOSURE_INDICATOR = 'R' LEFT OUTER JOIN CPCE.CASE_REPORT AS CRXREPORT ON CRXREPORT.CASE_KEY = CCHILDRNR.CASE_KEY AND CRXREPORT.DISCLOSURE_INDICATOR = 'X' LEFT OUTER JOIN CPCE.STATUS AS STS ON C.CASE_STATUS_ID = STS.STATUS_ID LEFT OUTER JOIN CPCE.STATUS AS STSQA ON C.QA_INDICATOR_ID = STSQA.STATUS_ID LEFT OUTER JOIN CPCE.SUB_STATUS AS SSTS ON CD.SUB_STATUS_ID = SSTS.SUB_STATUS_ID LEFT OUTER JOIN CPCE.SOURCE_OF_REFERRAL AS SREF ON CD.REFERRAL_SOURCE_ID = SREF.REFERRAL_SOURCE_ID LEFT OUTER JOIN CPCE.INTR_SOURCE AS ISRC ON CD.INTR_SOURCE_ID = ISRC.INTR_SOURCE_ID LEFT OUTER JOIN CPCE.COUNTRY AS CTRY ON CD.COUNTRY_KEY = CTRY.COUNTRY_KEY LEFT OUTER JOIN CPCE.COUNTRY AS CTOB ON CD.OUTBOUND_COUNTRY_KEY = CTOB.COUNTRY_KEY LEFT OUTER JOIN CPCE.COUNTRY AS CTIB ON CD.INBOUND_COUNTRY_KEY = CTIB.COUNTRY_KEY   LEFT OUTER JOIN CPCE.PRIORITY AS CSPR ON C.INITIAL_PRIORITY_ID = CSPR.PRIORITY_ID LEFT OUTER JOIN CPCE.PRIORITY AS CPFR ON C.FINAL_PRIORITY_ID = CPFR.PRIORITY_ID LEFT OUTER JOIN CPCE.RISK_TYPE AS RT ON CD.RISK_TYPE_ID = RT.RISK_TYPE_ID LEFT OUTER JOIN CPCE.BUSINESS_GROUP AS BG ON C.BUSINESS_GROUP_ID = BG.BUSINESS_GROUP_ID LEFT OUTER JOIN CPCE.CASE_BROKEN_RULE AS CBR ON C.CASE_KEY = CBR.CASE_KEY LEFT OUTER JOIN CPCE.USER_LOCATION AS UL ON AUSER.LOCATION_ID = UL.USER_LOCATION_ID LEFT OUTER JOIN CPCE.INVESTIGATIVE_GROUP AS INV ON INV.INV_GRP_ID = C.INV_GRP_ID LEFT OUTER JOIN CPCE.GROUP_CASE AS GRPCASE ON GRPCASE.CASE_KEY = C.CASE_KEY LEFT OUTER JOIN CPCE.CASE_METRIC AS CASEMETRIC ON CASEMETRIC.CASE_KEY = C.CASE_KEY LEFT OUTER JOIN CPCE.RISK_LEVEL AS RL ON CD.RISK_LEVEL_CODE = RL.RISK_LEVEL_CODE LEFT OUTER JOIN CPCE.GENERIC_SUBJECT_TYPE AS GST ON GST.ATTRIBUTE_ID = S.SUBJECT_CATEGORY LEFT OUTER JOIN CPCE.MYSTERY_SHOPPING_RESULT AS MSR ON MSR.ID = CD.MYSTERY_SHOPPING_RESULT_ID LEFT OUTER JOIN CPCE.CASE_APPOINTMENT AS CA ON C.CASE_KEY = CA.CASE_KEY LEFT OUTER JOIN IWRA.AGENT_PROFILE AS A ON A.ACCOUNT_NUMBER = CS.SUBJECT_ID LEFT OUTER JOIN iwra.agent_profile AS agtloc ON agtloc.LOCATION_ID = CS.SUBJECT_ID  AND agtloc.STATUS = 'A' LEFT OUTER JOIN IWRA.AGENT_LOCATION AS AL ON AL.LOCATION_ID = CS.SUBJECT_ID LEFT OUTER JOIN PDSPRODUCT PD ON C.CASE_KEY = PD.CASEKEY  LEFT OUTER JOIN CPCE.QUEUE Qu ON C.QUEUE_ID = Qu.Queue_ID LEFT OUTER JOIN CPCE.RISK_LEVEL AS RL2 ON CD.RESOLUTION_FINAL = RL2.RISK_LEVEL_CODE LEFT OUTER JOIN CPCE.COT_MASTER CMA on CMA.COT_FIELD_NAME = 'AgentBusiness' and CMA.ACM_VALUE = A.AGENT_BUSINESS LEFT OUTER JOIN CPCE.COT_MASTER CMO on CMO.COT_FIELD_NAME = 'OwnershipType' and CMO.ACM_VALUE = A.OWNERSHIP_TYPE LEFT OUTER JOIN CPCE.LAW_ENFORCEMENT_AGENCY LEA on LEA.LAW_ENFORCEMENT_AGENCY_ID = CD.LAW_ENFORCEMENT_AGENCY_ID LEFT OUTER JOIN CPCE.CASE_Documents_References CDR ON C.CASE_KEY = CDR.CASE_KEY LEFT OUTER JOIN CPCE.CASE_QA_RUBRIC AS QR ON C.CASE_ID = QR.CASE_ID LEFT OUTER JOIN CPCE.CL_QR_QA_STATUS_MAPPING QA_STAT_MAP ON QA_STAT_MAP.CL_QR_CASE_MODE_ID = QR.CASE_MODE AND QA_STAT_MAP.CL_QR_STATUS_ID = QR.STATUS_ID LEFT OUTER JOIN CPCE.CL_QR_QA_STATUS QA_STAT ON QA_STAT.CL_QR_QA_STATUS_ID = QA_STAT_MAP.CL_QR_QA_STATUS_ID LEFT OUTER JOIN CPCE.SUB_STATUS AS SSTS1 ON QR.QA_CURRENT_SUB_STATUS_ID = SSTS1.SUB_STATUS_ID LEFT OUTER JOIN USERTABLE QRUSER ON QRUSER.USER_ID = QR.ANALYST_ID LEFT OUTER JOIN CPCE.CASE_ACTION AS CAT ON CAT.CASE_ACTION_ID = (SELECT CASE_ACTION_ID FROM CPCE.CASE_ACTION WHERE C.CASE_KEY = CASE_KEY ORDER BY CASE_ACTION_ID DESC FETCH FIRST 1 ROWS ONLY) LEFT JOIN CPCE.SLA_DUE_DATE_EXTEND SLD ON SLD.DUE_DATE_EXTEND_ID = (SELECT DUE_DATE_EXTEND_ID FROM CPCE.SLA_DUE_DATE_EXTEND WHERE CAT.CASE_ACTION_ID = CASE_ACTION_ID ORDER BY DUE_DATE_EXTEND_ID DESC FETCH FIRST 1 ROWS ONLY)  LEFT OUTER JOIN CPCE.CASE_DETAILS_EXTN AS CDE ON C.CASE_KEY = CDE.CASE_KEY  LEFT OUTER JOIN CPCE.CASE_DISPOSITION_DETAIL AS CDD ON C.CASE_KEY = CDD.CASE_KEY  LEFT OUTER JOIN CPCE.CASE_LINK AS CL ON C.CASE_KEY = CL.CASE_KEY  FETCH FIRST 20000 ROWS ONLY",
		"select dateadd('day',5,current_timestamp)",
		"select decrypt('aravindh')",
		"WITH usertable	 (		  user_id,		  login_id,		  first_name,		  middle_name,		  last_name,		  location_id,		  institution_id	 ) 	 AS	 (			SELECT user_id,				   login_id,				   first_name,				   middle_name,				   last_name,				   location_id,				   institution_id			FROM   USER	 )      select user_id,		  login_id,		  first_name,		  middle_name,		  last_name,		  location_id,		  institution_id	from usertable"
		};
	foreach (var element in SelectCmds)
	{

		try
		{
			var x = GetDataFromSFCon(element);
			x.Dump();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
		}
	}
}

DataTable GetDataFromSFCon(string scmd)
{
	var cdataStr = "Host=cap.us-east-1.snowflakecomputing.com;account=cap;user=rameshkumar.venkatraman@westernunion.com;password=vp6VOYpw;Database=IWATCH;Schema=CPCE;Warehouse=DEMO_WH;";
	DataTable dt = new DataTable();
	using (var conn = new SnowflakeDbConnection())
	{
		conn.ConnectionString=cdataStr;
		conn.Open();
		var dap = new SnowflakeDbDataAdapter(scmd, conn);
		dap.Fill(dt);
	}
	return dt;
}