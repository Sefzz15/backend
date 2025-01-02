// -- UPDATE users SET uname = 'new_username', upass = 'new_password' WHERE uid = 14;
// -- DELETE FROM users WHERE uid = 14;


// -- create database mydatabase;
// use mydatabase;
// -- -----------------------------------------------------

// DROP TABLE IF EXISTS users;
// DROP TABLE IF EXISTS OrderDetails;
// DROP TABLE IF EXISTS Orders;
// DROP TABLE IF EXISTS Products;
// DROP TABLE IF EXISTS Customers;

// -- -----------------------------------------------------

// CREATE TABLE Users (
// 	uid INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
// 	uname VARCHAR(40),
// 	upass VARCHAR(255)
// );

// -- -----------------------------------------------------

// CREATE TABLE Customers (
// 	c_id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
// 	first_name VARCHAR(50),
// 	last_name VARCHAR(50),
// 	email VARCHAR(100),
// 	phone VARCHAR(15) DEFAULT NULL,
// 	address VARCHAR(255) DEFAULT NULL,
// 	city VARCHAR(50)
// );

// -- -----------------------------------------------------

// CREATE TABLE Products (
// 	p_id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
// 	p_name VARCHAR(100),
// 	description TEXT  DEFAULT NULL,
// 	price DECIMAL(10, 2),
// 	stock_quantity INT
// );

// -- -----------------------------------------------------

// CREATE TABLE Orders (
//     o_id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
//     c_id INT,
//     o_date DATE NOT NULL,
//     total_amount DECIMAL(10, 2),
//     status ENUM('Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled', 'Refunded', 'Failed', 'On Hold') NOT NULL DEFAULT 'Pending',
//     FOREIGN KEY (c_id) REFERENCES Customers(c_id) ON DELETE CASCADE
// );



// -- -----------------------------------------------------

// CREATE TABLE OrderDetails (
//     o_details_id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
//     o_id INT,
//     p_id INT,
//     quantity INT,
//     price DECIMAL(10, 2),
//     FOREIGN KEY (o_id) REFERENCES Orders(o_id) ON DELETE CASCADE,
//     FOREIGN KEY (p_id) REFERENCES Products(p_id),
//     CONSTRAINT unique_order_product UNIQUE (o_id, p_id)
// );


// -- -----------------------------------------------------

// insert into users (uname, upass ) values 
// ('admin', 'admin'),
// ('sefzz', 'sefzz'),
// ('rita', 'rita');

// -- -----------------------------------------------------

// insert into Customers(first_name, last_name, email, phone, address, city) values 
// ('admin', 'admin', 'admin@gmail.com', '6912345678', 'Admin 13', 'Admin'),
// ('sefzz', 'sefzz', 'sefzz@gmail.com', '6923456789', 'Sefzz 28', 'Sefzz'),
// ('rita', 'rita', 'rita@gmail.com', '6934567891', 'Rita 45', 'Rita'),
// ('va', 'va', 'va@gmail.com', '6945678912', 'va 19', 'va'),
// ('ko', 'ko', 'ko@gmail.com', '6956789123', 'ko 12', 'ko'),
// ('frida', 'frida', 'frida@gmail.com', '6967891234', 'Frida 30', 'frida');

// -- -----------------------------------------------------

// INSERT INTO Products (p_id, p_name, Description, Price, stock_quantity) VALUES
// (1, 'Laptop', 'A high-performance laptop with 16GB RAM and 512GB SSD.', 999.99, 50),
// (2, 'Smartphone', 'A latest-generation smartphone with a stunning display.', 699.99, 100),
// (3, 'Headphones', 'Noise-cancelling headphones with superior sound quality.', 199.99, 200),
// (4, 'Gaming Chair', 'Ergonomic gaming chair with adjustable armrests.', 149.99, 30),
// (5, 'Mechanical Keyboard', 'RGB mechanical keyboard with blue switches.', 89.99, 75),
// (6, 'Monitor', '27-inch 4K UHD monitor with HDR support.', 349.99, 40),
// (7, 'Mouse', 'Wireless mouse with ergonomic design and high DPI.', 49.99, 150),
// (8, 'External SSD', '1TB portable SSD with USB-C connectivity.', 129.99, 60),
// (9, 'Smartwatch', 'Fitness-focused smartwatch with heart-rate tracking.', 249.99, 80),
// (10, 'Bluetooth Speaker', 'Compact Bluetooth speaker with rich bass.', 59.99, 120);

// -- -----------------------------------------------------

// INSERT INTO Orders (c_id, o_date, total_amount, Status) VALUES
// (1, '2024-12-01', 1699.98, 'Processing'),
// (2, '2024-12-02', 399.98, 'Shipped'),
// (3, '2024-12-03', 149.99, 'Pending'),   
// (4, '2024-12-04', 89.99, 'Delivered'), 
// (5, '2024-12-05', 349.99, 'Cancelled'),  
// (6, '2024-12-06', 99.98, 'Processing'),
// (1, '2024-12-07', 129.99, 'Pending'),    
// (3, '2024-12-08', 249.99, 'Delivered'),  
// (3, '2024-12-09', 179.97, 'Shipped'),     
// (5, '2024-12-10', 999.99, 'Processing'),  
// (6, '2024-12-11', 699.99, 'Pending'),
// (1, '2024-12-12', 1999.98, 'Shipped'),
// (2, '2024-12-13', 699.98, 'Delivered'),
// (5, '2024-12-14', 699.98, 'Processing'),
// (6, '2024-12-15', 149.99, 'On Hold');

// -- -----------------------------------------------------

// INSERT INTO OrderDetails (o_id, p_id, Quantity, Price) VALUES
// (1, 1, 1, 999.99),  
// (1, 2, 1, 699.99),  
// (2, 3, 2, 199.99),  
// (3, 4, 1, 149.99),  
// (4, 5, 1, 89.99),   
// (5, 6, 1, 349.99),  
// (6, 7, 2, 49.99),   
// (7, 8, 1, 129.99),  
// (8, 9, 1, 249.99),  
// (9, 10, 3, 59.99),  
// (10, 1, 1, 999.99), 
// (11, 2, 1, 699.99), 
// (12, 3, 1, 199.99), 
// (13, 5, 2, 89.99),  
// (14, 6, 1, 349.99); 

// -- -----------------------------------------------------

// select * from users;
// select * from Customers;
// select * from Products;
// select * from Orders;
// select * from OrderDetails;



// SELECT 
//     c.first_name AS CustomerName,
//     o.o_id AS OrderID,  -- Ensure the correct column name is used here
//     o.o_date AS OrderDate,
//     p.p_name AS ProductName,
//     od.quantity AS Quantity,
//     od.price AS PricePerUnit,
//     (od.quantity * od.price) AS TotalPriceForProduct,
//     o.total_amount AS TotalAmount
// FROM 
//     Customers c
// JOIN 
//     Orders o ON c.c_id = o.c_id
// JOIN 
//     OrderDetails od ON o.o_id = od.o_id
// JOIN 
//     Products p ON od.p_id = p.p_id
// WHERE 
//     c.first_name = 'admin';