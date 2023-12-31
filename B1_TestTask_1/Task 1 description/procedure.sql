USE [testTask_1]
GO
/****** Object:  StoredProcedure [dbo].[CalculateSumAndMedian]    Script Date: 08.12.2023 18:17:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[CalculateSumAndMedian]
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SumOfIntegers BIGINT;
    DECLARE @MedianOfDoubles FLOAT;

    -- Calculate the sum of integers
    SELECT @SumOfIntegers = SUM(CAST(IntegerColumn AS BIGINT))
    FROM StringsData;

    -- Calculate the median of doubles
    SELECT @MedianOfDoubles = (
        SELECT AVG(CAST(DoubleColumn AS FLOAT)) AS Median
        FROM (
            SELECT CAST(DoubleColumn AS FLOAT) AS DoubleColumn,
                   ROW_NUMBER() OVER (ORDER BY DoubleColumn) AS RowAsc,
                   ROW_NUMBER() OVER (ORDER BY DoubleColumn DESC) AS RowDesc
            FROM StringsData
        ) AS Subquery
        WHERE RowAsc = RowDesc OR RowAsc + 1 = RowDesc OR RowAsc = RowDesc + 1
    );

    -- Return the results
    SELECT @SumOfIntegers AS SumOfIntegers, @MedianOfDoubles AS MedianOfDoubles;
END;