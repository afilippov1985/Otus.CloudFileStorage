create database users_auth_db;

create table users_auth
(
	id uuid not null,
	user_name varchar(100) not null,
	user_password varchar(100) not null,
	reg_date timestamp not null
);

create unique index users_auth_id_uix on users_auth(id);

create unique index users_auth_pk on users_auth(id);

create unique index users_auth_name_password_uix on users_auth(user_name, user_password);