ALTER PROCEDURE sp_InsertUpdateQuestion
@queId BIGINT = 0,
@title NVARCHAR(200),
@details NVARCHAR(500),
@isMultiChoice BIT,
@options XML
AS

SELECT
  xc.value('QueId[1]', 'BIGINT') as QueId,
  xc.value('Title[1]', 'NVARCHAR(100)') as Title,
  xc.value('OrderNo[1]', 'INT') as OrderNo,
  xc.value('Details[1]', 'NVARCHAR(200)') as Details,
  xc.value('IsCorrect[1]', 'bit') as IsCorrect
  into #OptionsTable
  FROM 
  @options.nodes('/OptionList/Option') AS xt(xc)

	IF @queId != 0 
	BEGIN
		DECLARE @CorrectAnsID NVARCHAR(50) = null;
		DELETE FROM QueOptionMaster WHERE FK_QueMasterId = @queId;

		INSERT INTO QueOptionMaster(FK_QueMasterId,Title,OrderNo,Details,IsCorrect)
			SELECT t.QueId,t.Title,t.OrderNo,t.Details,t.IsCorrect
			FROM #OptionsTable t

		SET @CorrectAnsID = STUFF( (SELECT ','+ CAST(PK_QueOptionMasterId as nvarchar(64)) FROM QueOptionMaster WHERE( FK_QueMasterId = @queId AND IsCorrect = 1) ORDER BY QueOptionMaster.PK_QueOptionMasterId FOR XML PATH('')), 1, 1, '')

		UPDATE QueMaster
			SET Title = @title, Details = @details , CorrectAnsId = @CorrectAnsID , IsMultichoice = @isMultiChoice
			WHERE PK_QueMasterId = @queId;
	END

	ELSE
	BEGIN
		DECLARE @questionId BIGINT = 0;
		DECLARE @CorrectAnsIDs NVARCHAR(50) = null;

		INSERT INTO QueMaster(Title,Details,IsMultichoice,IsActive)
			VALUES(@title,@details,@isMultiChoice,1)

		SET @questionId = SCOPE_IDENTITY()

		INSERT INTO QueOptionMaster(FK_QueMasterId,Title,OrderNo,Details,IsCorrect)
			SELECT @questionId,t.Title,t.OrderNo,t.Details,t.IsCorrect
			FROM #OptionsTable t

		SET @CorrectAnsIDs = STUFF( (SELECT ','+ CAST(PK_QueOptionMasterId as nvarchar(64)) FROM QueOptionMaster WHERE( FK_QueMasterId = @questionId AND IsCorrect = 1) ORDER BY QueOptionMaster.PK_QueOptionMasterId FOR XML PATH('')), 1, 1, '')

		UPDATE QueMaster
			SET CorrectAnsId = @CorrectAnsIDs
			WHERE PK_QueMasterId = @questionId;

	END

	DROP TABLE #OptionsTable;
GO

----------------------------------------------------------

ALTER PROCEDURE sp_InsertUpdatePaper
@paperId BIGINT = 0,
@title NVARCHAR(50),
@details NVARCHAR(200),
@examDate DATE,
@startDate TIME,
@endDate TIME,
@totalMarks INT = 0,
@passingMarks INT = 0,
@questions XML
AS

SELECT
  xc.value('QueId[1]', 'BIGINT') as QueId,
  xc.value('IsIncluded[1]', 'BIT') as IsIncluded,
  xc.value('OrderNo[1]', 'INT') as OrderNo
  into #QuestionTable
  FROM 
  @questions.nodes('/QuestionList/Question') AS xt(xc)

	IF @paperId != 0 
	BEGIN
		DECLARE @noOfQue INT = 0;

		DELETE FROM PaperQueMaster WHERE FK_PaperMasterId = @paperId;

		INSERT INTO PaperQueMaster(FK_PaperMasterId,FK_QueMasterId,OrderNo)
			SELECT @paperId,t.QueId,t.OrderNo
			FROM #QuestionTable as t WHERE t.IsIncluded = 1;

		SET @noOfQue = (SELECT COUNT(DISTINCT QueId) FROM #QuestionTable as t WHERE t.IsIncluded = 1 )

		UPDATE PaperMaster
			SET Title = @title, Details = @details, StartTime = @startDate, EndTime = @endDate, ExamDate = @examDate, NoOfQuestion = @noOfQue, TotalMarks = @totalMarks, PassingMarks = @passingMarks
			WHERE PK_PaperMasterId = @paperId;
	END

	ELSE
	BEGIN
		DECLARE @papermasterID BIGINT = 0;
		DECLARE @noOfQuestions INT = 0;

		INSERT INTO PaperMaster(Title,Details,StartTime,EndTime,ExamDate,TotalMarks,PassingMarks,IsActive)
			VALUES(@title,@details,@startDate,@endDate,@examDate,@totalMarks,@passingMarks,1)

		SET @papermasterID =  SCOPE_IDENTITY()
		SET @noOfQuestions = (SELECT COUNT(DISTINCT QueId) FROM #QuestionTable as t WHERE t.IsIncluded = 1 )
		
		INSERT INTO PaperQueMaster(FK_PaperMasterId,FK_QueMasterId,OrderNo)
			SELECT @papermasterID,t.QueId,t.OrderNo
			FROM #QuestionTable as t WHERE t.IsIncluded = 1;

		UPDATE PaperMaster 
			SET NoOfQuestion = @noOfQuestions
			WHERE PK_PaperMasterId = @papermasterID;
	END

	DROP TABLE #QuestionTable;
GO

---------------------------------------------------------------------------

--sp_GetQuestionByPaperID @paperId = 0
ALTER PROCEDURE sp_GetQuestionByPaperID
@paperId BIGINT = 0
AS

select PK_QueMasterId,Title,pqm.OrderNo,(case when pqm.PK_PaperQueMasterId is not null then CAST(1 as bit) else CAST(0 as bit) end) as IsSelected  from QueMaster qm
left join PaperQueMaster pqm on pqm.FK_QueMasterId=qm.PK_QueMasterId and pqm.FK_PaperMasterId=@paperId
where qm.IsActive=1

Go
------------------------------------------------------------------------
--sp_GetPaperByPrimaryKey 1
ALTER PROCEDURE sp_GetPaperByPrimaryKey
@paperId BIGINT = 0
AS

SELECT * FROM PaperMaster as pm WHERE PK_PaperMasterId = @paperId AND pm.IsActive = 1

Go
---------------------------------------------------------------------
ALTER PROCEDURE sp_GetPaperList
AS
declare @examDate DATE

SET @examDate = GETDATE()

SELECT * FROM PaperMaster as pm WHERE pm.IsActive = 1 AND pm.ExamDate = @examDate

Go
---------------------------------------------------------------------

ALTER PROCEDURE sp_GetQueByPaperID
@paperId BIGINT = 0
AS

    SELECT DISTINCT DENSE_RANK() OVER(ORDER BY pqm.OrderNo,pqm.FK_QueMasterId) AS QuestionOrder,qm.PK_QueMasterId,qm.Title,qm.IsMultichoice,
		(SELECT ROW_NUMBER() OVER(PARTITION BY qm.PK_QueMasterId ORDER BY qom.OrderNo) AS OptionOrder,qom.PK_QueOptionMasterId,qom.Title as OptionTitle 
			FROM QueOptionMaster as qom
			WHERE qom.FK_QueMasterId = qm.PK_QueMasterId AND qm.IsActive = 1
			ORDER BY OptionOrder for Json path ) qom

        FROM PaperQueMaster as pqm
		LEFT JOIN QueMaster as qm ON pqm.FK_QueMasterId = qm.PK_QueMasterId
		LEFT JOIN QueOptionMaster as qom ON qm.PK_QueMasterId = qom.FK_QueMasterId
		WHERE pqm.FK_PaperMasterId = @paperId AND qm.IsActive = 1
		ORDER BY QuestionOrder
		FOR JSON path

Go
---------------------------------------------------------------------

ALTER PROCEDURE sp_PaperCompareTime
@paperId BIGINT = 0,
@time TIME
AS

SELECT PK_PaperMasterId FROM PaperMaster as pm WHERE pm.PK_PaperMasterId = @paperId AND pm.IsActive = 1 AND (@time BETWEEN pm.StartTime AND pm.EndTime)

Go
---------------------------------------------------------------------
ALTER PROCEDURE sp_InsertTestConduct
@userName NVARCHAR(50),
@age INT,
@conductDateTime DATETIME,
@paperId BIGINT,
@questionAns XML 
AS
declare @testConductId BIGINT =0;

SELECT
  xc.value('QueId[1]', 'BIGINT') as QueId,
  xc.value('OpId[1]', 'BIGINT') as AnsOptionId,
  xc.value('IsSelected[1]', 'BIT') as IsSelected
  into #QuestionAnsTable
  FROM 
  @questionAns.nodes('/Paper/Questions/OptionsList/Option') AS xt(xc)

  INSERT INTO TestConductMaster(UserName,Age,ConductDateTime,FK_PaperMasterId)
	VALUES(@userName,@age,@conductDateTime,@paperId)

	SET @testConductId = SCOPE_IDENTITY()

	INSERT INTO TestConductAns(FK_TestConductMasterId,FK_QueMasterId,SelectedAnsOptionId)
		SELECT DISTINCT @testConductId, t.QueId,STUFF( (SELECT ','+ CAST(ttt.AnsOptionId as nvarchar(64)) FROM #QuestionAnsTable as ttt WHERE( ttt.QueId = t.QueId AND ttt.IsSelected = 1) ORDER BY ttt.AnsOptionId FOR XML PATH('')), 1, 1, '')
		FROM #QuestionAnsTable as t where t.IsSelected = 1

	UPDATE TestConductMaster SET ResultMarks = (
												SELECT COUNT(1)  FROM TestConductAns as tca 
												INNER JOIN QueMaster as qm ON  tca.FK_QueMasterId = qm.PK_QueMasterId AND tca.SelectedAnsOptionId = qm.CorrectAnsId
												AND tca.FK_TestConductMasterId =@testConductId											
												)
		WHERE TestConductMaster.PK_TestConductMasterId = @testConductId	

DROP TABLE #QuestionAnsTable
Go

---------------------------------------------------------------------
--sp_PaperListByDate
ALTER PROCEDURE sp_PaperListByDate
@date Date = null
AS

SELECT DISTINCT pm.Title
	FROM PaperMaster pm
	INNER JOIN TestConductMaster as tcm ON pm.PK_PaperMasterId = tcm.FK_PaperMasterId
	WHERE CAST(tcm.ConductDateTime as date) = IsNull(@date,tcm.ConductDateTime )

Go
---------------------------------------------------------------------

ALTER PROCEDURE sp_ResultReport
@paperId BIGINT = 0
AS

SELECT tcm.UserName,pm.TotalMarks,pm.PassingMarks,tcm.ResultMarks,(case when tcm.ResultMarks >= pm.PassingMarks then 'Pass' else 'Fail' end) as Result 
	FROM TestConductMaster as tcm
	LEFT JOIN PaperMaster as pm ON tcm.FK_PaperMasterId = pm.PK_PaperMasterId 
	WHERE tcm.FK_PaperMasterId = @paperId

Go

---------------------------------------------------------------------

ALTER PROCEDURE sp_TestSolution
@paperId BIGINT = 0
AS
    SELECT DISTINCT DENSE_RANK() OVER(ORDER BY pqm.OrderNo,pqm.FK_QueMasterId) AS QuestionOrder,qm.PK_QueMasterId,qm.Title,qm.IsMultichoice,
		(SELECT ROW_NUMBER() OVER(PARTITION BY qm.PK_QueMasterId ORDER BY qom.OrderNo) AS OptionOrder,qom.PK_QueOptionMasterId,qom.Title as OptionTitle ,qom.IsCorrect
			FROM QueOptionMaster as qom
			WHERE qom.FK_QueMasterId = qm.PK_QueMasterId AND qm.IsActive = 1
			ORDER BY OptionOrder for Json path ) qom

        FROM TestConductMaster as tcm
		INNER JOIN PaperQueMaster as pqm ON tcm.FK_PaperMasterId = pqm.FK_PaperMasterId 
		LEFT JOIN QueMaster as qm ON pqm.FK_QueMasterId = qm.PK_QueMasterId
		LEFT JOIN QueOptionMaster as qom ON qm.PK_QueMasterId = qom.FK_QueMasterId
		WHERE tcm.FK_PaperMasterId = @paperId AND qm.IsActive = 1 
		ORDER BY QuestionOrder
		FOR JSON path
Go

---------------------------------------------------------------------
--sp_StudentTestSolution @paperId=8
ALTER PROCEDURE sp_StudentTestSolution
@paperId BIGINT = 0
AS
	SELECT tcmm.UserName,pm.TotalMarks,pm.PassingMarks,tcmm.ResultMarks,(case when tcmm.ResultMarks >= pm.PassingMarks then 'Pass' else 'Fail' end) as Result,
		(SELECT DISTINCT DENSE_RANK() OVER(ORDER BY pqm.OrderNo,pqm.FK_QueMasterId) AS QuestionOrder,qm.PK_QueMasterId,qm.Title,qm.IsMultichoice,
			(SELECT DISTINCT ROW_NUMBER() OVER(PARTITION BY qm.PK_QueMasterId ORDER BY qom.OrderNo) AS OptionOrder,qom.PK_QueOptionMasterId,qom.Title as OptionTitle ,qom.IsCorrect,
				(case when (SELECT * FROM SplitString(tca.SelectedAnsOptionId, ',') as t WHERE  t.OptionId= qom.PK_QueOptionMasterId) is not null then CAST(1 as bit) else CAST(0 as bit) end) as IsSelected 
				FROM QueOptionMaster as qom
				LEFT JOIN TestConductAns as tca ON qom.FK_QueMasterId = tca.FK_QueMasterId
				WHERE qom.FK_QueMasterId = qm.PK_QueMasterId AND tca.FK_TestConductMasterId = tcm.PK_TestConductMasterId
				ORDER BY OptionOrder for Json path 
			) qom

			FROM PaperQueMaster as pqm
			INNER JOIN TestConductMaster as tcm ON tcm.FK_PaperMasterId = pqm.FK_PaperMasterId
			LEFT JOIN QueMaster as qm ON pqm.FK_QueMasterId = qm.PK_QueMasterId
			WHERE tcm.PK_TestConductMasterId = tcmm.PK_TestConductMasterId
			ORDER BY QuestionOrder FOR JSON path
		) Questions

	FROM TestConductMaster AS tcmm
	INNER JOIN PaperMaster as pm ON tcmm.FK_PaperMasterId = pm.PK_PaperMasterId
	WHERE tcmm.FK_PaperMasterId = @paperId
	FOR JSON PATH
Go


---------------------------------------------------------------------
--sp_StudentTestSolution2 @paperId=8
CREATE PROCEDURE sp_StudentTestSolution2
@paperId BIGINT = 0
AS
	SELECT DISTINCT tcm.UserName,pm.TotalMarks,pm.PassingMarks,tcm.ResultMarks,(case when tcm.ResultMarks >= pm.PassingMarks then 'Pass' else 'Fail' end) as Result,
				DENSE_RANK() OVER(PARTITION BY pqm.FK_PaperMasterId ORDER BY pqm.OrderNo,pqm.FK_QueMasterId) AS QuestionOrder,qm.PK_QueMasterId,qm.Title,qm.IsMultichoice,
				DENSE_RANK() OVER(PARTITION BY pqm.FK_QueMasterId ORDER BY qom.OrderNo) AS OptionOrder,qom.PK_QueOptionMasterId,qom.Title as OptionTitle ,qom.IsCorrect,
				(case when (SELECT * FROM SplitString(tca.SelectedAnsOptionId, ',') as t WHERE  t.OptionId= qom.PK_QueOptionMasterId) is not null then CAST(1 as bit) else CAST(0 as bit) end) as IsSelected 
	FROM PaperMaster as pm
	INNER JOIN TestConductMaster as tcm ON tcm.FK_PaperMasterId = pm.PK_PaperMasterId
	INNER JOIN TestConductAns as tca ON tcm.PK_TestConductMasterId = tca.FK_TestConductMasterId
	INNER JOIN QueMaster as qm ON tca.FK_QueMasterId = PK_QueMasterId
	INNER JOIN QueOptionMaster as qom ON qm.PK_QueMasterId = qom.FK_QueMasterId
	INNER JOIN PaperQueMaster as pqm ON pqm.FK_PaperMasterId = pm.PK_PaperMasterId AND qm.PK_QueMasterId = pqm.FK_QueMasterId

	WHERE pm.PK_PaperMasterId = @paperId
	ORDER BY tcm.UserName,QuestionOrder,OptionOrder
Go
---------------------------------------------------------------------

create FUNCTION SplitString(@String NVARCHAR(50), @Delimiter char(1))       
returns @temptable TABLE (OptionId BIGINT)       
as       
begin      
    declare @idx int       
    declare @slice varchar(8000)       

    select @idx = 1       
        if len(@String)<1 or @String is null  return       

    while @idx!= 0       
    begin       
        set @idx = charindex(@Delimiter,@String)       
        if @idx!=0       
            set @slice = left(@String,@idx - 1)       
        else       
            set @slice = @String       

        if(len(@slice)>0)  
            insert into @temptable(OptionId) values(@slice)       

        set @String = right(@String,len(@String) - @idx)       
        if len(@String) = 0 break       
    end   
return 
end;