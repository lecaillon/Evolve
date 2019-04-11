CREATE TABLE product_information (
    product_id           INT PRIMARY KEY NOT NULL,
    product_name         STRING(50) UNIQUE NOT NULL,
    product_description  STRING(2000),
    category_id          STRING(1) NOT NULL CHECK (category_id IN ('A','B','C')),
    weight_class         INT,
    warranty_period      INT CONSTRAINT valid_warranty CHECK (warranty_period BETWEEN 0 AND 24),
    supplier_id          INT,
    product_status       STRING(20),
    list_price           DECIMAL(8,2),
    min_price            DECIMAL(8,2),
    catalog_url          STRING(50) UNIQUE,
    date_added           DATE DEFAULT CURRENT_DATE(),
    misc                 JSONB,     
    CONSTRAINT price_check CHECK (list_price >= min_price),
    INDEX date_added_idx (date_added),
    INDEX supp_id_prod_status_idx (supplier_id, product_status),
    INVERTED INDEX details (misc)
);

-------------------------------------------------

CREATE TABLE customers (
    id INT PRIMARY KEY,
    name STRING
);

CREATE TABLE orders (
    id INT PRIMARY KEY,
    customer_id INT REFERENCES customers(id) ON DELETE CASCADE
);

INSERT INTO customers VALUES (1, 'Lauren');
INSERT INTO orders VALUES (1,1);
DELETE FROM customers WHERE id = 1;
SELECT * FROM orders;

-------------------------------------------------

CREATE TABLE names (
    id INT PRIMARY KEY,
    first_name STRING,
    last_name STRING,
    full_name STRING AS (CONCAT(first_name, ' ', last_name)) STORED
);

INSERT INTO names (id, first_name, last_name) VALUES
    (1, 'Lola', 'McDog'),
    (2, 'Carl', 'Kimball'),
    (3, 'Ernie', 'Narayan');
