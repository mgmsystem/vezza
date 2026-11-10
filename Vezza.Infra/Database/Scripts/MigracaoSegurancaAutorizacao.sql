ALTER TABLE Autorizacao
    MODIFY COLUMN Codigo VARCHAR(64) NOT NULL,
    ADD COLUMN Tentativas INT NOT NULL DEFAULT 0;

-- Códigos anteriores eram armazenados em texto puro e devem ser invalidados.
UPDATE Autorizacao
SET Utilizado = 1;
