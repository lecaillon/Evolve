
-- Create table out_of_order
CREATE TABLE out_of_order (
     did    integer PRIMARY KEY,
     name   varchar(40) NOT NULL CHECK (name <> '')
);
