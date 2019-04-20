CREATE SEQUENCE customer_seq;

CREATE TABLE customer_list (
    id INT PRIMARY KEY DEFAULT nextval('customer_seq'),
    customer string,
    address string
);

