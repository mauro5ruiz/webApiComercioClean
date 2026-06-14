IF COL_LENGTH('Compras', 'CreditoAplicado') IS NULL
BEGIN
    ALTER TABLE Compras
    ADD CreditoAplicado DECIMAL(18, 2) NOT NULL
        CONSTRAINT DF_Compras_CreditoAplicado DEFAULT (0);
END;
