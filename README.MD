Разработка интерактивных планов зданий университета для работы с it оборудованием.
1. Конфигурация клиента
Переменные окружения

REACT_APP_BASE_URL: URL хоста API сервера.
REACT_APP_SKIP_CATALOGUE_VALIDATION: Пропуск проверки размещения комнат перед созданием. По умолчанию: false.

Константы

Расположены в app/config/constants.ts.

Запуск клиента

Установить зависимости: npm i
Запустить сервер разработки: npm start
Собрать: npm build

2. Конфигурация сервера

Требуется ASP.NET Core Web SDK (.NET 9 или выше).

Запуск сервера

Запустить сервер локально: dotnet run
Собрать: dotnet publish

Конфигурация приложения в appsettings.json в проекте WebApi:

3. Настройка базы данных

Требуется серверная база данных (PostgreSQL).
База данных должна быть создана заранее.
Выполните миграции из проекта миграций или включите IN_PROCESS_MIGRATIONS_ON в appsettings.json.

Docker Compose для базы данных
Пример docker-compose.yml для настройки базы данных PostgreSQL:
version: '3.8'
services:
  db:
    image: postgres:16
    environment:
      POSTGRES_USER: appuser
      POSTGRES_PASSWORD: apppassword
      POSTGRES_DB: appdb
    ports:
      - "5432:5432"
    volumes:
      - db-data:/var/lib/postgresql/data
volumes:
  db-data:

Запуск базы данных

Сохраните файл docker-compose.yml.
Выполните: docker-compose up -d
Подключитесь к базе данных, используя строку подключения:Host=localhost;Port=5432;Database=appdb;Username=appuser;Password=apppassword


