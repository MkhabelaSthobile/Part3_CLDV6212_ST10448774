-- Creating Tbale User for (Customer or Admin) data
CREATE TABLE Users (
	Id			 INT PRIMARY KEY IDENTITY(1,1),
	Username	 NVARCHAR(100) NOT NULL,
	PasswordHash NVARCHAR(256) NOT NULL,
	Role		 NVARCHAR(20) NOT NULL --'Customer' or 'Admin'
);

-- Inserting Predetermined Customer & Admin Login data into Table Users
INSERT INTO Users (Username, PasswordHash, Role)
VALUES ('customer101', 'customerpass123', 'Customer'), 
	   ('admin01', 'adminpass123', 'Admin');

SELECT * FROM Users;


-- Creating Table Cart for shopping cart data
CREATE TABLE Cart (
	Id				 INT PRIMARY KEY IDENTITY,
	CustomerUsername NVARCHAR(100),
	ProductId		 NVARCHAR(100),
	Quantity		 INT
);

SELECT * FROM Cart;