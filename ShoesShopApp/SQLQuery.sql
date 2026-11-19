/* =====================================================================
   ShoesShop - база данных ООО «Обувь» (СУБД: Microsoft SQL Server)
   Скрипт создаёт БД, таблицы (3НФ, ссылочная целостность) и загружает
   данные из файлов заказчика (Tovar, user_import, Заказ_import, Пункты выдачи).
   Кодировка файла: UTF-8 with BOM. Выполнять целиком в SSMS.
   ===================================================================== */
USE master;
GO
IF DB_ID(N'ShoesShop') IS NOT NULL
BEGIN
    ALTER DATABASE ShoesShop SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ShoesShop;
END
GO
CREATE DATABASE ShoesShop COLLATE Cyrillic_General_CI_AS;
GO
USE ShoesShop;
GO

/* ---------- Справочники ---------- */
CREATE TABLE Roles (
    RoleId   INT IDENTITY(1,1) NOT NULL,
    RoleName NVARCHAR(50)      NOT NULL,
    CONSTRAINT PK_Roles PRIMARY KEY (RoleId),
    CONSTRAINT UQ_Roles_RoleName UNIQUE (RoleName)
);
CREATE TABLE Categories (
    CategoryId   INT IDENTITY(1,1) NOT NULL,
    CategoryName NVARCHAR(50)      NOT NULL,
    CONSTRAINT PK_Categories PRIMARY KEY (CategoryId),
    CONSTRAINT UQ_Categories_CategoryName UNIQUE (CategoryName)
);
CREATE TABLE Manufacturers (
    ManufacturerId   INT IDENTITY(1,1) NOT NULL,
    ManufacturerName NVARCHAR(100)     NOT NULL,
    CONSTRAINT PK_Manufacturers PRIMARY KEY (ManufacturerId),
    CONSTRAINT UQ_Manufacturers_ManufacturerName UNIQUE (ManufacturerName)
);
CREATE TABLE Suppliers (
    SupplierId   INT IDENTITY(1,1) NOT NULL,
    SupplierName NVARCHAR(100)     NOT NULL,
    CONSTRAINT PK_Suppliers PRIMARY KEY (SupplierId),
    CONSTRAINT UQ_Suppliers_SupplierName UNIQUE (SupplierName)
);
CREATE TABLE OrderStatuses (
    StatusId   INT IDENTITY(1,1) NOT NULL,
    StatusName NVARCHAR(50)      NOT NULL,
    CONSTRAINT PK_OrderStatuses PRIMARY KEY (StatusId),
    CONSTRAINT UQ_OrderStatuses_StatusName UNIQUE (StatusName)
);
CREATE TABLE PickupPoints (
    PickupPointId INT IDENTITY(1,1) NOT NULL,
    Address       NVARCHAR(250)     NOT NULL,
    CONSTRAINT PK_PickupPoints PRIMARY KEY (PickupPointId),
    CONSTRAINT UQ_PickupPoints_Address UNIQUE (Address)
);

/* ---------- Основные таблицы ---------- */
CREATE TABLE Users (
    UserId   INT IDENTITY(1,1) NOT NULL,
    RoleId   INT               NOT NULL,
    FullName NVARCHAR(150)     NOT NULL,
    Login    NVARCHAR(100)     NOT NULL,
    Password NVARCHAR(50)      NOT NULL,
    CONSTRAINT PK_Users PRIMARY KEY (UserId),
    CONSTRAINT UQ_Users_Login UNIQUE (Login),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles (RoleId)
);
CREATE TABLE Products (
    ProductId      INT IDENTITY(1,1) NOT NULL,
    Article        NVARCHAR(20)      NOT NULL,
    ProductName    NVARCHAR(100)     NOT NULL,
    Unit           NVARCHAR(20)      NOT NULL,
    Price          DECIMAL(10,2)     NOT NULL,
    Discount       INT               NOT NULL CONSTRAINT DF_Products_Discount DEFAULT 0,
    Quantity       INT               NOT NULL CONSTRAINT DF_Products_Quantity DEFAULT 0,
    Description    NVARCHAR(500)     NULL,
    PhotoPath      NVARCHAR(260)     NULL,
    CategoryId     INT               NOT NULL,
    ManufacturerId INT               NOT NULL,
    SupplierId     INT               NOT NULL,
    CONSTRAINT PK_Products PRIMARY KEY (ProductId),
    CONSTRAINT UQ_Products_Article UNIQUE (Article),
    CONSTRAINT CK_Products_Price CHECK (Price >= 0),
    CONSTRAINT CK_Products_Discount CHECK (Discount BETWEEN 0 AND 100),
    CONSTRAINT CK_Products_Quantity CHECK (Quantity >= 0),
    CONSTRAINT FK_Products_Categories    FOREIGN KEY (CategoryId)     REFERENCES Categories (CategoryId),
    CONSTRAINT FK_Products_Manufacturers FOREIGN KEY (ManufacturerId) REFERENCES Manufacturers (ManufacturerId),
    CONSTRAINT FK_Products_Suppliers     FOREIGN KEY (SupplierId)     REFERENCES Suppliers (SupplierId)
);
CREATE TABLE Orders (
    OrderId       INT IDENTITY(1,1) NOT NULL,
    OrderDate     DATE              NOT NULL,
    DeliveryDate  DATE              NOT NULL,
    PickupPointId INT               NOT NULL,
    UserId        INT               NOT NULL,
    PickupCode    INT               NOT NULL,
    StatusId      INT               NOT NULL,
    CONSTRAINT PK_Orders PRIMARY KEY (OrderId),
    CONSTRAINT CK_Orders_Dates CHECK (DeliveryDate >= OrderDate),
    CONSTRAINT FK_Orders_PickupPoints  FOREIGN KEY (PickupPointId) REFERENCES PickupPoints (PickupPointId),
    CONSTRAINT FK_Orders_Users         FOREIGN KEY (UserId)        REFERENCES Users (UserId),
    CONSTRAINT FK_Orders_OrderStatuses FOREIGN KEY (StatusId)      REFERENCES OrderStatuses (StatusId)
);
/* Состав заказа. При удалении заказа его позиции удаляются каскадно;
   товар, входящий в заказ, удалить нельзя (нет каскада со стороны Products). */
CREATE TABLE OrderItems (
    OrderItemId INT IDENTITY(1,1) NOT NULL,
    OrderId     INT               NOT NULL,
    ProductId   INT               NOT NULL,
    Quantity    INT               NOT NULL,
    CONSTRAINT PK_OrderItems PRIMARY KEY (OrderItemId),
    CONSTRAINT UQ_OrderItems_Order_Product UNIQUE (OrderId, ProductId),
    CONSTRAINT CK_OrderItems_Quantity CHECK (Quantity > 0),
    CONSTRAINT FK_OrderItems_Orders   FOREIGN KEY (OrderId)   REFERENCES Orders (OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products (ProductId)
);
GO

/* ---------- Загрузка данных ---------- */

INSERT INTO Roles (RoleName) VALUES
    (N'Администратор'),
    (N'Менеджер'),
    (N'Авторизированный клиент');

INSERT INTO Categories (CategoryName) VALUES
    (N'Женская обувь'),
    (N'Мужская обувь');

INSERT INTO Manufacturers (ManufacturerName) VALUES
    (N'Alessio Nesca'),
    (N'CROSBY'),
    (N'Kari'),
    (N'Marco Tozzi'),
    (N'Rieker'),
    (N'Рос');

INSERT INTO Suppliers (SupplierName) VALUES
    (N'Kari'),
    (N'Обувь для вас');

INSERT INTO OrderStatuses (StatusName) VALUES
    (N'Завершен'),
    (N'Новый');

-- Номер пункта выдачи в заказах = порядковый номер строки в файле, поэтому ID задаём явно
SET IDENTITY_INSERT PickupPoints ON;
INSERT INTO PickupPoints (PickupPointId, Address) VALUES
    (1, N'420151, г. Лесной, ул. Вишневая, 32'),
    (2, N'125061, г. Лесной, ул. Подгорная, 8'),
    (3, N'630370, г. Лесной, ул. Шоссейная, 24'),
    (4, N'400562, г. Лесной, ул. Зеленая, 32'),
    (5, N'614510, г. Лесной, ул. Маяковского, 47'),
    (6, N'410542, г. Лесной, ул. Светлая, 46'),
    (7, N'620839, г. Лесной, ул. Цветочная, 8'),
    (8, N'443890, г. Лесной, ул. Коммунистическая, 1'),
    (9, N'603379, г. Лесной, ул. Спортивная, 46'),
    (10, N'603721, г. Лесной, ул. Гоголя, 41'),
    (11, N'410172, г. Лесной, ул. Северная, 13'),
    (12, N'614611, г. Лесной, ул. Молодежная, 50'),
    (13, N'454311, г.Лесной, ул. Новая, 19'),
    (14, N'660007, г.Лесной, ул. Октябрьская, 19'),
    (15, N'603036, г. Лесной, ул. Садовая, 4'),
    (16, N'394060, г.Лесной, ул. Фрунзе, 43'),
    (17, N'410661, г. Лесной, ул. Школьная, 50'),
    (18, N'625590, г. Лесной, ул. Коммунистическая, 20'),
    (19, N'625683, г. Лесной, ул. 8 Марта'),
    (20, N'450983, г.Лесной, ул. Комсомольская, 26'),
    (21, N'394782, г. Лесной, ул. Чехова, 3'),
    (22, N'603002, г. Лесной, ул. Дзержинского, 28'),
    (23, N'450558, г. Лесной, ул. Набережная, 30'),
    (24, N'344288, г. Лесной, ул. Чехова, 1'),
    (25, N'614164, г.Лесной, ул. Степная, 30'),
    (26, N'394242, г. Лесной, ул. Коммунистическая, 43'),
    (27, N'660540, г. Лесной, ул. Солнечная, 25'),
    (28, N'125837, г. Лесной, ул. Шоссейная, 40'),
    (29, N'125703, г. Лесной, ул. Партизанская, 49'),
    (30, N'625283, г. Лесной, ул. Победы, 46'),
    (31, N'614753, г. Лесной, ул. Полевая, 35'),
    (32, N'426030, г. Лесной, ул. Маяковского, 44'),
    (33, N'450375, г. Лесной ул. Клубная, 44'),
    (34, N'625560, г. Лесной, ул. Некрасова, 12'),
    (35, N'630201, г. Лесной, ул. Комсомольская, 17'),
    (36, N'190949, г. Лесной, ул. Мичурина, 26');
SET IDENTITY_INSERT PickupPoints OFF;

INSERT INTO Users (RoleId, FullName, Login, Password)
SELECT r.RoleId, v.FullName, v.Login, v.Password
FROM (VALUES
    (N'Администратор', N'Никифорова Весения Николаевна', N'94d5ous@gmail.com', N'uzWC67'),
    (N'Администратор', N'Сазонов Руслан Германович', N'uth4iz@mail.com', N'2L6KZG'),
    (N'Администратор', N'Одинцов Серафим Артёмович', N'yzls62@outlook.com', N'JlFRCZ'),
    (N'Менеджер', N'Степанов Михаил Артёмович', N'1diph5e@tutanota.com', N'8ntwUp'),
    (N'Менеджер', N'Ворсин Петр Евгеньевич', N'tjde7c@yahoo.com', N'YOyhfR'),
    (N'Менеджер', N'Старикова Елена Павловна', N'wpmrc3do@tutanota.com', N'RSbvHv'),
    (N'Авторизированный клиент', N'Михайлюк Анна Вячеславовна', N'5d4zbu@tutanota.com', N'rwVDh9'),
    (N'Авторизированный клиент', N'Ситдикова Елена Анатольевна', N'ptec8ym@yahoo.com', N'LdNyos'),
    (N'Авторизированный клиент', N'Ворсин Петр Евгеньевич', N'1qz4kw@mail.com', N'gynQMT'),
    (N'Авторизированный клиент', N'Старикова Елена Павловна', N'4np6se@mail.com', N'AtnDjr')
) AS v (RoleName, FullName, Login, Password)
JOIN Roles r ON r.RoleName = v.RoleName;
IF @@ROWCOUNT <> 10 THROW 50001, N'Users: загружены не все строки', 1;

INSERT INTO Products (Article, ProductName, Unit, Price, Discount, Quantity, Description, PhotoPath, CategoryId, ManufacturerId, SupplierId)
SELECT v.Article, v.ProductName, v.Unit, v.Price, v.Discount, v.Quantity, v.Description, v.PhotoPath, c.CategoryId, m.ManufacturerId, s.SupplierId
FROM (VALUES
    (N'А112Т4', N'Ботинки', N'шт.', 4990, 3, 6, N'Женские Ботинки демисезонные kari', N'Images\1.jpg', N'Женская обувь', N'Kari', N'Kari'),
    (N'F635R4', N'Ботинки', N'шт.', 3244, 2, 13, N'Ботинки Marco Tozzi женские демисезонные, размер 39, цвет бежевый', N'Images\2.jpg', N'Женская обувь', N'Marco Tozzi', N'Обувь для вас'),
    (N'H782T5', N'Туфли', N'шт.', 4499, 4, 5, N'Туфли kari мужские классика MYZ21AW-450A, размер 43, цвет: черный', N'Images\3.jpg', N'Мужская обувь', N'Kari', N'Kari'),
    (N'G783F5', N'Ботинки', N'шт.', 5900, 2, 8, N'Мужские ботинки Рос-Обувь кожаные с натуральным мехом', N'Images\4.jpg', N'Мужская обувь', N'Рос', N'Kari'),
    (N'J384T6', N'Ботинки', N'шт.', 3800, 2, 16, N'B3430/14 Полуботинки мужские Rieker', N'Images\5.jpg', N'Мужская обувь', N'Rieker', N'Обувь для вас'),
    (N'D572U8', N'Кроссовки', N'шт.', 4100, 3, 6, N'129615-4 Кроссовки мужские', N'Images\6.jpg', N'Мужская обувь', N'Рос', N'Обувь для вас'),
    (N'F572H7', N'Туфли', N'шт.', 2700, 2, 14, N'Туфли Marco Tozzi женские летние, размер 39, цвет черный', N'Images\7.jpg', N'Женская обувь', N'Marco Tozzi', N'Kari'),
    (N'D329H3', N'Полуботинки', N'шт.', 1890, 4, 4, N'Полуботинки Alessio Nesca женские 3-30797-47, размер 37, цвет: бордовый', N'Images\8.jpg', N'Женская обувь', N'Alessio Nesca', N'Обувь для вас'),
    (N'B320R5', N'Туфли', N'шт.', 4300, 2, 6, N'Туфли Rieker женские демисезонные, размер 41, цвет коричневый', N'Images\9.jpg', N'Женская обувь', N'Rieker', N'Kari'),
    (N'G432E4', N'Туфли', N'шт.', 2800, 3, 15, N'Туфли kari женские TR-YR-413017, размер 37, цвет: черный', N'Images\10.jpg', N'Женская обувь', N'Kari', N'Kari'),
    (N'S213E3', N'Полуботинки', N'шт.', 2156, 3, 6, N'407700/01-01 Полуботинки мужские CROSBY', NULL, N'Мужская обувь', N'CROSBY', N'Обувь для вас'),
    (N'E482R4', N'Полуботинки', N'шт.', 1800, 2, 14, N'Полуботинки kari женские MYZ20S-149, размер 41, цвет: черный', NULL, N'Женская обувь', N'Kari', N'Kari'),
    (N'S634B5', N'Кеды', N'шт.', 5500, 3, 0, N'Кеды Caprice мужские демисезонные, размер 42, цвет черный', NULL, N'Мужская обувь', N'CROSBY', N'Обувь для вас'),
    (N'K345R4', N'Полуботинки', N'шт.', 2100, 2, 3, N'407700/01-02 Полуботинки мужские CROSBY', NULL, N'Мужская обувь', N'CROSBY', N'Обувь для вас'),
    (N'O754F4', N'Туфли', N'шт.', 5400, 4, 18, N'Туфли женские демисезонные Rieker артикул 55073-68/37', NULL, N'Женская обувь', N'Rieker', N'Обувь для вас'),
    (N'G531F4', N'Ботинки', N'шт.', 6600, 12, 9, N'Ботинки женские зимние ROMER арт. 893167-01 Черный', NULL, N'Женская обувь', N'Kari', N'Kari'),
    (N'J542F5', N'Тапочки', N'шт.', 500, 13, 0, N'Тапочки мужские Арт.70701-55-67син р.41', NULL, N'Мужская обувь', N'Kari', N'Kari'),
    (N'B431R5', N'Ботинки', N'шт.', 2700, 2, 5, N'Мужские кожаные ботинки/мужские ботинки', NULL, N'Мужская обувь', N'Rieker', N'Обувь для вас'),
    (N'P764G4', N'Туфли', N'шт.', 6800, 15, 15, N'Туфли женские, ARGO, размер 38', NULL, N'Женская обувь', N'CROSBY', N'Kari'),
    (N'C436G5', N'Ботинки', N'шт.', 10200, 15, 9, N'Ботинки женские, ARGO, размер 40', NULL, N'Женская обувь', N'Alessio Nesca', N'Kari'),
    (N'F427R5', N'Ботинки', N'шт.', 11800, 15, 11, N'Ботинки на молнии с декоративной пряжкой FRAU', NULL, N'Женская обувь', N'Rieker', N'Обувь для вас'),
    (N'N457T5', N'Полуботинки', N'шт.', 4600, 3, 13, N'Полуботинки Ботинки черные зимние, мех', NULL, N'Женская обувь', N'CROSBY', N'Kari'),
    (N'D364R4', N'Туфли', N'шт.', 12400, 16, 5, N'Туфли Luiza Belly женские Kate-lazo черные из натуральной замши', NULL, N'Женская обувь', N'Kari', N'Kari'),
    (N'S326R5', N'Тапочки', N'шт.', 9900, 17, 15, N'Мужские кожаные тапочки "Профиль С.Дали"', NULL, N'Мужская обувь', N'CROSBY', N'Обувь для вас'),
    (N'L754R4', N'Полуботинки', N'шт.', 1700, 2, 7, N'Полуботинки kari женские WB2020SS-26, размер 38, цвет: черный', NULL, N'Женская обувь', N'Kari', N'Kari'),
    (N'M542T5', N'Кроссовки', N'шт.', 2800, 18, 3, N'Кроссовки мужские TOFA', NULL, N'Мужская обувь', N'Rieker', N'Обувь для вас'),
    (N'D268G5', N'Туфли', N'шт.', 4399, 3, 12, N'Туфли Rieker женские демисезонные, размер 36, цвет коричневый', NULL, N'Женская обувь', N'Rieker', N'Обувь для вас'),
    (N'T324F5', N'Сапоги', N'шт.', 4699, 2, 5, N'Сапоги замша Цвет: синий', NULL, N'Женская обувь', N'CROSBY', N'Kari'),
    (N'K358H6', N'Тапочки', N'шт.', 599, 20, 2, N'Тапочки мужские син р.41', NULL, N'Мужская обувь', N'Rieker', N'Kari'),
    (N'H535R5', N'Ботинки', N'шт.', 2300, 2, 7, N'Женские Ботинки демисезонные', NULL, N'Женская обувь', N'Rieker', N'Обувь для вас')
) AS v (Article, ProductName, Unit, Price, Discount, Quantity, Description, PhotoPath, CategoryName, ManufacturerName, SupplierName)
JOIN Categories    c ON c.CategoryName     = v.CategoryName
JOIN Manufacturers m ON m.ManufacturerName = v.ManufacturerName
JOIN Suppliers     s ON s.SupplierName     = v.SupplierName;
IF @@ROWCOUNT <> 30 THROW 50002, N'Products: загружены не все строки', 1;

-- Клиент в файле указан по ФИО. ФИО «Ворсин П. Е.» и «Старикова Е. П.» встречаются дважды
-- (менеджер и клиент), но в заказах не участвуют; для связи используем уникальный Login.
-- ДАННЫЕ: заказ 7: дата '30.02.2025' не существует, заменена на 2025-02-28
SET IDENTITY_INSERT Orders ON;
INSERT INTO Orders (OrderId, OrderDate, DeliveryDate, PickupPointId, UserId, PickupCode, StatusId)
SELECT v.OrderId, v.OrderDate, v.DeliveryDate, v.PickupPointId, u.UserId, v.PickupCode, st.StatusId
FROM (VALUES
    (1, '2025-02-27', '2025-04-20', 1, N'1diph5e@tutanota.com', 901, N'Завершен'),
    (2, '2022-09-28', '2025-04-21', 11, N'94d5ous@gmail.com', 902, N'Завершен'),
    (3, '2025-03-21', '2025-04-22', 2, N'uth4iz@mail.com', 903, N'Завершен'),
    (4, '2025-02-20', '2025-04-23', 11, N'yzls62@outlook.com', 904, N'Завершен'),
    (5, '2025-03-17', '2025-04-24', 2, N'1diph5e@tutanota.com', 905, N'Завершен'),
    (6, '2025-03-01', '2025-04-25', 15, N'94d5ous@gmail.com', 906, N'Завершен'),
    (7, '2025-02-28', '2025-04-26', 3, N'uth4iz@mail.com', 907, N'Завершен'),
    (8, '2025-03-31', '2025-04-27', 19, N'yzls62@outlook.com', 908, N'Новый'),
    (9, '2025-04-02', '2025-04-28', 5, N'1diph5e@tutanota.com', 909, N'Новый'),
    (10, '2025-04-03', '2025-04-29', 19, N'1diph5e@tutanota.com', 910, N'Новый')
) AS v (OrderId, OrderDate, DeliveryDate, PickupPointId, Login, PickupCode, StatusName)
JOIN Users u ON u.Login = v.Login
JOIN OrderStatuses st ON st.StatusName = v.StatusName;
IF @@ROWCOUNT <> 10 THROW 50003, N'Orders: загружены не все строки', 1;
SET IDENTITY_INSERT Orders OFF;

-- Состав заказов: «Артикул заказа» в файле = пары «артикул, количество»
INSERT INTO OrderItems (OrderId, ProductId, Quantity)
SELECT v.OrderId, p.ProductId, v.Quantity
FROM (VALUES
    (1, N'А112Т4', 2),
    (1, N'F635R4', 2),
    (2, N'H782T5', 1),
    (2, N'G783F5', 1),
    (3, N'J384T6', 10),
    (3, N'D572U8', 10),
    (4, N'F572H7', 5),
    (4, N'D329H3', 4),
    (5, N'А112Т4', 2),
    (5, N'F635R4', 2),
    (6, N'H782T5', 1),
    (6, N'G783F5', 1),
    (7, N'J384T6', 10),
    (7, N'D572U8', 10),
    (8, N'F572H7', 5),
    (8, N'D329H3', 4),
    (9, N'B320R5', 5),
    (9, N'G432E4', 1),
    (10, N'S213E3', 5),
    (10, N'E482R4', 5)
) AS v (OrderId, Article, Quantity)
JOIN Products p ON p.Article = v.Article;
IF @@ROWCOUNT <> 20 THROW 50004, N'OrderItems: загружены не все строки', 1;
GO

-- Контроль загрузки
SELECT N'Roles' AS TableName, COUNT(*) AS RowCnt FROM Roles UNION ALL SELECT N'Users', COUNT(*) FROM Users UNION ALL SELECT N'Categories', COUNT(*) FROM Categories UNION ALL SELECT N'Manufacturers', COUNT(*) FROM Manufacturers UNION ALL SELECT N'Suppliers', COUNT(*) FROM Suppliers UNION ALL SELECT N'Products', COUNT(*) FROM Products UNION ALL SELECT N'PickupPoints', COUNT(*) FROM PickupPoints UNION ALL SELECT N'OrderStatuses', COUNT(*) FROM OrderStatuses UNION ALL SELECT N'Orders', COUNT(*) FROM Orders UNION ALL SELECT N'OrderItems', COUNT(*) FROM OrderItems;
GO