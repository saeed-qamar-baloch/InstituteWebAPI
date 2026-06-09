-- ============================================================
-- Run this in SSMS against your Rozhn database.
-- Each block is guarded with IF NOT EXISTS so it's safe to
-- re-run even if some tables already exist.
-- ============================================================

-- 1. ResultApprovals
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ResultApprovals')
BEGIN
    CREATE TABLE [ResultApprovals] (
        [ApprovalID]       UNIQUEIDENTIFIER NOT NULL,
        [TermID]           UNIQUEIDENTIFIER NOT NULL,
        [CurrentClassID]   UNIQUEIDENTIFIER NOT NULL,
        [IsApproved]       BIT              NOT NULL DEFAULT 0,
        [ApprovedByUserID] NVARCHAR(MAX)    NULL,
        [ApprovedAt]       DATETIME2        NULL,
        [Remarks]          NVARCHAR(MAX)    NULL,
        [CreatedAt]        DATETIME2        NOT NULL,
        [UpdatedAt]        DATETIME2        NULL,
        CONSTRAINT [PK_ResultApprovals] PRIMARY KEY ([ApprovalID]),
        CONSTRAINT [FK_ResultApprovals_Term_TermID]
            FOREIGN KEY ([TermID]) REFERENCES [Term]([TermID]) ON DELETE CASCADE,
        CONSTRAINT [FK_ResultApprovals_CurrentClasses_CurrentClassID]
            FOREIGN KEY ([CurrentClassID]) REFERENCES [CurrentClasses]([CurrentClassID]) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX [IX_ResultApprovals_TermID_CurrentClassID]
        ON [ResultApprovals] ([TermID], [CurrentClassID]);
    PRINT 'Created: ResultApprovals';
END
ELSE PRINT 'Already exists: ResultApprovals';

-- 2. TerminalPassingMarks
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TerminalPassingMarks')
BEGIN
    CREATE TABLE [TerminalPassingMarks] (
        [TerminalPassingMarkID] UNIQUEIDENTIFIER NOT NULL,
        [TermID]                UNIQUEIDENTIFIER NOT NULL,
        [CurrentClassID]        UNIQUEIDENTIFIER NOT NULL,
        [PassingMarks]          REAL             NOT NULL,
        [CreatedAt]             DATETIME2        NOT NULL,
        [UpdatedAt]             DATETIME2        NULL,
        CONSTRAINT [PK_TerminalPassingMarks] PRIMARY KEY ([TerminalPassingMarkID]),
        CONSTRAINT [FK_TerminalPassingMarks_Term_TermID]
            FOREIGN KEY ([TermID]) REFERENCES [Term]([TermID]) ON DELETE CASCADE,
        CONSTRAINT [FK_TerminalPassingMarks_CurrentClasses_CurrentClassID]
            FOREIGN KEY ([CurrentClassID]) REFERENCES [CurrentClasses]([CurrentClassID]) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX [IX_TerminalPassingMarks_TermID_CurrentClassID]
        ON [TerminalPassingMarks] ([TermID], [CurrentClassID]);
    PRINT 'Created: TerminalPassingMarks';
END
ELSE PRINT 'Already exists: TerminalPassingMarks';

-- 3. MarkEditRequests
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MarkEditRequests')
BEGIN
    CREATE TABLE [MarkEditRequests] (
        [RequestID]          UNIQUEIDENTIFIER NOT NULL,
        [TeacherID]          UNIQUEIDENTIFIER NOT NULL,
        [StudentMarkID]      UNIQUEIDENTIFIER NOT NULL,
        [CurrentMarks]       REAL             NOT NULL,
        [RequestedMarks]     REAL             NOT NULL,
        [Reason]             NVARCHAR(MAX)    NOT NULL,
        [Status]             INT              NOT NULL DEFAULT 0,
        [ReviewedByUserID]   NVARCHAR(MAX)    NULL,
        [ReviewedAt]         DATETIME2        NULL,
        [ReviewRemarks]      NVARCHAR(MAX)    NULL,
        [CreatedAt]          DATETIME2        NOT NULL,
        [ModifiedAt]         DATETIME2        NOT NULL,
        CONSTRAINT [PK_MarkEditRequests] PRIMARY KEY ([RequestID]),
        CONSTRAINT [FK_MarkEditRequests_Teachers_TeacherID]
            FOREIGN KEY ([TeacherID]) REFERENCES [Teachers]([TeacherID]) ON DELETE CASCADE,
        CONSTRAINT [FK_MarkEditRequests_StudentMarks_StudentMarkID]
            FOREIGN KEY ([StudentMarkID]) REFERENCES [StudentMarks]([StudentMarkID])
    );
    CREATE INDEX [IX_MarkEditRequests_TeacherID]    ON [MarkEditRequests] ([TeacherID]);
    CREATE INDEX [IX_MarkEditRequests_StudentMarkID] ON [MarkEditRequests] ([StudentMarkID]);
    PRINT 'Created: MarkEditRequests';
END
ELSE PRINT 'Already exists: MarkEditRequests';

-- 4. TeacherDailyAttendances
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TeacherDailyAttendances')
BEGIN
    CREATE TABLE [TeacherDailyAttendances] (
        [TeacherDailyAttendanceID] UNIQUEIDENTIFIER NOT NULL,
        [TeacherID]                UNIQUEIDENTIFIER NOT NULL,
        [AttendanceDate]           DATE             NOT NULL,
        [Status]                   INT              NOT NULL DEFAULT 0,
        [ScannedBarcode]           NVARCHAR(MAX)    NULL,
        [ScannedAt]                DATETIME2        NULL,
        [MarkedByUserID]           NVARCHAR(MAX)    NULL,
        [Remarks]                  NVARCHAR(MAX)    NULL,
        [CreatedOn]                DATETIME2        NOT NULL,
        [UpdatedOn]                DATETIME2        NULL,
        CONSTRAINT [PK_TeacherDailyAttendances] PRIMARY KEY ([TeacherDailyAttendanceID]),
        CONSTRAINT [FK_TeacherDailyAttendances_Teachers_TeacherID]
            FOREIGN KEY ([TeacherID]) REFERENCES [Teachers]([TeacherID]) ON DELETE CASCADE
    );
    CREATE UNIQUE INDEX [IX_TeacherDailyAttendances_TeacherID_AttendanceDate]
        ON [TeacherDailyAttendances] ([TeacherID], [AttendanceDate]);
    PRINT 'Created: TeacherDailyAttendances';
END
ELSE PRINT 'Already exists: TeacherDailyAttendances';

-- 5. Notifications
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE [Notifications] (
        [NotificationID]   UNIQUEIDENTIFIER NOT NULL,
        [NotificationType] INT              NOT NULL DEFAULT 0,
        [RecipientType]    INT              NOT NULL DEFAULT 0,
        [RecipientID]      UNIQUEIDENTIFIER NULL,
        [Channel]          INT              NOT NULL DEFAULT 0,
        [Title]            NVARCHAR(MAX)    NOT NULL,
        [Message]          NVARCHAR(MAX)    NOT NULL,
        [Status]           INT              NOT NULL DEFAULT 0,
        [SentAt]           DATETIME2        NULL,
        [ErrorMessage]     NVARCHAR(MAX)    NULL,
        [CreatedByUserID]  NVARCHAR(MAX)    NULL,
        [CreatedAt]        DATETIME2        NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY ([NotificationID])
    );
    CREATE INDEX [IX_Notifications_RecipientID] ON [Notifications] ([RecipientID]);
    CREATE INDEX [IX_Notifications_Status]       ON [Notifications] ([Status]);
    PRINT 'Created: Notifications';
END
ELSE PRINT 'Already exists: Notifications';

-- 6. Guardians
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Guardians')
BEGIN
    CREATE TABLE [Guardians] (
        [GuardianID]   UNIQUEIDENTIFIER NOT NULL,
        [StudentID]    UNIQUEIDENTIFIER NOT NULL,
        [GuardianName] NVARCHAR(MAX)    NOT NULL,
        [Relation]     NVARCHAR(MAX)    NOT NULL,
        [Contact]      NVARCHAR(MAX)    NOT NULL,
        [Cnic]         NVARCHAR(MAX)    NULL,
        [Address]      NVARCHAR(MAX)    NULL,
        [Occupation]   NVARCHAR(MAX)    NULL,
        [Remarks]      NVARCHAR(MAX)    NULL,
        [CreatedAt]    DATETIME2        NOT NULL,
        [ModifiedAt]   DATETIME2        NOT NULL,
        CONSTRAINT [PK_Guardians] PRIMARY KEY ([GuardianID]),
        CONSTRAINT [FK_Guardians_Students_StudentID]
            FOREIGN KEY ([StudentID]) REFERENCES [Students]([StudentID]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_Guardians_StudentID] ON [Guardians] ([StudentID]);
    PRINT 'Created: Guardians';
END
ELSE PRINT 'Already exists: Guardians';

-- 7. StudentFeeHistories
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'StudentFeeHistories')
BEGIN
    CREATE TABLE [StudentFeeHistories] (
        [FeeHistoryID]  UNIQUEIDENTIFIER NOT NULL,
        [AdmissionID]   UNIQUEIDENTIFIER NOT NULL,
        [CourseID]      UNIQUEIDENTIFIER NOT NULL,
        [FeeAmount]     DECIMAL(18,2)    NOT NULL,
        [EffectiveFrom] DATE             NOT NULL,
        [EffectiveTo]   DATE             NULL,
        [IsActive]      BIT              NOT NULL DEFAULT 1,
        [Remarks]       NVARCHAR(MAX)    NULL,
        [CreatedAt]     DATETIME2        NOT NULL,
        CONSTRAINT [PK_StudentFeeHistories] PRIMARY KEY ([FeeHistoryID]),
        CONSTRAINT [FK_StudentFeeHistories_Admissions_AdmissionID]
            FOREIGN KEY ([AdmissionID]) REFERENCES [Admissions]([AdmissionID]),
        CONSTRAINT [FK_StudentFeeHistories_Courses_CourseID]
            FOREIGN KEY ([CourseID]) REFERENCES [Courses]([CourseID]) ON DELETE CASCADE
    );
    CREATE INDEX [IX_StudentFeeHistories_AdmissionID_EffectiveFrom]
        ON [StudentFeeHistories] ([AdmissionID], [EffectiveFrom]);
    PRINT 'Created: StudentFeeHistories';
END
ELSE PRINT 'Already exists: StudentFeeHistories';

-- 8. Scholarships
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Scholarships')
BEGIN
    CREATE TABLE [Scholarships] (
        [ScholarshipID]    UNIQUEIDENTIFIER NOT NULL,
        [StudentID]        UNIQUEIDENTIFIER NOT NULL,
        [AdmissionID]      UNIQUEIDENTIFIER NOT NULL,
        [DiscountPercent]  INT              NOT NULL,
        [FromMonth]        DATE             NOT NULL,
        [ToMonth]          DATE             NOT NULL,
        [Reason]           NVARCHAR(MAX)    NULL,
        [Status]           INT              NOT NULL DEFAULT 0,
        [CreatedByUserID]  NVARCHAR(MAX)    NULL,
        [CreatedAt]        DATETIME2        NOT NULL,
        [ModifiedAt]       DATETIME2        NOT NULL,
        CONSTRAINT [PK_Scholarships] PRIMARY KEY ([ScholarshipID]),
        CONSTRAINT [FK_Scholarships_Students_StudentID]
            FOREIGN KEY ([StudentID]) REFERENCES [Students]([StudentID]) ON DELETE CASCADE,
        CONSTRAINT [FK_Scholarships_Admissions_AdmissionID]
            FOREIGN KEY ([AdmissionID]) REFERENCES [Admissions]([AdmissionID])
    );
    CREATE INDEX [IX_Scholarships_StudentID]   ON [Scholarships] ([StudentID]);
    CREATE INDEX [IX_Scholarships_AdmissionID] ON [Scholarships] ([AdmissionID]);
    PRINT 'Created: Scholarships';
END
ELSE PRINT 'Already exists: Scholarships';

-- Done
PRINT '--- All missing tables created successfully ---';
