CREATE TABLE Usuarios (
    UsuarioId INT AUTO_INCREMENT PRIMARY KEY,
    Nome VARCHAR(500) NOT NULL,
    Email VARCHAR(500) NOT NULL UNIQUE,
    Senha VARCHAR(500) NOT NULL,
    Nivel INT NOT NULL
) ENGINE=InnoDB;

CREATE TABLE Autorizacao (
    AutorizacaoId INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(500) NOT NULL,    
    Tipo  VARCHAR(500) NOT NULL,    
    Codigo VARCHAR(64) NOT NULL,
    Validade DATETIME NOT NULL,
    Utilizado BIT NOT NULL,
    Tentativas INT NOT NULL DEFAULT 0
) ENGINE=InnoDB;

CREATE TABLE Cliente (
    ClienteId int NOT NULL AUTO_INCREMENT,
    RazaoSocial varchar(500) DEFAULT NULL,
    CnpjCpf varchar(30) DEFAULT NULL,
    Cep varchar(20) DEFAULT NULL,
    Logradouro varchar(500) DEFAULT NULL,
    Numero varchar(50) DEFAULT NULL,
    Complemento varchar(500) DEFAULT NULL,
    Bairro varchar(500) DEFAULT NULL,
    Cidade varchar(500) DEFAULT NULL,
    Estado varchar(2) DEFAULT NULL,
    Telefone varchar(50) DEFAULT NULL,
    DataCadastro datetime DEFAULT NULL,
    PRIMARY KEY (ClienteId)
) ENGINE=InnoDB;

CREATE TABLE Obra (
    ObraId int NOT NULL AUTO_INCREMENT,
    ClienteId int DEFAULT NULL,
    Descricao varchar(5000) DEFAULT NULL,
    Cep varchar(20) DEFAULT NULL,
    Logradouro varchar(500) DEFAULT NULL,
    Numero varchar(50) DEFAULT NULL,
    Complemento varchar(500) DEFAULT NULL,
    Bairro varchar(500) DEFAULT NULL,
    Cidade varchar(500) DEFAULT NULL,
    Estado varchar(2) DEFAULT NULL,
    DataCadastro datetime DEFAULT NULL,
    PRIMARY KEY (ObraId)
) ENGINE=InnoDB;

CREATE TABLE Orcamento (
    OrcamentoId int NOT NULL AUTO_INCREMENT,
    ObraId int DEFAULT NULL,
    Data datetime DEFAULT NULL,
    ObjetoProposta TEXT DEFAULT NULL,
    Valor decimal(19,4) DEFAULT NULL,
    CondPagamento TEXT DEFAULT NULL,
    Prazos TEXT DEFAULT NULL,
    Comentarios TEXT DEFAULT NULL,
    NotasGerais TEXT DEFAULT NULL,
    InicioObra datetime DEFAULT NULL,
    TerminoObra datetime DEFAULT NULL,
    Situacao varchar(50) DEFAULT NULL,
    PRIMARY KEY (OrcamentoId)
) ENGINE=InnoDB;

CREATE TABLE OrcamentoItem (
    OrcamentoItemId int NOT NULL AUTO_INCREMENT,
    OrcamentoId int NULL,
    Descricao varchar(5000) NULL,
    Valor decimal(19,4) NULL,
    PRIMARY KEY (OrcamentoItemId)
) ENGINE=InnoDB;

CREATE TABLE Fluxo (
    FluxoId INT AUTO_INCREMENT PRIMARY KEY,
    OrcamentoId INT NULL,
    Data DATETIME NULL,
    Descricao VARCHAR(5000) NULL,
    Debito_Credito VARCHAR(5) NULL,
    Valor DECIMAL(19,4) NULL,
    Categoria VARCHAR(500) NULL
) ENGINE=InnoDB;
