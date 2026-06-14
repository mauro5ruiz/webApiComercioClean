IF COL_LENGTH('Ventas', 'CreditoAplicado') IS NULL
BEGIN
    ALTER TABLE Ventas
    ADD CreditoAplicado DECIMAL(18, 2) NOT NULL
        CONSTRAINT DF_Ventas_CreditoAplicado DEFAULT (0);
END;

IF EXISTS (
    SELECT 1
    FROM sys.computed_columns
    WHERE object_id = OBJECT_ID('dbo.Ventas')
      AND name = 'SaldoPendiente'
)
BEGIN
    ALTER TABLE Ventas DROP COLUMN SaldoPendiente;

    ALTER TABLE Ventas
    ADD SaldoPendiente AS (ISNULL([Total], 0) - ISNULL([TotalPagado], 0) - ISNULL([CreditoAplicado], 0));
END;
