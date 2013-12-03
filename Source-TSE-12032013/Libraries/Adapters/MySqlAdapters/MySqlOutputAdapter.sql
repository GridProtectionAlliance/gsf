CREATE DATABASE OutputAdapter;

-- Uncomment the following GRANT statement to automatically
-- create a user with access to the OutputAdapter database.
-- MAKE SURE TO CHANGE THE USERNAME AND PASSWORD.

-- GRANT ALL ON OutputAdapter.* TO sampleUser IDENTIFIED BY 'samplePassword';

USE OutputAdapter;

CREATE TABLE Measurement (
	SignalID NCHAR(36) NULL,
	Timestamp BIGINT NOT NULL,
	Value DOUBLE NOT NULL
);