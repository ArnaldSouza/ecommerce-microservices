-- Criar databases
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'EstoqueDB')
BEGIN
    CREATE DATABASE EstoqueDB;
END

IF NOT EXISTS (SELECT name FROM sys. databases WHERE name = 'VendasDB')  
BEGIN
    CREATE DATABASE VendasDB;
END

-- Aguardar criação
WAITFOR DELAY '00:00:05';

PRINT 'Databases EstoqueDB e VendasDB criados com sucesso! ';