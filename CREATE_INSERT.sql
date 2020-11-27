CREATE DATABASE OnlineExam
GO

USE OnlineExam

CREATE TABLE QueMaster 
(
	PK_QueMasterId  BIGINT IDENTITY(1, 1) NOT NULL, 
    Title NVARCHAR(200) NOT NULL,
	Details NVARCHAR(500) ,
	CorrectAnsId NVARCHAR(50),
	IsMultichoice BIT  NOT NULL,
	IsActive  BIT  NOT NULL,
    PRIMARY KEY (PK_QueMasterID)
)
GO

CREATE TABLE QueOptionMaster
(
	PK_QueOptionMasterId   BIGINT IDENTITY(1, 1) NOT NULL, 
    FK_QueMasterId BIGINT NOT NULL,
	Title NVARCHAR(100) NOT NULL,
	OrderNo INT,
	Details NVARCHAR(200),
	IsCorrect BIT  NOT NULL,
    PRIMARY KEY (PK_QueOptionMasterID),
	FOREIGN KEY (FK_QueMasterId) REFERENCES QueMaster(PK_QueMasterId)
)
Go

CREATE TABLE PaperMaster
(
	PK_PaperMasterId BIGINT IDENTITY(1, 1) NOT NULL, 
    Title NVARCHAR(50) NOT NULL,
	Details NVARCHAR(200),
	StartTime TIME,
	EndTime TIME,
	ExamDate DATE,
	NoOfQuestion INT,
	TotalMarks INT,
	PassingMarks INT,
	IsActive BIT  NOT NULL,
    PRIMARY KEY (PK_PaperMasterId)
)
Go

CREATE TABLE PaperQueMaster
(
	PK_PaperQueMasterId BIGINT IDENTITY(1, 1) NOT NULL, 
    FK_PaperMasterId BIGINT NOT NULL,
	FK_QueMasterId BIGINT,
	OrderNo INT,
    PRIMARY KEY (PK_PaperQueMasterId),
	FOREIGN KEY (FK_PaperMasterId) REFERENCES PaperMaster(PK_PaperMasterId),
	FOREIGN KEY (FK_QueMasterId) REFERENCES QueMaster(PK_QueMasterId)
)
Go

CREATE TABLE TestConductMaster
(
	PK_TestConductMasterId BIGINT IDENTITY(1, 1) NOT NULL, 
    UserName NVARCHAR(50) NOT NULL,
	Age INT,
	ConductDateTime DATETIME,
    PRIMARY KEY (PK_TestConductMasterID)
)
ALTER TABLE TestConductMaster ADD FK_PaperMasterId BIGINT , FOREIGN KEY (FK_PaperMasterId) REFERENCES PaperMaster(PK_PaperMasterId)
ALTER TABLE TestConductMaster ADD ResultMarks BIGINT
Go

CREATE TABLE TestConductAns
(
	PK_TestConductAnsId BIGINT IDENTITY(1, 1) NOT NULL, 
    FK_TestConductMasterId BIGINT NOT NULL,
	FK_QueMasterId BIGINT NOT NULL,
	SelectedAnsOptionId NVARCHAR(50),
    PRIMARY KEY (PK_TestConductAnsId),
	FOREIGN KEY (FK_TestConductMasterId) REFERENCES TestConductMaster(PK_TestConductMasterId),
	FOREIGN KEY (FK_QueMasterId) REFERENCES QueMaster(PK_QueMasterId)
)
Go

CREATE TABLE Student
(
	PK_StudentId BIGINT IDENTITY(1, 1) NOT NULL, 
	Name  NVARCHAR(50) NOT NULL,
	MobileNo NVARCHAR(15),
	Standard INT NOT NULL,
	Division  NVARCHAR(10),
	IsActive BIT NOT NULL,
    PRIMARY KEY (PK_StudentId)
)
Go

CREATE TABLE FeeHead
(
	PK_FeeHeadId BIGINT IDENTITY(1, 1) NOT NULL, 
	HeadName  NVARCHAR(50) NOT NULL,
	IsOptional BIT NOT NULL,
	IsActive BIT NOT NULL,
    PRIMARY KEY (PK_FeeHeadId)
)
Go

CREATE TABLE FeeStructur
(
	PK_FeeStructurId BIGINT IDENTITY(1, 1) NOT NULL, 
	StructurName  NVARCHAR(50) NOT NULL,
	TotalFee MONEY,
	StartDate DATE,
	EndDate DATE,
	IsActive BIT NOT NULL,
    PRIMARY KEY (PK_FeeStructurId)
)
Go

CREATE TABLE FeeStructurHead
(
	PK_FeeStructurHeadId BIGINT IDENTITY(1, 1) NOT NULL, 
    FK_FeeStructurId BIGINT ,
	FK_FeeHeadId BIGINT ,
	Amount MONEY,
	IsOptional BIT NOT NULL,
    PRIMARY KEY (PK_FeeStructurHeadId),
	FOREIGN KEY (FK_FeeStructurId) REFERENCES FeeStructur(PK_FeeStructurId),
	FOREIGN KEY (FK_FeeHeadId) REFERENCES FeeHead(PK_FeeHeadId)
)
Go

CREATE TABLE StudentFeeSchedule
(
	PK_StudentFeeScheduleId BIGINT IDENTITY(1, 1) NOT NULL, 
	FK_FeeStructurId BIGINT ,
    TotalFeeAmount MONEY,
	ScheduleDate DATE NOT NULL,
	DiscountAmountPercentage INT,
	TotalPayableAmount MONEY,
	FK_StudentID BIGINT NOT NULL,
    PRIMARY KEY (PK_StudentFeeScheduleId),
	FOREIGN KEY (FK_StudentId) REFERENCES Student(PK_StudentId),
	FOREIGN KEY (FK_FeeStructurId) REFERENCES FeeStructur(PK_FeeStructurId)
)
Go
ALTER TABLE StudentFeeSchedule ADD FK_FeeStructurId BIGINT , FOREIGN KEY (FK_FeeStructurId) REFERENCES FeeStructur(PK_FeeStructurId)

CREATE TABLE StudentFeeCollection
(
	PK_StudentFeeCollectionId BIGINT IDENTITY(1, 1) NOT NULL, 
    FK_StudentId BIGINT NOT NULL,
	FK_StudentFeeScheduleId BIGINT NOT NULL,
	FK_FeeStructurId BIGINT NOT NULL,
	PaidAmount MONEY NOT NULL,
	FeePaidDate DATE NOT NULL,
	FeeCancellationDate DATE,
	SeqNo INT,
	ReceiptNO BIGINT,
    PRIMARY KEY (PK_StudentFeeCollectionId),
	FOREIGN KEY (FK_StudentId) REFERENCES Student(PK_StudentId),
	FOREIGN KEY (FK_StudentFeeScheduleId) REFERENCES StudentFeeSchedule(PK_StudentFeeScheduleId),
	FOREIGN KEY (FK_FeeStructurId) REFERENCES FeeStructur(PK_FeeStructurId)
)
Go

CREATE TABLE StudentFeeCollectionHead
(
	PK_StudentFeeCollectionHeadId BIGINT IDENTITY(1, 1) NOT NULL, 
	FK_StudentFeeCollectionId BIGINT NOT NULL,
    FK_StudentId BIGINT NOT NULL,
	FK_StudentFeeScheduleId BIGINT NOT NULL,
	PaidAmount MONEY NOT NULL,
	FK_FeeHeadId BIGINT,
	PaidDate DATE,
	IsFeeCancel BIT,
    PRIMARY KEY (PK_StudentFeeCollectionHeadId),
	FOREIGN KEY (FK_StudentFeeCollectionId) REFERENCES StudentFeeCollection(PK_StudentFeeCollectionId),
	FOREIGN KEY (FK_StudentId) REFERENCES Student(PK_StudentId),
	FOREIGN KEY (FK_StudentFeeScheduleId) REFERENCES StudentFeeSchedule(PK_StudentFeeScheduleId),
	FOREIGN KEY (FK_FeeHeadId) REFERENCES FeeHead(PK_FeeHeadId),
)
Go



INSERT INTO QueMaster
VALUES('que1','','1',0,1)

INSERT INTO QueOptionMaster
VALUES(1,'option1',1,'',1),
(1,'option2',2,'',0)

INSERT INTO PaperMaster
VALUES('paper1','','08:00:00','08:30:00','2020-08-28',10,10,3,1)

INSERT INTO PaperQueMaster
VALUES(1,1,1)

INSERT INTO TestConductMaster
VALUES('username1',21,'2020-08-28 08:00:00')

INSERT INTO TestConductAns
VALUES(1,1,'1')


INSERT INTO Student
VALUES('Bhavik','6352215516',1,'A',1),
('Bhavik2','mobNo2',1,'B',1),
('Bhavik3','mobNo3',1,'A',1),
('Bhavik4','mobNo4',1,'B',1),
('Bhavik5','mobNo5',2,'A',1),
('Bhavik6','mobNo6',2,'B',1),
('Bhavik7','mobNo7',2,'A',1),
('Bhavik8','mobNo8',2,'B',1),
('Bhavik9','mobNo9',3,'A',1),
('Bhavik10','mobNo10',3,'B',1)

INSERT INTO FeeHead
VALUES('SchoolFee',0,1),
('TutionFee',0,1),
('SportFee',0,1),
('AdmissionFee',1,1)

INSERT INTO StudentFeeSchedule
VALUES(2500,'2020-09-17',0,2500,1,1)



SELECT @@SERVERNAME

SELECT * FROM QueMaster
SELECT * FROM QueOptionMaster
UPDATE QueOptionMaster SET Title = 'option2' , OrderNo = 2,IsCorrect = 0 where PK_QueOptionMasterId = 2