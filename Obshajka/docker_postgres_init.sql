CREATE TABLE IF NOT EXISTS users
(
    id BIGSERIAL PRIMARY KEY NOT NULL,
    name CHARACTER VARYING(30) NOT NULL,
    email CHARACTER VARYING(35) UNIQUE NOT NULL,
    password CHARACTER VARYING(35) NOT NULL
);

CREATE TABLE IF NOT EXISTS advertisements
(
    id BIGSERIAL PRIMARY KEY NOT NULL,
    creator_Id BIGINT NOT NULL,
    title CHARACTER VARYING(30) NOT NULL, 
    description TEXT,
    dormitory_Id INTEGER NOT NULL,
    price INTEGER,
    image TEXT,
    date_of_addition DATE NOT NULL,
    FOREIGN KEY (creator_id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS Dormitory_Id_Index ON advertisements (Creator_Id, Dormitory_Id);