CREATE DATABASE users_auth_db;

CREATE SEQUENCE userid_seq START 2;

CREATE TABLE users_auth
(
	"Id" INTEGER not null DEFAULT nextval('userid_seq'),
	"UserName" varchar(100) not null,
	"UserPassword" varchar(100) not null,
	"RegDate" timestamp not null,
	PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX users_auth_username_idx ON users_auth ("UserName");

INSERT INTO users_auth ("Id", "UserName", "UserPassword", "RegDate") VALUES (1, '1', 'AadGMxakRZvcx/nqDbAgEDyfTIXZvtuSV1iiTWUJPw/bLSgcYwNFkaUXzULq6jybNw==', '2022-08-21 00:00:00');
