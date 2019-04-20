INSERT INTO customer_list (customer, address)
  VALUES
    ('Lauren', '123 Main Street'),
    ('Jesse', '456 Broad Ave'),
    ('Amruta', '9876 Green Parkway');

SELECT * FROM customer_list;

SELECT currval('customer_seq');
