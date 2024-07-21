--############################## CREATE USER ###############################################

CREATE ROLE ai_db_users WITH
	NOLOGIN
	NOSUPERUSER
	NOCREATEDB
	NOCREATEROLE
	INHERIT
	NOREPLICATION
	CONNECTION LIMIT -1
	VALID UNTIL '2050-12-31T17:09:26+05:30' 
	PASSWORD 'postgres';		

CREATE USER dbuser WITH
	LOGIN
	CREATEDB
	CREATEROLE
	CONNECTION LIMIT -1
	VALID UNTIL '2050-12-31T12:00:00+05:30' 
	PASSWORD 'postgres';

GRANT ai_db_users TO dbuser;

GRANT ai_db_users TO "postgres";

GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO ai_db_users;

--############################### CREATE CORE DATABASE ##############################################

CREATE DATABASE ai_demo_products
		WITH 
		OWNER = ai_db_users
		ENCODING = 'UTF8'
		LC_COLLATE = 'en_US.UTF-8'
		LC_CTYPE = 'en_US.UTF-8'
		Template = template0
		CONNECTION LIMIT = -1;

GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO dbuser;

--################################ CREATE TABLES #############################################

DROP TABLE IF EXISTS public.products;

CREATE TABLE IF NOT EXISTS public.products
(
    id serial NOT NULL,
    name character varying(255) COLLATE pg_catalog."default" NOT NULL,
    description character varying(255) COLLATE pg_catalog."default" NOT NULL,
    supplier_id integer NOT NULL,
    category_id integer NOT NULL,
	in_stock integer,
    CONSTRAINT products_pkey PRIMARY KEY (id)
);

DROP TABLE IF EXISTS public.suppliers;

CREATE TABLE IF NOT EXISTS public.suppliers
(
    id serial NOT NULL,
    name character varying(255) COLLATE pg_catalog."default" NOT NULL,
    address character varying(255) COLLATE pg_catalog."default",
    CONSTRAINT suppliers_pkey PRIMARY KEY (id)
);

DROP TABLE IF EXISTS public.categories;

CREATE TABLE IF NOT EXISTS public.categories
(
    id serial NOT NULL,
    name character varying(255) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT categories_pkey PRIMARY KEY (id)
);


ALTER TABLE IF EXISTS public.products
    ADD CONSTRAINT fk_suppliers_id FOREIGN KEY (supplier_id)
    REFERENCES public.suppliers (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION
    NOT VALID;


ALTER TABLE IF EXISTS public.products
    ADD CONSTRAINT fk_categories_id FOREIGN KEY (category_id)
    REFERENCES public.categories (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION
    NOT VALID;