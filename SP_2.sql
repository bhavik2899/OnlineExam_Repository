ALTER PROCEDURE Sp_GetWholeFeeStructureByPK
@feeStructureId BIGINT = 0
AS
BEGIN

	SELECT fh.PK_FeeHeadId,fh.HeadName,fh.IsOptional,fsh.Amount,fs.PK_FeeStructurId,fs.StructurName,fs.TotalFee,fs.StartDate,fs.EndDate
	FROM FeeHead as fh
		LEFT JOIN FeeStructurHead as fsh ON fh.PK_FeeHeadId = fsh.FK_FeeHeadId AND fsh.FK_FeeStructurId = @feeStructureId
		LEFT JOIN FeeStructur as fs ON fsh.FK_FeeStructurId = fs.PK_FeeStructurId
	WHERE fh.IsActive = 1 

END

--------------------------------------------------------------

ALTER PROCEDURE Sp_InsertUpdateFeeStructure
@feeStructureId BIGINT = 0,
@structureName NVARCHAR(50),
@startDate DATE,
@endDate DATE,
@feeStructureHead XML
AS
BEGIN

Begin Try	
	begin transaction 
		DECLARE @totalAmount MONEY = 0

		SELECT		
			xc.value('FeeHeadId[1]', 'BIGINT') as FeeHeadId,
			xc.value('Amount[1]', 'MONEY') as Amount,
			xc.value('IsOptional[1]', 'BIT') as IsOptional
			into #FeeHeadTable
			FROM 
			@feeStructureHead.nodes('/FeeHeadList/FeeHead') AS xt(xc)

		SET @totalAmount = (SELECT SUM(Amount) FROM #FeeHeadTable as t WHERE t.IsOptional = 0)

		if @feeStructureId != 0
		BEGIN
			DELETE FROM FeeStructurHead WHERE FK_FeeStructurId = @feeStructureId

			UPDATE FeeStructur 
				SET StructurName = @structureName, TotalFee = @totalAmount,StartDate = @startDate,EndDate = @endDate
				WHERE PK_FeeStructurId = @feeStructureId

			INSERT INTO FeeStructurHead(FK_FeeStructurId,FK_FeeHeadId,Amount,IsOptional)
				SELECT @feeStructureId,t.FeeHeadId,t.Amount,t.IsOptional
				FROM #FeeHeadTable as t 
		END

		ELSE
		BEGIN			
			DECLARE @feeId BIGINT = 0 
						
			INSERT INTO FeeStructur(StructurName,TotalFee,StartDate,EndDate,IsActive)
			VALUES(@structureName,@totalAmount,@startDate,@endDate,1)

			SET @feeId = SCOPE_IDENTITY()
			
			INSERT INTO FeeStructurHead(FK_FeeStructurId,FK_FeeHeadId,Amount,IsOptional)
			SELECT @feeId,t.FeeHeadId,t.Amount,t.IsOptional
			FROM #FeeHeadTable as t 
		END

		DROP TABLE #FeeHeadTable
    COMMIT
End Try
    Begin Catch
		IF @@TRANCOUNT > 0
        ROLLBACK
    End Catch
END

----------------------------------------------------------------------------

ALTER PROCEDURE Sp_GetStudentFeeScheduleByFeeStructureID
@feeStructureId BIGINT = 0
AS
BEGIN

;With cteStudentFeeSchedule AS(
	SELECT DISTINCT sfs.FK_FeeStructurId,sfs.PK_StudentFeeScheduleId,sfs.DiscountAmountPercentage,sfs.TotalPayableAmount,sfs.FK_StudentID,(case when sfc.PK_StudentFeeCollectionId is not null then CAST(1 as bit) else CAST(0 as bit) end) as UnPaid
		FROM FeeStructur as fs
			LEFT JOIN StudentFeeSchedule as sfs ON fs.PK_FeeStructurId = sfs.FK_FeeStructurId
			LEFT JOIN StudentFeeCollection as sfc ON  fs.PK_FeeStructurId = sfc.FK_FeeStructurId AND sfs.PK_StudentFeeScheduleId = sfc.FK_StudentFeeScheduleId AND sfc.FeeCancellationDate Is NULL
			WHERE fs.PK_FeeStructurId =1
)

SELECT cte.*,s.PK_StudentId,s.Name
	FROM cteStudentFeeSchedule as cte
		RIGHT JOIN Student as s ON cte.FK_StudentID = s.PK_StudentId
		WHERE s.IsActive = 1
END

----------------------------------------------------------------------------

ALTER PROCEDURE Sp_InsertUpdateStudentFeeSchedule
@studentFeeSchedule XML
AS
BEGIN

Begin Try	
	begin transaction 

	SELECT
	  xc.value('FeeStructurId[1]', 'BIGINT') as FeeStructurId,	  
	  xc.value('scheduleDate[1]', 'DATE') as scheduleDate,
	  xc.value('DiscountAmountPercentage[1]', 'INT') as DiscountAmountPercentage,
	  xc.value('TotalPayableAmount[1]', 'MONEY') as TotalPayableAmount,
	  xc.value('StudentID[1]', 'BIGINT') as StudentID,
	  xc.value('IsInclude[1]', 'BIT') as IsInclude
	  into #StudentFeeScheduleTable
	FROM 
	@studentFeeSchedule.nodes('/StudentFeeScheduleList/StudentFeeSchedule') AS xt(xc)

	UPDATE StudentFeeSchedule 
		SET FK_FeeStructurId = t.FeeStructurId, TotalFeeAmount = t.TotalPayableAmount,ScheduleDate = t.scheduleDate,DiscountAmountPercentage = t.DiscountAmountPercentage,TotalPayableAmount = t.TotalPayableAmount,FK_StudentID = StudentID
		FROM StudentFeeSchedule as sfs
		INNER JOIN #StudentFeeScheduleTable as t ON sfs.FK_FeeStructurId = t.FeeStructurId AND sfs.FK_StudentID = t.StudentID
		WHERE t.IsInclude = 1 ;

	DELETE t
		FROM #StudentFeeScheduleTable as t
		INNER JOIN StudentFeeSchedule as sfs ON t.FeeStructurId = sfs.FK_FeeStructurId  AND t.StudentID = sfs.FK_StudentID
		where t.IsInclude = 1;

	DELETE sfs
		FROM StudentFeeSchedule as sfs
		INNER JOIN #StudentFeeScheduleTable as t ON sfs.FK_FeeStructurId = t.FeeStructurId AND sfs.FK_StudentID = t.StudentID
		where t.IsInclude = 0;

	INSERT INTO StudentFeeSchedule(FK_FeeStructurId,TotalFeeAmount,ScheduleDate,DiscountAmountPercentage,TotalPayableAmount,FK_StudentID)
		SELECT t.FeeStructurId,t.TotalPayableAmount,t.scheduleDate,t.DiscountAmountPercentage,t.TotalPayableAmount,t.StudentID
		FROM #StudentFeeScheduleTable as t
		WHERE t.IsInclude = 1;

	DROP TABLE #StudentFeeScheduleTable
    COMMIT
End Try
    Begin Catch
		IF @@TRANCOUNT > 0
        ROLLBACK
    End Catch
END

----------------------------------------------------------------------------

ALTER PROCEDURE Sp_GetStudentFeeCollectionByStudent
@studentId BIGINT = 0
AS
BEGIN

SELECT DISTINCT fs.PK_FeeStructurId,fs.StructurName,sfs.PK_StudentFeeScheduleId,sfs.TotalFeeAmount,sfs.TotalPayableAmount	
	FROM StudentFeeSchedule as sfs
		INNER JOIN FeeStructur as fs ON sfs.FK_FeeStructurId = fs.PK_FeeStructurId
		WHERE sfs.FK_StudentID = @studentId

END

----------------------------------------------------------------------------

ALTER PROCEDURE Sp_GetStudentFeeCollectionByFeeStructure
@studentId BIGINT = 0,
@feeStructureId BIGINT = 0
AS
BEGIN

SELECT DISTINCT fs.PK_FeeStructurId,fs.StructurName,sfs.PK_StudentFeeScheduleId,sfs.TotalFeeAmount,sfs.TotalPayableAmount,fh.PK_FeeHeadId,fh.HeadName,fh.IsOptional,fsh.Amount*(100 - sfs.DiscountAmountPercentage)/100 as AmountByHead,sfc.PK_StudentFeeCollectionId,sfc.PaidAmount,sfch.PaidAmount as PaidHeadAmount
	FROM StudentFeeSchedule as sfs
		INNER JOIN FeeStructur as fs ON sfs.FK_FeeStructurId = fs.PK_FeeStructurId
		INNER JOIN FeeStructurHead as fsh ON fs.PK_FeeStructurId = fsh.FK_FeeStructurId
		INNER JOIN FeeHead as fh ON fsh.FK_FeeHeadId = fh.PK_FeeHeadId
		LEFT JOIN StudentFeeCollection as sfc 
			ON sfc.FK_StudentFeeScheduleId = sfs.PK_StudentFeeScheduleId AND sfc.FK_FeeStructurId = fs.PK_FeeStructurId AND sfc.FeeCancellationDate IS NULL
		LEFT JOIN StudentFeeCollectionHead as sfch
			ON sfch.FK_StudentFeeCollectionId = sfc.PK_StudentFeeCollectionId AND sfch.FK_StudentFeeScheduleId = sfs.PK_StudentFeeScheduleId AND sfch.FK_FeeHeadId = fh.PK_FeeHeadId AND sfch.IsFeeCancel = 0
		WHERE sfs.FK_StudentID = @studentId AND fs.PK_FeeStructurId = @feeStructureId
END

----------------------------------------------------------------------------

ALTER PROCEDURE Sp_InsertUpdateStudentFeeCollection
@studentId BIGINT = 0,
@feeStructureId BIGINT = 0,
@feeScheduleId BIGINT = 0,
@paidDate DATE,
@studentFeeCollection XML
AS
BEGIN

Begin Try	
	begin transaction 

	DECLARE @feeCollectionID BIGINT = 0;
	DECLARE @seqNo INT = 0;

	SELECT
	  xc.value('FeeHeadId[1]', 'BIGINT') as FeeHeadId,	  
	  xc.value('PayHeadAmount[1]', 'MONEY') as PayHeadAmount,
	  xc.value('IsOptional[1]', 'BIT') as IsOptional
	  into #StudentFeeCollectionTable
	FROM 
	@studentFeeCollection.nodes('/StudentFeeCollectionList/StudentFeeCollectionByHeadList/StudentFeeCollectionByHead') AS xt(xc)

	INSERT INTO StudentFeeCollection(FK_StudentId,FK_StudentFeeScheduleId,FK_FeeStructurId,PaidAmount,FeePaidDate)
		SELECT @studentId,@feeScheduleId,@feeStructureId,SUM(t.PayHeadAmount ),@paidDate
		FROM #StudentFeeCollectionTable as t

	SET @feeCollectionID = SCOPE_IDENTITY();
	UPDATE StudentFeeCollection SET ReceiptNO = @feeCollectionID WHERE PK_StudentFeeCollectionId = @feeCollectionID;

	INSERT INTO StudentFeeCollectionHead(FK_StudentFeeCollectionId,FK_StudentId,FK_StudentFeeScheduleId,PaidAmount,FK_FeeHeadId,PaidDate,IsFeeCancel)
		SELECT @feeCollectionID,@studentId,@feeScheduleId,t.PayHeadAmount,t.FeeHeadId,@paidDate,0
		FROM #StudentFeeCollectionTable as t

	UPDATE StudentFeeSchedule SET TotalPayableAmount = TotalPayableAmount - (SELECT SUM(t.PayHeadAmount) FROM #StudentFeeCollectionTable as t WHERE t.IsOptional = 0)
		WHERE  StudentFeeSchedule.PK_StudentFeeScheduleId = @feeScheduleId

	DROP TABLE #StudentFeeCollectionTable
    COMMIT
End Try
    Begin Catch
		IF @@TRANCOUNT > 0
        ROLLBACK
    End Catch
END

------------------------------------------------------------------------

ALTER PROCEDURE Sp_Report_Student_FeeCollection
AS
BEGIN

	DECLARE @cols AS NVARCHAR(MAX),
		@cols1 AS NVARCHAR(MAX),
		@query  AS NVARCHAR(MAX),
		@cols_ AS NVARCHAR(MAX),
		@cols1_ AS NVARCHAR(MAX)

	SELECT  DISTINCT fs.StructurName+'_SplitByPaid_'+fh.HeadName AS PaidStructure_Head,fs.StructurName+'_SplitByTotal_'+fh.HeadName AS TotalStructure_Head,fs.StructurName,s.PK_StudentId,s.Name,ISNULL(sfs.TotalFeeAmount,0) AS TotalFeeAmount, ISNULL(sfs.TotalPayableAmount,0) AS TotalPayableAmount,
			ISNULL(fsh.Amount*(100 - sfs.DiscountAmountPercentage)/100,0) as AmountByHead,ISNULL(sfc.PaidAmount,0) As PaidAmount,
			ISNULL((SELECT DISTINCT SUM(sfch.PaidAmount) FROM StudentFeeCollectionHead as sfch WHERE sfch.FK_StudentFeeScheduleId = sfs.PK_StudentFeeScheduleId AND sfch.FK_FeeHeadId = fh.PK_FeeHeadId),0) as PaidHeadAmount,
			fs.PK_FeeStructurId,fh.PK_FeeHeadId
		INTO #StudentFeeDetailsTable
		FROM FeeStructur as fs
			INNER JOIN FeeStructurHead as fsh ON fs.PK_FeeStructurId = fsh.FK_FeeStructurId
			INNER JOIN FeeHead as fh ON fsh.FK_FeeHeadId = fh.PK_FeeHeadId AND fh.IsOptional = 0
			INNER JOIN StudentFeeSchedule as sfs ON sfs.FK_FeeStructurId = fs.PK_FeeStructurId
			FULL JOIN Student as s ON sfs.FK_StudentID = s.PK_StudentId
			LEFT JOIN StudentFeeCollectionHead as sfch
				ON sfch.FK_StudentFeeScheduleId = sfs.PK_StudentFeeScheduleId AND sfch.FK_FeeHeadId = fh.PK_FeeHeadId AND sfch.IsFeeCancel = 0	
			LEFT JOIN StudentFeeCollection as sfc 
				ON sfc.PK_StudentFeeCollectionId = sfch.FK_StudentFeeScheduleId AND sfc.FeeCancellationDate IS NULL
		WHERE s.IsActive = 1 AND (fs.IsActive IS NULL OR fs.IsActive = 1)

	SET @cols = STUFF((SELECT distinct ',' + QUOTENAME(t.PaidStructure_Head) 
				FROM #StudentFeeDetailsTable t
				FOR XML PATH(''), TYPE
				).value('.', 'NVARCHAR(MAX)') 
			,1,1,'')

	SET @cols_ = STUFF((SELECT distinct ',sum(' + QUOTENAME(t.PaidStructure_Head)+') as' + QUOTENAME(t.PaidStructure_Head)
            FROM #StudentFeeDetailsTable t
            FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)') 
        ,1,1,'')

	SET @cols1 = STUFF((SELECT distinct ',' + QUOTENAME(t.TotalStructure_Head) 
            FROM #StudentFeeDetailsTable t
            FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)') 
        ,1,1,'')

	SET @cols1_ = STUFF((SELECT distinct ',sum(' + QUOTENAME(t.TotalStructure_Head) +') as' + QUOTENAME(t.TotalStructure_Head)
            FROM #StudentFeeDetailsTable t
            FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)') 
        ,1,1,'')	

	set @query = 'SELECT DISTINCT PK_StudentId,Name,' + @cols1_+','+@cols_ + ' from 
            (
                select PK_StudentId,Name,AmountByHead AS TotalHeadAmount,TotalStructure_Head,PaidHeadAmount,PaidStructure_Head
                from #StudentFeeDetailsTable Group by PK_StudentId,Name,AmountByHead,TotalStructure_Head,PaidHeadAmount,PaidStructure_Head
           ) as x
		    pivot 
            (
                SUM(TotalHeadAmount)
                for TotalStructure_Head in (' + @cols1 + ')
            ) as p1
            pivot 
            (
                 SUM(PaidHeadAmount)
                for PaidStructure_Head in (' + @cols + ')
            ) as p2 group by PK_StudentId,Name
			 ORDER BY PK_StudentId'

	execute(@query)
	DROP TABLE #StudentFeeDetailsTable		
END

-----------------------------------------------------------------------------
